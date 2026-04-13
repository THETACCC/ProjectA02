using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockTutorialManager : MonoBehaviour
{
    [Header("Old Tutorials")]
    public GameObject myDragTutorial;
    public GameObject myRotateTutorial; // old rotate tutorial, kept for backup

    [Header("Strong Guide")]
    public StrongGuideOverlay strongGuideOverlay;
    public RectTransform rotateGuideTarget;

    [TextArea(2, 4)]
    public string rotateGuideText = "Right mouse click to rotate the block.";

    public GameObject rotateGuideTextPanel;
    public TMP_Text rotateGuideTMP;
    public Vector2 rotateGuideTextOffset = new Vector2(180f, 0f);

    private bool waitingForRotateRightClick = false;
    private bool rotateGuideShowing = false;

    private void Start()
    {
        if (rotateGuideTextPanel != null)
            rotateGuideTextPanel.SetActive(false);

        if (strongGuideOverlay != null)
            strongGuideOverlay.Hide();
    }

    private void Update()
    {
        if (waitingForRotateRightClick && Input.GetMouseButtonDown(1))
        {
            HideRotateStrongGuide();
        }
    }

    public void enableDragTutorial()
    {
        if (myDragTutorial != null)
            myDragTutorial.SetActive(true);

        HideRotateStrongGuide();
    }

    public void enableRotateTutorial()
    {
        if (rotateGuideShowing)
            return;

        if (myDragTutorial != null)
            myDragTutorial.SetActive(false);

        // ===== OLD VERSION: kept for backup =====
        // if (myRotateTutorial != null)
        //     myRotateTutorial.SetActive(true);
        // ========================================

        ShowRotateStrongGuide();
    }

    public void disableAllTutorial()
    {
        if (myDragTutorial != null)
            myDragTutorial.SetActive(false);

        // ===== OLD VERSION: kept for backup =====
        // if (myRotateTutorial != null)
        //     myRotateTutorial.SetActive(false);
        // ========================================

        HideRotateStrongGuide();
    }

    private void ShowRotateStrongGuide()
    {
        rotateGuideShowing = true;
        waitingForRotateRightClick = true;

        if (strongGuideOverlay != null && rotateGuideTarget != null)
        {
            strongGuideOverlay.Show(
                rotateGuideTarget,
                StrongGuideOverlay.HoleShape.Circle,
                24f,
                true
            );
        }

        if (rotateGuideTMP != null)
            rotateGuideTMP.text = rotateGuideText;

        if (rotateGuideTextPanel != null)
        {
            rotateGuideTextPanel.SetActive(true);
            PositionTextPanelNearTarget();
        }
    }

    private void HideRotateStrongGuide()
    {
        rotateGuideShowing = false;
        waitingForRotateRightClick = false;

        if (strongGuideOverlay != null)
            strongGuideOverlay.Hide();

        if (rotateGuideTextPanel != null)
            rotateGuideTextPanel.SetActive(false);

        // ===== OLD VERSION: kept for backup =====
        // if (myRotateTutorial != null)
        //     myRotateTutorial.SetActive(false);
        // ========================================
    }

    private void PositionTextPanelNearTarget()
    {
        if (rotateGuideTarget == null || rotateGuideTextPanel == null)
            return;

        RectTransform textRect = rotateGuideTextPanel.GetComponent<RectTransform>();
        if (textRect == null)
            return;

        Vector3[] corners = new Vector3[4];
        rotateGuideTarget.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        RectTransform canvasRect = textRect.parent as RectTransform;
        if (canvasRect == null)
            return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldCenter);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 localPoint
        );

        textRect.anchoredPosition = localPoint + rotateGuideTextOffset;
    }
}