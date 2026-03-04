using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class ChapterData
{
    // 已收集数量（最小0）
    public List<int> rewards;

    // 每关总奖励数（默认3，可被运行时更新）
    public List<int> rewardTotals;

    public List<bool> cleared;
    public List<float> bestTimes;
}

[Serializable]
public class SaveData
{
    public List<ChapterData> chapters;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Configure number of levels per chapter (e.g. {5,6,4})")]
    public int[] levelsPerChapter;

    [Header("Rewards")]
    [Tooltip("New/empty save 初始化每关 rewardTotals 的默认值（现在=3；以后想改就改这个）")]
    public int defaultRewardTotalPerLevel = 3;

    // PlayerPrefs keys
    private const string PP_LastTriggerScene = "LastTriggerScene";
    private const string PP_SceneRegistry = "JZ_SavedPosScenes"; // ✅ LevelTrigger 写位置时会把 sceneName 注册到这里

    // ========== Playtest Unlock ==========
    public enum UnlockPolicy
    {
        Normal,
        UnlockAll_Session,
        UnlockAll_Persist
    }

    [Header("Dev / Playtest")]
    [Tooltip("Normal=逐关解锁；UnlockAll_Session=本次运行全解锁(不写存档)；UnlockAll_Persist=把所有关卡写成已通关(写存档)")]
    public UnlockPolicy unlockPolicy = UnlockPolicy.Normal;

    [Header("Dev / Debug")]
    [Tooltip("勾上后：启动时清空所有记录（rewards/cleared/bestTimes/rewardTotals），并写入存档。执行一次后会自动取消勾选。")]
    public bool resetAllProgressOnNextLaunch = false;

    [Header("Tutorial / Start Behavior")]
    [Tooltip("如果存档是全新/清零，Start 将直接进入 tutorial 场景")]
    public bool startToTutorialWhenFresh = true;

    [Tooltip("Tutorial 关卡的 Scene 名字（Build Settings 里要存在；名字要和真实 scene 完全一致）")]
    public string tutorialSceneName = "Chapter0Level0";

    private SaveData data;
    private string savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");

        if (levelsPerChapter == null || levelsPerChapter.Length == 0)
        {
            Debug.LogError("[SaveManager] levelsPerChapter is not set!");
            data = new SaveData { chapters = new List<ChapterData>() };
            return;
        }

        if (File.Exists(savePath))
        {
            Load();
            EnsureStructure();
        }
        else
        {
            data = new SaveData { chapters = new List<ChapterData>() };
            EnsureStructure();
            Save();
        }

        if (resetAllProgressOnNextLaunch)
        {
            ResetAllProgress();
            Save();
            resetAllProgressOnNextLaunch = false;
            Debug.Log("[SaveManager] ✅ ResetAllProgress done and saved.");
        }

