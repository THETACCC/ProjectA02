using UnityEngine;

public class BlockTutorialManager : MonoBehaviour
{
    [Header("Old Tutorials")]
    public GameObject myDragTutorial;
    public GameObject myRotateTutorial; // old rotate tutorial, kept for backup

    [Header("Strong Guide")]
    public StrongGuideController strongGuideController;

    [Header("Rotate Guide Targets")]
    public RectTransform rotateGuideTarget1;
    public RectTransform rotateGuideTarget2;

    [Header("Rotate Guide Message")]
    [TextArea]
    public string rotateGuideMessage = "Right mouse click to rotate the block.";

    [Header("Optional Overrides")]
    public bool overrideRotateStyle = false;
    public StrongGuideOverlay.HoleShape rotateGuideShape = StrongGuideOverlay.HoleShape.Circle;
    public float rotateGuidePadding = 24f;
    public bool rotateUseManualHoleSize = false;
    public Vector2 rotateManualHoleSize = new Vector2(220f, 220f);
    public StrongGuideController.GuideTextAnchor rotateTextAnchor = StrongGuideController.GuideTextAnchor.Right;
    public Vector2 rotateTextExtraOffset = Vector2.zero;

    private void Awake()
    {
        if (strongGuideController == null)
            strongGuideController = FindObjectOfType<StrongGuideController>(true);
    }

    public void enableDragTutorial()
    {
        if (myDragTutorial != null)
            myDragTutorial.SetActive(true);

        if (strongGuideController != null)
            strongGuideController.HideGuide();
    }

    public void enableRotateTutorial()
    {
        if (myDragTutorial != null)
            myDragTutorial.SetActive(false);

        // ===== OLD VERSION: kept for backup =====
        // if (myRotateTutorial != null)
        //     myRotateTutorial.SetActive(true);
        // ========================================

        if (strongGuideController == null)
            return;

        bool hasTarget1 = rotateGuideTarget1 != null;
        bool hasTarget2 = rotateGuideTarget2 != null;

        if (!hasTarget1 && !hasTarget2)
            return;

        if (!overrideRotateStyle)
        {
            if (hasTarget1 && hasTarget2)
            {
                strongGuideController.ShowGuideTwo(
                    rotateGuideTarget1,
                    rotateGuideTarget2,
                    rotateGuideMessage,
                    null,
                    -1f,
                    null,
                    null,
                    null,
                    -1f,
                    null,
                    null,
                    null,
                    null,
                    true
                );
            }
            else
            {
                RectTransform singleTarget = hasTarget1 ? rotateGuideTarget1 : rotateGuideTarget2;

                strongGuideController.ShowRightClickRotate(
                    singleTarget,
                    rotateGuideMessage
                );
            }
        }
        else
        {
            if (hasTarget1 && hasTarget2)
            {
                strongGuideController.ShowGuideTwo(
                    rotateGuideTarget1,
                    rotateGuideTarget2,
                    rotateGuideMessage,
                    rotateGuideShape,
                    rotateGuidePadding,
                    rotateUseManualHoleSize,
                    rotateManualHoleSize,
                    rotateGuideShape,
                    rotateGuidePadding,
                    rotateUseManualHoleSize,
                    rotateManualHoleSize,
                    rotateTextAnchor,
                    rotateTextExtraOffset,
                    true
                );
            }
            else
            {
                RectTransform singleTarget = hasTarget1 ? rotateGuideTarget1 : rotateGuideTarget2;

                strongGuideController.ShowRightClickRotate(
                    singleTarget,
                    rotateGuideMessage,
                    rotateGuideShape,
                    rotateGuidePadding,
                    rotateUseManualHoleSize,
                    rotateManualHoleSize,
                    rotateTextAnchor,
                    rotateTextExtraOffset
                );
            }
        }
    }

    public void disableAllTutorial()
    {
        if (myDragTutorial != null)
            myDragTutorial.SetActive(false);

        // ===== OLD VERSION: kept for backup =====
        // if (myRotateTutorial != null)
        //     myRotateTutorial.SetActive(false);
        // ========================================

        if (strongGuideController != null)
            strongGuideController.HideGuide();
    }
}