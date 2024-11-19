using SKCell;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            // Save the current trigger's position when the player enters
            PlayerPrefs.SetFloat("LastTriggerX", transform.position.x);
            PlayerPrefs.SetFloat("LastTriggerY", transform.position.y);
            PlayerPrefs.SetFloat("LastTriggerZ", transform.position.z);
            PlayerPrefs.SetString("LastTriggerScene", SceneManager.GetActiveScene().name);

            Debug.Log("Saved Player Position: " + transform.position);

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
