using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] Renderer[] targetRenderers;
    [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashDuration = 0.08f;
    [SerializeField] string colorProperty = "_BaseColor";

    MaterialPropertyBlock propertyBlock;
    Coroutine flashRoutine;

    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>();
    }

    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        SetColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        ClearColor();
        flashRoutine = null;
    }

    void SetColor(Color color)
    {
        foreach (Renderer r in targetRenderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorProperty, color);
            r.SetPropertyBlock(propertyBlock);
        }
    }

    void ClearColor()
    {
        foreach (Renderer r in targetRenderers)
        {
            if (r == null) continue;
            propertyBlock.Clear();
            r.SetPropertyBlock(propertyBlock);
        }
    }
}
