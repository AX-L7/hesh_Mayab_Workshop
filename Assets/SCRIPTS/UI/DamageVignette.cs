using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Image vignetteImage;
    [SerializeField] Color damageColor = new Color(1f, 0f, 0f, 0.4f);
    [SerializeField] float flashDuration = 0.3f;
    [SerializeField] float fadeSpeed = 2f;

    [Header("Low Health Pulse")]
    [SerializeField] bool pulseOnLowHealth = true;
    [SerializeField] float lowHealthThreshold = 0.25f;
    [SerializeField] float pulseSpeed = 2f;
    [SerializeField] float pulseAlpha = 0.15f;

    float currentAlpha;
    float healthPercent = 1f;

    void Awake()
    {
        if (vignetteImage != null)
        {
            Color c = damageColor;
            c.a = 0f;
            vignetteImage.color = c;
        }
    }

    void Update()
    {
        if (vignetteImage == null) return;

        if (currentAlpha > 0f)
        {
            currentAlpha -= fadeSpeed * Time.deltaTime;
            currentAlpha = Mathf.Max(0f, currentAlpha);
        }

        float targetAlpha = currentAlpha;

        if (pulseOnLowHealth && healthPercent <= lowHealthThreshold)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * 0.5f + 0.5f;
            targetAlpha = Mathf.Max(targetAlpha, pulse * pulseAlpha);
        }

        Color c = damageColor;
        c.a = targetAlpha;
        vignetteImage.color = c;
    }

    public void Flash()
    {
        currentAlpha = damageColor.a;
    }

    public void SetHealthPercent(float percent)
    {
        healthPercent = percent;
    }
}
