using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMenuScrollUI : MonoBehaviour
{
    [Header("Anchors (can be moving children; we SNAPSHOT positions at start)")]
    public Transform[] anchors;          // 你现在拖的child也行（会动没事）
    public Transform[] moduleRoots;      // 你要移动的 empty parent（pivot）

    [Header("Optional visuals (actual visible child)")]
    [Tooltip("不填则自动找：优先Graphic(UI)，否则第一个child，否则root本身。")]
    public Transform[] moduleVisuals;

    [Header("Space (coordinate root)")]
    [Tooltip("建议填：anchors/modules都在同一个Canvas/父物体下。为空用this.transform。")]
    public Transform space;

    [Header("Top/Center/Bottom are defined by SLOT index")]
    public int topSlotIndex = 7;
    public int centerSlotIndex = 0;  // 这就是“左边中间位置”的slot
    public int bottomSlotIndex = 1;

    [Header("Move")]
    public float lerpDuration = 0.4f;

    [Header("Input")]
    public bool listenMouseScroll = true;
    public float scrollThreshold = 0.01f;
    public bool listenArrowKeys = true;
    public KeyCode clockwiseKey = KeyCode.UpArrow;            // 顺时针
    public KeyCode counterClockwiseKey = KeyCode.DownArrow;   // 逆时针
    public bool invertClockwise = false;

    [Header("Rendering Range (fix flicker)")]
    [Tooltip("完全可见半径：1=3个(上中下), 2=5个, 3=7个")]
    public int fullyVisibleRadius = 2;    // 推荐 2（5个）
    [Tooltip("预渲染半径：比fullyVisibleRadius大1即可避免闪烁（例如3=7个）")]
    public int preRenderRadius = 3;       // 推荐 3（7个）
    [Range(0f, 1f)]
    public float preRenderMinAlpha = 0.12f;

    [Header("Interaction")]
    [Tooltip("只允许上/中/下可以点，其它即使可见也不拦截Raycast，避免点歪。")]
    public bool onlyTopCenterBottomInteractable = true;

    [Header("Center Scaling (scale children, not root)")]
    [Tooltip("中心位(左边中间)的模块：其 parent 的所有直接 child 放大到原始的这个倍数。")]
    public float centerChildrenScaleMultiplier = 1.33f;

    [Header("Init mapping")]
    [Tooltip("不要求你moduleRoots顺序正确：用“唯一最近slot匹配”自动建立初始映射。建议开。")]
    public bool autoUniqueNearestMapping = true;

    public int TopModuleIndex { get; private set; } = -1;
    public int CenterModuleIndex { get; private set; } = -1;
    public int BottomModuleIndex { get; private set; } = -1;

    // ===== internal =====
    Vector3[] _slotLocalPos;        // 记录下来的固定slot（space local）
    Vector3[] _visualOffsetLocal;   // visualLocal - rootLocal
    int[] _baseSlotIndex;           // module i 初始属于哪个slot
    CanvasGroup[] _cg;

    // child scale cache (direct children)
    Transform[][] _scaleChildren;
    Vector3[][] _childOrigScales;

    int _offsetSteps;
    bool _animating;
    int _queuedSteps;

    IEnumerator Start()
    {
        if (space == null) space = transform;

        if (anchors == null || moduleRoots == null || anchors.Length < 3 || anchors.Length != moduleRoots.Length)
        {
            Debug.LogError("[MenuMenuScrollUI] anchors and moduleRoots must exist and have same length (>=3).");
            yield break;
        }

        int n = anchors.Length;

        fullyVisibleRadius = Mathf.Clamp(fullyVisibleRadius, 0, n / 2);
        preRenderRadius = Mathf.Clamp(preRenderRadius, fullyVisibleRadius, n / 2);

        if (moduleVisuals == null || moduleVisuals.Length != n)
            moduleVisuals = new Transform[n];

        _slotLocalPos = new Vector3[n];
        _visualOffsetLocal = new Vector3[n];
        _baseSlotIndex = new int[n];
        _cg = new CanvasGroup[n];

        _scaleChildren = new Transform[n][];
        _childOrigScales = new Vector3[n][];

        // 等UI布局稳定（Canvas/LayoutGroup 常在第一帧后才就位）
        yield return null;
        yield return new WaitForEndOfFrame();

        SnapshotSlots(); // 记录slots（之后anchors动不动都无所谓）

        for (int i = 0; i < n; i++)
        {
            var root = moduleRoots[i];
            if (!root)
            {
                Debug.LogError($"[MenuMenuScrollUI] moduleRoots[{i}] null");
                yield break;
            }

            if (!moduleVisuals[i])
                moduleVisuals[i] = AutoPickVisual(root);

            Vector3 rootLocal = space.InverseTransformPoint(root.position);
            Vector3 visLocal = space.InverseTransformPoint(moduleVisuals[i].position);
            _visualOffsetLocal[i] = visLocal - rootLocal;

            var cg = root.GetComponent<CanvasGroup>();
            if (!cg) cg = root.gameObject.AddComponent<CanvasGroup>();
            _cg[i] = cg;

            // cache direct children scales (so we can restore)
            CacheChildrenScales(i);

            var btn = root.GetComponentInChildren<Button>(true);
            if (btn)
            {
                int cap = i;
                btn.onClick.AddListener(() => OnModuleClicked(cap));
            }
        }

        if (autoUniqueNearestMapping) BuildUniqueNearestMapping();
        else for (int i = 0; i < n; i++) _baseSlotIndex[i] = i;

        SnapAllToSlots();
        RefreshTopCenterBottom();
        ApplyVisibilityAndScaleImmediate(); // 初始一次
    }

    void Update()
    {
        if (_slotLocalPos == null) return;

        if (listenMouseScroll)
        {
            float s = Input.mouseScrollDelta.y;
            if (s > scrollThreshold) StepClockwise();
            else if (s < -scrollThreshold) StepCounterClockwise();
        }

        if (listenArrowKeys)
        {
            if (Input.GetKeyDown(clockwiseKey)) StepClockwise();
            if (Input.GetKeyDown(counterClockwiseKey)) StepCounterClockwise();
        }
    }

    // ===== public =====
    public void StepClockwise() => RequestStep(invertClockwise ? -1 : +1);
    public void StepCounterClockwise() => RequestStep(invertClockwise ? +1 : -1);

    public void OnModuleClicked(int moduleIndex)
    {
        if (_animating) return;
        if (moduleIndex == TopModuleIndex) StepCounterClockwise();
        else if (moduleIndex == BottomModuleIndex) StepClockwise();
        else { /* Center clicked -> Start/Confirm */ }
    }

    // anchor会动：这里“记录下来”
    public void SnapshotSlots()
    {
        int n = anchors.Length;
        for (int i = 0; i < n; i++)
            _slotLocalPos[i] = space.InverseTransformPoint(anchors[i].position);
    }

    // ===== core move =====
    void RequestStep(int dirClockwise)
    {
        if (_animating)
        {
            _queuedSteps = Mathf.Clamp(_queuedSteps + dirClockwise, -5, 5);
            return;
        }

        int prevOffset = _offsetSteps;
        _offsetSteps = Mod(_offsetSteps + dirClockwise, _slotLocalPos.Length);
        StartCoroutine(CoMoveAll(prevOffset, _offsetSteps));
    }

    IEnumerator CoMoveAll(int prevOffset, int newOffset)
    {
        _animating = true;

        int n = moduleRoots.Length;

        Vector3[] startPos = new Vector3[n];
        Vector3[] targetPos = new Vector3[n];

        float[] startA = new float[n];
        float[] targetA = new float[n];

        float[] startMul = new float[n];
        float[] targetMul = new float[n];

        bool[] targetInteract = new bool[n];

        // 先算目标（位置 + alpha + scaleMul），然后一起lerp，避免“闪一下/跳一下”
        for (int i = 0; i < n; i++)
        {
            startPos[i] = moduleRoots[i].position;

            int slotNew = SlotForModuleWithOffset(i, newOffset);
            Vector3 slotLocal = _slotLocalPos[slotNew];

            Vector3 rootTargetLocal = slotLocal - _visualOffsetLocal[i];
            targetPos[i] = space.TransformPoint(rootTargetLocal);

            startA[i] = _cg[i].alpha;
            int distNew = CircularDistance(slotNew, centerSlotIndex, n);
            targetA[i] = ComputeAlpha(distNew);

            int slotPrev = SlotForModuleWithOffset(i, prevOffset);
            startMul[i] = (slotPrev == centerSlotIndex) ? centerChildrenScaleMultiplier : 1f;
            targetMul[i] = (slotNew == centerSlotIndex) ? centerChildrenScaleMultiplier : 1f;

            targetInteract[i] = !onlyTopCenterBottomInteractable
                ? (distNew <= fullyVisibleRadius)
                : (slotNew == topSlotIndex || slotNew == centerSlotIndex || slotNew == bottomSlotIndex);
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, lerpDuration);
            float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            for (int i = 0; i < n; i++)
            {
                moduleRoots[i].position = Vector3.Lerp(startPos[i], targetPos[i], e);
                _cg[i].alpha = Mathf.Lerp(startA[i], targetA[i], e);

                float mul = Mathf.Lerp(startMul[i], targetMul[i], e);
                ApplyChildrenScaleMultiplier(i, mul);
            }

            yield return null;
        }

        for (int i = 0; i < n; i++)
        {
            moduleRoots[i].position = targetPos[i];
            _cg[i].alpha = targetA[i];

            _cg[i].blocksRaycasts = targetInteract[i];
            _cg[i].interactable = targetInteract[i];

            ApplyChildrenScaleMultiplier(i, targetMul[i]); // snap
        }

        RefreshTopCenterBottom();

        _animating = false;

        if (_queuedSteps != 0)
        {
            int next = _queuedSteps > 0 ? 1 : -1;
            _queuedSteps -= next;
            RequestStep(next);
        }
    }

    float ComputeAlpha(int dist)
    {
        if (dist <= fullyVisibleRadius) return 1f;
        if (dist > preRenderRadius) return 0f;

        int span = Mathf.Max(1, preRenderRadius - fullyVisibleRadius);
        float u = (dist - fullyVisibleRadius) / (float)span;
        return Mathf.Lerp(1f, preRenderMinAlpha, u);
    }

    // ===== mapping =====
    int CurrentSlotForModule(int moduleIndex)
        => SlotForModuleWithOffset(moduleIndex, _offsetSteps);

    int SlotForModuleWithOffset(int moduleIndex, int offset)
        => Mod(_baseSlotIndex[moduleIndex] + offset, _slotLocalPos.Length);

    void RefreshTopCenterBottom()
    {
        TopModuleIndex = FindModuleAtSlot(topSlotIndex);
        CenterModuleIndex = FindModuleAtSlot(centerSlotIndex);
        BottomModuleIndex = FindModuleAtSlot(bottomSlotIndex);
    }

    int FindModuleAtSlot(int slotIndex)
    {
        for (int i = 0; i < moduleRoots.Length; i++)
            if (CurrentSlotForModule(i) == slotIndex) return i;
        return -1;
    }

    void BuildUniqueNearestMapping()
    {
        int n = moduleRoots.Length;
        var pairs = new List<(float d, int m, int s)>(n * n);

        for (int m = 0; m < n; m++)
        {
            Vector3 vLocal = space.InverseTransformPoint(moduleVisuals[m] ? moduleVisuals[m].position : moduleRoots[m].position);
            for (int s = 0; s < n; s++)
            {
                float d = (vLocal - _slotLocalPos[s]).sqrMagnitude;
                pairs.Add((d, m, s));
            }
        }

        pairs.Sort((a, b) => a.d.CompareTo(b.d));

        bool[] usedM = new bool[n];
        bool[] usedS = new bool[n];

        for (int i = 0; i < pairs.Count; i++)
        {
            var p = pairs[i];
            if (usedM[p.m] || usedS[p.s]) continue;
            usedM[p.m] = true;
            usedS[p.s] = true;
            _baseSlotIndex[p.m] = p.s;
        }
    }

    void SnapAllToSlots()
    {
        for (int i = 0; i < moduleRoots.Length; i++)
        {
            int slot = CurrentSlotForModule(i);
            Vector3 rootTargetLocal = _slotLocalPos[slot] - _visualOffsetLocal[i];
            moduleRoots[i].position = space.TransformPoint(rootTargetLocal);
        }
    }

    void ApplyVisibilityAndScaleImmediate()
    {
        int n = moduleRoots.Length;
        for (int i = 0; i < n; i++)
        {
            int slot = CurrentSlotForModule(i);
            int dist = CircularDistance(slot, centerSlotIndex, n);

            float a = ComputeAlpha(dist);
            _cg[i].alpha = a;

            bool interact = !onlyTopCenterBottomInteractable
                ? (dist <= fullyVisibleRadius)
                : (slot == topSlotIndex || slot == centerSlotIndex || slot == bottomSlotIndex);

            _cg[i].blocksRaycasts = interact;
            _cg[i].interactable = interact;

            float mul = (slot == centerSlotIndex) ? centerChildrenScaleMultiplier : 1f;
            ApplyChildrenScaleMultiplier(i, mul);
        }
    }

    // ===== child scaling cache/apply =====
    void CacheChildrenScales(int moduleIndex)
    {
        Transform root = moduleRoots[moduleIndex];
        int c = root.childCount;

        // 直接child（不会出现层级重复乘）
        var children = new Transform[c];
        var scales = new Vector3[c];
        for (int i = 0; i < c; i++)
        {
            children[i] = root.GetChild(i);
            scales[i] = children[i].localScale;
        }
        _scaleChildren[moduleIndex] = children;
        _childOrigScales[moduleIndex] = scales;
    }

    void ApplyChildrenScaleMultiplier(int moduleIndex, float mul)
    {
        var children = _scaleChildren[moduleIndex];
        var orig = _childOrigScales[moduleIndex];
        if (children == null || orig == null) return;

        for (int i = 0; i < children.Length; i++)
        {
            if (!children[i]) continue;
            children[i].localScale = orig[i] * mul;
        }
    }

    // ===== util =====
    static int Mod(int a, int n) => (a % n + n) % n;

    static int CircularDistance(int a, int b, int n)
    {
        int d = Mod(a - b, n);
        return Mathf.Min(d, n - d);
    }

    static Transform AutoPickVisual(Transform root)
    {
        var g = root.GetComponentInChildren<Graphic>(true);
        if (g) return g.transform;
        if (root.childCount > 0) return root.GetChild(0);
        return root;
    }
}