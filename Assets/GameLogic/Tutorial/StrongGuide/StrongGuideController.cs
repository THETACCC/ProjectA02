using TMPro;
using UnityEngine;

[ExecuteAlways]
public class StrongGuideController : MonoBehaviour
{
    public enum GuideTextAnchor
    {
        Right,
        Left,
        Up,
        Down
    }

    [Header("Auto References")]
    [SerializeField] private StrongGuideOverlay overlay;
    [SerializeField] private RectTransform textPanel;
    [SerializeField] private TMP_Text guideTMP;

    [Header("Default Tutorial Style")]
    [SerializeField] private StrongGuideOverlay.HoleShape defaultShape = StrongGuideOverlay.HoleShape.Circle;
    [SerializeField] private float defaultPadding = 24f;
    [SerializeField] private bool defaultUseManualHoleSize = false;
    [SerializeField] private Vector2 defaultManualHoleSize = new Vector2(220f, 220f);

    [Header("Default Text Style")]
    [SerializeField] private GuideTextAnchor defaultTextAnchor = GuideTextAnchor.Right;
    [SerializeField] private Vector2 defaultTextOffset = Vector2.zero;
    [SerializeField] private bool hideOnRightMouseDown = true;

    [Header("Editor Preview")]
    [SerializeField] private bool previewInEditor = false;
    [SerializeField] private RectTransform previewTarget;
    [SerializeField] private string previewMessage = "Preview Text";
    [SerializeField] private StrongGuideOverlay.HoleShape previewShape = StrongGuideOverlay.HoleShape.Circle;
    [SerializeField] private float previewPadding = 24f;
    [SerializeField] private bool previewUseManualHoleSize = false;
    [SerializeField] private Vector2 previewManualHoleSize = new Vector2(220f, 220f);
    [SerializeField] private GuideTextAnchor previewTextAnchor = GuideTextAnchor.Right;
    [SerializeField] private Vector2 previewExtraTextOffset = Vector2.zero;

    private bool waitingForRightMouseDown = false;
    private bool isShowing = false;

    private void Awake()
    {
        AutoAssignIfNeeded();
    }

    private void Start()
    {
        if (Application.isPlaying)
            HideGuide();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (hideOnRightMouseDown && waitingForRightMouseDown && Input.GetMouseButtonDown(1))
        {
            HideGuide();
        }
    }

