using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SKCell;

public class MenuController : MonoBehaviour
{
    [Header("Levels To Load - Chapter 0 World")]
    [SerializeField] private SceneTitle sceneToLoad_Chp1_World;
    private bool startLoading = false;
    private FlowManager flowManager;
    private GameObject flowmanager;

    [SerializeField] private GameObject noSavedGameDialog = null;

    private int uiLayerMask;
    private bool gamePaused = false;

    [Header("Start Game Animations (UI)")]
    [SerializeField] private RectTransform circle;
    [SerializeField] private float Circle_PosY_end = 542f;

    [SerializeField] private RectTransform circleBig;
    [SerializeField] private float CircleBig_PosX_end = 995f;
    [SerializeField] private float CircleBig_PosY_end = -6.103516e-05f;

    [SerializeField] private RectTransform Title;
    [SerializeField] private float Title_PosX_end = -937f;
    [SerializeField] private float Title_PosY_end = 113f;
    [SerializeField] private float Title_scale_end = 0.8f;

    [SerializeField] private float uiLerpDuration = 0.35f;
    [SerializeField] private AnimationCurve ease = null;
    [SerializeField] private bool useUnscaledTime = true;

    private Vector2 _circleStart;
    private Vector2 _circleBigStart;
    private Vector2 _titleStart;
    private Vector3 _titleScaleStart;

    private Coroutine _uiCo;

    //Settings icon animation
    [Header("Bouncy Toggle Object")]
    [SerializeField] private Transform bouncyObject;                
    [SerializeField] private float bouncyHiddenOffsetX = -350f;
    [SerializeField] private float bouncyMoveDuration = 0.35f;
    [SerializeField] private AnimationCurve bouncyEase = null;
    [SerializeField] private bool bouncyUseUnscaledTime = true;

    private bool _bouncyInitialized = false;
    private bool _bouncyShown = false;
    private Coroutine _bouncyCo;

    private Vector3 _bouncyFinalWorldPos;
    private Vector2 _bouncyFinalAnchoredPos;
    private bool _bouncyIsUI = false;

    //Button rotate animation
    [Header("Button Rotate")]
    [SerializeField] private RectTransform rotateButtonTarget;
    [SerializeField] private float rotateDuration = 0.25f;
    [SerializeField] private AnimationCurve rotateEase = null;
    [SerializeField] private bool rotateUseUnscaledTime = true;

    private float _rotateStartZ;
    private bool _rotateInitialized = false;
    private Coroutine _rotateCo;


    private void Start()
    {
        if (circle != null) _circleStart = circle.anchoredPosition;

        if (circleBig != null)
        {
            _circleBigStart = circleBig.anchoredPosition; // ✅ 修正：记录 circleBig 自己的起点
        }

        if (Title != null)
        {
            _titleStart = Title.anchoredPosition;
            _titleScaleStart = Title.localScale;
        }

        flowmanager = GameObject.Find("FlowManager");
        if (flowmanager != null)
        {
            flowManager = flowmanager.GetComponent<FlowManager>();
        }
        else
        {
            Debug.LogError("FlowManager GameObject not found!");
        }
    }

    public void newGameDialogYes()
    {
        LoadNextLevel(sceneToLoad_Chp1_World);
        print("LOADING LEVEL! " + sceneToLoad_Chp1_World);
        ResumeGame();
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        if (!gamePaused)
        {
            EnableOnlyUILayerRaycasts(true);
            Time.timeScale = 0;
            DisableInputOutsideUI(true);

            var timer = GetLevelTimer();
            if (timer != null) timer.PauseFromMenu();

            gamePaused = true;
        }
    }

    public void ResumeGame()
    {
        if (gamePaused)
        {
            EnableOnlyUILayerRaycasts(false);
            Time.timeScale = 1;
            DisableInputOutsideUI(false);

            var timer = GetLevelTimer();
            if (timer != null) timer.ResumeFromMenu();

            gamePaused = false;
        }
    }

    private void EnableOnlyUILayerRaycasts(bool uiOnly)
    {
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;

        if (eventSystem != null)
        {
            foreach (var obj in FindObjectsOfType<Collider2D>())
            {
                if (uiOnly)
                {
                    if (obj.gameObject.layer != LayerMask.NameToLayer("UI"))
                    {
                        obj.GetComponent<Collider2D>().enabled = false;
                    }
                }
                else
                {
                    obj.GetComponent<Collider2D>().enabled = true;
                }
            }
        }
    }

