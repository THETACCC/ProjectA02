using UnityEngine;

public class BlockTutorialManager04 : MonoBehaviour
{
    private enum TutorialStage
    {
        WaitingForLanding,
        GreenBlockGuide,
        Finished
    }

    [Header("Players")]
    public PlayerController player1;
    public PlayerController player2;

    [Header("Strong Guide")]
    public StrongGuideController strongGuideController;

    [Header("Green Block Guide")]
    public RectTransform greenBlockGuideTarget;

    [TextArea]
    public string greenBlockGuideMessage = "Green blocks, marked with green indicators above, can be placed in any empty space.";

    [Header("Optional Overrides")]
    public bool overrideGreenBlockStyle = false;
    public StrongGuideOverlay.HoleShape greenBlockGuideShape = StrongGuideOverlay.HoleShape.Circle;
    public float greenBlockGuidePadding = 24f;
    public bool greenBlockUseManualHoleSize = false;
    public Vector2 greenBlockManualHoleSize = new Vector2(220f, 220f);
    public StrongGuideController.GuideTextAnchor greenBlockTextAnchor = StrongGuideController.GuideTextAnchor.Right;
    public Vector2 greenBlockTextExtraOffset = Vector2.zero;

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
                EnableGreenBlockTutorial();
            }

            return;
        }

        if (currentStage == TutorialStage.GreenBlockGuide)
        {
            if (Application.isFocused && Input.GetMouseButtonDown(0))
            {
                FinishGreenBlockTutorial();
            }

            return;
        }
    }

    public void EnableGreenBlockTutorial()
    {
        if (strongGuideController == null || greenBlockGuideTarget == null)
            return;

        currentStage = TutorialStage.GreenBlockGuide;

        if (!overrideGreenBlockStyle)
        {
            strongGuideController.ShowGuide(
                greenBlockGuideTarget,
                greenBlockGuideMessage
            );
        }
        else
        {
            strongGuideController.ShowGuide(
                greenBlockGuideTarget,
                greenBlockGuideMessage,
                greenBlockGuideShape,
                greenBlockGuidePadding,
                greenBlockUseManualHoleSize,
                greenBlockManualHoleSize,
                greenBlockTextAnchor,
                greenBlockTextExtraOffset
            );
        }
    }

    public void FinishGreenBlockTutorial()
    {
        if (currentStage != TutorialStage.GreenBlockGuide)
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