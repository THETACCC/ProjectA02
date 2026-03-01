using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMenuScrollUI : MonoBehaviour
{
    [Header("Anchors (can be moving children; used only for INITIAL snapshot)")]
    public Transform[] anchors;          // OK if moving; we only use it at startup
    public Transform[] moduleRoots;      // empty parents (pivots) you actually move

    [Header("Optional visuals (actual visible child)")]
    [Tooltip("If not assigned: auto pick Graphic(UI) > first child > root.")]
    public Transform[] moduleVisuals;

    [Header("Space (coordinate root)")]
    [Tooltip("Recommended: anchors/modules under the same canvas/root. If null uses this.transform.")]
    public Transform space;

    [Header("Top/Center/Bottom are defined by SLOT index")]
    public int topSlotIndex = 7;
    public int centerSlotIndex = 0;  // the left-middle focus slot
    public int bottomSlotIndex = 1;

    [Header("Move")]
    public float lerpDuration = 0.4f;

    [Header("Input")]
    public bool listenMouseScroll = true;
    public float scrollThreshold = 0.01f;
    public bool listenArrowKeys = true;
    public KeyCode clockwiseKey = KeyCode.DownArrow;             // clockwise
    public KeyCode counterClockwiseKey = KeyCode.UpArrow;    // counter-clockwise
    public bool invertClockwise = false;

    [Header("Rendering Range (fix flicker)")]
    [Tooltip("Fully visible radius: 1=3 (top/center/bottom), 2=5, 3=7")]
    public int fullyVisibleRadius = 2;     // recommended 2 (5 visible)
    [Tooltip("Pre-render radius: usually fullyVisibleRadius+1 to prevent flicker (e.g. 3 => 7 items)")]
    public int preRenderRadius = 3;        // recommended 3 (7 pre-render)

    [Header("Interaction")]
    [Tooltip("Only Top/Center/Bottom are interactable; others (even if visible) won't block raycasts.")]
    public bool onlyTopCenterBottomInteractable = true;

    [Header("Center Scaling (scale children, not root)")]
    [Tooltip("When a module is in center slot, scale ALL direct children of its root by this multiplier.")]
    public float centerChildrenScaleMultiplier = 1.33f;

    [Header("Init mapping")]
    [Tooltip("If true, modules don't need to be ordered: we build a unique nearest-slot mapping at runtime.")]
    public bool autoUniqueNearestMapping = true;

    [Header("Auto Refresh On Screen/Canvas Size Change")]
    [Tooltip("On resize we DO NOT rebuild slot order and DO NOT reset positions. We re-capture slots from current module visuals.")]
    public bool autoRefreshOnResize = true;
    public float resizeDebounceSeconds = 0.05f;

    [Header("Chapter Enter (optional)")]
    public MenuController menuController;

    public int TopModuleIndex { get; private set; } = -1;
    public int CenterModuleIndex { get; private set; } = -1;
    public int BottomModuleIndex { get; private set; } = -1;

    // ===== internal =====
    Vector3[] _slotLocalPos;        // slots in space local
    Vector3[] _visualOffsetLocal;   // visualLocal - rootLocal
    int[] _baseSlotIndex;           // module i initially belongs to which slot
    CanvasGroup[] _cg;

    Transform[][] _scaleChildren;   // direct children to scale (per module)
    Vector3[][] _childOrigScales;   // cached original local scales

    int _offsetSteps;
    bool _animating;
    int _queuedSteps;

    int _lastScreenW, _lastScreenH;
    Coroutine _refreshCo;
    Coroutine _moveCo;

    IEnumerator Start()
    {
        if (space == null) space = transform;

        if (moduleRoots == null || moduleRoots.Length < 3)
        {
            Debug.LogError("[MenuMenuScrollUI] moduleRoots must exist and have length >= 3.");
            yield break;
        }

        int n = moduleRoots.Length;

        // anchors must match length if you want initial snapshot from anchors
        bool hasAnchors = (anchors != null && anchors.Length == n);

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

        // Let UI/layout settle
        yield return null;
        yield return new WaitForEndOfFrame();

        // 1) INITIAL slots:
        // - if anchors provided: snapshot anchors (even if moving) ONCE
        // - otherwise: snapshot from current modules (keeps your manual layout)
        if (hasAnchors) SnapshotSlotsFromAnchors();
        else SnapshotSlotsFromCurrentModules();

        // 2) setup visuals, offsets, CanvasGroups, scale caches, click hooks
        for (int i = 0; i < n; i++)
        {
            var root = moduleRoots[i];
            if (!root)
            {
                Debug.LogError($"[MenuMenuScrollUI] moduleRoots[{i}] is null");
                yield break;
            }

            if (!moduleVisuals[i])
                moduleVisuals[i] = AutoPickVisual(root);

            RecomputeVisualOffset(i);

            var cg = root.GetComponent<CanvasGroup>();
            if (!cg) cg = root.gameObject.AddComponent<CanvasGroup>();
            _cg[i] = cg;
            _cg[i].alpha = 1f;

            CacheChildrenScales(i);

            var btn = root.GetComponentInChildren<Button>(true);
            if (btn)
            {
                int cap = i;
                btn.onClick.AddListener(() => OnModuleClicked(cap));
            }
        }

        // 3) build base mapping
        if (autoUniqueNearestMapping) BuildUniqueNearestMapping();
        else for (int i = 0; i < n; i++) _baseSlotIndex[i] = i;

        // 4) snap once
        SnapAllToSlots();
        RefreshTopCenterBottom();
        ApplyVisibilityAndScaleImmediate();

        _lastScreenW = Screen.width;
        _lastScreenH = Screen.height;
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

        if (autoRefreshOnResize)
        {
            if (Screen.width != _lastScreenW || Screen.height != _lastScreenH)
            {
                _lastScreenW = Screen.width;
                _lastScreenH = Screen.height;
                MarkLayoutDirty();
            }
        }
    }

    // Works when this is on a RectTransform; harmless otherwise
    void OnRectTransformDimensionsChange()
    {
        if (autoRefreshOnResize)
            MarkLayoutDirty();
    }

    // ===== public =====
    public void StepClockwise() => RequestStep(invertClockwise ? -1 : +1);
    public void StepCounterClockwise() => RequestStep(invertClockwise ? +1 : -1);

    public void OnModuleClicked(int moduleIndex)
    {
        if (_animating) return;

        if (moduleIndex == TopModuleIndex) StepClockwise();
        else if (moduleIndex == BottomModuleIndex) StepCounterClockwise();
        else
        {
            // Center clicked -> Try enter chapter
            if (!menuController) menuController = FindObjectOfType<MenuController>(true);

            var chapterBtn = moduleRoots[moduleIndex].GetComponentInChildren<StartUIChapterButton>(true);
            if (chapterBtn) chapterBtn.TryEnter(menuController);
        }
    }

    // ===== resize refresh (NO RESET / NO REORDER) =====
    void MarkLayoutDirty()
    {
        if (!isActiveAndEnabled) return;

        if (_refreshCo != null) StopCoroutine(_refreshCo);
        _refreshCo = StartCoroutine(CoRefreshAfterLayout_NoReset());
    }

    IEnumerator CoRefreshAfterLayout_NoReset()
    {
        if (resizeDebounceSeconds > 0f)
            yield return new WaitForSeconds(resizeDebounceSeconds);

        // Stop movement animation to avoid fighting with refresh (but we do NOT change offset/order)
        if (_moveCo != null)
        {
            StopCoroutine(_moveCo);
            _moveCo = null;
            _animating = false;
            _queuedSteps = 0;
        }

        // Let UI/layout settle
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        // Recompute offsets (layout changes can move visuals within roots)
        RecomputeAllVisualOffsets();

        // IMPORTANT: do NOT snapshot from anchors on resize (anchors are moving).
        // Instead, capture slots FROM CURRENT modules, preserving current order & positions.
        SnapshotSlotsFromCurrentModules();

        // Snap to those captured slots (this should produce ~no movement)
        SnapAllToSlots();
        RefreshTopCenterBottom();
        ApplyVisibilityAndScaleImmediate();
    }

    // ===== slots snapshot =====
    void SnapshotSlotsFromAnchors()
    {
        int n = anchors.Length;
        for (int i = 0; i < n; i++)
            _slotLocalPos[i] = space.InverseTransformPoint(anchors[i].position);
    }

    // Capture slots using CURRENT module visuals, keeping the current arrangement.
    // Each slot index gets the visual position of the module currently assigned to that slot.
    void SnapshotSlotsFromCurrentModules()
    {
        int n = moduleRoots.Length;

        for (int i = 0; i < n; i++)
        {
            if (!moduleVisuals[i])
                moduleVisuals[i] = AutoPickVisual(moduleRoots[i]);

            int slot = CurrentSlotForModule(i);
            Vector3 vLocal = space.InverseTransformPoint(moduleVisuals[i].position);
            _slotLocalPos[slot] = vLocal;
        }
    }

    // ===== offsets =====
    void RecomputeAllVisualOffsets()
    {
        for (int i = 0; i < moduleRoots.Length; i++)
        {
            if (!moduleVisuals[i])
                moduleVisuals[i] = AutoPickVisual(moduleRoots[i]);
            RecomputeVisualOffset(i);
        }
    }

    void RecomputeVisualOffset(int i)
    {
        Vector3 rootLocal = space.InverseTransformPoint(moduleRoots[i].position);
        Vector3 visLocal = space.InverseTransformPoint(moduleVisuals[i].position);
        _visualOffsetLocal[i] = visLocal - rootLocal;
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

        _moveCo = StartCoroutine(CoMoveAll(prevOffset, _offsetSteps));
    }

    IEnumerator CoMoveAll(int prevOffset, int newOffset)
    {
        _animating = true;

        int n = moduleRoots.Length;

        Vector3[] startPos = new Vector3[n];
        Vector3[] targetPos = new Vector3[n];

        float[] startMul = new float[n];
        float[] targetMul = new float[n];

        bool[] targetInteract = new bool[n];

        bool[] startVisible = new bool[n];
        bool[] targetVisible = new bool[n];

        for (int i = 0; i < n; i++)
        {
            startPos[i] = moduleRoots[i].position;

            int slotNew = SlotForModuleWithOffset(i, newOffset);
            Vector3 slotLocal = _slotLocalPos[slotNew];

            Vector3 rootTargetLocal = slotLocal - _visualOffsetLocal[i];
            targetPos[i] = space.TransformPoint(rootTargetLocal);

            int slotPrev = SlotForModuleWithOffset(i, prevOffset);
            startMul[i] = (slotPrev == centerSlotIndex) ? centerChildrenScaleMultiplier : 1f;
            targetMul[i] = (slotNew == centerSlotIndex) ? centerChildrenScaleMultiplier : 1f;

            int distPrev = CircularDistance(slotPrev, centerSlotIndex, n);
            int distNew = CircularDistance(slotNew, centerSlotIndex, n);

            startVisible[i] = ComputeVisible(distPrev);
            targetVisible[i] = ComputeVisible(distNew);

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
                if (!moduleRoots[i].gameObject.activeSelf) continue;

                moduleRoots[i].position = Vector3.Lerp(startPos[i], targetPos[i], e);

                float mul = Mathf.Lerp(startMul[i], targetMul[i], e);
                ApplyChildrenScaleMultiplier(i, mul);
            }

            yield return null;
        }

        for (int i = 0; i < n; i++)
        {
            if (moduleRoots[i].gameObject.activeSelf)
                moduleRoots[i].position = targetPos[i];

            _cg[i].blocksRaycasts = targetInteract[i];
            _cg[i].interactable = targetInteract[i];

            ApplyChildrenScaleMultiplier(i, targetMul[i]);
        }

        RefreshTopCenterBottom();

        _animating = false;
        _moveCo = null;

        if (_queuedSteps != 0)
        {
            int next = _queuedSteps > 0 ? 1 : -1;
            _queuedSteps -= next;
            RequestStep(next);
        }
    }
    bool ComputeVisible(int dist) => true;

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

            bool visible = ComputeVisible(dist);

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