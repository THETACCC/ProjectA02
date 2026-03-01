// StartUIChapterButton.cs
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class StartUIChapterButton : MonoBehaviour
{
    [Header("Chapter")]
    [Tooltip("0..3 (Chapter0/1/2/3)")]
    public int chapterIndex = 0;

    [Tooltip("可选：不填就会用 Auto Scene Name（Chapter{index}World）")]
    public string sceneNameOverride = "";

    [Tooltip("如果你的章节场景命名为 Chapter0World/Chapter1World... 勾上即可")]
    public bool autoSceneName = true;

    [Header("Lock Visual (per-module)")]
    [Tooltip("锁图标/遮罩（每个 module 自己一套）。开关它即可。")]
    public GameObject lockVisual;

    [Tooltip("可选：锁住时淡出，但不阻断点击（否则无法用点击旋转轮盘）。")]
    public CanvasGroup fadeGroup;
    public float lockedAlpha = 0.35f;

    [Header("Mode")]
    [Tooltip("勾上：这个脚本只负责锁视觉（不改任何文字）。共享文字由 SummaryPanel 更新。")]
    public bool onlyToggleLockVisual = true;

    [Header("Optional local texts (ONLY if onlyToggleLockVisual=false)")]
    public TMP_Text chapterTitleText;     // "CHAPTER 0"
    public TMP_Text unlockedLevelsText;   // "N/VI"
    public TMP_Text rewardsText;          // "00/00"

    [Header("Click Rules")]
    [Tooltip("在 MenuMenuScrollUI 轮盘里：只有当它在中心位时才允许进入章节")]
    public bool requireCenterInScrollUI = true;

    [Header("Runtime")]
    public bool isLocked = false;

    private MenuMenuScrollUI _scrollUI;
    private int _scrollModuleIndex = -1;
    private Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);

        if (!fadeGroup) fadeGroup = GetComponent<CanvasGroup>() ?? GetComponentInChildren<CanvasGroup>(true);

        _scrollUI = GetComponentInParent<MenuMenuScrollUI>();
        _scrollModuleIndex = FindMyModuleIndexInScrollUI(_scrollUI);

        if (!lockVisual)
            lockVisual = AutoFindLockVisual();

        if (lockVisual)
            ConfigureLockVisualNoRaycast(lockVisual);

        if (_btn)
        {
            _btn.onClick.RemoveListener(OnClick);
            _btn.onClick.AddListener(OnClick);
        }
    }

    void Start() => RefreshUI();
    void OnEnable() => RefreshUI();

    public void RefreshUI()
    {
        var sm = SaveManager.Instance;

        bool chapterUnlocked = IsChapterUnlocked(sm, chapterIndex);
        isLocked = !chapterUnlocked;

        if (lockVisual) lockVisual.SetActive(isLocked);
        if (fadeGroup) fadeGroup.alpha = isLocked ? lockedAlpha : 1f;

        if (onlyToggleLockVisual) return;

        // ---- local per-module texts (only if you want them) ----
        int totalLevels = GetChapterLevelCount(sm, chapterIndex);

        int unlockedCount = 0;
        if (chapterUnlocked && sm != null && totalLevels > 0)
            for (int i = 0; i < totalLevels; i++)
                if (sm.IsLevelUnlocked(chapterIndex, i)) unlockedCount++;

        int collected = 0;
        int total = 0;
        if (sm != null && totalLevels > 0)
        {
            for (int i = 0; i < totalLevels; i++)
            {
                collected += Mathf.Max(0, sm.GetLevelRewards(chapterIndex, i));
                total += Mathf.Max(0, sm.GetLevelRewardTotal(chapterIndex, i));
            }
        }

        if (chapterTitleText) chapterTitleText.text = $"CHAPTER {chapterIndex}";
        if (unlockedLevelsText) unlockedLevelsText.text = $"{ToRomanOrN(unlockedCount)}/{ToRomanOrN(totalLevels)}";
        if (rewardsText) rewardsText.text = $"{collected:00}/{total:00}";
    }

    private void OnClick()
    {
        // 让 MenuMenuScrollUI 还能处理 top/bottom 点击旋转
        // 所以这里不去 disable Button，只在这里阻止“进入章节”即可
        RefreshUI();
        if (isLocked) return;

        if (requireCenterInScrollUI && _scrollUI != null && _scrollModuleIndex >= 0)
        {
            if (_scrollUI.CenterModuleIndex != _scrollModuleIndex)
                return;
        }

        string targetScene = ResolveSceneName();
        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogError("[StartUIChapterButton] scene name is empty.");
            return;
        }

        SceneManager.LoadScene(targetScene);
    }

    private string ResolveSceneName()
    {
        if (!string.IsNullOrWhiteSpace(sceneNameOverride))
            return sceneNameOverride;

        if (autoSceneName)
            return $"Chapter{chapterIndex}World";

        return "";
    }

    // Chapter0 always unlocked; chapter i>0 requires previous chapter ALL cleared
    private static bool IsChapterUnlocked(SaveManager sm, int ci)
    {
        if (ci <= 0) return true;
        if (sm == null) return false;

        if (sm.unlockPolicy == SaveManager.UnlockPolicy.UnlockAll_Session)
            return true;

        int prev = ci - 1;
        int prevCount = GetChapterLevelCount(sm, prev);
        if (prevCount <= 0) return false;

        for (int i = 0; i < prevCount; i++)
            if (!sm.IsLevelCleared(prev, i)) return false;

        return true;
    }

    private static int GetChapterLevelCount(SaveManager sm, int ci)
    {
        if (sm == null || sm.levelsPerChapter == null) return 0;
        if (ci < 0 || ci >= sm.levelsPerChapter.Length) return 0;
        return Mathf.Max(0, sm.levelsPerChapter[ci]);
    }

    private static string ToRomanOrN(int value)
    {
        if (value <= 0) return "N";
        if (value > 3999) return value.ToString();

        var map = new (int v, string s)[]
        {
            (1000,"M"), (900,"CM"), (500,"D"), (400,"CD"),
            (100,"C"), (90,"XC"), (50,"L"), (40,"XL"),
            (10,"X"), (9,"IX"), (5,"V"), (4,"IV"), (1,"I")
        };

        int n = value;
        var sb = new StringBuilder();
        foreach (var (v, s) in map)
            while (n >= v) { sb.Append(s); n -= v; }

        return sb.ToString();
    }

    private int FindMyModuleIndexInScrollUI(MenuMenuScrollUI scroll)
    {
        if (!scroll || scroll.moduleRoots == null) return -1;

        Transform t = transform;
        while (t != null)
        {
            for (int i = 0; i < scroll.moduleRoots.Length; i++)
                if (scroll.moduleRoots[i] == t) return i;
            t = t.parent;
        }
        return -1;
    }

    private GameObject AutoFindLockVisual()
    {
        // 找名字包含 lock 的子物体
        var trs = GetComponentsInChildren<Transform>(true);
        foreach (var tr in trs)
        {
            if (!tr) continue;
            if (tr.name.ToLowerInvariant().Contains("lock"))
                return tr.gameObject;
        }
        return null;
    }

    private void ConfigureLockVisualNoRaycast(GameObject go)
    {
        // 让锁图标不吃点击，确保轮盘点击还能旋转
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        var graphics = go.GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
            g.raycastTarget = false;
    }
}