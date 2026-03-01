using UnityEngine;
using UnityEngine.UI;
using SKCell;

[DisallowMultipleComponent]
public class StartUIChapterButton : MonoBehaviour
{
    [Header("Chapter")]
    [Tooltip("0..3")]
    public int chapterIndex = 0;

    [Header("Scene (SceneTitle)")]
    public SceneTitle chapterWorldScene;

    [Header("Override Lock (Inspector)")]
    [Tooltip("强制锁：无视存档逻辑，永远显示锁；Center 点击也不会进入（适合 Chapter2/3 还没做完）")]
    public bool forceLock = false;

    [Header("Lock Visual (per module)")]
    public GameObject lockVisual;

    [Header("Runtime")]
    public bool isLocked = false;

    void Awake()
    {
        if (!lockVisual) lockVisual = AutoFindLockVisual();
        if (lockVisual) ConfigureLockVisualNoRaycast(lockVisual);
    }

    void Start() => RefreshUI();
    void OnEnable() => RefreshUI();

    public void RefreshUI()
    {
        var sm = SaveManager.Instance;

        bool unlockedByProgress = IsChapterUnlocked(sm, chapterIndex);
        isLocked = forceLock || !unlockedByProgress;

        if (lockVisual) lockVisual.SetActive(isLocked);
    }

    /// <summary>
    /// 只在“中心被点击”时由 MenuMenuScrollUI 调用
    /// </summary>
    public void TryEnter(MenuController menuController)
    {
        RefreshUI();
        if (isLocked) return;
        if (!menuController)
        {
            Debug.LogError("[StartUIChapterButton] MenuController is null.");
            return;
        }

        menuController.StartChapterWorld(chapterWorldScene);
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

    GameObject AutoFindLockVisual()
    {
        var trs = GetComponentsInChildren<Transform>(true);
        foreach (var tr in trs)
        {
            if (!tr) continue;
            if (tr.name.ToLowerInvariant().Contains("lock"))
                return tr.gameObject;
        }
        return null;
    }

    void ConfigureLockVisualNoRaycast(GameObject go)
    {
        // 锁图标不吃点击，避免挡住按钮
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        var graphics = go.GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
            g.raycastTarget = false;
    }
}