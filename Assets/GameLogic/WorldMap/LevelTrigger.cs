using SKCell;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;  // Add the necessary using directive for SceneManager

public class LevelTrigger : MonoBehaviour
{
    [SerializeField] private ImageMover imageMover; // Reference to the ImageMover script
    private bool allowInput = false;
    private FlowManager flowManager;
    public SceneTitle scenetitle;
    private GameObject flowmanager;
    private bool startloading = false;

    private void Start()
    {
        flowmanager = GameObject.Find("FlowManager");
        flowManager = flowmanager.GetComponent<FlowManager>();
    }

    private void Update()
    {
        if (allowInput && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("INPUT");
            LoadNextLevel();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            allowInput = true;

            // Save the current trigger's position when the player enters
            PlayerPrefs.SetFloat("LastTriggerX", transform.position.x);
            PlayerPrefs.SetFloat("LastTriggerY", transform.position.y);
            PlayerPrefs.SetFloat("LastTriggerZ", transform.position.z);
            PlayerPrefs.SetString("LastTriggerScene", SceneManager.GetActiveScene().name);

            if (imageMover != null)
            {
                print("moving to end");
                StopAllCoroutines();
                StartCoroutine(imageMover.MoveImageToEnd()); 
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            allowInput = false;

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
