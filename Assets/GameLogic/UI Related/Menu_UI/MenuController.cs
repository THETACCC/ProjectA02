using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SKCell;

public class MenuController : MonoBehaviour
{
    [Header("Levels To Load")]
    [SerializeField] private SceneTitle sceneToLoad;  // Pick from available SceneTitles
    private bool startLoading = false;
    private FlowManager flowManager;
    private GameObject flowmanager;

    [SerializeField] private GameObject noSavedGameDialog = null;

    private void Start()
    {
        flowmanager = GameObject.Find("FlowManager");
        flowManager = flowmanager.GetComponent<FlowManager>();
    }

    public void newGameDialogYes()
    {
        // Load the scene with a delay using the SceneTitle
        LoadNextLevel(sceneToLoad);
    }

    /*
    public void LoadGameDialogYes()
    {
        if (PlayerPrefs.HasKey("SavedLevel"))
        {
            // Load the saved level with a delay
            string savedLevel = PlayerPrefs.GetString("SavedLevel");
            SceneTitle savedSceneTitle = (SceneTitle)System.Enum.Parse(typeof(SceneTitle), savedLevel); // Convert string to SceneTitle
            LoadNextLevel(savedSceneTitle);
        }
        else
        {
            noSavedGameDialog.SetActive(true);
        }
    }
    */

    public void ExitButton()
    {
        Application.Quit();
    }

    private void LoadNextLevel(SceneTitle sceneTitle)
    {
        // Use SKUtils.InvokeAction to add a delay before loading the scene
        SKUtils.InvokeAction(0.2f, () =>
        {
            flowManager.LoadScene(new SceneInfo()
            {
                index = sceneTitle,  // Load the scene by SceneTitle
            });
        });
        startLoading = true;
    }
}
