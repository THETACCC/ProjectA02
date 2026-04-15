using UnityEngine;

public class BlockTutorialManager03 : MonoBehaviour
{
    private enum TutorialStage
    {
        WaitingForLanding,
        BlueBlockGuide,
        RotateGuide,
        Finished
    }

    [Header("Players")]
    public PlayerController player1;
    public PlayerController player2;

    [Header("Strong Guide")]
    public StrongGuideController strongGuideController;

    [Header("Stage 1 - Blue Block Guide")]
    public RectTransform blueBlockGuideTarget;

    [TextArea]
    public string blueBlockGuideMessage = "Blue blocks are duplicated on both sides at the same position.";

    public bool overrideBlueBlockStyle = false;
    public StrongGuideOverlay.HoleShape blueBlockGuideShape = StrongGuideOverlay.HoleShape.Circle;
    public float blueBlockGuidePadding = 24f;
    public bool blueBlockUseManualHoleSize = false;
    public Vector2 blueBlockManualHoleSize = new Vector2(220f, 220f);
    public StrongGuideController.GuideTextAnchor blueBlockTextAnchor = StrongGuideController.GuideTextAnchor.Right;
    public Vector2 blueBlockTextExtraOffset = Vector2.zero;

    [Header("Stage 2 - Rotate Guide")]
    public RectTransform rotateGuideTarget1;
    public RectTransform rotateGuideTarget2;

    [TextArea]
    public string rotateGuideMessage = "Press R to rotate the maps.";

    public bool overrideRotateStyle = false;
    public StrongGuideOverlay.HoleShape rotateGuideShape = StrongGuideOverlay.HoleShape.Circle;
    public float rotateGuidePadding = 24f;
    public bool rotateUseManualHoleSize = false;
    public Vector2 rotateManualHoleSize = new Vector2(220f, 220f);
    public StrongGuideController.GuideTextAnchor rotateTextAnchor = StrongGuideController.GuideTextAnchor.Up;
    public Vector2 rotateTextExtraOffset = Vector2.zero;

    private TutorialStage currentStage = TutorialStage.WaitingForLanding;

    private void Awake()
    {
        if (strongGuideController == null)
            strongGuideController = FindObjectOfType<StrongGuideController>(true);

        if (player1 == null)
        {
            GameObject p1 = GameObject.FindGameObjectWithTag("Player1");
            if (p1 != null)
                player1 = p1.GetComponent<PlayerController>();
        }

        if (player2 == null)
        {
            GameObject p2 = GameObject.FindGameObjectWithTag("Player2");
            if (p2 != null)
                player2 = p2.GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (currentStage == TutorialStage.Finished)
            return;

        if (currentStage == TutorialStage.WaitingForLanding)
        {
            if (player1 != null && player2 != null &&
                player1.hasLanded && player2.hasLanded)
            {
                EnableBlueBlockTutorial();
            }

            return;
        }

        if (currentStage == TutorialStage.BlueBlockGuide)
        {
            if (Application.isFocused && Input.GetMouseButtonDown(0))
            {
                NotifyBlueBlockClicked();
            }

            return;
        }

        if (currentStage == TutorialStage.RotateGuide)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                FinishRotateGuide();
            }
        }
    }

    // =========================================================
    // Stage 1
    // =========================================================
    public void EnableBlueBlockTutorial()
    {
        if (strongGuideController == null || blueBlockGuideTarget == null)
            return;

        currentStage = TutorialStage.BlueBlockGuide;

        if (!overrideBlueBlockStyle)
        {
            strongGuideController.ShowGuide(
                blueBlockGuideTarget,
                blueBlockGuideMessage
            );
        }
        else
        {
            strongGuideController.ShowGuide(
                blueBlockGuideTarget,
                blueBlockGuideMessage,
                blueBlockGuideShape,
                blueBlockGuidePadding,
                blueBlockUseManualHoleSize,
                blueBlockManualHoleSize,
                blueBlockTextAnchor,
                blueBlockTextExtraOffset
            );
        }
    }

    public void NotifyBlueBlockClicked()
    {
        if (currentStage != TutorialStage.BlueBlockGuide)
            return;

        if (strongGuideController != null)
            strongGuideController.HideGuide();

        EnableRotateTutorial();
    }

    // =========================================================
    // Stage 2
    // =========================================================
    public void EnableRotateTutorial()
    {
        if (strongGuideController == null)
            return;

        bool hasTarget1 = rotateGuideTarget1 != null;
        bool hasTarget2 = rotateGuideTarget2 != null;

        if (!hasTarget1 && !hasTarget2)
        {
            currentStage = TutorialStage.Finished;
            return;
        }

        currentStage = TutorialStage.RotateGuide;

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
                    rotateGuideMessage,
                    null,
                    -1f,
                    null,
                    null,
                    rotateTextAnchor,
                    rotateTextExtraOffset
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

    public void FinishRotateGuide()
    {
        if (currentStage != TutorialStage.RotateGuide)
            return;

        if (strongGuideController != null)
            strongGuideController.HideGuide();

        currentStage = TutorialStage.Finished;
    }

    public void disableAllTutorial()
    {
        if (strongGuideController != null)
            strongGuideController.HideGuide();

        currentStage = TutorialStage.Finished;
    }
}