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
    [SerializeField] private GameObject uiSelectLevel;        // Reference to the UI_SelectLevel GameObject.

    private FlowManager flowManager;
    public SceneTitle scenetitle;                            // Set this per LevelTrigger instance in the Inspector.
    private bool startloading = false;
    private Coroutine uiCoroutine = null;

    // Flag that allows level loading by pressing F.
    private bool allowInput = false;

    void Start()
    {
        // Get all UI animation components (even if disabled) among children.
        completeUIs = GetComponentsInChildren<CompleteUI>(true);
        bouncyUIs = GetComponentsInChildren<BouncyUI>(true);

        // Set up FlowManager (assumes there is one in your scene named "FlowManager").
        GameObject flowmanagerObj = GameObject.Find("FlowManager");
        if (flowmanagerObj != null)
            flowManager = flowmanagerObj.GetComponent<FlowManager>();

        // Ensure the select-level UI is initially inactive.
        if (uiSelectLevel != null)
            uiSelectLevel.SetActive(false);

        // Check if the player is already overlapping the trigger at start (for example, if they just landed).
        CheckInitialPlayerOverlap();
    }

    void CheckInitialPlayerOverlap()
    {
        Collider triggerCollider = GetComponent<Collider>();
        Collider[] hits = Physics.OverlapBox(triggerCollider.bounds.center, triggerCollider.bounds.extents, triggerCollider.transform.rotation);
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
        // Activate the select-level UI.
        if (uiSelectLevel != null)
            uiSelectLevel.SetActive(true);

        // Animate the image mover if assigned.
        if (imageMover != null)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToEnd());
        }

        // Immediately set UI elements to their hidden state.
        foreach (CompleteUI ui in completeUIs)
            ui.InstantHide();
        foreach (BouncyUI ui in bouncyUIs)
            ui.InstantHide();

        // Now animate them to show simultaneously.
        foreach (CompleteUI ui in completeUIs)
            StartCoroutine(ui.AnimateShow());
        foreach (BouncyUI ui in bouncyUIs)
            StartCoroutine(ui.AnimateShow());

        yield return new WaitForSeconds(0.35f); // Wait for animations to complete.
    }

    IEnumerator AnimateUIHide()
    {
        // Animate hiding of UI elements.
        foreach (CompleteUI ui in completeUIs)
            StartCoroutine(ui.AnimateHide());
        foreach (BouncyUI ui in bouncyUIs)
            StartCoroutine(ui.AnimateHide());

        yield return new WaitForSeconds(0.25f); // Wait for animations to finish.

        if (uiSelectLevel != null)
            uiSelectLevel.SetActive(false);

        if (imageMover != null)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToStart());
        }
    }

    public void LoadNextLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        // Save the trigger's position so that the player's last trigger can be restored later, scene-specifically.
        PlayerPrefs.SetFloat(sceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(sceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(sceneName + "_LastTriggerZ", transform.position.z);
        PlayerPrefs.SetString("LastTriggerScene", sceneName);

        // Wait a short moment before loading the next level.
        SKUtils.InvokeAction(0.2f, () =>
        {
            flowManager.LoadScene(new SceneInfo()
            {
                index = scenetitle,
            });
        });
        startloading = true;
    }
}
