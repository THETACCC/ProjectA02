using UnityEngine;

public class LevelUIManager : MonoBehaviour
{
    [Header("Assign 5 reward SLOT parent objects here (not the child)")]
    public GameObject[] rewardGetOBJ;   // 5 个槽位父物体（从左到右）
    public RewardManager rewardManager;

    [Header("Child name to enable when collected")]
    [SerializeField] private string rewardGetChildName = "Reward_Get";

    private GameObject[] _rewardGetChild;
    private int lastCount = -1;
    private int lastTotal = -1;

    void Start()
    {
        TryFindRewardManager();
        CacheRewardGetChildren();

        // 默认先全隐藏，等 total > 0 再按规则显示（避免一开始 total=0 时乱亮）
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

        int total = GetTotalRewardsInLevel();
        if (total <= 0)
        {
            SetAllSlotsActive(false);
            SetAllRewardGetActive(false);
            lastCount = -1;
            lastTotal = -1;
            return;
        }

        int count = Mathf.Clamp(rewardManager.rewardsReachedCount, 0, total);

        if (count != lastCount || total != lastTotal)
        {
            UpdateRewardUI_LeftAligned(count, total);
            lastCount = count;
            lastTotal = total;
        }
    }

    private void UpdateRewardUI_LeftAligned(int count, int total)
    {
        int slots = rewardGetOBJ.Length;
        total = Mathf.Clamp(total, 0, slots);

        // ✅ 左对齐：只显示最前面的 total 个槽位父物体
        int start = 0;

        // 1) 父物体显示/隐藏（尾巴消失）
        for (int i = 0; i < slots; i++)
        {
            bool slotActive = (i >= start && i < start + total); // i < total
            if (rewardGetOBJ[i] != null) rewardGetOBJ[i].SetActive(slotActive);
        }

        // 2) 点亮前 count 个 Reward_Get child
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
            if (child == null)
                child = FindChildRecursiveIgnoreCase(slot.transform, "reward_Get");

            if (child == null)
            {
                Debug.LogWarning($"[LevelUIManager] Slot '{slot.name}' missing child '{rewardGetChildName}' (case-insensitive).");
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