    private void OnValidate()
    {
        AutoAssignIfNeeded();

        if (!Application.isPlaying && previewInEditor)
        {
            ApplyPreview();
        }
        else if (!Application.isPlaying && !previewInEditor)
        {
            HideGuideInEditorOnly();
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

    private void ApplyPreview()
    {
        if (previewTarget == null || overlay == null)
            return;

        overlay.Show(
            previewTarget,
            previewShape,
            previewPadding,
            true,
            previewUseManualHoleSize,
            previewManualHoleSize
        );

        if (guideTMP != null)
            guideTMP.text = previewMessage;

        if (textPanel != null)
        {
            textPanel.gameObject.SetActive(true);

            Vector2 finalOffset =
                GetAnchorBaseOffset(previewTextAnchor) +
                defaultTextOffset +
                previewExtraTextOffset;

            PositionTextPanelNearTarget(previewTarget, finalOffset);
        }
    }

    private void HideGuideInEditorOnly()
    {
        if (Application.isPlaying)
            return;

        if (overlay != null)
            overlay.Hide();

        if (textPanel != null)
            textPanel.gameObject.SetActive(false);
    }

    public void ShowGuide(
        RectTransform target,
        string message,
        StrongGuideOverlay.HoleShape? shape = null,
        float padding = -1f,
        bool? useManualHoleSize = null,
        Vector2? manualHoleSize = null,
        GuideTextAnchor? anchor = null,
        Vector2? extraTextOffset = null,
        bool hideOnRightClick = false)
    {
        AutoAssignIfNeeded();

        if (overlay == null || target == null)
        {
            Debug.LogWarning("[StrongGuideController] Missing overlay or target.");
            return;
        }

        StrongGuideOverlay.HoleShape finalShape = shape ?? defaultShape;
        float finalPadding = padding >= 0f ? padding : defaultPadding;
        bool finalUseManualHoleSize = useManualHoleSize ?? defaultUseManualHoleSize;
        Vector2 finalManualHoleSize = manualHoleSize ?? defaultManualHoleSize;
        GuideTextAnchor finalAnchor = anchor ?? defaultTextAnchor;
        Vector2 finalExtraOffset = extraTextOffset ?? Vector2.zero;

        overlay.Show(
            target,
            finalShape,
            finalPadding,
            true,
            finalUseManualHoleSize,
            finalManualHoleSize
        );

        ApplyText(target, message, finalAnchor, finalExtraOffset);

        waitingForRightMouseDown = hideOnRightClick;
        isShowing = true;
    }

    public void ShowGuideTwo(
    RectTransform targetA,
    RectTransform targetB,
    string message,
    StrongGuideOverlay.HoleShape? shapeA = null,
    float paddingA = -1f,
    bool? useManualHoleSizeA = null,
    Vector2? manualHoleSizeA = null,
    StrongGuideOverlay.HoleShape? shapeB = null,
    float paddingB = -1f,
    bool? useManualHoleSizeB = null,
    Vector2? manualHoleSizeB = null,
    GuideTextAnchor? anchor = null,
    Vector2? extraTextOffset = null,
    bool hideOnRightClick = false)
    {
        AutoAssignIfNeeded();

        if (overlay == null || (targetA == null && targetB == null))
        {
            Debug.LogWarning("[StrongGuideController] Missing overlay or guide targets.");
            return;
        }

        StrongGuideOverlay.HoleShape finalShapeA = shapeA ?? defaultShape;
        float finalPaddingA = paddingA >= 0f ? paddingA : defaultPadding;
        bool finalUseManualA = useManualHoleSizeA ?? defaultUseManualHoleSize;
        Vector2 finalManualA = manualHoleSizeA ?? defaultManualHoleSize;

        StrongGuideOverlay.HoleShape finalShapeB = shapeB ?? defaultShape;
        float finalPaddingB = paddingB >= 0f ? paddingB : defaultPadding;
        bool finalUseManualB = useManualHoleSizeB ?? defaultUseManualHoleSize;
        Vector2 finalManualB = manualHoleSizeB ?? defaultManualHoleSize;

        GuideTextAnchor finalAnchor = anchor ?? defaultTextAnchor;
        Vector2 finalExtraOffset = extraTextOffset ?? Vector2.zero;

        if (targetA != null && targetB != null)
        {
            overlay.ShowTwo(
                targetA,
                finalShapeA,
                finalPaddingA,
                finalUseManualA,
                finalManualA,
                targetB,
                finalShapeB,
                finalPaddingB,
                finalUseManualB,
                finalManualB,
                true
            );

            ApplyText(targetA, message, finalAnchor, finalExtraOffset);
        }
        else
        {
            RectTransform singleTarget = targetA != null ? targetA : targetB;
            StrongGuideOverlay.HoleShape singleShape = targetA != null ? finalShapeA : finalShapeB;
            float singlePadding = targetA != null ? finalPaddingA : finalPaddingB;
            bool singleUseManual = targetA != null ? finalUseManualA : finalUseManualB;
            Vector2 singleManual = targetA != null ? finalManualA : finalManualB;

            overlay.Show(
                singleTarget,
                singleShape,
                singlePadding,
                true,
                singleUseManual,
                singleManual
            );

            ApplyText(singleTarget, message, finalAnchor, finalExtraOffset);
        }

        waitingForRightMouseDown = hideOnRightClick;
        isShowing = true;
    }

    public void ShowRightClickRotate(
        RectTransform target,
        string message = "Right mouse click to rotate the block.",
        StrongGuideOverlay.HoleShape? shape = null,
        float padding = -1f,
        bool? useManualHoleSize = null,
        Vector2? manualHoleSize = null,
        GuideTextAnchor? anchor = null,
        Vector2? extraOffset = null)
    {
        ShowGuide(
            target,
            message,
            shape,
            padding,
            useManualHoleSize,
            manualHoleSize,
            anchor,
            extraOffset,
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

    private void ApplyText(RectTransform target, string message, GuideTextAnchor anchor, Vector2 extraTextOffset)
    {
        if (guideTMP != null)
            guideTMP.text = message;

        if (textPanel != null && target != null)
        {
            textPanel.gameObject.SetActive(true);

            Vector2 finalOffset =
                GetAnchorBaseOffset(anchor) +
                defaultTextOffset +
                extraTextOffset;

            PositionTextPanelNearTarget(target, finalOffset);
        }
    }

    private Vector2 GetAnchorBaseOffset(GuideTextAnchor anchor)
    {
        switch (anchor)
        {
            case GuideTextAnchor.Left:
                return new Vector2(-180f, 0f);
            case GuideTextAnchor.Up:
                return new Vector2(0f, 120f);
            case GuideTextAnchor.Down:
                return new Vector2(0f, -120f);
            case GuideTextAnchor.Right:
            default:
                return new Vector2(180f, 0f);
        }
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