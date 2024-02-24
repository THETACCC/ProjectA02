using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPass : MonoBehaviour
{
    private Enterfinishleft left;
    private EnterFinishRight right;
    private FlowManager flowManager;
    public SceneTitle scenetitle;
    public static int t_spawnPoint;

    public GameObject leftfinish;
    public GameObject rightfinish;
    private GameObject flowmanager;

    private bool startloading = false;

    private void Start()
    {
        left = leftfinish.GetComponent<Enterfinishleft>();
        right = rightfinish.GetComponent<EnterFinishRight>();
        flowmanager = GameObject.Find("FlowManager");
        flowManager = flowmanager.GetComponent<FlowManager>();
    }

    private void Update()
    {

        if (left.leftreached && right.rightreached && !startloading)
        {
            Debug.Log("reached");
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
}
