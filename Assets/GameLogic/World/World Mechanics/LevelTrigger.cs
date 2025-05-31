using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using SKCell;

public class LevelTrigger : MonoBehaviour
{
    // References for UI animations.
    private CompleteUI[] completeUIs;
    private BouncyUI[] bouncyUIs;

    [SerializeField] private ImageMover imageMover;         // Reference to the ImageMover script.
    [SerializeField] private GameObject uiSelectLevel;      // Reference to the UI_SelectLevel GameObject (for animations).

    private GameObject worldChapterCanvas;                   // Will be located at runtime (contains Btn_Start).
    private FlowManager flowManager;
    public SceneTitle scenetitle;                            // Set this per LevelTrigger instance in the Inspector.
    private bool startloading = false;
    private Coroutine uiCoroutine = null;

    // Flag that allows level loading by pressing F.
    private bool allowInput = false;

    void Start()
    {
        // 1) Get all UI animation components (even if disabled) among children.
        completeUIs = GetComponentsInChildren<CompleteUI>(true);
        bouncyUIs = GetComponentsInChildren<BouncyUI>(true);

        // 2) Set up FlowManager (assumes there is one in your scene named "FlowManager").
        GameObject flowmanagerObj = GameObject.Find("FlowManager");
        if (flowmanagerObj != null)
            flowManager = flowmanagerObj.GetComponent<FlowManager>();

        // 3) Ensure the select-level UI is initially inactive.
        if (uiSelectLevel != null)
            uiSelectLevel.SetActive(false);

        // 4) Dynamically locate the WorldChapter_Canvas by finding the StartLevelButton in children.
        //    Assume its direct parent is the canvas container that holds the button.
        StartLevelButton btnScript = GetComponentInChildren<StartLevelButton>(true);
        if (btnScript != null)
        {
            worldChapterCanvas = btnScript.transform.parent.gameObject;
            worldChapterCanvas.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[LevelTrigger:{name}] Could not find a StartLevelButton child, so no worldChapterCanvas will be shown.");
        }

        // 5) Check if the player is already overlapping the trigger at start (for example, they landed inside).
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

        // When the player enters, allow input and play the show animation.
        allowInput = true;

        if (uiCoroutine != null)
            StopCoroutine(uiCoroutine);
        uiCoroutine = StartCoroutine(AnimateUIShow());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // When the player leaves, disallow input and play the hide animation.
        allowInput = false;
        if (uiCoroutine != null)
            StopCoroutine(uiCoroutine);
        uiCoroutine = StartCoroutine(AnimateUIHide());
    }

    IEnumerator AnimateUIShow()
    {
        // 1) Activate the select-level UI for animations.
        if (uiSelectLevel != null)
            uiSelectLevel.SetActive(true);

        // 2) Also activate worldChapterCanvas (where Btn_Start lives), if found.
        if (worldChapterCanvas != null)
            worldChapterCanvas.SetActive(true);

        // 3) Animate the image mover if assigned.
        if (imageMover != null)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToEnd());
        }

        // 4) Immediately set UI elements to their hidden state.
        foreach (CompleteUI ui in completeUIs)
            ui.InstantHide();
        foreach (BouncyUI ui in bouncyUIs)
            ui.InstantHide();

        // 5) Now animate them to show simultaneously.
        foreach (CompleteUI ui in completeUIs)
            StartCoroutine(ui.AnimateShow());
        foreach (BouncyUI ui in bouncyUIs)
            StartCoroutine(ui.AnimateShow());

        yield return new WaitForSeconds(0.35f); // Wait for animations to complete.
    }

    IEnumerator AnimateUIHide()
    {
        // 1) Animate hiding of UI elements.
        foreach (CompleteUI ui in completeUIs)
            StartCoroutine(ui.AnimateHide());
        foreach (BouncyUI ui in bouncyUIs)
            StartCoroutine(ui.AnimateHide());

        yield return new WaitForSeconds(0.25f); // Wait for animations to finish.

        // 2) Deactivate the select-level UI.
        if (uiSelectLevel != null)
            uiSelectLevel.SetActive(false);

        // 3) Deactivate the worldChapterCanvas so the button disappears.
        if (worldChapterCanvas != null)
            worldChapterCanvas.SetActive(false);

        // 4) Animate imageMover back to start if assigned.
        if (imageMover != null)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToStart());
        }
    }

    public void LoadNextLevel()
    {
        // Prevent double-calls by exiting early if we’ve already started loading.
        if (startloading) return;
        startloading = true;

        // 1) Save the world position under the current scene name.
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerZ", transform.position.z);

        // 2) Save under the target level’s name (in case you return later).
        string targetSceneName = scenetitle.ToString();
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(targetSceneName + "_LastTriggerZ", transform.position.z);
        PlayerPrefs.SetString("LastTriggerScene", targetSceneName);

        // 3) Wait a short moment before loading the next level.
        SKUtils.InvokeAction(0.2f, () =>
        {
            flowManager.LoadScene(new SceneInfo()
            {
                index = scenetitle,
            });
        });
    }
}
