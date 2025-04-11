using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using SKCell;

public class LevelTrigger : MonoBehaviour
{
    private CompleteUI[] completeUIs;
    private BouncyUI[] bouncyUIs;

    // Existing level-loading and UI fields
    [SerializeField] private ImageMover imageMover; // Reference to the ImageMover script.
    [SerializeField] private GameObject uiSelectLevel; // Reference to the UI_SelectLevel GameObject.
    private FlowManager flowManager;
    public SceneTitle scenetitle;
    private GameObject flowmanager;
    private bool startloading = false;

    void Start()
    {
        // Find all instances of CompleteUI and BouncyUI in children,
        // including disabled GameObjects (if any).
        completeUIs = GetComponentsInChildren<CompleteUI>(true);
        bouncyUIs = GetComponentsInChildren<BouncyUI>(true);

        // Existing FlowManager / UI_SelectLevel setup.
        flowmanager = GameObject.Find("FlowManager");
        if (flowmanager != null)
        {
            flowManager = flowmanager.GetComponent<FlowManager>();
        }
        if (uiSelectLevel != null)
        {
            uiSelectLevel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Allow input if needed.
            // Activate any additional UI elements, such as UI_SelectLevel.
            if (uiSelectLevel != null)
            {
                uiSelectLevel.SetActive(true);
            }
            // Trigger ImageMover movement if assigned.
            if (imageMover != null)
            {
                StopAllCoroutines();
                StartCoroutine(imageMover.MoveImageToEnd());
            }
            // Trigger all CompleteUI animations.
            foreach (CompleteUI ui in completeUIs)
            {
                ui.TriggerShow();
            }
            // Trigger all BouncyUI animations.
            foreach (BouncyUI ui in bouncyUIs)
            {
                ui.TriggerToTarget();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Deactivate any additional UI elements.
            if (uiSelectLevel != null)
            {
                uiSelectLevel.SetActive(false);
            }
            if (imageMover != null)
            {
                StopAllCoroutines();
                StartCoroutine(imageMover.MoveImageToStart());
            }
            // Trigger the hide/reset animations on all CompleteUI.
            foreach (CompleteUI ui in completeUIs)
            {
                ui.TriggerHide();
            }
            // Trigger the reset animations on all BouncyUI.
            foreach (BouncyUI ui in bouncyUIs)
            {
                ui.TriggerToStart();
            }
        }
    }

    // Optional level-loading functionality remains unchanged.
    public void LoadNextLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetFloat(sceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(sceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(sceneName + "_LastTriggerZ", transform.position.z);
        PlayerPrefs.SetString("LastTriggerScene", sceneName);

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
