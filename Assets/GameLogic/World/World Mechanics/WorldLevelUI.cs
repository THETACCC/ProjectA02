using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class WorldLevelUI : MonoBehaviour
{
    public static WorldLevelUI Instance { get; private set; }

    [Header("Hide/Show")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool hideOnStart = true;

    [Header("Slide Animation (like ImageMover)")]
    [Tooltip("Which RectTransform to move. Usually the panel root (often the same object as CanvasGroup).")]
    [SerializeField] private RectTransform moveRoot;

    [Tooltip("UI point off-screen on the left (RectTransform). Name suggestion: Slide_Start")]
    [SerializeField] private RectTransform slideStartPoint;

    [Tooltip("UI end position on-screen (RectTransform). Name suggestion: Slide_End")]
    [SerializeField] private RectTransform slideEndPoint;

    [SerializeField] private float slideDuration = 0.35f;

    [Tooltip("If true, use unscaled time (UI still animates when Time.timeScale=0).")]
    [SerializeField] private bool useUnscaledTime = true;

    [Tooltip("Disable raycast/interactable while sliding.")]
    [SerializeField] private bool disableInteractWhileMoving = true;

    [Header("Input")]
    [SerializeField] private bool listenSpaceToStart = true;

    [Header("Auto-wire by child names")]
    [SerializeField] private bool autoWireByNames = true;

    [Header("UI Refs (optional if autoWireByNames=true)")]
    [SerializeField] private TMP_Text txtLevel;       // Txt_Level
    [SerializeField] private TMP_Text txtTime;        // Txt_Time
    [SerializeField] private TMP_Text txtRewards;     // Txt_Rewards
    //[SerializeField] private Image imgComplete;       // Img_Complete
    [SerializeField] private Button btnStart;         // Btn_Start


    private readonly List<LevelTrigger> insideOrder = new List<LevelTrigger>(8);
    private LevelTrigger current;

    private Coroutine slideCo;
    private bool isShown = false;

    [SerializeField] private TMP_Text txtChapter;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (autoWireByNames) AutoWire();

        // If moveRoot not assigned, try to use CanvasGroup's RectTransform, otherwise use self
        if (moveRoot == null)
        {
            if (canvasGroup != null) moveRoot = canvasGroup.GetComponent<RectTransform>();
            if (moveRoot == null) moveRoot = GetComponent<RectTransform>();
        }

        if (btnStart)
        {
            btnStart.onClick.RemoveAllListeners();
            btnStart.onClick.AddListener(OnClickStart);
        }

        if (hideOnStart) HideInstant();
        else ShowInstant();

        RefreshChapterLabel();
    }

    private void Update()
    {
        if (!listenSpaceToStart) return;
        if (current == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
            OnClickStart();
    }

    public void EnterTrigger(LevelTrigger t)
    {
        if (t == null) return;

        if (!insideOrder.Contains(t))
            insideOrder.Add(t);

        SetCurrent(t);
    }

    public void ExitTrigger(LevelTrigger t)
    {
        if (t == null) return;

        insideOrder.Remove(t);

        if (current == t)
        {
            current = null;

            if (insideOrder.Count > 0)
                SetCurrent(insideOrder[insideOrder.Count - 1]);
            else
                Hide(); // slide out to left
        }
    }

    private void SetCurrent(LevelTrigger t)
    {
        current = t;
        RefreshUI();
        Show(); // slide in from left
    }

    private void RefreshUI()
    {
        if (current == null) return;

        var sceneKey = current.scenetitle.ToString();
        var (chap0, lvl0) = ParseChapterLevel(sceneKey);

        if (txtLevel)
        {
            txtLevel.text = $"Level {lvl0 + 1}";
        }

        bool cleared = false;
        if (SaveManager.Instance != null)
            cleared = SaveManager.Instance.IsLevelCleared(chap0, lvl0);

        //if (imgComplete) imgComplete.gameObject.SetActive(cleared);

        float best = -1f;
        if (SaveManager.Instance != null)
            best = SaveManager.Instance.GetBestTime(chap0, lvl0);

        if (txtTime) txtTime.text = FormatSeconds(best);

        if (txtRewards)
        {
            if (SaveManager.Instance != null)
                txtRewards.text = SaveManager.Instance.GetLevelRewardProgressText(chap0, lvl0);
            else
                txtRewards.text = "0/0";
        }
    }

    private void OnClickStart()
    {
        if (current == null) return;
        current.LoadNextLevel();
    }

    // -------------------------
    // Slide Show/Hide
    // -------------------------

    public void Show()
    {
        // fallback: if missing refs, keep original instant behavior
        if (canvasGroup == null || moveRoot == null || slideEndPoint == null || slideStartPoint == null || slideDuration <= 0f)
        {
            ShowInstant();
            return;
        }

        isShown = true;

        // make visible during slide
        canvasGroup.alpha = 1f;
        if (disableInteractWhileMoving)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        StartSlide(slideEndPoint, toVisible: true);
    }

    public void Hide()
    {
        // disable interaction immediately
        if (canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            // keep alpha=1 during slide out (so it doesn't "fade"), then set 0 at end
            if (canvasGroup.alpha <= 0f) canvasGroup.alpha = 1f;
        }

        isShown = false;

        // slide out if possible
        if (canvasGroup == null || moveRoot == null || slideEndPoint == null || slideStartPoint == null || slideDuration <= 0f)
        {
            HideInstant();
        }
        else
        {
            StartSlide(slideStartPoint, toVisible: false);
        }

        // keep your original data reset behavior
        current = null;
        insideOrder.Clear();
    }

    private void ShowInstant()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            gameObject.SetActive(true);
        }

        isShown = true;

        if (moveRoot && slideEndPoint)
            moveRoot.position = slideEndPoint.position;
    }

    private void HideInstant()
    {
        if (moveRoot && slideStartPoint)
            moveRoot.position = slideStartPoint.position;

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            gameObject.SetActive(false);
        }

        isShown = false;
        current = null;
        insideOrder.Clear();
    }

    private void StartSlide(RectTransform targetPoint, bool toVisible)
    {
        if (slideCo != null) StopCoroutine(slideCo);
        slideCo = StartCoroutine(CoSlide(targetPoint, toVisible));
    }

    private IEnumerator CoSlide(RectTransform targetPoint, bool toVisible)
    {
        Vector3 from = moveRoot.position;
        Vector3 to = targetPoint.position;

        float t = 0f;

        while (t < slideDuration)
        {
            float p = (slideDuration <= 0f) ? 1f : (t / slideDuration);
            moveRoot.position = Vector3.Lerp(from, to, p);

            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        moveRoot.position = to;

        if (canvasGroup)
        {
            if (toVisible && isShown)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        slideCo = null;
    }

    // -------------------------
    // AutoWire / Helpers
    // -------------------------

    private void AutoWire()
    {
        if (txtTime == null) txtTime = FindByName<TMP_Text>("Txt_Time");
        if (txtLevel == null) txtLevel = FindByName<TMP_Text>("Txt_Level");
        if (txtChapter == null) txtChapter = FindByName<TMP_Text>("Txt_Chapter");
        if (txtRewards == null)
        {
            txtRewards = FindByName<TMP_Text>("Txt_Rewards");
            if (txtRewards == null) txtRewards = FindByName<TMP_Text>("Txt_Reward");
        }
        //if (imgComplete == null) imgComplete = FindByName<Image>("Img_Complete");
        if (btnStart == null) btnStart = FindByName<Button>("Btn_Start");

        // slide points by common names
        if (slideStartPoint == null) slideStartPoint = FindByName<RectTransform>("Slide_Start");
        if (slideEndPoint == null) slideEndPoint = FindByName<RectTransform>("Slide_End");

        // moveRoot: if CanvasGroup exists, prefer its RectTransform
        if (moveRoot == null && canvasGroup != null)
            moveRoot = canvasGroup.GetComponent<RectTransform>();
    }

    private T FindByName<T>(string objName) where T : Component
    {
        foreach (var tr in GetComponentsInChildren<Transform>(true))
            if (tr.name == objName) return tr.GetComponent<T>();
        return null;
    }

    private (int chap0, int lvl0) ParseChapterLevel(string sceneKey)
    {
        var m = Regex.Match(sceneKey, @"Chapter\s*(\d+)\s*[_\-\s]\s*Level\s*(\d+)", RegexOptions.IgnoreCase);
        if (!m.Success) return (0, 0);

        int chap = int.Parse(m.Groups[1].Value);
        int lvl1 = int.Parse(m.Groups[2].Value);
        return (Mathf.Max(0, chap), Mathf.Max(0, lvl1 - 1));
    }

    private string FormatSeconds(float sec)
    {
        if (sec < 0f || sec <= 0.0001f) return "--:--:--";

        int totalMs = Mathf.RoundToInt(sec * 1000f);
        int minutes = totalMs / 60000;
        int seconds = (totalMs % 60000) / 1000;
        int centi = (totalMs % 1000) / 10;

        return $"{minutes:00}:{seconds:00}:{centi:00}";
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        RefreshChapterLabel();
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
    }

    private void HandleActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        RefreshChapterLabel(newScene.name);
    }

    private void RefreshChapterLabel(string sceneNameOverride = null)
    {
        if (!txtChapter) return;

        string sceneName = string.IsNullOrEmpty(sceneNameOverride)
            ? SceneManager.GetActiveScene().name
            : sceneNameOverride;

        // Match: Chapter0World / Chapter1World ...
        var m = Regex.Match(sceneName, @"Chapter\s*(\d+)\s*World", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            int chap = int.Parse(m.Groups[1].Value); // keep 0-based as in name
            txtChapter.text = $"Chapter {chap}";
        }
    }
}