using UnityEngine;

public class BlockTutorialManager01 : MonoBehaviour
{
    private enum TutorialStage
    {
        WaitingForLanding,
        PositionGuide,
        RewardGuide,
        Finished
    }

    [Header("Players")]
    public PlayerController player1;
    public PlayerController player2;

    [Header("Strong Guide")]
    public StrongGuideController strongGuideController;

    // -------------------------
    // Stage 1: Select Start Position
    // -------------------------
    [Header("Stage 1 - Position Guide Targets (assign 1 or 2)")]
    public RectTransform positionGuideTarget1;
    public RectTransform positionGuideTarget2;

    [Header("Stage 1 - Message")]
    [TextArea]
    public string positionGuideMessage = "Use the mouse to reposition the players before starting.";

    [Header("Stage 1 - Optional Overrides")]
    public bool overridePositionStyle = false;
    public StrongGuideOverlay.HoleShape positionGuideShape = StrongGuideOverlay.HoleShape.Circle;
    public float positionGuidePadding = 24f;
    public bool positionUseManualHoleSize = false;
    public Vector2 positionManualHoleSize = new Vector2(220f, 220f);
    public StrongGuideController.GuideTextAnchor positionTextAnchor = StrongGuideController.GuideTextAnchor.Up;
    public Vector2 positionTextExtraOffset = Vector2.zero;

    // -------------------------
    // Stage 2: Reward Guide
    // -------------------------
    [Header("Stage 2 - Reward Guide Targets (assign 1 or 2)")]
    public RectTransform rewardGuideTarget1;
    public RectTransform rewardGuideTarget2;

    [Header("Stage 2 - Message")]
    [TextArea]
    public string rewardGuideMessage = "Collect the rewards to unlock more levels!";

    [Header("Stage 2 - Optional Overrides")]
    public bool overrideRewardStyle = false;

    public StrongGuideOverlay.HoleShape rewardGuideShape1 = StrongGuideOverlay.HoleShape.Circle;
    public float rewardGuidePadding1 = 24f;
    public bool rewardUseManualHoleSize1 = false;
    public Vector2 rewardManualHoleSize1 = new Vector2(220f, 220f);

    public StrongGuideOverlay.HoleShape rewardGuideShape2 = StrongGuideOverlay.HoleShape.Circle;
    public float rewardGuidePadding2 = 24f;
    public bool rewardUseManualHoleSize2 = false;
    public Vector2 rewardManualHoleSize2 = new Vector2(220f, 220f);

    public StrongGuideController.GuideTextAnchor rewardTextAnchor = StrongGuideController.GuideTextAnchor.Up;
    public Vector2 rewardTextExtraOffset = Vector2.zero;

    [Header("Stage 2 - Dismiss Input")]
    public bool dismissRewardOnLeftClick = true;
    public bool dismissRewardOnRightClick = true;
    public bool dismissRewardOnSpace = true;
    public bool dismissRewardOnEnter = true;

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
        if (currentStage == TutorialStage.Finished) return;
        if (player1 == null || player2 == null) return;

        if (currentStage == TutorialStage.WaitingForLanding)
        {
            if (player1.hasLanded && player2.hasLanded)
            {
                EnablePositionTutorial();
                currentStage = TutorialStage.PositionGuide;
            }

            return;
        }

