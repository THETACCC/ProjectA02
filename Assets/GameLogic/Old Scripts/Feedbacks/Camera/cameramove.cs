using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SKCell;


public class cameramove : MonoBehaviour
{
    private GameObject levelcontroller;
    private GameObject mainCamera;
    public LevelController controller;
    private Transform targettransform;
        //Get level loader
    private LevelLoader levelLoader;

    private float startZ = 0f; // Starting X position
    public float targetZ = -10f; // Target X position
    private float lerpSpeed = 0.005f;

    // Start is called before the first frame update
    void Start()
    {
        GameObject level_loader = GameObject.Find("LevelLoader");
        levelLoader = level_loader.GetComponent<LevelLoader>();
        mainCamera = GameObject.Find("myVirtualCamera");
        targettransform = mainCamera.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

         if (controller.phase == LevelPhase.Running)
            {

            //Debug.Log("MOVINg");
            // Lerp the x position from startX to targetX
                targettransform.SetPositionZ(Mathf.Lerp(targettransform.position.z, targetZ, lerpSpeed));
            }  

        
    }
}
