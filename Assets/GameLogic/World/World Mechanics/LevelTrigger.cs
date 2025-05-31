using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using SKCell;

public class LevelTrigger : MonoBehaviour
{
    // References for UI animations.
    private CompleteUI[] completeUIs;
    private BouncyUI[] bouncyUIs;

    [SerializeField] private ImageMover imageMover;
    private GameObject worldChapterCanvas;   // Will be found at runtime
    private FlowManager flowManager;
    public SceneTitle scenetitle;
    private bool startloading = false;
    private Coroutine uiCoroutine = null;

    // Flag that allows level loading by pressing F.
    private bool allowInput = false;

    void Start()
    {
        // 1) Get all UI animation components among children (even if disabled).
        completeUIs = GetComponentsInChildren<CompleteUI>(true);
        bouncyUIs = GetComponentsInChildren<BouncyUI>(true);

        // 2) Find FlowManager in scene.
        GameObject flowmanagerObj = GameObject.Find("FlowManager");
        if (flowmanagerObj != null)
            flowManager = flowmanagerObj.GetComponent<FlowManager>();

        // 3) Dynamically locate the WorldChapter_Canvas by finding the StartLevelButton in children.
        //    Then assume its direct parent is the canvas container that we should toggle.
        StartLevelButton btnScript = GetComponentInChildren<StartLevelButton>(true);
        if (btnScript != null)
        {
            worldChapterCanvas = btnScript.transform.parent.gameObject;
            // Ensure it starts inactive.
            worldChapterCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[LevelTrigger:{name}] Could not find a StartLevelButton child, so no canvas will be shown.");
        }

        // 4) Check if player overlaps this trigger at start.
        CheckInitialPlayerOverlap();
    }

    void CheckInitialPlayerOverlap()
    {
        Collider triggerCollider = GetComponent<Collider>();
        Collider[] hits = Physics.OverlapBox(
            triggerCollider.bounds.center,
            triggerCollider.bounds.extents,
            triggerCollider.transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                allowInput = true;
                if (uiCoroutine != null) StopCoroutine(uiCoroutine);
                uiCoroutine = StartCoroutine(AnimateUIShow());
                break;
            }
        }
    }

    void Update()
    {
        // Allow loading the next level by pressing F if the player is inside the trigger.
        if (allowInput && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F key pressed - loading next level.");
            LoadNextLevel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        allowInput = true;
        if (uiCoroutine != null)
            StopCoroutine(uiCoroutine);
        uiCoroutine = StartCoroutine(AnimateUIShow());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        allowInput = false;
        if (uiCoroutine != null)
            StopCoroutine(uiCoroutine);
        uiCoroutine = StartCoroutine(AnimateUIHide());
    }

    IEnumerator AnimateUIShow()
    {
        // 1) Activate the worldChapterCanvas (where Btn_Start lives), if found.
        if (worldChapterCanvas != null)
            worldChapterCanvas.SetActive(true);

        // 2) Wait one frame so Unity finishes enabling all children under worldChapterCanvas.
        yield return null;

        // 3) (Optional) Log that we found the button under this canvas.
        if (worldChapterCanvas != null)
        {
            StartLevelButton btnScript = worldChapterCanvas.GetComponentInChildren<StartLevelButton>(true);
            if (btnScript != null)
            {
                Debug.Log($"[LevelTrigger:{name}] Found StartLevelButton under '{worldChapterCanvas.name}'.");
            }
            else
            {
                Debug.LogWarning($"[LevelTrigger:{name}] Could not find ANY StartLevelButton under '{worldChapterCanvas.name}'.");
            }
        }

        // 4) Animate any UI elements (CompleteUI, BouncyUI, imageMover).
        if (imageMover != null)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToEnd());
        }

        foreach (CompleteUI ui in completeUIs)
            ui.InstantHide();
        foreach (BouncyUI ui in bouncyUIs)
            ui.InstantHide();

        foreach (CompleteUI ui in completeUIs)
            StartCoroutine(ui.AnimateShow());
        foreach (BouncyUI ui in bouncyUIs)
            StartCoroutine(ui.AnimateShow());

        yield return new WaitForSeconds(0.35f);
    }

    IEnumerator AnimateUIHide()
    {
        // Animate hiding of UI elements.
        foreach (CompleteUI ui in completeUIs)
            StartCoroutine(ui.AnimateHide());
        foreach (BouncyUI ui in bouncyUIs)
            StartCoroutine(ui.AnimateHide());

        yield return new WaitForSeconds(0.25f);

        if (worldChapterCanvas != null)
            worldChapterCanvas.SetActive(false);

        if (imageMover != null)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToStart());
        }
    }

    public void LoadNextLevel()
    {
        // Prevent double‐calls by exiting early if already started.
        if (startloading) return;
        startloading = true;

        // Save the trigger’s position under the target scene name.
        string targetSceneName = scenetitle.ToString();
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerZ", transform.position.z);
        PlayerPrefs.SetString("LastTriggerScene", targetSceneName);

        // Wait a short moment before loading the next level.
        SKUtils.InvokeAction(0.2f, () =>
        {
            flowManager.LoadScene(new SceneInfo()
            {
                index = scenetitle,
            });
        });
    }
}