    private void DisableInputOutsideUI(bool disable)
    {
        var dragObjects = FindObjectsOfType<Block>();
        foreach (var dragObject in dragObjects)
        {
            dragObject.enabled = !disable;
        }

        var playerMoveScripts = FindObjectsOfType<SubPositionIndicator>();
        foreach (var script in playerMoveScripts)
        {
            script.enabled = !disable;
        }
    }

    private void LoadNextLevel(SceneTitle sceneTitle)
    {
        SKUtils.InvokeAction(0.2f, () =>
        {
            flowManager.LoadScene(new SceneInfo()
            {
                index = sceneTitle,
            });
        });

        startLoading = true;
    }

    public void ResetCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        ResumeGame();
    }

    private LevelTimer GetLevelTimer()
    {
        if (LevelTimer.Instance != null) return LevelTimer.Instance;
        return FindObjectOfType<LevelTimer>();
    }

    public void StartChapterWorld(SceneTitle chapterWorldScene)
    {
        if (startLoading) return;

        LoadNextLevel(chapterWorldScene);
    }


    // ---------------- UI Animation API ----------------

    // ONE call: circle + circleBig + title to end, and title scales to end
    public void MoveUIToEnd()
    {
        if (circle == null || circleBig == null || Title == null) return;

        Vector2 circleEnd = new Vector2(_circleStart.x, Circle_PosY_end);
        Vector2 circleBigEnd = new Vector2(CircleBig_PosX_end, CircleBig_PosY_end);
        Vector2 titleEnd = new Vector2(Title_PosX_end, Title_PosY_end);
        Vector3 titleScaleEnd = new Vector3(Title_scale_end, Title_scale_end, _titleScaleStart.z);

        StartUIMove(circleEnd, circleBigEnd, titleEnd, titleScaleEnd);
    }

    // ONE call: circle + circleBig + title back to start, and title scales back to start
    public void MoveUIToStart()
    {
        if (circle == null || circleBig == null || Title == null) return;
        StartUIMove(_circleStart, _circleBigStart, _titleStart, _titleScaleStart);
    }

    private void StartUIMove(Vector2 circleTarget, Vector2 circleBigTarget, Vector2 titleTarget, Vector3 titleScaleTarget)
    {
        if (_uiCo != null) StopCoroutine(_uiCo);
        _uiCo = StartCoroutine(CoMoveUI(circleTarget, circleBigTarget, titleTarget, titleScaleTarget));
    }

    private IEnumerator CoMoveUI(Vector2 circleTarget, Vector2 circleBigTarget, Vector2 titleTarget, Vector3 titleScaleTarget)
    {
        Vector2 circleFrom = circle.anchoredPosition;
        Vector2 circleBigFrom = circleBig.anchoredPosition;
        Vector2 titleFrom = Title.anchoredPosition;
        Vector3 titleScaleFrom = Title.localScale;

        float dur = Mathf.Max(0.0001f, uiLerpDuration);
        float t = 0f;

        while (t < 1f)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt / dur;

            float u = Mathf.Clamp01(t);
            float k = (ease != null && ease.length > 0) ? ease.Evaluate(u) : u;

            circle.anchoredPosition = Vector2.LerpUnclamped(circleFrom, circleTarget, k);
            circleBig.anchoredPosition = Vector2.LerpUnclamped(circleBigFrom, circleBigTarget, k);
            Title.anchoredPosition = Vector2.LerpUnclamped(titleFrom, titleTarget, k);
            Title.localScale = Vector3.LerpUnclamped(titleScaleFrom, titleScaleTarget, k);

            yield return null;
        }

        circle.anchoredPosition = circleTarget;
        circleBig.anchoredPosition = circleBigTarget;
        Title.anchoredPosition = titleTarget;
        Title.localScale = titleScaleTarget;

        _uiCo = null;
    }

    // ---------------- Settings icon animation ----------------
    private void InitBouncyObjectIfNeeded()
    {
        if (_bouncyInitialized) return;
        if (bouncyObject == null) return;

        // 自动判断是不是 UI
        var rt = bouncyObject as RectTransform;
        _bouncyIsUI = (rt != null);

        if (_bouncyIsUI)
        {
            _bouncyFinalAnchoredPos = rt.anchoredPosition;
        }
        else
        {
            _bouncyFinalWorldPos = bouncyObject.position; 
        }

        // 初始默认：隐藏状态
        SetBouncyToHiddenInstant();
        bouncyObject.gameObject.SetActive(false);
        _bouncyShown = false;

        _bouncyInitialized = true;
    }

    private void SetBouncyToHiddenInstant()
    {
        if (bouncyObject == null) return;

        if (_bouncyIsUI)
        {
            var rt = (RectTransform)bouncyObject;
            rt.anchoredPosition = _bouncyFinalAnchoredPos + Vector2.right * bouncyHiddenOffsetX;
        }
        else
        {
            bouncyObject.position = _bouncyFinalWorldPos + Vector3.right * bouncyHiddenOffsetX;
        }
    }

    /// <summary>
    /// 第一次点击：从左边弹出到最终位置并显示
    /// 再次点击：弹回左边并隐藏（disable）
    /// </summary>
    public void ToggleBouncyObject()
    {
        if (bouncyObject == null) return;

        InitBouncyObjectIfNeeded();

        if (_bouncyCo != null) StopCoroutine(_bouncyCo);

        if (!_bouncyShown)
        {
            // 显示：先瞬间放到隐藏位，再 enable，再弹到最终位
            SetBouncyToHiddenInstant();
            bouncyObject.gameObject.SetActive(true);

            _bouncyCo = StartCoroutine(CoBouncyMove(show: true));
            ToggleRotateButton(open: true);   // 逆时针转90
            _bouncyShown = true;
        }
        else
        {
            // 隐藏：从当前位置弹回隐藏位，结束后 disable
            _bouncyCo = StartCoroutine(CoBouncyMove(show: false));
            ToggleRotateButton(open: false);  // 顺时针90
            _bouncyShown = false;
        }
    }

    private IEnumerator CoBouncyMove(bool show)
    {
        float dur = Mathf.Max(0.0001f, bouncyMoveDuration);
        float t = 0f;

        if (_bouncyIsUI)
        {
            var rt = (RectTransform)bouncyObject;
            Vector2 from = rt.anchoredPosition;
            Vector2 to = show
                ? _bouncyFinalAnchoredPos
                : _bouncyFinalAnchoredPos + Vector2.right * bouncyHiddenOffsetX;

            while (t < 1f)
            {
                float dt = bouncyUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt / dur;

                float u = Mathf.Clamp01(t);
                float k = (bouncyEase != null && bouncyEase.length > 0) ? bouncyEase.Evaluate(u) : u;

                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
                yield return null;
            }

            rt.anchoredPosition = to;
        }
        else
        {
            Vector3 from = bouncyObject.position;
            Vector3 to = show
                ? _bouncyFinalWorldPos
                : _bouncyFinalWorldPos + Vector3.right * bouncyHiddenOffsetX;

            while (t < 1f)
            {
                float dt = bouncyUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt / dur;

                float u = Mathf.Clamp01(t);
                float k = (bouncyEase != null && bouncyEase.length > 0) ? bouncyEase.Evaluate(u) : u;

                bouncyObject.position = Vector3.LerpUnclamped(from, to, k);
                yield return null;
            }

            bouncyObject.position = to;
        }

        // 隐藏结束：disable
        if (!show)
            bouncyObject.gameObject.SetActive(false);

        _bouncyCo = null;
    }

    // ---------------- Button rotate animation ----------------

    private void InitRotateIfNeeded()
    {
        if (_rotateInitialized) return;
        if (rotateButtonTarget == null) return;

        // 记录“关闭时”的初始角度（你摆好的角度）
        _rotateStartZ = rotateButtonTarget.localEulerAngles.z;
        _rotateInitialized = true;
    }

    public void ToggleRotateButton(bool open)
    {
        if (rotateButtonTarget == null) return;

        InitRotateIfNeeded();

        if (_rotateCo != null) StopCoroutine(_rotateCo);

        float targetZ = open ? _rotateStartZ + 90f : _rotateStartZ; // open 逆时针 +90；close 回到初始角度
        _rotateCo = StartCoroutine(CoRotateZ(targetZ));
    }

    private IEnumerator CoRotateZ(float targetZ)
    {
        float dur = Mathf.Max(0.0001f, rotateDuration);
        float t = 0f;

        float fromZ = rotateButtonTarget.localEulerAngles.z;

        // 处理 0/360 跳变，确保走最短路径
        float from = Mathf.DeltaAngle(0f, fromZ);
        float to = Mathf.DeltaAngle(0f, targetZ);

        while (t < 1f)
        {
            float dt = rotateUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt / dur;

            float u = Mathf.Clamp01(t);
            float k = (rotateEase != null && rotateEase.length > 0) ? rotateEase.Evaluate(u) : u;

            float z = Mathf.LerpUnclamped(from, to, k);
            rotateButtonTarget.localEulerAngles = new Vector3(0f, 0f, z);

            yield return null;
        }

        rotateButtonTarget.localEulerAngles = new Vector3(0f, 0f, to);
        _rotateCo = null;
    }
}