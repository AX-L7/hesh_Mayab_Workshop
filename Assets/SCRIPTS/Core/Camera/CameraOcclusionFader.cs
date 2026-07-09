using System.Collections.Generic;
using UnityEngine;

public class CameraOcclusionFader : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    public LayerMask occludeLayer;       // layer(s) to fade

    [Header("Fade Settings")]
    public float fadeSpeed = 5f;
    public float fadedAmount = 0.85f; // how transparent when blocking

    static readonly int FadeID = Shader.PropertyToID("_FadeAmount");

    // Track objects we're currently fading so we can restore them
    Dictionary<Renderer, MaterialPropertyBlock> _faded = new();
    HashSet<Renderer> _current = new();
    MaterialPropertyBlock _mpb;

    void Awake() => _mpb = new MaterialPropertyBlock();

    void LateUpdate()
    {
        if (!player) return;

        _current.Clear();

        // Cast from camera to player
        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;
        var hits = Physics.RaycastAll(transform.position, dir.normalized,
                                           dist, occludeLayer);

        foreach (var hit in hits)
        {
            var r = hit.collider.GetComponent<Renderer>();
            if (r == null) continue;

            _current.Add(r);

            if (!_faded.ContainsKey(r))
                _faded[r] = new MaterialPropertyBlock();

            r.GetPropertyBlock(_faded[r]);
            float cur = _faded[r].GetFloat(FadeID);
            _faded[r].SetFloat(FadeID,
                Mathf.Lerp(cur, fadedAmount, Time.deltaTime * fadeSpeed));
            r.SetPropertyBlock(_faded[r]);
        }

        // Restore objects no longer blocking
        var toRestore = new List<Renderer>();
        foreach (var kv in _faded)
        {
            if (_current.Contains(kv.Key)) continue;

            kv.Key.GetPropertyBlock(kv.Value);
            float cur = kv.Value.GetFloat(FadeID);
            float next = Mathf.Lerp(cur, 0f, Time.deltaTime * fadeSpeed);
            kv.Value.SetFloat(FadeID, next);
            kv.Key.SetPropertyBlock(kv.Value);

            if (next < 0.01f) toRestore.Add(kv.Key);
        }
        foreach (var r in toRestore) _faded.Remove(r);
    }
}