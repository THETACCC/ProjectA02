using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChapterData
{
    // Rewards for each level in this chapter
    public List<int> rewards;
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
        Debug.Log($"[SaveManager] Awake - path={savePath}, chapters={levelsPerChapter?.Length}");

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
            Debug.Log("[SaveManager] Loaded existing data: " + JsonUtility.ToJson(data));
        }
        else
        {
            data = new SaveData();
            EnsureStructure();
            Save();
            Debug.Log("[SaveManager] Initialized new data: " + JsonUtility.ToJson(data));
        }
    }

    private void Load()
    {
        try
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
                data = new SaveData();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed: {e.Message}");
            data = new SaveData();
        }
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log("[SaveManager] Data saved: " + json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save failed: {e.Message}");
        }
    }

    private void EnsureStructure()
    {
        // Initialize chapters list
        if (data.chapters == null || data.chapters.Count != levelsPerChapter.Length)
            data.chapters = new List<ChapterData>(levelsPerChapter.Length);

        // Ensure each chapter has the correct number of levels
        for (int i = 0; i < levelsPerChapter.Length; i++)
        {
            if (i >= data.chapters.Count)
            {
                // Add new chapter with default zeros
                data.chapters.Add(new ChapterData { rewards = new List<int>(new int[levelsPerChapter[i]]) });
            }
            else
            {
                var chapter = data.chapters[i];
                if (chapter.rewards == null || chapter.rewards.Count != levelsPerChapter[i])
                    chapter.rewards = new List<int>(new int[levelsPerChapter[i]]);
            }
        }
    }

    /// <summary>
    /// Overwrite the saved rewards for a specific level.
    /// </summary>
    public void SetLevelRewards(int chapterIndex, int levelIndex, int rewards)
    {
        if (data?.chapters == null ||
            chapterIndex < 0 || chapterIndex >= data.chapters.Count ||
            levelIndex < 0 || levelIndex >= data.chapters[chapterIndex].rewards.Count)
        {
            Debug.LogError($"[SaveManager] Invalid indices c={chapterIndex}, l={levelIndex}");
            return;
        }
        data.chapters[chapterIndex].rewards[levelIndex] = rewards;
        Save();
    }

    /// <summary>
    /// Retrieve saved rewards for a specific level.
    /// </summary>
    public int GetLevelRewards(int chapterIndex, int levelIndex)
    {
        if (data?.chapters == null ||
            chapterIndex < 0 || chapterIndex >= data.chapters.Count ||
            levelIndex < 0 || levelIndex >= data.chapters[chapterIndex].rewards.Count)
            return 0;
        return data.chapters[chapterIndex].rewards[levelIndex];
    }

    /// <summary>
    /// Sum up rewards for all levels in a chapter.
    /// </summary>
    public int GetChapterTotal(int chapterIndex)
    {
        if (data?.chapters == null ||
            chapterIndex < 0 || chapterIndex >= data.chapters.Count)
            return 0;

        int sum = 0;
        foreach (var r in data.chapters[chapterIndex].rewards)
            sum += r;
        return sum;
    }
}
