using SKCell;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class LevelPass : MonoBehaviour
{
    // Auto-derived indices from scene name
    private int chapterIndex;
    private int levelIndex;

    // Preserve from original
    public static int t_spawnPoint;
    public GameObject leftfinish;
    public GameObject rightfinish;
    public GameObject FinishUI;

    [Header("Scene Flow")]
    public SceneTitle scenetitle;

    // Internal references
    private RewardManager rewardManager;
    private FlowManager flowManager;
    private Enterfinishleft left;
    private EnterFinishRight right;

    // Prevent multiple loads
    private bool startLoading = false;

    void Awake()
    {
        // Parse scene name "ChapterX_LevelY" for X and Y
        string sceneName = SceneManager.GetActiveScene().name;
        var match = Regex.Match(sceneName, @"^Chapter(\d+)_Level(\d+)$");
        if (match.Success)
        {
            chapterIndex = int.Parse(match.Groups[1].Value);
            int rawLevel = int.Parse(match.Groups[2].Value);
            levelIndex = rawLevel - 1;
            if (levelIndex < 0)
            {
                Debug.LogWarning($"[LevelPass] Parsed level {rawLevel}, but levelIndex is now {levelIndex}. Clamping to 0.");
                levelIndex = 0;
            }
        }
        else
        {
            Debug.LogWarning($"[LevelPass] Scene name '{sceneName}' didn't match Chapter#_Level# format.");
        }
    }

    void Start()
    {
        // Preserve original find-by-tag behavior
        leftfinish = GameObject.FindGameObjectWithTag("FinishLeft");
        rightfinish = GameObject.FindGameObjectWithTag("FinishRight");

        // Cache finish triggers
        left = leftfinish.GetComponent<Enterfinishleft>();
        right = rightfinish.GetComponent<EnterFinishRight>();

        // Cache managers
        flowManager = GameObject.Find("FlowManager").GetComponent<FlowManager>();
        rewardManager = FindObjectOfType<RewardManager>();
    }

    void Update()
    {
        // Show UI when both endpoints reached
        if (left.leftreached && right.rightreached && !FinishUI.activeSelf && !startLoading)
        {
            Debug.Log("[LevelPass] Level complete!");

            float used = -1f;

            if (LevelTimer.Instance != null)
                used = LevelTimer.Instance.StopTimer();

            // 写入 save.json bestTimes
            if (SaveManager.Instance && used >= 0f)
                SaveManager.Instance.SetBestTimeIfBetter(chapterIndex, levelIndex, used);

            // 刷新best文本
            if (LevelTimer.Instance != null)
                LevelTimer.Instance.RefreshBestUI();

            FinishUI.SetActive(true);
        }
    }

    /// <summary>
    /// Called by the Finish UI button to proceed to the next level
    /// </summary>
    public void LoadNextLevel()
    {
        if (startLoading) return;
        if (!(left.leftreached && right.rightreached)) return;

        int rewardsThisRun = rewardManager ? rewardManager.rewardsReachedCount : 0;

        // ✅ 记通关（奖励可为0），用于解锁世界里的下一个关卡点
        if (SaveManager.Instance)
            SaveManager.Instance.MarkLevelCompleted(chapterIndex, levelIndex, rewardsThisRun);

        // 你原有的切场景逻辑保持
        SKUtils.InvokeAction(0.2f, () =>
        {
            flowManager.LoadScene(new SceneInfo { index = scenetitle });
        });

        startLoading = true;
    }

}
