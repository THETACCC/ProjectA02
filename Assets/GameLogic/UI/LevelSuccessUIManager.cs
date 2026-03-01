using UnityEngine;

public class LevelSuccessUIManager : MonoBehaviour
{
    [Header("Assign 5 reward SLOT parent objects here (not the child)")]
    public GameObject[] rewardGetOBJ;   // 5 个槽位父物体（从左到右）
    public RewardManager rewardManager;

    [Header("Child name to enable when collected")]
    [SerializeField] private string rewardGetChildName = "Reward_Get"; // 兼容 reward_Get：下面会忽略大小写匹配

    private GameObject[] _rewardGetChild; // 缓存每个槽位的 Reward_Get child（GameObject）
    private int lastCount = -1;
    private int lastTotal = -1;

    public bool isLevelPass = false;

    private MenuController _menuController;

    void Start()
    {
        TryFindRewardManager();
        CacheRewardGetChildren();

        // 先全隐藏，等 total > 0 & isLevelPass 再显示
        SetAllSlotsActive(false);
        SetAllRewardGetActive(false);

        // 你原来的 MenuController 缓存逻辑保留
        _menuController = FindObjectOfType<MenuController>();
        if (_menuController == null)
        {
            var all = Resources.FindObjectsOfTypeAll<MenuController>();
            foreach (var mc in all)
            {
                if (mc == null) continue;
                if (!mc.gameObject.scene.IsValid()) continue;
                if (!mc.gameObject.scene.isLoaded) continue;
                _menuController = mc;
                break;
            }
        }
    }

    void Update()
    {
        if (!isLevelPass) return;

        if (rewardManager == null)
        {
            TryFindRewardManager();
            return;
        }

        int total = GetTotalRewardsInLevel();
        if (total <= 0)
        {
            // total 还没初始化出来：先不要显示，避免闪
            SetAllSlotsActive(false);
            SetAllRewardGetActive(false);
            lastCount = -1;
            lastTotal = -1;
            return;
        }

        int count = Mathf.Clamp(rewardManager.rewardsReachedCount, 0, total);

        if (count != lastCount || total != lastTotal)
        {
            UpdateRewardUI_SuccessCentered(count, total);
            lastCount = count;
            lastTotal = total;
        }
    }

    private void UpdateRewardUI_SuccessCentered(int count, int total)
    {
        int slots = rewardGetOBJ.Length;
        total = Mathf.Clamp(total, 0, slots);

        // ✅ 居中：total=3, slots=5 => start=1（只显示中间三个槽位父物体）
        int start = Mathf.FloorToInt((slots - total) * 0.5f);
        start = Mathf.Clamp(start, 0, slots - total);

        // 1) 先决定哪些槽位父物体显示/隐藏（头尾消失）
        for (int i = 0; i < slots; i++)
        {
            bool slotActive = (i >= start && i < start + total);
            if (rewardGetOBJ[i] != null) rewardGetOBJ[i].SetActive(slotActive);
        }

        // 2) 在可见槽位内：点亮前 count 个 Reward_Get child
        // （注意：count 是“捡到多少个”，不是具体哪个 reward，所以就是从左到右点亮）
        for (int i = 0; i < slots; i++)
        {
            if (_rewardGetChild[i] == null) continue;

            bool slotActive = (i >= start && i < start + total);
            if (!slotActive)
            {
                _rewardGetChild[i].SetActive(false);
                continue;
            }

            int localIndex = i - start; // 0..total-1
            _rewardGetChild[i].SetActive(localIndex < count);
        }
    }

    // ✅ Replay 按钮
    public void LevelReplay()
    {
        if (_menuController == null)
        {
            _menuController = FindObjectOfType<MenuController>();
            if (_menuController == null)
            {
                var all = Resources.FindObjectsOfTypeAll<MenuController>();
                foreach (var mc in all)
                {
                    if (mc == null) continue;
                    if (!mc.gameObject.scene.IsValid()) continue;
                    if (!mc.gameObject.scene.isLoaded) continue;
                    _menuController = mc;
                    break;
                }
            }
        }

        if (_menuController != null) _menuController.ResetCurrentScene();
        else Debug.LogError("[LevelSuccessUIManager] MenuController not found in the current scene.");
    }

    // ---------------- helpers ----------------

    private void TryFindRewardManager()
    {
        if (rewardManager != null) return;
        var go = GameObject.FindGameObjectWithTag("RewardManager");
        if (go != null) rewardManager = go.GetComponent<RewardManager>();
    }

    private int GetTotalRewardsInLevel()
    {
        if (rewardManager == null) return 0;
        if (rewardManager.totalRewardsInLevel > 0) return rewardManager.totalRewardsInLevel;
        if (rewardManager.RewardObjects != null && rewardManager.RewardObjects.Length > 0) return rewardManager.RewardObjects.Length;
        return 0;
    }

    private void CacheRewardGetChildren()
    {
        _rewardGetChild = new GameObject[rewardGetOBJ.Length];
        for (int i = 0; i < rewardGetOBJ.Length; i++)
        {
            var slot = rewardGetOBJ[i];
            if (slot == null) continue;

            var child = FindChildRecursiveIgnoreCase(slot.transform, rewardGetChildName);
            // 兼容你说的 reward_Get / Reward_Get：忽略大小写即可
            if (child == null)
                child = FindChildRecursiveIgnoreCase(slot.transform, "reward_Get");

            if (child == null)
            {
                Debug.LogWarning($"[LevelSuccessUIManager] Slot '{slot.name}' missing child '{rewardGetChildName}' (case-insensitive).");
                continue;
            }

            _rewardGetChild[i] = child.gameObject;
        }
    }

    private static Transform FindChildRecursiveIgnoreCase(Transform root, string targetName)
    {
        if (root == null || string.IsNullOrEmpty(targetName)) return null;

        var stack = new System.Collections.Generic.Stack<Transform>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var t = stack.Pop();
            if (string.Equals(t.name, targetName, System.StringComparison.OrdinalIgnoreCase))
                return t;

            for (int i = 0; i < t.childCount; i++)
                stack.Push(t.GetChild(i));
        }

        return null;
    }

    private void SetAllSlotsActive(bool on)
    {
        for (int i = 0; i < rewardGetOBJ.Length; i++)
            if (rewardGetOBJ[i] != null) rewardGetOBJ[i].SetActive(on);
    }

    private void SetAllRewardGetActive(bool on)
    {
        if (_rewardGetChild == null) return;
        for (int i = 0; i < _rewardGetChild.Length; i++)
            if (_rewardGetChild[i] != null) _rewardGetChild[i].SetActive(on);
    }
}