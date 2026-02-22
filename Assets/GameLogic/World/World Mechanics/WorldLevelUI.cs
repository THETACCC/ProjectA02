using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldLevelUI : MonoBehaviour
{
    public static WorldLevelUI Instance { get; private set; }

    [Header("Hide/Show")]
    [SerializeField] private CanvasGroup canvasGroup; // optional; auto-find if null
    [SerializeField] private bool hideOnStart = true;

    [Header("Input")]
    [SerializeField] private bool listenSpaceToStart = true;

    [Header("Auto-wire by child names")]
    [SerializeField] private bool autoWireByNames = true;

    [Header("UI Refs (optional if autoWireByNames=true)")]
    [SerializeField] private TMP_Text txtLevel;       // Txt_Level
    [SerializeField] private TMP_Text txtTime;        // Txt_Time
    [SerializeField] private Image imgComplete;       // Img_Complete
    [SerializeField] private Button btnStart;         // Btn_Start

    [Header("Level text mode")]
    [Tooltip("true=show LevelTrigger GameObject name; false=show Chapter/Level numbers")]
    [SerializeField] private bool useTriggerNameAsLevelText = true;

    private readonly List<LevelTrigger> insideOrder = new List<LevelTrigger>(8);
    private LevelTrigger current;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (autoWireByNames) AutoWire();

        if (btnStart)
        {
            btnStart.onClick.RemoveAllListeners();
            btnStart.onClick.AddListener(OnClickStart);
        }

        if (hideOnStart) Hide();
        else Show();
    }

    private void Update()
    {
        if (!listenSpaceToStart) return;
        if (current == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
            OnClickStart();
    }

    // ===== called by LevelTrigger =====
    public void EnterTrigger(LevelTrigger t)
    {
        if (t == null) return;

        if (!insideOrder.Contains(t))
            insideOrder.Add(t);

        SetCurrent(t);
    }

    public void ExitTrigger(LevelTrigger t)
    {
        if (t == null) return;

        insideOrder.Remove(t);

        if (current == t)
        {
            current = null;

            if (insideOrder.Count > 0)
                SetCurrent(insideOrder[insideOrder.Count - 1]);
            else
                Hide();
        }
    }

    private void SetCurrent(LevelTrigger t)
    {
        current = t;
        RefreshUI();
        Show();
    }

    private void RefreshUI()
    {
        if (current == null) return;

        var sceneKey = current.scenetitle.ToString();
        var (chap0, lvl0) = ParseChapterLevel(sceneKey);

        if (txtLevel)
            txtLevel.text = useTriggerNameAsLevelText
                ? current.gameObject.name
                : $"Chapter {chap0 + 1} - Level {lvl0 + 1}";

        bool cleared = false;
        if (SaveManager.Instance != null)
            cleared = SaveManager.Instance.IsLevelCleared(chap0, lvl0);

        if (imgComplete) imgComplete.gameObject.SetActive(cleared);

        float best = -1f;
        if (SaveManager.Instance != null)
            best = SaveManager.Instance.GetBestTime(chap0, lvl0);

        string timeStr = FormatSeconds(best);

        if (txtTime) txtTime.text = timeStr;
    }

    private void OnClickStart()
    {
        if (current == null) return;

        current.LoadNextLevel();
    }

    public void Show()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        current = null;
        insideOrder.Clear();
    }

    // ===== helpers =====
    private void AutoWire()
    {
        if (txtTime == null) txtTime = FindByName<TMP_Text>("Txt_Time");
        if (txtLevel == null) txtLevel = FindByName<TMP_Text>("Txt_Level");
        if (imgComplete == null) imgComplete = FindByName<Image>("Img_Complete");
        if (btnStart == null) btnStart = FindByName<Button>("Btn_Start");
    }

    private T FindByName<T>(string objName) where T : Component
    {
        foreach (var tr in GetComponentsInChildren<Transform>(true))
            if (tr.name == objName) return tr.GetComponent<T>();
        return null;
    }

    private (int chap0, int lvl0) ParseChapterLevel(string sceneKey)
    {
        var m = Regex.Match(sceneKey, @"Chapter\s*(\d+)\s*[_\-\s]\s*Level\s*(\d+)", RegexOptions.IgnoreCase);
        if (!m.Success) return (0, 0);

        int chap = int.Parse(m.Groups[1].Value);
        int lvl1 = int.Parse(m.Groups[2].Value);
        return (Mathf.Max(0, chap), Mathf.Max(0, lvl1 - 1));
    }

    private string FormatSeconds(float sec)
    {
        // -1 = no record (your design)
        // 0 or near 0 should ALSO be treated as "no record" to avoid showing 00:00.00
        if (sec < 0f || sec <= 0.0001f)
            return "--:--.--";

        int totalMs = Mathf.RoundToInt(sec * 1000f);
        int minutes = totalMs / 60000;
        int seconds = (totalMs % 60000) / 1000;
        int centi = (totalMs % 1000) / 10;
        return $"{minutes:00}:{seconds:00}.{centi:00}";
    }
}