using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftRightCameraSwing : MonoBehaviour
{

    public LevelController levelController;
    public GameObject myCamera;
    private Vector3 leftSwing = new Vector3(52, 60,10);
    private Vector3 rightSwing = new Vector3(52, 30, -10);
    private Vector3 OriginalSwing = new Vector3(45, 45, 0);

    public float lerpSpeed = 3f;
    public float lerpSpeedSwing = 5f;
    // Start is called before the first frame update
    void Start()
    {
        GameObject controllerOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        levelController = controllerOBJ.GetComponent<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(levelController.isAnyBlockDragging)
        {
            Vector3 mousePosition = Input.mousePosition;

            // Determine the middle of the screen
            float screenWidth = Screen.width;
            float middleOfScreen = screenWidth / 2;

            // If the mouse is on the left side of the screen, lerp to the left swing
            // If the mouse is on the left side of the screen, lerp to the left swing rotation
            /*
            if (mousePosition.x > middleOfScreen)
            {
                myCamera.transform.rotation = Quaternion.Lerp(
                    myCamera.transform.rotation,
                    Quaternion.Euler(leftSwing),
                    Time.deltaTime * lerpSpeed
                );
            }
            // If the mouse is on the right side of the screen, lerp to the right swing rotation
            else
            {
                myCamera.transform.rotation = Quaternion.Lerp(
                    myCamera.transform.rotation,
                    Quaternion.Euler(rightSwing),
                    Time.deltaTime * lerpSpeed
                );
            }
            */

            float distanceFromCenter = (mousePosition.x - middleOfScreen) / middleOfScreen;

            // Clamp the value between -1 and 1 (i.e., -1 is far left, 1 is far right)
            distanceFromCenter = Mathf.Clamp(distanceFromCenter, -1f, 1f);

            // Lerp between leftSwing and rightSwing based on the distance from center
            Vector3 targetRotation = Vector3.Lerp(rightSwing, leftSwing, (distanceFromCenter + 1) / 2);

            // Smoothly rotate the camera towards the target rotation
            myCamera.transform.rotation = Quaternion.Lerp(
                myCamera.transform.rotation,
                Quaternion.Euler(targetRotation),
                Time.deltaTime * lerpSpeedSwing
            );

        }
        else
        {
            // Lerp back to the original rotation if no block is being dragged
            myCamera.transform.rotation = Quaternion.Lerp(
                myCamera.transform.rotation,
                Quaternion.Euler(OriginalSwing),
                Time.deltaTime * lerpSpeed
            );
        }
    }
}
