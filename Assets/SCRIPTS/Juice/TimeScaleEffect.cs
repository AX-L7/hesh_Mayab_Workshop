using System.Collections;
using UnityEngine;

public class TimeScaleEffect : MonoBehaviour
{
    [Header("Defaults")]
    [SerializeField] float defaultSlowScale = 0.1f;
    [SerializeField] float defaultDuration = 0.15f;

    public static TimeScaleEffect Instance { get; private set; }

    Coroutine currentEffect;

    void Awake()
    {
        Instance = this;
    }

    public void HitStop()
    {
        DoEffect(defaultSlowScale, defaultDuration);
    }

    public void DoEffect(float timeScale, float duration)
    {
        if (currentEffect != null)
            StopCoroutine(currentEffect);
        currentEffect = StartCoroutine(TimeScaleRoutine(timeScale, duration));
    }

    IEnumerator TimeScaleRoutine(float targetScale, float duration)
    {
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = 0.02f * targetScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        currentEffect = null;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
