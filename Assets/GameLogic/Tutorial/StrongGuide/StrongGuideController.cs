using TMPro;
using UnityEngine;

public class StrongGuideController : MonoBehaviour
{
    [Header("References")]
    public StrongGuideOverlay overlay;
    public RectTransform textPanel;
    public TMP_Text guideText;

    [Header("Default")]
    public StrongGuideOverlay.HoleShape defaultShape = StrongGuideOverlay.HoleShape.Circle;
    public float defaultPadding = 24f;
    public Vector2 defaultTextOffset = new Vector2(180f, 0f);

    private void Start()
    {
        HideGuide();
    }

    public void ShowGuide(RectTransform target, string message)
    {
        ShowGuide(target, message, defaultShape, defaultPadding, defaultTextOffset, true);
    }

    public void ShowGuide(
        RectTransform target,
        string message,
        StrongGuideOverlay.HoleShape shape,
        float padding,
        Vector2 textOffset,
        bool snap = true)
    {
        if (overlay != null)
            overlay.Show(target, shape, padding, snap);

        if (guideText != null)
            guideText.text = message;

        if (textPanel != null)
        {
            textPanel.gameObject.SetActive(true);
            PositionTextPanel(target, textOffset);
        }

        gameObject.SetActive(true);
    }

    public void HideGuide()
    {
        if (overlay != null)
            overlay.Hide();

        if (textPanel != null)
            textPanel.gameObject.SetActive(false);
    }

    private void PositionTextPanel(RectTransform target, Vector2 offset)
    {
        if (target == null || textPanel == null) return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        RectTransform canvasRect = textPanel.parent as RectTransform;
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