using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using SKCell;

public class LevelTrigger : MonoBehaviour
{
    private CompleteUI[] completeUIs;
    private BouncyUI[] bouncyUIs;

    [SerializeField] private ImageMover imageMover;
    [SerializeField] private GameObject uiSelectLevel;

    private FlowManager flowManager;
    public SceneTitle scenetitle;
    private bool startloading = false;

    private Coroutine uiCoroutine = null;

    void Start()
    {
        completeUIs = GetComponentsInChildren<CompleteUI>(true);
        bouncyUIs = GetComponentsInChildren<BouncyUI>(true);

        GameObject flowmanagerObj = GameObject.Find("FlowManager");
        if (flowmanagerObj != null)
            flowManager = flowmanagerObj.GetComponent<FlowManager>();

        if (uiSelectLevel != null)
            uiSelectLevel.SetActive(false);

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
                if (uiCoroutine != null) StopCoroutine(uiCoroutine);
                uiCoroutine = StartCoroutine(AnimateUIShow());
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (uiCoroutine != null) StopCoroutine(uiCoroutine);
        uiCoroutine = StartCoroutine(AnimateUIShow());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (uiCoroutine != null) StopCoroutine(uiCoroutine);
        uiCoroutine = StartCoroutine(AnimateUIHide());
    }

    IEnumerator AnimateUIShow()
    {
        if (uiSelectLevel != null)
            uiSelectLevel.SetActive(true);

        if (imageMover != null)
        {
            StopAllCoroutines();
            StartCoroutine(imageMover.MoveImageToEnd());
        }

        foreach (CompleteUI ui in completeUIs)
            ui.InstantHide();  // Ensure starts hidden
        foreach (BouncyUI ui in bouncyUIs)
            ui.InstantHide();

        // Now animate them simultaneously:
        foreach (CompleteUI ui in completeUIs)
            StartCoroutine(ui.AnimateShow());
        foreach (BouncyUI ui in bouncyUIs)
            StartCoroutine(ui.AnimateShow());

        yield return new WaitForSeconds(0.35f); // Wait slightly longer than your animations
    }

    IEnumerator AnimateUIHide()
    {
        foreach (CompleteUI ui in completeUIs)
            StartCoroutine(ui.AnimateHide());
        foreach (BouncyUI ui in bouncyUIs)
            StartCoroutine(ui.AnimateHide());

        yield return new WaitForSeconds(0.25f); // Wait slightly longer than your animations

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
