using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
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
    }

    private void Update()
    {
        if(allowInput)
        {
            if(Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("INPUT");
                LoadNextLevel();
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            allowInput = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            allowInput = false;
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

