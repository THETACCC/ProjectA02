using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightHouseControll : MonoBehaviour
{
    public GameObject myLight;
    public GameObject myLighthouse;
    public LevelController controller;
    public float rotationSpeed = 2.0f;
    private Quaternion targetRotation;
    private Quaternion targetRotationHouse;
    private bool isRight = true;


    // Start is called before the first frame update
    void Start()
    {
        //Get the Level Controller
        GameObject controllerOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        controller = controllerOBJ.GetComponent<LevelController>();

        targetRotation = Quaternion.Euler(myLight.transform.eulerAngles.x, 180, myLight.transform.eulerAngles.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.phase == LevelPhase.Placing)
        {
            myLighthouse.transform.eulerAngles = new Vector3(myLighthouse.transform.eulerAngles.x, 232, myLighthouse.transform.eulerAngles.z);
            myLight.transform.eulerAngles = new Vector3(myLight.transform.eulerAngles.x, 225, myLight.transform.eulerAngles.z);
        }
        if (controller.phase == LevelPhase.Running)
        {


            if (!controller.isPlayingRight)
            {
                targetRotation = Quaternion.Euler(myLight.transform.eulerAngles.x, 180, myLight.transform.eulerAngles.z);
                targetRotationHouse = Quaternion.Euler(myLighthouse.transform.eulerAngles.x, 195, myLighthouse.transform.eulerAngles.z);
            }
            else
            {
                targetRotation = Quaternion.Euler(myLight.transform.eulerAngles.x, 270, myLight.transform.eulerAngles.z);
                targetRotationHouse = Quaternion.Euler(myLighthouse.transform.eulerAngles.x, 262, myLighthouse.transform.eulerAngles.z);
            }

            // Smoothly lerp the current rotation to the target rotation over time
            myLight.transform.rotation = Quaternion.Lerp(myLight.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            myLighthouse.transform.rotation = Quaternion.Lerp(myLighthouse.transform.rotation, targetRotationHouse, rotationSpeed * Time.deltaTime);

        }
    }
}
