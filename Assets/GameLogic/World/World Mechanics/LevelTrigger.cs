using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using SKCell;

[DisallowMultipleComponent]
public class LevelTrigger : MonoBehaviour
{
    [Header("Scene binding")]
    [Tooltip("必须设置，比如 Chapter0_Level1 / Chapter1_Level6 等。只从这里解析章节与关卡。")]
    public SceneTitle scenetitle;

    [Header("Optional refs (动画/按钮)")]
    [SerializeField] private ImageMover imageMover;
    [SerializeField] private GameObject uiSelectLevel;   // 进入提示UI
    private GameObject worldChapterCanvas;               // 含 Start 按钮的父物体（可选）
    private CompleteUI[] completeUIs;
    private BouncyUI[] bouncyUIs;

    private FlowManager flowManager;
    private Coroutine uiCoroutine;
    private bool allowInput = false;
    private bool startloading = false;

    // —— 从 scenetitle 解析出的内部索引（Chapter=0基，Level=0基）
    private int chapterIndex0 = 0;
    private int levelIndex0 = 0;

    // —— 临时“软隐藏”：只关渲染器/碰撞器，仍保持脚本运行，用于等待 SaveManager
    private Collider[] _colliders;
    private Renderer[] _renderers;
    private Canvas[] _canvases;

    private void Awake()
    {
        // 解析 scenetitle（支持 Chapter0_Level1 / Chapter 0 - Level 1 / Chapter0 Level1 等）
        var s = scenetitle.ToString();
        var m = Regex.Match(s, @"Chapter\s*(\d+)\s*[_\-\s]\s*Level\s*(\d+)", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            int chap = int.Parse(m.Groups[1].Value); // 已是0基
            int lvl1 = int.Parse(m.Groups[2].Value); // 1基
            chapterIndex0 = Mathf.Max(0, chap);
            levelIndex0 = Mathf.Max(0, lvl1 - 1);   // 转0基
        }
        else
        {
            Debug.LogError($"[LevelTrigger:{name}] scenetitle '{s}' 未按 'Chapter#_Level#' 格式；将按(0,0)处理。");
            chapterIndex0 = 0;
            levelIndex0 = 0;
        }

        // 缓存可软隐藏组件
        _colliders = GetComponentsInChildren<Collider>(true);
        _renderers = GetComponentsInChildren<Renderer>(true);
        _canvases = GetComponentsInChildren<Canvas>(true);

        // 初始软隐藏，等 SaveManager 判定后再决定显示或彻底关掉
        SetSoftHidden(true);
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAndDecideVisibility());
    }

    private IEnumerator WaitAndDecideVisibility()
    {
        // 等 SaveManager 就绪（最多几帧）
        int tries = 0;
        while (SaveManager.Instance == null && tries < 10)
        {
            tries++;
            yield return null;
        }

        if (SaveManager.Instance == null)
        {
            // 保险：如果依然没有存档系统，就保持完全不可见
            gameObject.SetActive(false);
            yield break;
        }

        // 判定解锁
        bool unlocked = SaveManager.Instance.IsLevelUnlocked(chapterIndex0, levelIndex0);
        Debug.Log($"[LevelTrigger:{name}] Unlocked? {unlocked}  (c={chapterIndex0}, l0={levelIndex0}, scenetitle={scenetitle})");

        if (!unlocked)
        {
            // 未解锁：彻底隐藏
            gameObject.SetActive(false);
            yield break;
        }

        // 已解锁：恢复可见和交互，再做你的UI初始化
        SetSoftHidden(false);
        InitUIAndFlow();
        CheckInitialPlayerOverlap();
    }

    private void InitUIAndFlow()
    {
        completeUIs = GetComponentsInChildren<CompleteUI>(true);
        bouncyUIs = GetComponentsInChildren<BouncyUI>(true);

        var fmObj = GameObject.Find("FlowManager");
        if (fmObj) flowManager = fmObj.GetComponent<FlowManager>();

        if (uiSelectLevel) uiSelectLevel.SetActive(false);

        var btnScript = GetComponentInChildren<StartLevelButton>(true);
        if (btnScript)
        {
            worldChapterCanvas = btnScript.transform.parent.gameObject;
            worldChapterCanvas.SetActive(false);
        }
    }

    private void SetSoftHidden(bool hidden)
    {
        if (_colliders != null)
            foreach (var c in _colliders) if (c) c.enabled = !hidden;

        if (_renderers != null)
            foreach (var r in _renderers) if (r) r.enabled = !hidden;

        if (_canvases != null)
            foreach (var cv in _canvases) if (cv) cv.enabled = !hidden;

        if (uiSelectLevel) uiSelectLevel.SetActive(!hidden && uiSelectLevel.activeSelf); // 不强行开
        if (worldChapterCanvas) worldChapterCanvas.SetActive(!hidden && worldChapterCanvas.activeSelf);
    }

    private void CheckInitialPlayerOverlap()
    {
        var trigger = GetComponent<Collider>();
        if (!trigger) return;

        Collider[] hits = Physics.OverlapBox(
            trigger.bounds.center,
            trigger.bounds.extents,
            trigger.transform.rotation
        );

        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                allowInput = true;
                if (uiCoroutine != null) StopCoroutine(uiCoroutine);
                uiCoroutine = StartCoroutine(AnimateUIShow());
                break;
            }
        }
    }

    private void Update()
    {
        if (allowInput && Input.GetKeyDown(KeyCode.F))
        {
            LoadNextLevel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        allowInput = true;
        if (uiCoroutine != null) StopCoroutine(uiCoroutine);
        uiCoroutine = StartCoroutine(AnimateUIShow());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        allowInput = false;
        if (uiCoroutine != null) StopCoroutine(uiCoroutine);
        uiCoroutine = StartCoroutine(AnimateUIHide());
    }

    IEnumerator AnimateUIShow()
    {
        if (uiSelectLevel) uiSelectLevel.SetActive(true);
        if (worldChapterCanvas) worldChapterCanvas.SetActive(true);

        if (imageMover)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToEnd());
        }

        if (completeUIs != null) foreach (var ui in completeUIs) ui.InstantHide();
        if (bouncyUIs != null) foreach (var ui in bouncyUIs) ui.InstantHide();

        if (completeUIs != null) foreach (var ui in completeUIs) StartCoroutine(ui.AnimateShow());
        if (bouncyUIs != null) foreach (var ui in bouncyUIs) StartCoroutine(ui.AnimateShow());

        yield return new WaitForSeconds(0.35f);
    }

    IEnumerator AnimateUIHide()
    {
        if (completeUIs != null) foreach (var ui in completeUIs) StartCoroutine(ui.AnimateHide());
        if (bouncyUIs != null) foreach (var ui in bouncyUIs) StartCoroutine(ui.AnimateHide());

        yield return new WaitForSeconds(0.25f);

        if (uiSelectLevel) uiSelectLevel.SetActive(false);
        if (worldChapterCanvas) worldChapterCanvas.SetActive(false);

        if (imageMover)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToStart());
        }
    }

    public void LoadNextLevel()
    {
        if (startloading) return;
        startloading = true;

        // —— 你的原有保存/切关逻辑 ——（保持不变）
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerZ", transform.position.z);

        string targetSceneName = scenetitle.ToString();
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerZ", transform.position.z);
        PlayerPrefs.SetString("LastTriggerScene", targetSceneName);

        SKUtils.InvokeAction(0.2f, () =>
        {
            if (flowManager)
                flowManager.LoadScene(new SceneInfo() { index = scenetitle });
            else
                SceneManager.LoadScene(targetSceneName); // 兜底
        });
    }
}