        if (currentStage == TutorialStage.RewardGuide)
        {
            if (ShouldDismissRewardGuide())
            {
                NotifyRewardGuideFinished();
            }
        }
    }

    // =========================================================
    // Stage 1
    // =========================================================
    public void EnablePositionTutorial()
    {
        if (strongGuideController == null)
            return;

        bool hasTarget1 = positionGuideTarget1 != null;
        bool hasTarget2 = positionGuideTarget2 != null;

        if (!hasTarget1 && !hasTarget2)
            return;

        if (!overridePositionStyle)
        {
            if (hasTarget1 && hasTarget2)
            {
                strongGuideController.ShowGuideTwo(
                    positionGuideTarget1,
                    positionGuideTarget2,
                    positionGuideMessage
                );
            }
            else
            {
                RectTransform singleTarget = hasTarget1 ? positionGuideTarget1 : positionGuideTarget2;

                strongGuideController.ShowGuide(
                    singleTarget,
                    positionGuideMessage
                );
            }
        }
        else
        {
            if (hasTarget1 && hasTarget2)
            {
                strongGuideController.ShowGuideTwo(
                    positionGuideTarget1,
                    positionGuideTarget2,
                    positionGuideMessage,
                    positionGuideShape,
                    positionGuidePadding,
                    positionUseManualHoleSize,
                    positionManualHoleSize,
                    positionGuideShape,
                    positionGuidePadding,
                    positionUseManualHoleSize,
                    positionManualHoleSize,
                    positionTextAnchor,
                    positionTextExtraOffset
                );
            }
            else
            {
                RectTransform singleTarget = hasTarget1 ? positionGuideTarget1 : positionGuideTarget2;

                strongGuideController.ShowGuide(
                    singleTarget,
                    positionGuideMessage,
                    positionGuideShape,
                    positionGuidePadding,
                    positionUseManualHoleSize,
                    positionManualHoleSize,
                    positionTextAnchor,
                    positionTextExtraOffset
                );
            }
        }
    }

    public void NotifyPositionChanged()
    {
        if (currentStage != TutorialStage.PositionGuide)
            return;

        if (strongGuideController != null)
            strongGuideController.HideGuide();

        bool hasReward1 = rewardGuideTarget1 != null;
        bool hasReward2 = rewardGuideTarget2 != null;

        if (hasReward1 || hasReward2)
        {
            EnableRewardTutorial();
            currentStage = TutorialStage.RewardGuide;
        }
        else
        {
            currentStage = TutorialStage.Finished;
        }
    }

    // =========================================================
    // Stage 2
    // =========================================================
    public void EnableRewardTutorial()
    {
        if (strongGuideController == null)
            return;

        bool hasTarget1 = rewardGuideTarget1 != null;
        bool hasTarget2 = rewardGuideTarget2 != null;

        if (!hasTarget1 && !hasTarget2)
            return;

        if (!overrideRewardStyle)
        {
            if (hasTarget1 && hasTarget2)
            {
                strongGuideController.ShowGuideTwo(
                    rewardGuideTarget1,
                    rewardGuideTarget2,
                    rewardGuideMessage
                );
            }
            else
            {
                RectTransform singleTarget = hasTarget1 ? rewardGuideTarget1 : rewardGuideTarget2;

                strongGuideController.ShowGuide(
                    singleTarget,
                    rewardGuideMessage
                );
            }
        }
        else
        {
            if (hasTarget1 && hasTarget2)
            {
                strongGuideController.ShowGuideTwo(
                    rewardGuideTarget1,
                    rewardGuideTarget2,
                    rewardGuideMessage,
                    rewardGuideShape1,
                    rewardGuidePadding1,
                    rewardUseManualHoleSize1,
                    rewardManualHoleSize1,
                    rewardGuideShape2,
                    rewardGuidePadding2,
                    rewardUseManualHoleSize2,
                    rewardManualHoleSize2,
                    rewardTextAnchor,
                    rewardTextExtraOffset
                );
            }
            else
            {
                RectTransform singleTarget = hasTarget1 ? rewardGuideTarget1 : rewardGuideTarget2;
                StrongGuideOverlay.HoleShape singleShape = hasTarget1 ? rewardGuideShape1 : rewardGuideShape2;
                float singlePadding = hasTarget1 ? rewardGuidePadding1 : rewardGuidePadding2;
                bool singleUseManual = hasTarget1 ? rewardUseManualHoleSize1 : rewardUseManualHoleSize2;
                Vector2 singleManualSize = hasTarget1 ? rewardManualHoleSize1 : rewardManualHoleSize2;

                strongGuideController.ShowGuide(
                    singleTarget,
                    rewardGuideMessage,
                    singleShape,
                    singlePadding,
                    singleUseManual,
                    singleManualSize,
                    rewardTextAnchor,
                    rewardTextExtraOffset
                );
            }
        }
    }

    private bool ShouldDismissRewardGuide()
    {
        if (dismissRewardOnLeftClick && Input.GetMouseButtonDown(0))
            return true;

        if (dismissRewardOnRightClick && Input.GetMouseButtonDown(1))
            return true;

        if (dismissRewardOnSpace && Input.GetKeyDown(KeyCode.Space))
            return true;

        if (dismissRewardOnEnter &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            return true;

        return false;
    }

    public void NotifyRewardGuideFinished()
    {
        if (currentStage != TutorialStage.RewardGuide)
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