using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChapterData
{
    // Rewards for each level in this chapter
    public List<int> rewards;

    //是否已通关（用于解锁下一关 & 世界里显示隐藏）
    public List<bool> cleared;
}

[Serializable]
public class SaveData
{
    // One ChapterData per chapter
    public List<ChapterData> chapters;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Configure number of levels per chapter (e.g. {5,6,4})")]
    public int[] levelsPerChapter;

    private SaveData data;
    private string savePath;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        //Debug.Log($"[SaveManager] Awake - path={savePath}, chapters={levelsPerChapter?.Length}");

        if (levelsPerChapter == null || levelsPerChapter.Length == 0)
        {
            Debug.LogError("[SaveManager] levelsPerChapter is not set in the Inspector!");
            data = new SaveData();
            return;
        }

        if (File.Exists(savePath))
        {
            Load();
            EnsureStructure();
            //Debug.Log("[SaveManager] Loaded existing data: " + JsonUtility.ToJson(data));
        }
        else
        {
            data = new SaveData();
            EnsureStructure();
            Save();
            //Debug.Log("[SaveManager] Initialized new data: " + JsonUtility.ToJson(data));
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
            //Debug.Log("[SaveManager] Data saved: " + json);
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

                // 兼容旧存档：如果旧奖励>0，认定为已通关
                if (ch.cleared.Count == 0 && ch.rewards != null)
                {
                    int m = Mathf.Min(ch.rewards.Count, count);
                    for (int i = 0; i < m; i++)
                        if (ch.rewards[i] > 0) newList[i] = true;
                }
                ch.cleared = newList;
            }
        }
    }

    // ===== 你原有接口，保留 =====
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

    // ===== 新增：通关/解锁逻辑 =====
    public void MarkLevelCompleted(int chapterIndex, int levelIndex, int rewards)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return;
        data.chapters[chapterIndex].cleared[levelIndex] = true;   // 套路：通关 = true
        data.chapters[chapterIndex].rewards[levelIndex] = rewards; // 可为0
        Save();
    }

    public bool IsLevelCleared(int chapterIndex, int levelIndex)
    {
        if (!ValidateLevelIndex(chapterIndex, levelIndex)) return false;
        return data.chapters[chapterIndex].cleared[levelIndex];
    }

    // 规则：每章第1关(索引0)默认解锁；其他关卡=前一关已通关
    public bool IsLevelUnlocked(int chapterIndex, int levelIndex)
    {
        if (!ValidateChapterIndex(chapterIndex)) return false;
        if (levelIndex < 0 || levelIndex >= levelsPerChapter[chapterIndex]) return false;
        if (levelIndex == 0) return true;
        return IsLevelCleared(chapterIndex, levelIndex - 1);
    }

    // ===== 校验辅助 =====
    private bool ValidateChapterIndex(int ci)
    {
        if (data?.chapters == null) return false;
        return ci >= 0 && ci < data.chapters.Count;
    }
    private bool ValidateLevelIndex(int ci, int li)
    {
        if (!ValidateChapterIndex(ci)) return false;
        var ch = data.chapters[ci];
        return ch != null && ch.rewards != null && ch.cleared != null &&
               li >= 0 && li < ch.rewards.Count && li < ch.cleared.Count;
    }
}
