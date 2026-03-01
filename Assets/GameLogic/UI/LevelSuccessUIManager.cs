using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSuccessUIManager : MonoBehaviour
{
    [Header("Assign 5 reward SLOT parent objects here (not the child)")]
    public GameObject[] rewardGetOBJ;   // 5 个槽位父物体（从左到右）
    public RewardManager rewardManager;

    [Header("Child name to enable when collected")]
    [SerializeField] private string rewardGetChildName = "Reward_Get"; // 兼容 reward_Get：忽略大小写匹配

    [Header("Sequence Animation")]
    [Tooltip("每个 Reward_Get 之间的间隔（如果不使用动画时长，就用这个）")]
    [SerializeField] private float revealInterval = 0.25f;

    [Tooltip("尽量用 Animator/Animation 的 clip 时长当作等待时间（拿不到就回退到 revealInterval）")]
    [SerializeField] private bool waitByClipLength = true;

    [Tooltip("如果用 clip 时长，额外再加一点间隔（更像一个接一个）")]
    [SerializeField] private float extraGap = 0.05f;

    private GameObject[] _rewardGetChild; // 缓存每个槽位的 Reward_Get child（GameObject）

    public bool isLevelPass = false;

    private MenuController _menuController;

    private bool _sequenceStarted = false;
    private Coroutine _sequenceCo;

    void Start()
    {
        TryFindRewardManager();
        CacheRewardGetChildren();

        // ✅ 不要在 Start 把父槽位全关掉（容易影响你原 UI 动画/入场）
        // 只先把 Reward_Get 关掉，等结算时再依次打开播放
        SetAllRewardGetActive(false);

        // MenuController 缓存（保留你的逻辑）
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
        if (!isLevelPass)
        {
            // 如果你会把 isLevelPass 关回 false（比如退出/重进），允许下次再播
            if (_sequenceStarted)
            {
                _sequenceStarted = false;
                if (_sequenceCo != null) StopCoroutine(_sequenceCo);
                _sequenceCo = null;
            }
            return;
        }

        if (rewardManager == null)
        {
            TryFindRewardManager();
            return;
        }

        int total = GetTotalRewardsInLevel();
        if (total <= 0) return;

        int count = Mathf.Clamp(rewardManager.rewardsReachedCount, 0, total);

        // ✅ 只启动一次序列播放
        if (!_sequenceStarted)
        {
            _sequenceStarted = true;
            _sequenceCo = StartCoroutine(CoPlayRewardsLeftToRight(total, count));
        }
    }

    private IEnumerator CoPlayRewardsLeftToRight(int total, int count)
    {
        int slots = rewardGetOBJ.Length;
        total = Mathf.Clamp(total, 0, slots);

        // ✅ 居中：total=3, slots=5 => start=1（只显示中间三个槽位父物体）
        int start = Mathf.FloorToInt((slots - total) * 0.5f);
        start = Mathf.Clamp(start, 0, slots - total);

        // 1) 决定哪些槽位父物体显示/隐藏（头尾消失）
        for (int i = 0; i < slots; i++)
        {
            bool slotActive = (i >= start && i < start + total);
            if (rewardGetOBJ[i] != null) rewardGetOBJ[i].SetActive(slotActive);
        }

        // 2) 可见槽位内：先全部关掉 Reward_Get（防止之前残留）
        for (int i = start; i < start + total; i++)
        {
            if (_rewardGetChild[i] != null) _rewardGetChild[i].SetActive(false);
        }

        // （可选）等一帧，让 UI 入场动画先落地
        yield return null;

        // 3) 从左到右依次播放：只播 count 个
        for (int local = 0; local < count; local++)
        {
            int idx = start + local; // 左到右
            var go = _rewardGetChild[idx];
            if (go == null) continue;

            // 打开 + 重播动画
            PlayGetAnim(go);

            // 等待：优先用 clip 时长，否则 revealInterval
            float wait = revealInterval;
            if (waitByClipLength)
            {
                float len = GetAnimLength(go);
                if (len > 0.01f) wait = len;
            }

            yield return new WaitForSeconds(wait + extraGap);
        }
    }

    private void PlayGetAnim(GameObject go)
    {
        if (!go) return;

        // 先确保激活
        if (!go.activeSelf) go.SetActive(true);

        // Animator（常见）
        var anim = go.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
            anim.Play(0, 0, 0f);
            anim.Update(0f);
            return;
        }

        // Legacy Animation（如果你用的是 Animation 组件）
        var legacy = go.GetComponent<Animation>();
        if (legacy != null)
        {
            legacy.Stop();
            legacy.Play();
        }
    }

    private float GetAnimLength(GameObject go)
    {
        // Animator clip length（尽量拿当前 state 的 length）
        var anim = go.GetComponent<Animator>();
        if (anim != null)
        {
            var st = anim.GetCurrentAnimatorStateInfo(0);
            if (st.length > 0.01f) return st.length;
        }

        // Legacy Animation clip length
        var legacy = go.GetComponent<Animation>();
        if (legacy != null && legacy.clip != null)
        {
            return legacy.clip.length;
        }

        return 0f;
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

        var stack = new Stack<Transform>();
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

    private void SetAllRewardGetActive(bool on)
    {
        if (_rewardGetChild == null) return;
        for (int i = 0; i < _rewardGetChild.Length; i++)
            if (_rewardGetChild[i] != null) _rewardGetChild[i].SetActive(on);
    }
}