using TMPro;
using UnityEngine;

public class StrongGuideController : MonoBehaviour
{
    [Header("Auto References")]
    [SerializeField] private StrongGuideOverlay overlay;
    [SerializeField] private RectTransform textPanel;
    [SerializeField] private TMP_Text guideTMP;

    [Header("Defaults")]
    [SerializeField] private Vector2 defaultTextOffset = new Vector2(180f, 0f);
    [SerializeField] private float defaultPadding = 24f;
    [SerializeField] private bool hideOnRightMouseDown = true;

    private bool waitingForRightMouseDown = false;
    private bool isShowing = false;

    private void Awake()
    {
        AutoAssignIfNeeded();
    }

    private void Start()
    {
        HideGuide();
    }

    private void Update()
    {
        if (hideOnRightMouseDown && waitingForRightMouseDown && Input.GetMouseButtonDown(1))
        {
            HideGuide();
        }
    }

    private void AutoAssignIfNeeded()
    {
        if (overlay == null)
            overlay = GetComponentInChildren<StrongGuideOverlay>(true);

        if (textPanel == null)
        {
            Transform t = transform.Find("TextPanel");
            if (t != null)
                textPanel = t as RectTransform;
        }

        if (guideTMP == null && textPanel != null)
            guideTMP = textPanel.GetComponentInChildren<TMP_Text>(true);
    }

    public void ShowGuide(
        RectTransform target,
        string message,
        StrongGuideOverlay.HoleShape shape = StrongGuideOverlay.HoleShape.Circle,
        float padding = -1f,
        Vector2? textOffset = null,
        bool hideOnRightClick = false)
    {
        AutoAssignIfNeeded();

        if (overlay == null || target == null)
        {
            Debug.LogWarning("[StrongGuideController] Missing overlay or target.");
            return;
        }

        float finalPadding = padding >= 0f ? padding : defaultPadding;
        Vector2 finalOffset = textOffset ?? defaultTextOffset;

        overlay.Show(target, shape, finalPadding, true);

        if (guideTMP != null)
            guideTMP.text = message;

        if (textPanel != null)
        {
            textPanel.gameObject.SetActive(true);
            PositionTextPanelNearTarget(target, finalOffset);
        }

        waitingForRightMouseDown = hideOnRightClick;
        isShowing = true;
    }

    public void ShowRightClickRotate(RectTransform target, string message = "Right mouse click to rotate the block.")
    {
        ShowGuide(
            target,
            message,
            StrongGuideOverlay.HoleShape.Circle,
            defaultPadding,
            defaultTextOffset,
            true
        );
    }

    public void HideGuide()
    {
        AutoAssignIfNeeded();

        waitingForRightMouseDown = false;
        isShowing = false;

        if (overlay != null)
            overlay.Hide();

        if (textPanel != null)
            textPanel.gameObject.SetActive(false);
    }

    public bool IsShowing()
    {
        return isShowing;
    }

    private void PositionTextPanelNearTarget(RectTransform target, Vector2 offset)
    {
        if (target == null || textPanel == null)
            return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        RectTransform canvasRect = textPanel.parent as RectTransform;
        if (canvasRect == null)
            return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldCenter);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 localPoint
        );

        textPanel.anchoredPosition = localPoint + offset;
    }
}