        if (unlockPolicy == UnlockPolicy.UnlockAll_Persist)
        {
            MarkAllClearedPersist();
            Save();
        }
    }

    private void Load()
    {
        try
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) data = new SaveData { chapters = new List<ChapterData>() };
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed: {e.Message}");
            data = new SaveData { chapters = new List<ChapterData>() };
        }
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save failed: {e.Message}");
        }
    }

    private void EnsureStructure()
    {
        if (data.chapters == null) data.chapters = new List<ChapterData>();

        while (data.chapters.Count < levelsPerChapter.Length)
            data.chapters.Add(new ChapterData());
        if (data.chapters.Count > levelsPerChapter.Length)
            data.chapters.RemoveRange(levelsPerChapter.Length, data.chapters.Count - levelsPerChapter.Length);

        for (int c = 0; c < levelsPerChapter.Length; c++)
        {
            int count = levelsPerChapter[c];
            var ch = data.chapters[c] ?? (data.chapters[c] = new ChapterData());

            // rewards (collected)
            if (ch.rewards == null) ch.rewards = new List<int>();
            if (ch.rewards.Count != count)
            {
                var newList = new List<int>(new int[count]);
                int copy = Mathf.Min(ch.rewards.Count, count);
                for (int i = 0; i < copy; i++) newList[i] = Mathf.Max(0, ch.rewards[i]);
                ch.rewards = newList;
            }

            // rewardTotals (total per level)
            if (ch.rewardTotals == null) ch.rewardTotals = new List<int>();
            if (ch.rewardTotals.Count != count)
            {
                var newList = new List<int>(new int[count]);
                for (int i = 0; i < count; i++) newList[i] = Mathf.Max(0, defaultRewardTotalPerLevel);

                int copy = Mathf.Min(ch.rewardTotals.Count, count);
                for (int i = 0; i < copy; i++) newList[i] = Mathf.Max(0, ch.rewardTotals[i]);

                ch.rewardTotals = newList;
            }

            // cleared
            if (ch.cleared == null) ch.cleared = new List<bool>();
            if (ch.cleared.Count != count)
            {
                var newList = new List<bool>(new bool[count]);
                int copy = Mathf.Min(ch.cleared.Count, count);
                for (int i = 0; i < copy; i++) newList[i] = ch.cleared[i];

                // 兼容旧存档：如果旧存档没有 cleared，则 rewards>0 视为已通关
                if (ch.cleared.Count == 0 && ch.rewards != null)
                {
                    int m = Mathf.Min(ch.rewards.Count, count);
                    for (int i = 0; i < m; i++)
                        if (ch.rewards[i] > 0) newList[i] = true;
                }
                ch.cleared = newList;
            }

            // bestTimes
            if (ch.bestTimes == null) ch.bestTimes = new List<float>();
            if (ch.bestTimes.Count != count)
            {
                var newList = new List<float>(new float[count]);
                for (int i = 0; i < count; i++) newList[i] = -1f;

                int copy = Mathf.Min(ch.bestTimes.Count, count);
                for (int i = 0; i < copy; i++) newList[i] = ch.bestTimes[i];

                ch.bestTimes = newList;
            }

            // 防御：保证 collected 不超过 total
            for (int i = 0; i < count; i++)
            {
                int total = Mathf.Max(0, ch.rewardTotals[i]);
                ch.rewards[i] = Mathf.Clamp(ch.rewards[i], 0, total);
            }
        }
    }

    // =========================
    // Reset API
    // =========================
    public void ResetAllProgress()
    {
        if (data?.chapters == null) return;

        for (int c = 0; c < data.chapters.Count; c++)
        {
            var ch = data.chapters[c];
            if (ch == null) continue;

            if (ch.rewards != null)
                for (int i = 0; i < ch.rewards.Count; i++) ch.rewards[i] = 0;

            if (ch.cleared != null)
                for (int i = 0; i < ch.cleared.Count; i++) ch.cleared[i] = false;

            if (ch.bestTimes != null)
                for (int i = 0; i < ch.bestTimes.Count; i++) ch.bestTimes[i] = -1f;

            if (ch.rewardTotals != null)
                for (int i = 0; i < ch.rewardTotals.Count; i++)
                    ch.rewardTotals[i] = Mathf.Max(0, defaultRewardTotalPerLevel);
        }

        ClearAllSavedPlayerWorldPositions(); // 你原来的（可留可不留）
        ClearWorldScenePositionsHard();      // ✅ 新增这一刀
        Debug.Log($"[SaveManager] After reset, Has Chapter1World keys? X={PlayerPrefs.HasKey("Chapter1World_LastTriggerX")} Y={PlayerPrefs.HasKey("Chapter1World_LastTriggerY")} Z={PlayerPrefs.HasKey("Chapter1World_LastTriggerZ")}");
    }

    // =========================
    // Rewards (Collected)
    // =========================
    public void SetLevelRewards(int chapterIndex, int levelIndex, int rewards)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return;

        int total = GetLevelRewardTotal(chapterIndex, levelIndex);
        data.chapters[chapterIndex].rewards[levelIndex] = Mathf.Clamp(rewards, 0, total);
        Save();
    }

    // ✅ 兼容你工程里可能用的旧名字：GetLevelRewards / GetLevelReward
    public int GetLevelRewards(int chapterIndex, int levelIndex)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return 0;
        return Mathf.Max(0, data.chapters[chapterIndex].rewards[levelIndex]);
    }

    // =========================
    // Rewards (Total)
    // =========================
    public void SetLevelRewardTotal(int chapterIndex, int levelIndex, int total)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return;

        total = Mathf.Max(0, total);
        data.chapters[chapterIndex].rewardTotals[levelIndex] = total;

        // 同步 clamp collected
        int collected = data.chapters[chapterIndex].rewards[levelIndex];
        data.chapters[chapterIndex].rewards[levelIndex] = Mathf.Clamp(collected, 0, total);

        Save();
    }

    public int GetLevelRewardTotal(int chapterIndex, int levelIndex)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex))
            return Mathf.Max(0, defaultRewardTotalPerLevel);

        return Mathf.Max(0, data.chapters[chapterIndex].rewardTotals[levelIndex]);
    }

    public string GetLevelRewardProgressText(int chapterIndex, int levelIndex)
    {
        int collected = GetLevelRewards(chapterIndex, levelIndex);
        int total = GetLevelRewardTotal(chapterIndex, levelIndex);
        return $"{collected}/{total}";
    }

    // =========================
    // Chapter total
    // =========================
    public int GetChapterTotal(int chapterIndex)
    {
        if (data?.chapters == null || chapterIndex < 0 || chapterIndex >= data.chapters.Count) return 0;
        int sum = 0;
        var ch = data.chapters[chapterIndex];
        if (ch?.rewards == null) return 0;
        foreach (var r in ch.rewards) sum += Mathf.Max(0, r);
        return sum;
    }

    // =========================
    // Completion / Unlock
    // =========================
    public void MarkLevelCompleted(int chapterIndex, int levelIndex, int rewardsCollected, int rewardTotal = -1)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return;

        data.chapters[chapterIndex].cleared[levelIndex] = true;

        if (rewardTotal >= 0)
            data.chapters[chapterIndex].rewardTotals[levelIndex] = Mathf.Max(0, rewardTotal);

        int totalNow = GetLevelRewardTotal(chapterIndex, levelIndex);
        data.chapters[chapterIndex].rewards[levelIndex] = Mathf.Clamp(rewardsCollected, 0, totalNow);

        Save();
    }

    public bool IsLevelCleared(int chapterIndex, int levelIndex)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return false;
        return data.chapters[chapterIndex].cleared[levelIndex];
    }

    public bool IsLevelUnlocked(int chapterIndex, int levelIndex)
    {
        if (unlockPolicy == UnlockPolicy.UnlockAll_Session) return true;

        if (!ValidateChapterIndex(chapterIndex)) return false;
        if (levelIndex < 0 || levelIndex >= levelsPerChapter[chapterIndex]) return false;
        if (levelIndex == 0) return true;

        return IsLevelCleared(chapterIndex, levelIndex - 1);
    }

    // =========================
    // Best time
    // =========================
    public float GetBestTime(int chapterIndex, int levelIndex)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return -1f;

        var ch = data.chapters[chapterIndex];
        if (ch.bestTimes == null || levelIndex < 0 || levelIndex >= ch.bestTimes.Count) return -1f;
        return ch.bestTimes[levelIndex];
    }

    public bool SetBestTimeIfBetter(int chapterIndex, int levelIndex, float newTimeSeconds)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return false;
        if (newTimeSeconds < 0f) return false;

        var ch = data.chapters[chapterIndex];

        float oldBest = ch.bestTimes[levelIndex];

        if (oldBest < 0f || newTimeSeconds < oldBest)
        {
            ch.bestTimes[levelIndex] = newTimeSeconds;
            Save();
            return true;
        }

        return false;
    }

    // =========================
    // Tutorial / Start behavior
    // =========================
    public bool IsFreshSave()
    {
        if (data?.chapters == null) return true;

        for (int c = 0; c < data.chapters.Count; c++)
        {
            var ch = data.chapters[c];
            if (ch == null) continue;

            int levelCount = (levelsPerChapter != null && c >= 0 && c < levelsPerChapter.Length)
                ? levelsPerChapter[c]
                : 0;

            for (int i = 0; i < levelCount; i++)
            {
                if (ch.cleared != null && i < ch.cleared.Count && ch.cleared[i]) return false;
                if (ch.rewards != null && i < ch.rewards.Count && ch.rewards[i] > 0) return false;
                if (ch.bestTimes != null && i < ch.bestTimes.Count && ch.bestTimes[i] >= 0f) return false;
            }
        }
        return true;
    }

    public bool TryLoadTutorialIfFresh()
    {
        if (!startToTutorialWhenFresh) return false;
        if (!IsFreshSave()) return false;

        SceneManager.LoadScene(tutorialSceneName);
        return true;
    }

    // =========================
    // Playtest helpers
    // =========================
    public void SetUnlockPolicy(UnlockPolicy policy, bool persistNow = false)
    {
        unlockPolicy = policy;
        if (policy == UnlockPolicy.UnlockAll_Persist && persistNow)
        {
            MarkAllClearedPersist();
            Save();
        }
    }

    [ContextMenu("Playtest/Mark All Levels Cleared (Persist)")]
    public void MarkAllClearedPersist()
    {
        if (data?.chapters == null) return;

        for (int c = 0; c < data.chapters.Count; c++)
        {
            var ch = data.chapters[c];
            if (ch?.cleared == null) continue;
            for (int i = 0; i < ch.cleared.Count; i++)
                ch.cleared[i] = true;
        }
    }

    // =========================
    // Validation
    // =========================
    private bool ValidateChapterIndex(int ci)
    {
        if (data?.chapters == null) return false;
        return ci >= 0 && ci < data.chapters.Count;
    }

    private bool ValidateLevelIndex(int ci, int li)
    {
        if (!ValidateChapterIndex(ci)) return false;
        var ch = data.chapters[ci];

        return ch != null &&
               ch.rewards != null &&
               ch.rewardTotals != null &&
               ch.cleared != null &&
               ch.bestTimes != null &&
               li >= 0 &&
               li < ch.rewards.Count &&
               li < ch.rewardTotals.Count &&
               li < ch.cleared.Count &&
               li < ch.bestTimes.Count;
    }

    // =========================
    // ✅ Clear ALL saved player positions (registry + fallback)
    // =========================
    private void ClearAllSavedPlayerWorldPositions()
    {
        // 1) LastTriggerScene
        if (PlayerPrefs.HasKey(PP_LastTriggerScene))
            PlayerPrefs.DeleteKey(PP_LastTriggerScene);

        // 2) registry 里记过的全部 scene（最可靠）
        string raw = PlayerPrefs.GetString(PP_SceneRegistry, "");
        if (!string.IsNullOrEmpty(raw))
        {
            string[] scenes = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < scenes.Length; i++)
            {
                string s = scenes[i];
                PlayerPrefs.DeleteKey(s + "_LastTriggerX");
                PlayerPrefs.DeleteKey(s + "_LastTriggerY");
                PlayerPrefs.DeleteKey(s + "_LastTriggerZ");
            }
        }

        // 3) 兜底：按“存档 chapters 数量”来删 world（不要用 levelsPerChapter.Length）
        int chapterCountFromData = (data != null && data.chapters != null) ? data.chapters.Count : 0;
        int chapterCount = Mathf.Max(chapterCountFromData, (levelsPerChapter != null ? levelsPerChapter.Length : 0), 2);
        // ↑ 最少按 2 章处理，确保 Chapter1World 一定会被清

        for (int c = 0; c < chapterCount; c++)
        {
            // 清 world scene：Chapter0World / Chapter1World / ...
            string worldScene = $"Chapter{c}World";
            PlayerPrefs.DeleteKey(worldScene + "_LastTriggerX");
            PlayerPrefs.DeleteKey(worldScene + "_LastTriggerY");
            PlayerPrefs.DeleteKey(worldScene + "_LastTriggerZ");

            // 如果你关卡场景是 Chapter{c}_Level{n}，也可继续兜底清掉
            if (levelsPerChapter != null && c >= 0 && c < levelsPerChapter.Length)
            {
                int count = levelsPerChapter[c];
                for (int l0 = 0; l0 < count; l0++)
                {
                    string levelScene = $"Chapter{c}_Level{l0 + 1}";
                    PlayerPrefs.DeleteKey(levelScene + "_LastTriggerX");
                    PlayerPrefs.DeleteKey(levelScene + "_LastTriggerY");
                    PlayerPrefs.DeleteKey(levelScene + "_LastTriggerZ");
                }
            }
        }

        // 4) 清 registry 本身
        PlayerPrefs.DeleteKey(PP_SceneRegistry);

        // 5) 再兜底：当前场景
        string cur = SceneManager.GetActiveScene().name;
        PlayerPrefs.DeleteKey(cur + "_LastTriggerX");
        PlayerPrefs.DeleteKey(cur + "_LastTriggerY");
        PlayerPrefs.DeleteKey(cur + "_LastTriggerZ");

        PlayerPrefs.Save();
        Debug.Log("[SaveManager] ✅ Cleared all saved player positions (registry + ChapterWorld).");
    }

    // ✅ 强制清 world 场景位置（不依赖 registry，不依赖 levelsPerChapter）
    // 你现在至少有 Chapter0World / Chapter1World
    private void ClearWorldScenePositionsHard()
    {
        // 如果你以后有更多 chapter，想扩展就加名字
        string[] worlds = new[] { "Chapter0World", "Chapter1World" };

        foreach (var s in worlds)
        {
            PlayerPrefs.DeleteKey(s + "_LastTriggerX");
            PlayerPrefs.DeleteKey(s + "_LastTriggerY");
            PlayerPrefs.DeleteKey(s + "_LastTriggerZ");
        }

        PlayerPrefs.DeleteKey("LastTriggerScene");
        PlayerPrefs.Save();

        Debug.Log("[SaveManager] ✅ Hard-cleared world scene saved positions (Chapter0World/Chapter1World).");
    }
}