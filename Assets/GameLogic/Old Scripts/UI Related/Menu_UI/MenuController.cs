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
}