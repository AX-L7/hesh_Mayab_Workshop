using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraRayObstructionDither : MonoBehaviour
{
    [Header("Target (player)")]
    public Transform target;

    [Header("Raycast")]
    public LayerMask obstructionMask = ~0;                    
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Fade")]
    [Range(0f, 1f)] public float obstructFade = 0.25f;        
    public float fadeDownSpeed = 10f;                         
    public float fadeUpSpeed = 10f;                         
    public string fadeProperty = "_Fade";                     

    [Header("Material")]
    public Material ditherTemplate;                           

    [Header("Debug")]
    public bool drawRay = false;

    class Obstructor
    {
        public Renderer rend;
        public Material[] originals;      
        public Material[] ditherInst;    
        public float currentFade = 1f;
        public float targetFade = 1f;
    }

    readonly Dictionary<Renderer, Obstructor> _tracked = new();
    readonly HashSet<Renderer> _hitsThisFrame = new();

    int _fadeID;

    void Awake()
    {
        _fadeID = Shader.PropertyToID(fadeProperty);
        if (!ditherTemplate) Debug.LogWarning($"{nameof(CameraRayObstructionDither)} needs a ditherTemplate assigned.");
    }

    void LateUpdate()
    {
        if (!target || !ditherTemplate) return;

        Vector3 camPos = transform.position;
        Vector3 dir = target.position - camPos;
        float dist = dir.magnitude;
        if (dist <= 0f) return;
        dir /= dist;

        if (drawRay) Debug.DrawLine(camPos, target.position, Color.cyan);

        _hitsThisFrame.Clear();
        var hits = Physics.RaycastAll(camPos, dir, dist, obstructionMask, triggerInteraction);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.transform.IsChildOf(target)) continue;          

            var rend = h.collider.GetComponentInParent<Renderer>();
            if (!rend) continue;

            _hitsThisFrame.Add(rend);
            if (!_tracked.TryGetValue(rend, out var ob))          
                _tracked[rend] = ob = CreateObstructor(rend);

            ob.targetFade = obstructFade;                         
        }

    
        foreach (var kv in _tracked)
            if (!_hitsThisFrame.Contains(kv.Key))
                kv.Value.targetFade = 1f;

     
        float dt = Time.deltaTime;
        var toRestore = new List<Renderer>();

        foreach (var kv in _tracked)
        {
            var ob = kv.Value;
            float sp = ob.targetFade < ob.currentFade ? fadeDownSpeed : fadeUpSpeed;
            ob.currentFade = Mathf.MoveTowards(ob.currentFade, ob.targetFade, sp * dt);

            
            if (ob.currentFade < 0.999f && ob.rend.materials != ob.ditherInst)
                ob.rend.materials = ob.ditherInst;

           
            for (int i = 0; i < ob.ditherInst.Length; i++)
                ob.ditherInst[i].SetFloat(_fadeID, ob.currentFade);

           
            if (ob.currentFade >= 0.999f && ob.targetFade >= 0.999f)
                toRestore.Add(ob.rend);
        }

        foreach (var r in toRestore) RestoreRenderer(r);
    }

    Obstructor CreateObstructor(Renderer r)
    {
        var ob = new Obstructor { rend = r };
        ob.originals = r.sharedMaterials;
        ob.ditherInst = new Material[ob.originals.Length];

        for (int i = 0; i < ob.ditherInst.Length; i++)
        {
            ob.ditherInst[i] = new Material(ditherTemplate);
            ob.ditherInst[i].SetFloat(_fadeID, 1f);               
        }

        return ob;
    }

    void RestoreRenderer(Renderer r)
    {
        if (!_tracked.TryGetValue(r, out var ob)) return;

        r.sharedMaterials = ob.originals;                         
        foreach (var m in ob.ditherInst)
            if (m) Destroy(m);

        _tracked.Remove(r);
    }

    void OnDisable() => RestoreAll();
    void OnDestroy() => RestoreAll();

    void RestoreAll()
    {
        foreach (var kv in _tracked) kv.Key.sharedMaterials = kv.Value.originals;
        foreach (var kv in _tracked)
            foreach (var m in kv.Value.ditherInst)
                if (m) Destroy(m);
        _tracked.Clear();
    }
}
