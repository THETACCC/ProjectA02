using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("勾上后：启动时清空所有记录（rewards/cleared/bestTimes），并写入存档。执行一次后会自动取消勾选。")]
    public bool resetAllProgressOnNextLaunch = false;

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
                // 默认全部填 defaultRewardTotalPerLevel
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

            // 额外防御：保证 collected 不超过 total（如果你之后改了 total 更小）
            for (int i = 0; i < count; i++)
            {
                int total = Mathf.Max(0, ch.rewardTotals[i]);
                ch.rewards[i] = Mathf.Clamp(ch.rewards[i], 0, total);
            }
        }
    }

    // ===== Reset API =====
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

            // totals 重置回默认（避免你以后改默认值但旧存档还卡在旧 total）
            if (ch.rewardTotals != null)
                for (int i = 0; i < ch.rewardTotals.Count; i++)
                    ch.rewardTotals[i] = Mathf.Max(0, defaultRewardTotalPerLevel);
        }
    }

    // ===== Rewards (Collected) =====
    public void SetLevelRewards(int chapterIndex, int levelIndex, int rewards)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return;

        int total = GetLevelRewardTotal(chapterIndex, levelIndex);
        data.chapters[chapterIndex].rewards[levelIndex] = Mathf.Clamp(rewards, 0, total);
        Save();
    }

    public int GetLevelRewards(int chapterIndex, int levelIndex)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return 0;
        return Mathf.Max(0, data.chapters[chapterIndex].rewards[levelIndex]);
    }

    // ===== Rewards (Total) =====
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
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return Mathf.Max(0, defaultRewardTotalPerLevel);
        return Mathf.Max(0, data.chapters[chapterIndex].rewardTotals[levelIndex]);
    }

    public string GetLevelRewardProgressText(int chapterIndex, int levelIndex)
    {
        int collected = GetLevelRewards(chapterIndex, levelIndex);
        int total = GetLevelRewardTotal(chapterIndex, levelIndex);
        return $"{collected}/{total}";
    }

    public int GetChapterTotal(int chapterIndex)
    {
        if (data?.chapters == null || chapterIndex < 0 || chapterIndex >= data.chapters.Count) return 0;
        int sum = 0;
        foreach (var r in data.chapters[chapterIndex].rewards) sum += Mathf.Max(0, r);
        return sum;
    }

    // ✅ 这里改成：同时记录 collected 和 total（total 可选）
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

        if (ch.bestTimes == null)
        {
            ch.bestTimes = new List<float>();
            for (int i = 0; i < levelsPerChapter[chapterIndex]; i++) ch.bestTimes.Add(-1f);
        }

        float oldBest = ch.bestTimes[levelIndex];

        if (oldBest < 0f || newTimeSeconds < oldBest)
        {
            ch.bestTimes[levelIndex] = newTimeSeconds;
            Save();
            return true;
        }

        return false;
    }

    // ========= Playtest 辅助 =========
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

    // ========= 校验 =========
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
}