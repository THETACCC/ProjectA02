using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SKCell;
using MoreMountains.Feedbacks;

public class StartRunningPhase : MonoBehaviour
{
    private GameObject levelcontroller;
    private LevelController controller;
    private Transform mytransform;

    private float startX = 0f; // Starting X position
    private float targetX = -29f; // Target X position
    private float lerpTime = 1f; // Time to complete the lerp
    private float lerpSpeed = 0.025f;
    private float currentLerpTime = 0f; // Current lerp time

    public MMFeedbacks PlayFeedBack;

    //Get level loader
    private LevelLoader levelLoader;

    // Start is called before the first frame update
    void Start()
    {

        GameObject level_loader = GameObject.Find("LevelLoader");
        levelLoader = level_loader.GetComponent<LevelLoader>();
        levelcontroller = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        controller = levelcontroller.GetComponent<LevelController>();
        mytransform = GetComponent<Transform>();
        /*
        Camera cam;
        CommonUtils.StartProcedure(SKCurve.QuadraticIn, 1.0f, (t) =>
        {
           cam.transform.position = Vector3.Lerp(sbyte, else, t);
        });
        */
    }

    // Update is called once per frame
    void Update()
    {



        if (controller.phase == LevelPhase.Running)
        {


            // Lerp the x position from startX to targetX
            mytransform.SetPositionZ(Mathf.Lerp(mytransform.position.z, targetX, lerpSpeed));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            levelLoader.AlignBlockSelection();
            if (levelLoader.checkindex != 0)
            {
                Debug.Log("pressed- Go");
                PlayFeedBack.PlayFeedbacks();
            }


        }
    }
}
