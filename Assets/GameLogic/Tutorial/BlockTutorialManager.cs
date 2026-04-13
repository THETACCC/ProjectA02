using UnityEngine;

public class BlockTutorialManager : MonoBehaviour
{
    [Header("Old Tutorials")]
    public GameObject myDragTutorial;
    public GameObject myRotateTutorial; // old rotate tutorial, kept for backup

    [Header("Strong Guide")]
    public StrongGuideController strongGuideController;
    public RectTransform rotateGuideTarget;

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

        if (strongGuideController != null && rotateGuideTarget != null)
            strongGuideController.ShowRightClickRotate(rotateGuideTarget);
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