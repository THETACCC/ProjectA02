using UnityEngine;
using System.Collections.Generic;

public class LevelUIManager : MonoBehaviour
{
    [Header("Assign 5 SLOT parents (left->right)")]
    public GameObject[] rewardGetOBJ;  // 槽位父物体
    public RewardManager rewardManager;

    [SerializeField] private string rewardGetChildName = "Reward_Get";

    private GameObject[] _rewardGetChild;
    private int lastCount = -1;
    private int lastTotal = -1;
    private bool _cachedChildren = false;

    void Start()
    {
        TryFindRewardManager();
        CacheRewardGetChildren();

        // ✅ 一开始先不显示（等 RewardManager 初始化后再决定显示几个）
        SetAllSlotsActive(false);
        SetAllRewardGetActive(false);
    }

    void Update()
    {
        if (rewardManager == null)
        {
            TryFindRewardManager();
            return;
        }

        // ✅ 关键：区分 “还没初始化” vs “真的 0 个”
        bool initialized = rewardManager.RewardObjects != null; // Search 过一次就不会是 null（即使是 0 个）

        if (!initialized)
        {
            // 还没初始化出来：先别显示任何 reward UI
            SetAllSlotsActive(false);
            SetAllRewardGetActive(false);
            lastCount = -1; lastTotal = -1;
            return;
        }

        int total = GetTotalRewardsInLevel();              // 现在 total=0 就是真 0
        int count = rewardManager.rewardsReachedCount;

        total = Mathf.Clamp(total, 0, rewardGetOBJ.Length);
        count = Mathf.Clamp(count, 0, total);

        // ✅ 真 0：完全不显示 reward
        if (total == 0)
        {
            SetAllSlotsActive(false);
            SetAllRewardGetActive(false);
            lastCount = -1; lastTotal = 0;
            return;
        }

        if (total != lastTotal || count != lastCount)
        {
            ApplyLeftAligned(total, count);
            lastTotal = total;
            lastCount = count;
        }
    }

    private void ApplyLeftAligned(int total, int count)
    {
        int slots = rewardGetOBJ.Length;

        // 1) 左对齐显示前 total 个槽位，尾巴隐藏
        for (int i = 0; i < slots; i++)
        {
            bool slotActive = (i < total);
            if (rewardGetOBJ[i] != null) rewardGetOBJ[i].SetActive(slotActive);
        }

        // 2) 在可见槽位中，点亮前 count 个 Reward_Get
        for (int i = 0; i < slots; i++)
        {
            if (_rewardGetChild[i] == null) continue;

            if (i >= total) { _rewardGetChild[i].SetActive(false); continue; }
            _rewardGetChild[i].SetActive(i < count);
        }
    }

    // -------- helpers --------

    private void TryFindRewardManager()
    {
        if (rewardManager != null) return;
        var go = GameObject.FindGameObjectWithTag("RewardManager");
        if (go != null) rewardManager = go.GetComponent<RewardManager>();
    }

    private int GetTotalRewardsInLevel()
    {
        if (rewardManager == null) return 0;
        if (rewardManager.totalRewardsInLevel >= 0) return rewardManager.totalRewardsInLevel; // 0 也要返回
        if (rewardManager.RewardObjects != null) return rewardManager.RewardObjects.Length;
        return 0;
    }

    private void CacheRewardGetChildren()
    {
        if (_cachedChildren) return;
        _cachedChildren = true;

        _rewardGetChild = new GameObject[rewardGetOBJ.Length];
        for (int i = 0; i < rewardGetOBJ.Length; i++)
        {
            var slot = rewardGetOBJ[i];
            if (slot == null) continue;

            var t = FindChildRecursiveIgnoreCase(slot.transform, rewardGetChildName);
            if (t == null) t = FindChildRecursiveIgnoreCase(slot.transform, "reward_Get");

            if (t == null)
            {
                Debug.LogWarning($"[LevelUIManager] Slot '{slot.name}' missing child Reward_Get / reward_Get");
                continue;
            }

            _rewardGetChild[i] = t.gameObject;
        }
    }

    private static Transform FindChildRecursiveIgnoreCase(Transform root, string name)
    {
        var stack = new Stack<Transform>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            if (string.Equals(t.name, name, System.StringComparison.OrdinalIgnoreCase))
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