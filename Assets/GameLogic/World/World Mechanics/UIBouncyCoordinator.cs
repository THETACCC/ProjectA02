using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIBouncyCoordinator : MonoBehaviour
{
    [System.Serializable] public class TimedBouncy { public BouncyUI target; public float delay = 0f; }
    [System.Serializable] public class TimedComplete { public CompleteUI target; public float delay = 0f; }

    [Header("Lists (auto-filled in Awake)")]
    public List<TimedBouncy> bouncies = new List<TimedBouncy>();
    public List<TimedComplete> completes = new List<TimedComplete>();

    [Header("Complete show mode")]
    [SerializeField] private bool completeUseInstantShow = true; // true=直接alpha=1；false=走AnimateShow
    [SerializeField] private float completeExtraDelay = 0.0f;

    [Header("Timing")]
    [SerializeField] private bool autoPlayOnEnable = true;
    [SerializeField] private bool useUnscaledTime = true;

    int remainingToShow = -1;
    bool warmed = false;
    Coroutine running;
    float DT => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    void Awake()
    {
        // —— 自动收集 —— //
        if (bouncies.Count == 0)
        {
            foreach (var b in GetComponentsInChildren<BouncyUI>(true))
                bouncies.Add(new TimedBouncy { target = b, delay = 0f });
            Debug.Log($"[Coordinator] Auto-found BouncyUI: {bouncies.Count}", this);
        }
        if (completes.Count == 0)
        {
            foreach (var c in GetComponentsInChildren<CompleteUI>(true))
                completes.Add(new TimedComplete { target = c, delay = 0f });
            Debug.Log($"[Coordinator] Auto-found CompleteUI: {completes.Count}", this);
        }

        // —— 订阅事件 —— //
        foreach (var tb in bouncies)
        {
            if (tb.target == null) continue;
            tb.target.OnShowFinished -= HandleBouncyShowFinished; // 防重
            tb.target.OnShowFinished += HandleBouncyShowFinished;
        }
    }

    void OnEnable()
    {
        if (autoPlayOnEnable) ShowAll();
    }

    // ========== Public ==========
    public void ShowAll()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Co_ShowAll());
    }

    // 右键菜单：随时强制显示 completes（用于一键验证可见性）
    [ContextMenu("DEBUG/Force Show Completes")]
    public void Debug_ForceShowCompletes()
    {
        Debug.Log("[Coordinator] DEBUG force-show completes", this);
        ShowCompletesNow();
    }

    // ========== Impl ==========
    IEnumerator Co_ShowAll()
    {
        yield return StabilizeIfNeeded();

        // 没有 bouncy？那就直接显示 completes，避免“等不到事件”
        int validBouncy = 0;
        foreach (var t in bouncies) if (t.target) validBouncy++;
        remainingToShow = validBouncy;

        if (validBouncy == 0)
        {
            Debug.LogWarning("[Coordinator] No BouncyUI found. Showing completes immediately.", this);
            ShowCompletesNow();
            yield break;
        }

        // 播放 bouncy
        foreach (var t in bouncies)
        {
            if (!t.target) continue;
            Debug.Log($"[Coordinator] Play Bouncy: {t.target.name} (delay={t.delay})", t.target);
            StartCoroutine(Co_Delayed(t.delay, t.target.AnimateShow()));
        }

        running = null;
    }

    void HandleBouncyShowFinished(BouncyUI b)
    {
        if (remainingToShow <= 0) return;
        remainingToShow--;
        Debug.Log($"[Coordinator] Bouncy finished: {b.name}. Remaining={remainingToShow}", b);

        if (remainingToShow == 0)
        {
            // 全部完成 → 触发 Complete
            if (completeExtraDelay > 0f)
                StartCoroutine(Co_DelayAndShowCompletes(completeExtraDelay));
            else
                ShowCompletesNow();
        }
    }

    IEnumerator Co_DelayAndShowCompletes(float delay)
    {
        Debug.Log($"[Coordinator] All bouncy finished. Delay {delay}s then show completes.", this);
        float t = 0f; while (t < delay) { t += DT; yield return null; }
        ShowCompletesNow();
    }

    void ShowCompletesNow()
    {
        int shown = 0;
        foreach (var c in completes)
        {
            if (!c.target) continue;
            if (!c.target.gameObject.activeSelf) c.target.gameObject.SetActive(true);

            if (completeUseInstantShow)
            {
                c.target.InstantShow(); // 直接alpha=1
                Debug.Log($"[Coordinator] InstantShow Complete: {c.target.name}", c.target);
            }
            else
            {
                StartCoroutine(c.target.AnimateShow());
                Debug.Log($"[Coordinator] AnimateShow Complete: {c.target.name}", c.target);
            }
            shown++;
        }

        if (shown == 0)
            Debug.LogWarning("[Coordinator] No CompleteUI to show. (Did auto-find fail?)", this);
    }

    IEnumerator Co_Delayed(float delay, IEnumerator routine)
    {
        if (delay > 0f) { float t = 0f; while (t < delay) { t += DT; yield return null; } }
        yield return StartCoroutine(routine);
    }

    IEnumerator StabilizeIfNeeded()
    {
        if (warmed) yield break;

        // 清选中，避免首帧高亮
        var es = EventSystem.current;
        if (es && es.currentSelectedGameObject &&
            es.currentSelectedGameObject.transform.IsChildOf(transform))
            es.SetSelectedGameObject(null);

        // 临时关父链 RectMask2D（两帧）
        var masks = GetComponentsInParent<RectMask2D>(true);
        var maskOn = new bool[masks.Length];
        for (int i = 0; i < masks.Length; i++) { maskOn[i] = masks[i].enabled; masks[i].enabled = false; }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        yield return new WaitForEndOfFrame();
        foreach (var tmp in GetComponentsInChildren<TMP_Text>(true)) tmp.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < masks.Length; i++) masks[i].enabled = maskOn[i];

        warmed = true;
    }
}
