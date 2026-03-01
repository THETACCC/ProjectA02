// StartUIChapterSummaryPanel.cs
using System.Text;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class StartUIChapterSummaryPanel : MonoBehaviour
{
    [Header("Refs")]
    public MenuMenuScrollUI scrollUI;

    [Header("Shared TMP (assign ONCE here)")]
    public TMP_Text chapterTitleText;     // "CHAPTER 0"
    public TMP_Text unlockedLevelsText;   // "N/VI"
    public TMP_Text rewardsText;          // "00/00"

    [Header("Behavior")]
    [Tooltip("如果中心 module 不是 chapter（没有 StartUIChapterButton），就隐藏/清空文本")]
    public bool hideIfNoChapter = false;

    int _lastCenter = int.MinValue;
    StartUIChapterButton[] _moduleToButton;

    void Awake()
    {
        if (!scrollUI) scrollUI = FindObjectOfType<MenuMenuScrollUI>(true);
        BuildModuleMapping();
    }

    void OnEnable()
    {
        _lastCenter = int.MinValue;
        Refresh();
    }

    void Update()
    {
        if (!scrollUI) return;

        int c = scrollUI.CenterModuleIndex;
        if (c != _lastCenter)
        {
            _lastCenter = c;
            Refresh();
        }
    }

    void BuildModuleMapping()
    {
        if (!scrollUI || scrollUI.moduleRoots == null) return;

        int n = scrollUI.moduleRoots.Length;
        _moduleToButton = new StartUIChapterButton[n];

        for (int i = 0; i < n; i++)
        {
            var root = scrollUI.moduleRoots[i];
            if (!root) continue;
            _moduleToButton[i] = root.GetComponentInChildren<StartUIChapterButton>(true);
        }
    }

    public void Refresh()
    {
        if (!scrollUI) return;

        if (_moduleToButton == null || (scrollUI.moduleRoots != null && _moduleToButton.Length != scrollUI.moduleRoots.Length))
            BuildModuleMapping();

        int centerModule = scrollUI.CenterModuleIndex;
        if (centerModule < 0)
        {
            ClearOrHide();
            return;
        }

        int chapterIndex = ResolveChapterIndex(centerModule);
        if (chapterIndex < 0)
        {
            ClearOrHide();
            return;
        }

        var sm = SaveManager.Instance;
        int totalLevels = GetChapterLevelCount(sm, chapterIndex);

        bool unlocked = IsChapterUnlocked(sm, chapterIndex);

        int unlockedCount = 0;
        if (unlocked && sm != null && totalLevels > 0)
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

        if (hideIfNoChapter) gameObject.SetActive(true);
    }

    int ResolveChapterIndex(int centerModule)
    {
        if (_moduleToButton != null && centerModule >= 0 && centerModule < _moduleToButton.Length)
        {
            var b = _moduleToButton[centerModule];
            if (b) return b.chapterIndex;
        }
        return -1;
    }

    void ClearOrHide()
    {
        if (hideIfNoChapter)
        {
            gameObject.SetActive(false);
            return;
        }

        if (chapterTitleText) chapterTitleText.text = "";
        if (unlockedLevelsText) unlockedLevelsText.text = "";
        if (rewardsText) rewardsText.text = "";
    }

    // Chapter0 always unlocked; chapter i>0 requires previous chapter ALL cleared
    static bool IsChapterUnlocked(SaveManager sm, int ci)
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

    static int GetChapterLevelCount(SaveManager sm, int ci)
    {
        if (sm == null || sm.levelsPerChapter == null) return 0;
        if (ci < 0 || ci >= sm.levelsPerChapter.Length) return 0;
        return Mathf.Max(0, sm.levelsPerChapter[ci]);
    }

    // 0 -> "N"
    static string ToRomanOrN(int value)
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
}