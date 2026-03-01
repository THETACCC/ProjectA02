using SKCell;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using TMPro;

public class LevelPass : MonoBehaviour
{
    private int chapterIndex;
    private int levelIndex;

    public static int t_spawnPoint;
    public GameObject leftfinish;
    public GameObject rightfinish;
    public GameObject FinishUI;

    [Header("Finish UI (optional)")]
    [SerializeField] private TMP_Text txtRewards; // ✅ 显示 x/total（建议命名 Txt_Rewards）

    [Header("Scene Flow")]
    public SceneTitle scenetitle;

    private RewardManager rewardManager;
    private FlowManager flowManager;
    private Enterfinishleft left;
    private EnterFinishRight right;

    private bool startLoading = false;
    private bool isPlayed = false;

    void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        var match = Regex.Match(sceneName, @"^Chapter(\d+)_Level(\d+)$");
        if (match.Success)
        {
            chapterIndex = int.Parse(match.Groups[1].Value);
            int rawLevel = int.Parse(match.Groups[2].Value);
            levelIndex = Mathf.Max(0, rawLevel - 1);
        }
        else
        {
            Debug.LogWarning($"[LevelPass] Scene name '{sceneName}' didn't match Chapter#_Level# format.");
            chapterIndex = 0;
            levelIndex = 0;
        }
    }

    void Start()
    {
        leftfinish = GameObject.FindGameObjectWithTag("FinishLeft");
        rightfinish = GameObject.FindGameObjectWithTag("FinishRight");

        left = leftfinish.GetComponent<Enterfinishleft>();
        right = rightfinish.GetComponent<EnterFinishRight>();

        flowManager = GameObject.Find("FlowManager").GetComponent<FlowManager>();
        rewardManager = FindObjectOfType<RewardManager>();

        if (txtRewards == null && FinishUI != null)
        {
            // 尝试在 FinishUI 子物体里找 Txt_Rewards / Txt_Reward
            var all = FinishUI.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in all)
            {
                if (t.name == "Txt_Rewards" || t.name == "Txt_Reward")
                {
                    txtRewards = t;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (left.leftreached && right.rightreached && !FinishUI.activeSelf && !startLoading)
        {
            Debug.Log("[LevelPass] Level complete!");

            float used = -1f;
            if (LevelTimer.Instance != null)
                used = LevelTimer.Instance.StopTimer();

            if (SaveManager.Instance && used >= 0f)
                SaveManager.Instance.SetBestTimeIfBetter(chapterIndex, levelIndex, used);

            if (LevelTimer.Instance != null)
                LevelTimer.Instance.RefreshBestUI();

            if (!isPlayed)
            {
                AudioPlayer.instance.playlevelSuccessSound();
                isPlayed = true;
            }

            // ✅ 计算本局 rewards，并立刻显示 + 写入存档（避免玩家不点 next 就退出导致没保存）
            int collected = rewardManager ? rewardManager.rewardsReachedCount : 0;
            int total = rewardManager ? rewardManager.totalRewardsInLevel :
                       (SaveManager.Instance ? SaveManager.Instance.GetLevelRewardTotal(chapterIndex, levelIndex) : 0);

            if (txtRewards) txtRewards.text = $"{collected}/{total}";

            if (SaveManager.Instance)
                SaveManager.Instance.MarkLevelCompleted(chapterIndex, levelIndex, collected, total);

            FinishUI.SetActive(true);
        }
    }

    public void LoadNextLevel()
    {
        AudioPlayer.instance.playlevelEndSound();

        if (startLoading) return;
        if (!(left.leftreached && right.rightreached)) return;

        // 你原有的切场景逻辑保持
        SKUtils.InvokeAction(0.2f, () =>
        {
            flowManager.LoadScene(new SceneInfo { index = scenetitle });
        });

        startLoading = true;
    }
}