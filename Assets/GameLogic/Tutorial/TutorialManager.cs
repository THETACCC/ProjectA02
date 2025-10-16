using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public Enterfinishleft enterfinishleft;
    public Camera myMainCamera;

    public Vector3 myMainCameraRightPos;
    public GameObject myRightCollider;

    public bool isPositionToRight = false;
    public float transitionSpeed = 2f; // speed of camera movement

    private bool isTransitioning = false;

    void Update()
    {
        // Trigger the transition when left side is reached
        if (enterfinishleft.leftreached && !isPositionToRight && !isTransitioning)
        {
            myRightCollider.SetActive(false);
            isTransitioning = true;
        }

        // Smoothly move camera to target position
        if (isTransitioning)
        {
            myMainCamera.transform.position = Vector3.Lerp(
                myMainCamera.transform.position,
                myMainCameraRightPos,
                Time.deltaTime * transitionSpeed
            );

            // Check if camera is close enough to target
            if (Vector3.Distance(myMainCamera.transform.position, myMainCameraRightPos) < 0.01f)
            {
                myMainCamera.transform.position = myMainCameraRightPos; // snap to final position
                isPositionToRight = true;
                isTransitioning = false;
            }
        }
    }
}
