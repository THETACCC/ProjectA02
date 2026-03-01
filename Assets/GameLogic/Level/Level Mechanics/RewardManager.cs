using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour
{
    private LevelController levelController;

    private bool isRewardSetup = false;

    public GameObject[] RewardObjects;

    private RewardAcquire[] rewardAcquireScripts;
    private bool[] rewardReachedFlags;

    public int rewardsReachedCount = 0;

    // ✅ 这一关总共有多少 reward（用于显示 x/total）
    public int totalRewardsInLevel = 0;

    // scene -> indices
    private int chapterIndex = 0;
    private int levelIndex = 0;

    void Awake()
    {
        // Parse scene name "ChapterX_LevelY"
        string sceneName = SceneManager.GetActiveScene().name;
        var match = Regex.Match(sceneName, @"^Chapter(\d+)_Level(\d+)$");
        if (match.Success)
        {
            chapterIndex = int.Parse(match.Groups[1].Value);
            int rawLevel = int.Parse(match.Groups[2].Value);
            levelIndex = Mathf.Max(0, rawLevel - 1);
        }
    }

    void Start()
    {
        GameObject levelcontrollerOBVJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        if (levelcontrollerOBVJ) levelController = levelcontrollerOBVJ.GetComponent<LevelController>();
    }

    void Update()
    {
        if (!isRewardSetup)
        {
            // 如果场景里 Reward 一开始就存在，这里会立刻拿到 total
            SearchRewardObjects();

            if (totalRewardsInLevel > 0)
                isRewardSetup = true;
        }

        if (!isRewardSetup && levelController != null && levelController.phase == LevelPhase.Running)
        {
            SearchRewardObjects();
            if (totalRewardsInLevel > 0)
                isRewardSetup = true;
        }

        if (isRewardSetup)
            UpdateRewardCount();
    }

    void SearchRewardObjects()
    {
        RewardObjects = GameObject.FindGameObjectsWithTag("Reward");

        int count = RewardObjects.Length;
        rewardAcquireScripts = new RewardAcquire[count];
        rewardReachedFlags = new bool[count];

        // ✅ reset count & total
        rewardsReachedCount = 0;
        totalRewardsInLevel = count;

        for (int i = 0; i < count; i++)
        {
            GameObject go = RewardObjects[i];
            RewardAcquire acquireScript = go.GetComponent<RewardAcquire>();

            if (acquireScript != null)
            {
                rewardAcquireScripts[i] = acquireScript;

                // ✅ 如果一开始就已经 reached（比如存档还原/特殊逻辑），这里也要计入
                rewardReachedFlags[i] = acquireScript.isReached;
                if (acquireScript.isReached) rewardsReachedCount++;
            }
            else
            {
                Debug.LogWarning("[RewardManager] GameObject '"
                    + go.name
                    + "' has tag 'Reward' but does not contain a RewardAcquire component.");
                rewardAcquireScripts[i] = null;
                rewardReachedFlags[i] = false;
            }
        }

        if (totalRewardsInLevel > 0 && SaveManager.Instance != null)
            SaveManager.Instance.SetLevelRewardTotal(chapterIndex, levelIndex, totalRewardsInLevel);

        Debug.Log("[RewardManager] Found " + count + " reward object(s) in the scene.");
    }

    void UpdateRewardCount()
    {
        if (rewardAcquireScripts == null) return;

        for (int i = 0; i < rewardAcquireScripts.Length; i++)
        {
            RewardAcquire script = rewardAcquireScripts[i];
            if (script == null) continue;

            if (script.isReached && !rewardReachedFlags[i])
            {
                rewardsReachedCount++;
                rewardReachedFlags[i] = true;
            }
        }
    }
}