using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChapterData
{
    public List<int> rewards;
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

    // ========== Playtest Unlock ==========
    public enum UnlockPolicy
    {
        Normal,               // 正常：逐关解锁
        UnlockAll_Session,    // 本次运行全解锁（不写入存档）
        UnlockAll_Persist     // 将所有关卡标记为已通关（写入存档）
    }

    [Header("Dev / Playtest")]
    [Tooltip("Normal=逐关解锁；UnlockAll_Session=本次运行全解锁(不写存档)；UnlockAll_Persist=把所有关卡写成已通关(写存档)")]
    public UnlockPolicy unlockPolicy = UnlockPolicy.Normal;

    private SaveData data;
    private string savePath;

    private void Awake()
    {
        // Singleton
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

        // 若选择持久化全解锁：一次性把所有关卡标记为已通关并写盘
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

        // 章节数量对齐
        while (data.chapters.Count < levelsPerChapter.Length)
            data.chapters.Add(new ChapterData());
        if (data.chapters.Count > levelsPerChapter.Length)
            data.chapters.RemoveRange(levelsPerChapter.Length, data.chapters.Count - levelsPerChapter.Length);

        // 每章长度对齐
        for (int c = 0; c < levelsPerChapter.Length; c++)
        {
            int count = levelsPerChapter[c];
            var ch = data.chapters[c] ?? (data.chapters[c] = new ChapterData());

            // rewards
            if (ch.rewards == null) ch.rewards = new List<int>();
            if (ch.rewards.Count != count)
            {
                var newList = new List<int>(new int[count]);
                int copy = Mathf.Min(ch.rewards.Count, count);
                for (int i = 0; i < copy; i++) newList[i] = ch.rewards[i];
                ch.rewards = newList;
            }

            // cleared
            if (ch.cleared == null) ch.cleared = new List<bool>();
            if (ch.cleared.Count != count)
            {
                var newList = new List<bool>(new bool[count]);
                int copy = Mathf.Min(ch.cleared.Count, count);
                for (int i = 0; i < copy; i++) newList[i] = ch.cleared[i];

                // 兼容旧存档：奖励>0 视为已通关
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
                // 默认用 -1f 表示没有记录
                var newList = new List<float>(new float[count]);
                for (int i = 0; i < count; i++) newList[i] = -1f;

                int copy = Mathf.Min(ch.bestTimes.Count, count);
                for (int i = 0; i < copy; i++) newList[i] = ch.bestTimes[i];

                ch.bestTimes = newList;
            }
        }

    }

    public void SetLevelRewards(int chapterIndex, int levelIndex, int rewards)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return;
        data.chapters[chapterIndex].rewards[levelIndex] = rewards;
        Save();
    }

    public int GetLevelRewards(int chapterIndex, int levelIndex)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return 0;
        return data.chapters[chapterIndex].rewards[levelIndex];
    }

    public int GetChapterTotal(int chapterIndex)
    {
        if (data?.chapters == null || chapterIndex < 0 || chapterIndex >= data.chapters.Count) return 0;
        int sum = 0;
        foreach (var r in data.chapters[chapterIndex].rewards) sum += r;
        return sum;
    }

    public void MarkLevelCompleted(int chapterIndex, int levelIndex, int rewards)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return;
        data.chapters[chapterIndex].cleared[levelIndex] = true;
        data.chapters[chapterIndex].rewards[levelIndex] = rewards; // 可为0
        Save();
    }

    public bool IsLevelCleared(int chapterIndex, int levelIndex)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return false;
        return data.chapters[chapterIndex].cleared[levelIndex];
    }

    // 规则：第一关(索引0)默认解锁；其他关卡=前一关已通关
    public bool IsLevelUnlocked(int chapterIndex, int levelIndex)
    {
        // Playtest: Session 全解锁（不改存档）
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

    /// <summary>
    /// 如果 newTime 更好（更小）就替换并保存；返回是否更新了 best
    /// </summary>
    public bool SetBestTimeIfBetter(int chapterIndex, int levelIndex, float newTimeSeconds)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return false;
        if (newTimeSeconds < 0f) return false;

        var ch = data.chapters[chapterIndex];

        // 防御：确保结构存在（理论上 EnsureStructure 已做）
        if (ch.bestTimes == null)
        {
            ch.bestTimes = new List<float>();
            for (int i = 0; i < levelsPerChapter[chapterIndex]; i++) ch.bestTimes.Add(-1f);
        }

        float oldBest = ch.bestTimes[levelIndex];

        // oldBest < 0 表示还没记录；newTime 更小表示更好
        if (oldBest < 0f || newTimeSeconds < oldBest)
        {
            ch.bestTimes[levelIndex] = newTimeSeconds;
            Save();
            return true;
        }

        return false;
    }


    // ========= Playtest 辅助 =========

    /// <summary>
    /// 运行时切换解锁策略（比如在开发菜单/按钮里调用）
    /// </summary>
    public void SetUnlockPolicy(UnlockPolicy policy, bool persistNow = false)
    {
        unlockPolicy = policy;
        if (policy == UnlockPolicy.UnlockAll_Persist && persistNow)
        {
            MarkAllClearedPersist();
            Save();
        }
    }

    /// <summary>
    /// 一键把所有关卡标记为已通关（写入存档）
    /// </summary>
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
               ch.cleared != null &&
               ch.bestTimes != null &&
               li >= 0 &&
               li < ch.rewards.Count &&
               li < ch.cleared.Count &&
               li < ch.bestTimes.Count;
    }

}
