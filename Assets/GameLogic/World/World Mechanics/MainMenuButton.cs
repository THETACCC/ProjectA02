using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum MenuButtonType { StartGame, Settings, Credits, Exit }

public class MainMenuButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("References")]
    public MenuButtonType buttonType;
    public MainMenuPlayerMovement playerMovement;
    public TextMeshProUGUI label;

    [Tooltip("Only put images here that should FADE in/out on hover. Do NOT include SideLeft.")]
    public Image[] highlightImages;

    [Tooltip("SideLeft object (RectTransform) that should NEVER disappear, only scale on hover.")]
    public RectTransform sideLeft;

    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float hoverSpeed = 0.2f;
    public float clickScale = 0.9f;
    public float clickSpeed = 0.1f;

    [Header("SideLeft Hover Scale")]
    public float sideLeftHoverScale = 1.1f;
    public float sideLeftScaleSpeed = 0.2f;

    public Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color hoverColor = Color.white;

    Vector3 originalScale;
    Vector3 sideLeftOriginalScale;

    // --- fixes ---
    private bool _isHovered;
    private RectTransform _rt;
    private Canvas _canvas;
    private Camera _uiCam;

    public MenuController menuController;
    public GameObject mainMenu;
    public GameObject chapterMenu;

    void Awake()
    {
        _rt = transform as RectTransform;
        _canvas = GetComponentInParent<Canvas>();
        _uiCam = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _canvas.worldCamera : null;

        originalScale = transform.localScale;

        if (sideLeft != null)
            sideLeftOriginalScale = sideLeft.localScale;

        EnsureRaycastHitbox();

        // zero-out ONLY highlight images (SideLeft is NOT in this array)
        if (highlightImages != null)
        {
            for (int i = 0; i < highlightImages.Length; i++)
            {
                if (highlightImages[i] == null) continue;
                var c = highlightImages[i].color;
                c.a = 0f;
                highlightImages[i].color = c;
            }
        }

        if (label != null) label.color = defaultColor;
        _isHovered = false;
    }

    void Update()
    {
        // 兜底：有些情况下不会收到 OnPointerExit（盖面板/切 canvas/禁用等）
        if (_isHovered && !IsPointerOverThis())
        {
            ForceExitVisual();
        }
    }

    void OnDisable()
    {
        // 防止切二级界面/禁用对象后卡在 hover 状态
        ResetVisualImmediate();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        _isHovered = true;

        AudioPlayer.instance.playUIHoverSound();

        switch (buttonType)
        {
            case MenuButtonType.StartGame: playerMovement.OnHoverStartGame(); break;
            case MenuButtonType.Settings: playerMovement.OnHoverSettings(); break;
            case MenuButtonType.Credits: playerMovement.OnHoverCredits(); break;
            case MenuButtonType.Exit: playerMovement.OnHoverExit(); break;
        }

        StopAllCoroutines();
        StartCoroutine(ScaleTo(transform, originalScale * hoverScale, hoverSpeed));
        if (label != null) StartCoroutine(ColorTo(label, defaultColor, hoverColor, hoverSpeed));
        StartCoroutine(FadeImages(0f, 1f, hoverSpeed));

        if (sideLeft != null)
            StartCoroutine(ScaleTo(sideLeft, sideLeftOriginalScale * sideLeftHoverScale, sideLeftScaleSpeed));
    }

    public void OnPointerExit(PointerEventData e)
    {
        _isHovered = false;

        StopAllCoroutines();
        StartCoroutine(ScaleTo(transform, originalScale, hoverSpeed));
        if (label != null) StartCoroutine(ColorTo(label, hoverColor, defaultColor, hoverSpeed));
        StartCoroutine(FadeImages(1f, 0f, hoverSpeed));

        if (sideLeft != null)
            StartCoroutine(ScaleTo(sideLeft, sideLeftOriginalScale, sideLeftScaleSpeed));
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (buttonType == MenuButtonType.StartGame && SaveManager.Instance != null)
        {
            if (SaveManager.Instance.TryLoadTutorialIfFresh())
                return;
        }

        if (buttonType == MenuButtonType.StartGame)
        {
            if (mainMenu != null) mainMenu.SetActive(false);
            if (chapterMenu != null) chapterMenu.SetActive(true);
            if (menuController != null) menuController.MoveUIToEnd();
        }

        StopAllCoroutines();
        StartCoroutine(ClickEffect());
    }

    IEnumerator ScaleTo(Transform targetTransform, Vector3 target, float duration)
    {
        Vector3 start = targetTransform.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            targetTransform.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        targetTransform.localScale = target;
    }

    IEnumerator ColorTo(TextMeshProUGUI txt, Color from, Color to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            txt.color = Color.Lerp(from, to, t / duration);
            yield return null;
        }
        txt.color = to;
    }

    IEnumerator FadeImages(float from, float to, float duration)
    {
        if (highlightImages == null || highlightImages.Length == 0) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);

            for (int i = 0; i < highlightImages.Length; i++)
            {
                if (highlightImages[i] == null) continue;
                var c = highlightImages[i].color;
                c.a = alpha;
                highlightImages[i].color = c;
            }

            yield return null;
        }

        for (int i = 0; i < highlightImages.Length; i++)
        {
            if (highlightImages[i] == null) continue;
            var c = highlightImages[i].color;
            c.a = to;
            highlightImages[i].color = c;
        }
    }

    IEnumerator ClickEffect()
    {
        yield return ScaleTo(transform, originalScale * clickScale, clickSpeed);

        // 关键修复：用“是否还 hover 在本按钮”判断，而不是 IsPointerOverGameObject()
        bool stillHovered = _isHovered && IsPointerOverThis();
        Vector3 target = stillHovered ? originalScale * hoverScale : originalScale;

        yield return ScaleTo(transform, target, clickSpeed);

        // 再兜底一次：如果 click 后 UI 改了导致状态不一致，立刻纠正
        if (!IsPointerOverThis() && _isHovered)
        {
            ForceExitVisual();
        }
    }

    // ---------- helpers ----------

    private bool IsPointerOverThis()
    {
        if (_rt == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(_rt, Input.mousePosition, _uiCam);
    }

    private void ForceExitVisual()
    {
        _isHovered = false;
        StopAllCoroutines();

        StartCoroutine(ScaleTo(transform, originalScale, hoverSpeed));
        if (label != null) StartCoroutine(ColorTo(label, hoverColor, defaultColor, hoverSpeed));
        StartCoroutine(FadeImages(1f, 0f, hoverSpeed));

        if (sideLeft != null)
            StartCoroutine(ScaleTo(sideLeft, sideLeftOriginalScale, sideLeftScaleSpeed));
    }

    private void ResetVisualImmediate()
    {
        _isHovered = false;
        StopAllCoroutines();

        transform.localScale = originalScale;
        if (label != null) label.color = defaultColor;

        // highlights alpha=0
        if (highlightImages != null)
        {
            for (int i = 0; i < highlightImages.Length; i++)
            {
                if (highlightImages[i] == null) continue;
                var c = highlightImages[i].color;
                c.a = 0f;
                highlightImages[i].color = c;
            }
        }

        if (sideLeft != null) sideLeft.localScale = sideLeftOriginalScale;
    }

    private void EnsureRaycastHitbox()
    {
        // 解决“只有文字能 hover/点击”的手感问题：保证本物体上有一个可 raycast 的 Graphic
        var g = GetComponent<Graphic>();
        if (g == null)
        {
            var img = gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f); // 透明，不影响视觉
            img.raycastTarget = true;
        }
        else
        {
            g.raycastTarget = true;
        }
    }
}