using SKCell;
using System.Collections;
using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    [SerializeField] private ImageMover imageMover; // Reference to the ImageMover script

    private bool allowInput = false;
    private FlowManager flowManager;
    public SceneTitle scenetitle;
    public static int t_spawnPoint;
    private GameObject flowmanager;
    private bool startloading = false;

    private void Start()
    {
        flowmanager = GameObject.Find("FlowManager");
        flowManager = flowmanager.GetComponent<FlowManager>();





        // Print the location of the GameObject with FlowManager script
        if (flowmanager != null)
        {
            Debug.Log("FlowManager's location: " + flowmanager.transform.position);
        }



        // Ensure the image is hidden at the start
        if (imageMover != null)
        {
            imageMover.ResetImage();
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

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            allowInput = true;

            // Start moving the image from start to end position
            if (imageMover != null)
            {
                StartCoroutine(imageMover.MoveImageForward());
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            allowInput = false;

            // Move the image from end back to the start position
            if (imageMover != null)
            {
                StartCoroutine(imageMover.MoveImageBackward());
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
