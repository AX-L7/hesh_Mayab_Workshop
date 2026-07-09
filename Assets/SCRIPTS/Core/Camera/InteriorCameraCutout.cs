using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class InteriorCameraCutout : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Vector3 playerOffset = new Vector3(0f, 1f, 0f);

    [Header("Interior View")]
    [SerializeField] private LayerMask excludedLayers;
    [SerializeField] private bool onlyWhenOccluded = true;
    [SerializeField] private float occlusionRadius = 0.08f;

    [Header("Cutout")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 cutoutSize = new Vector2(420f, 420f);
    [SerializeField, Range(0f, 1f)] private float opacity = 1f;
    [SerializeField] private float fadeSpeed = 10f;

    [Header("Render Texture")]
    [SerializeField, Range(0.25f, 1f)] private float renderScale = 1f;
    [SerializeField] private bool flipVertically;

    private Camera mainCamera;
    private Camera interiorCamera;

    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform maskRect;
    private RectTransform imageRect;
    private RawImage rawImage;

    private RenderTexture renderTexture;
    private int lastWidth;
    private int lastHeight;

    private float visibility;
    private readonly Collider[] hits = new Collider[32];

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();

        FindPlayer();
        CreateRenderTexture();
        CreateInteriorCamera();
        CreateCanvas();

        canvas.gameObject.SetActive(false);
        interiorCamera.enabled = false;
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        RecreateRenderTextureIfNeeded();

        UpdateVisibility();
        UpdateCutoutPosition();
        UpdateLayout();
    }

    private void OnDestroy()
    {
        ReleaseRenderTexture();

        if (interiorCamera != null)
            Destroy(interiorCamera.gameObject);

        if (canvas != null)
            Destroy(canvas.gameObject);
    }

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject foundPlayer = GameObject.FindGameObjectWithTag(playerTag);

        if (foundPlayer != null)
            player = foundPlayer.transform;

        if (player == null)
        {
            Debug.LogError("No se encontró el Player.", this);
            enabled = false;
        }
    }

    private void CreateInteriorCamera()
    {
        GameObject cameraObject = new GameObject("Interior Camera");
        cameraObject.transform.SetParent(transform, false);

        interiorCamera = cameraObject.AddComponent<Camera>();
        interiorCamera.CopyFrom(mainCamera);

        interiorCamera.depth = mainCamera.depth - 1f;
        interiorCamera.targetTexture = renderTexture;
        interiorCamera.cullingMask = mainCamera.cullingMask & ~excludedLayers.value;

        UniversalAdditionalCameraData urpData =
            cameraObject.GetComponent<UniversalAdditionalCameraData>();

        if (urpData == null)
            urpData = cameraObject.AddComponent<UniversalAdditionalCameraData>();

        urpData.renderType = CameraRenderType.Base;
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Interior Cutout Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasRect = canvasObject.GetComponent<RectTransform>();

        GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
        raycaster.enabled = false;

        GameObject maskObject = new GameObject(
            "Interior Cutout Mask",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Mask)
        );

        maskObject.transform.SetParent(canvasObject.transform, false);

        maskRect = maskObject.GetComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.5f, 0.5f);
        maskRect.anchorMax = new Vector2(0.5f, 0.5f);
        maskRect.pivot = new Vector2(0.5f, 0.5f);

        Image maskImage = maskObject.GetComponent<Image>();
        maskImage.sprite = CreateCircleSprite(512);
        maskImage.raycastTarget = false;

        Mask mask = maskObject.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject imageObject = new GameObject(
            "Interior Camera Image",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage)
        );

        imageObject.transform.SetParent(maskObject.transform, false);

        rawImage = imageObject.GetComponent<RawImage>();
        rawImage.texture = renderTexture;
        rawImage.raycastTarget = false;

        imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void CreateRenderTexture()
    {
        int width = Mathf.Max(1, Mathf.RoundToInt(Screen.width * renderScale));
        int height = Mathf.Max(1, Mathf.RoundToInt(Screen.height * renderScale));

        renderTexture = new RenderTexture(width, height, 24)
        {
            name = "RT_InteriorCamera",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            antiAliasing = 1
        };

        renderTexture.Create();

        lastWidth = width;
        lastHeight = height;
    }

    private void RecreateRenderTextureIfNeeded()
    {
        int width = Mathf.Max(1, Mathf.RoundToInt(Screen.width * renderScale));
        int height = Mathf.Max(1, Mathf.RoundToInt(Screen.height * renderScale));

        if (width == lastWidth && height == lastHeight)
            return;

        ReleaseRenderTexture();
        CreateRenderTexture();

        interiorCamera.targetTexture = renderTexture;
        rawImage.texture = renderTexture;
    }

    private void ReleaseRenderTexture()
    {
        if (interiorCamera != null)
            interiorCamera.targetTexture = null;

        if (renderTexture == null)
            return;

        renderTexture.Release();
        Destroy(renderTexture);
        renderTexture = null;
    }

    private void UpdateVisibility()
    {
        bool shouldShow =
            !onlyWhenOccluded ||
            IsPlayerOccluded();

        float targetVisibility = shouldShow ? 1f : 0f;

        visibility = Mathf.MoveTowards(
            visibility,
            targetVisibility,
            fadeSpeed * Time.unscaledDeltaTime
        );

        bool active = visibility > 0.01f;

        canvas.gameObject.SetActive(active);
        interiorCamera.enabled = active;

        Color color = Color.white;
        color.a = visibility * opacity;

        rawImage.color = color;

        rawImage.uvRect = flipVertically
            ? new Rect(0f, 1f, 1f, -1f)
            : new Rect(0f, 0f, 1f, 1f);
    }

    private bool IsPlayerOccluded()
    {
        if (excludedLayers.value == 0)
            return false;

        Vector3 from = mainCamera.transform.position;
        Vector3 to = player.position + playerOffset;

        int count = Physics.OverlapCapsuleNonAlloc(
            from,
            to,
            occlusionRadius,
            hits,
            excludedLayers,
            QueryTriggerInteraction.Ignore
        );

        return count > 0;
    }

    private void UpdateCutoutPosition()
    {
        if (visibility <= 0f)
            return;

        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(player.position + playerOffset);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null,
            out Vector2 localPosition
        );

        maskRect.anchoredPosition = localPosition;
    }

    private void UpdateLayout()
    {
        maskRect.sizeDelta = cutoutSize;

        imageRect.sizeDelta = canvasRect.rect.size;
        imageRect.anchoredPosition = -maskRect.anchoredPosition;
    }

    private Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        )
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    center
                );

                float alpha = distance <= radius ? 1f : 0f;

                pixels[y * size + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f)
        );
    }
}