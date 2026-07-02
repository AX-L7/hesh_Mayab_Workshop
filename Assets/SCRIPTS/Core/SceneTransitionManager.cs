using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] Color fadeColor = Color.black;
    [SerializeField] CanvasGroup fadeCanvasGroup;

    public static SceneTransitionManager Instance { get; private set; }

    bool isTransitioning;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup == null)
            CreateFadeCanvas();

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    void CreateFadeCanvas()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        fadeCanvasGroup = canvasObj.AddComponent<CanvasGroup>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        Image image = imageObj.AddComponent<Image>();
        image.color = fadeColor;

        RectTransform rt = imageObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    public void LoadScene(int sceneIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneIndex));
    }

    IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(1f));
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(Fade(0f));
        isTransitioning = false;
    }

    IEnumerator TransitionRoutine(int sceneIndex)
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(1f));
        SceneManager.LoadScene(sceneIndex);
        yield return StartCoroutine(Fade(0f));
        isTransitioning = false;
    }

    IEnumerator Fade(float targetAlpha)
    {
        fadeCanvasGroup.blocksRaycasts = true;
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.5f;
    }
}
