using SKCell;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTrigger : MonoBehaviour
{
    [SerializeField] private ImageMover imageMover; // Reference to the ImageMover script
    [SerializeField] private GameObject uiSelectLevel; // Reference to the UI_SelectLevel GameObject
    private bool allowInput = false;
    private FlowManager flowManager;
    public SceneTitle scenetitle;
    private GameObject flowmanager;
    private bool startloading = false;

    private void Start()
    {
        flowmanager = GameObject.Find("FlowManager");
        flowManager = flowmanager.GetComponent<FlowManager>();

        // Ensure UI_SelectLevel starts as inactive
        if (uiSelectLevel != null)
        {
            uiSelectLevel.SetActive(false);
        }
    }

    private void Update()
    {
        if (allowInput && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("INPUT");
            LoadNextLevel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            allowInput = true;

            // Activate the UI_SelectLevel
            if (uiSelectLevel != null)
            {
                uiSelectLevel.SetActive(true);
            }

            if (imageMover != null)
            {
                print("moving to end");
                StopAllCoroutines();
                StartCoroutine(imageMover.MoveImageToEnd());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            allowInput = false;

            // Deactivate the UI_SelectLevel
            if (uiSelectLevel != null)
            {
                uiSelectLevel.SetActive(false);
            }

            if (imageMover != null)
            {
                print("moving to start");
                StopAllCoroutines();
                StartCoroutine(imageMover.MoveImageToStart());
            }
        }
    }

    private void LoadNextLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        // Save the current trigger's position when the player enters, using scene-specific keys
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
