using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance { get; private set; }

    [Header("UI References (assign in Inspector)")]
    [SerializeField] private TextMeshProUGUI hudTimerText;     // 关卡中显示
    [SerializeField] private TextMeshProUGUI finishTimerText;   // Finish UI里显示（可选）
    [SerializeField] private TextMeshProUGUI bestTimerText;     // 如果你想显示最好成绩（可选）

    [Header("Timer State (read only)")]
    [SerializeField] private bool isRunning;
    [SerializeField] private float elapsedSeconds;

    [SerializeField] private bool hasStarted = false;
    [SerializeField] private bool hasFinished = false;

    // 当前关卡索引（用于存best time）
    private int chapterIndex;
    private int levelIndex;

    //private const string BestKeyPrefix = "BEST_TIME_"; // PlayerPrefs key 前缀

    private bool pausedByMenu = false;
    private bool wasRunningBeforeMenuPause = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 如果你希望跨场景保留这个物体，打开下一行；否则每关一个也可以
        // DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 进关卡默认 0，不跑
        ResetTimer();

        // 尝试从场景名解析 Chapter/Level
        // Scene 名：ChapterX_LevelY
        ParseSceneIndices(scene.name);

        // 刷新UI初始显示 00:00，以及best
        UpdateAllTimerTexts();
        UpdateBestTextIfAny();
    }

    private void Update()
    {
        if (!isRunning) return;

        // 用 unscaledDeltaTime 更稳：如果你之后通关把 timeScale 设为0，也不影响
        elapsedSeconds += Time.unscaledDeltaTime;

        // 每帧更新HUD
        if (hudTimerText != null)
            hudTimerText.text = FormatMMSSCC(elapsedSeconds);
    }

    public void StartTimer()
    {
        // 通关后不允许再开始（避免归零）
        if (hasFinished) return;

        // 只允许第一次开始（避免反复按Space归零）
        if (hasStarted) return;

        elapsedSeconds = 0f;
        isRunning = true;
        hasStarted = true;

        UpdateAllTimerTexts();
    }

    /// <summary>停止计时并返回本次用时（秒）</summary>
    public float StopTimer()
    {
        // 如果从没开始过，就别动（防御）
        if (!hasStarted) return elapsedSeconds;

        isRunning = false;
        hasFinished = true;

        UpdateAllTimerTexts();  // 写最终时间到 HUD/Finish
        return elapsedSeconds;
    }

    public void ResetTimer()
    {
        isRunning = false;
        elapsedSeconds = 0f;
        hasStarted = false;
        hasFinished = false;
    }

    public float GetElapsedSeconds() => elapsedSeconds;

    private void UpdateAllTimerTexts()
    {
        string t = FormatMMSSCC(elapsedSeconds);

        if (hudTimerText != null) hudTimerText.text = t;

        // FinishUI 还没SetActive也没关系：TextMeshProUGUI 组件依然能被赋值
        if (finishTimerText != null) finishTimerText.text = t;
    }

    private void SaveBestTimeIfNeeded()
    {
        // 还没解析到关卡索引就先不存（避免key乱）
        if (chapterIndex < 0 || levelIndex < 0) return;

        // SaveManager 不存在就不存（例如某些测试场景没放 SaveManager）
        if (SaveManager.Instance == null) return;

        // ✅ 存到 save.json：更小就覆盖并 Save()
        SaveManager.Instance.SetBestTimeIfBetter(chapterIndex, levelIndex, elapsedSeconds);
    }

    private void UpdateBestTextIfAny()
    {
        if (bestTimerText == null) return;
        if (chapterIndex < 0 || levelIndex < 0) { bestTimerText.text = ""; return; }

        if (SaveManager.Instance == null) { bestTimerText.text = ""; return; }

        float best = SaveManager.Instance.GetBestTime(chapterIndex, levelIndex);
        bestTimerText.text = (best < 0f) ? "" : $"Best: {FormatMMSSCC(best)}";
    }


    //private string MakeBestKey(int chap, int lvl) => $"{BestKeyPrefix}C{chap}_L{lvl}";

    /*
    private string FormatMMSS(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        int mm = total / 60;
        int ss = total % 60;
        return $"{mm:00}:{ss:00}";
    }
    */
    private string FormatMMSSCC(float seconds)
    {
        if (seconds < 0f) seconds = 0f;

        int totalCentis = Mathf.FloorToInt(seconds * 100f); // 1/100 秒
        int mm = totalCentis / (60 * 100);
        int ss = (totalCentis / 100) % 60;
        int cc = totalCentis % 100;

        return $"{mm:00}:{ss:00}:{cc:00}";
    }


    private void ParseSceneIndices(string sceneName)
    {
        // 默认无效
        chapterIndex = -1;
        levelIndex = -1;

        // 期望格式：ChapterX_LevelY（Y从1开始，你LevelPass里做了 -1）
        // 这里简单解析，不用Regex也行
        try
        {
            if (!sceneName.StartsWith("Chapter")) return;
            int underscore = sceneName.IndexOf('_');
            if (underscore < 0) return;

            string chapStr = sceneName.Substring("Chapter".Length, underscore - "Chapter".Length); // X
            string levelPart = sceneName.Substring(underscore + 1); // LevelY
            if (!levelPart.StartsWith("Level")) return;

            string lvlStr = levelPart.Substring("Level".Length); // Y
            int chap = int.Parse(chapStr);
            int rawLevel = int.Parse(lvlStr);

            chapterIndex = chap;
            levelIndex = Mathf.Max(0, rawLevel - 1);
        }
        catch
        {
            // 解析失败就不存best
            chapterIndex = -1;
            levelIndex = -1;
        }
    }

    public void PauseFromMenu()
    {
        if (pausedByMenu) return;

        pausedByMenu = true;

        // 只有当它本来在跑，才记录“恢复时要继续跑”
        wasRunningBeforeMenuPause = isRunning;

        // 暂停：停止增长，但不清零
        isRunning = false;

        // 暂停那一刻也更新一下UI（可选）
        UpdateAllTimerTexts();
    }

    public void ResumeFromMenu()
    {
        if (!pausedByMenu) return;

        pausedByMenu = false;

        // 如果暂停前在跑，就继续跑；如果暂停前没开始（或已结束），就不动
        if (wasRunningBeforeMenuPause)
            isRunning = true;

        wasRunningBeforeMenuPause = false;

        UpdateAllTimerTexts();
    }

    public void RefreshBestUI()
    {
        UpdateBestTextIfAny();
    }

}
