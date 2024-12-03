/*using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SKCell;

public class MenuController : MonoBehaviour
{
    [Header("Levels To Load - Chapter 1 World")]
    [SerializeField] private SceneTitle sceneToLoad_Chp1_World;  // Pick from available SceneTitles
    private bool startLoading = false;
    private FlowManager flowManager;
    private GameObject flowmanager;

    [SerializeField] private GameObject noSavedGameDialog = null;

    private int uiLayerMask;

    private void Start()
    {
        flowmanager = GameObject.Find("FlowManager");
        flowManager = flowmanager.GetComponent<FlowManager>();

        // Set the layer mask to include only the UI layer
        uiLayerMask = LayerMask.GetMask("UI");
    }

    public void newGameDialogYes()
    {
        // Load the scene with a delay using the SceneTitle
        LoadNextLevel(sceneToLoad_Chp1_World);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        // Disable raycasts on everything except the UI
        EnableOnlyUILayerRaycasts(true);

        // Optionally, stop the time in-game
        Time.timeScale = 0;  // Freezes the game
    }

    public void ResumeGame()
    {
        // Re-enable raycasts for all layers
        EnableOnlyUILayerRaycasts(false);

        // Resume time in the game
        Time.timeScale = 1;  // Unfreeze the game
    }

    private void EnableOnlyUILayerRaycasts(bool uiOnly)
    {
        // Set the event system's raycast blocking based on uiOnly
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;

        if (eventSystem != null)
        {
            // Enable or disable raycast target on all non-UI objects
            foreach (var obj in FindObjectsOfType<Collider2D>())
            {
                if (uiOnly)
                {
                    // Disable raycast target for non-UI objects
                    if (obj.gameObject.layer != LayerMask.NameToLayer("UI"))
                    {
                        obj.GetComponent<Collider2D>().enabled = false;  // Disable collider
                    }
                }
                else
                {
                    // Re-enable all colliders
                    obj.GetComponent<Collider2D>().enabled = true;  // Enable collider
                }
            }
        }
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
*/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SKCell;
using static UnityEditor.VersionControl.Message;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Unity.VisualScripting;
using UnityEditor.PackageManager;

public class MenuController : MonoBehaviour
{
    [Header("Levels To Load - Chapter 1 World")]
    [SerializeField] private SceneTitle sceneToLoad_Chp1_World;  // Pick from available SceneTitles
    private bool startLoading = false;
    private FlowManager flowManager;
    private GameObject flowmanager;

    [SerializeField] private GameObject noSavedGameDialog = null;

    private int uiLayerMask;
    private bool gamePaused = false;

    private void Start()
    {
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
            // Disable raycasts on everything except the UI
            EnableOnlyUILayerRaycasts(true);

            // Optionally, stop the time in-game
            Time.timeScale = 0;  // Freezes the game

            // Disable interactions outside of UI
            DisableInputOutsideUI(true);

            gamePaused = true;
        }
    }

    public void ResumeGame()
    {
        if (gamePaused)
        {
            // Re-enable raycasts for all layers
            EnableOnlyUILayerRaycasts(false);

            // Resume time in the game
            Time.timeScale = 1;  // Unfreeze the game

            // Re-enable interactions outside of UI
            DisableInputOutsideUI(false);

            gamePaused = false;
        }
    }

    private void EnableOnlyUILayerRaycasts(bool uiOnly)
    {
        // Set the event system's raycast blocking based on uiOnly
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;

        if (eventSystem != null)
        {
            // Enable or disable raycast target on all non-UI objects
            foreach (var obj in FindObjectsOfType<Collider2D>())
            {
                if (uiOnly)
                {
                    // Disable raycast target for non-UI objects
                    if (obj.gameObject.layer != LayerMask.NameToLayer("UI"))
                    {
                        obj.GetComponent<Collider2D>().enabled = false;  // Disable collider
                    }
                }
                else
                {
                    // Re-enable all colliders
                    obj.GetComponent<Collider2D>().enabled = true;  // Enable collider
                }
            }
        }
    }

    // Disable or Enable input outside of UI
    private void DisableInputOutsideUI(bool disable)
    {
        // Get all draggable objects or interactive components and disable them during pause
        var dragObjects = FindObjectsOfType<Block>();  // Replace with actual drag-drop component or similar
        foreach (var dragObject in dragObjects)
        {
            dragObject.enabled = !disable;  // Disable drag functionality while paused
        }

        var playerMoveScripts = FindObjectsOfType<SubPositionIndicator>();  // Example, adjust for your player scripts
        foreach (var script in playerMoveScripts)
        {
            script.enabled = !disable;  // Disable movement during pause
        }
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
