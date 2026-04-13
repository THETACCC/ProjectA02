using UnityEngine;

public class BlockTutorialManager : MonoBehaviour
{
    [Header("Old Tutorials")]
    public GameObject myDragTutorial;
    public GameObject myRotateTutorial; // old rotate tutorial, kept for backup

    [Header("Strong Guide")]
    public StrongGuideController strongGuideController;
    public RectTransform rotateGuideTarget;

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

        if (strongGuideController == null || rotateGuideTarget == null)
            return;

        if (!overrideRotateStyle)
        {
            strongGuideController.ShowRightClickRotate(rotateGuideTarget);
        }
        else
        {
            strongGuideController.ShowRightClickRotate(
                rotateGuideTarget,
                "Right mouse click to rotate the block.",
                rotateGuideShape,
                rotateGuidePadding,
                rotateUseManualHoleSize,
                rotateManualHoleSize,
                rotateTextAnchor,
                rotateTextExtraOffset
            );
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