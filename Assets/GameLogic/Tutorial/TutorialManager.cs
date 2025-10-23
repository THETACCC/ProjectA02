using Cinemachine;
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

    //Tutorial Objects Active
    public GameObject TutorialMoveLow;

    //Tutorial Objects Active Phase 2
    public EnterFinishRight enterFinishRight;
    public bool isPositionToRight2 = false;
    private bool isTransitioning2 = false;

    public GameObject myFinalVectorPos;
    public CinemachineVirtualCamera myCamera;

    public GameObject TutorialMoveHigh2;
    public GameObject TutorialMoveLow2;
    public GameObject FinalTutorialSpeak;


    //Tutorial Showcase
    public GameObject TutorialUI;
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

            if(Vector3.Distance(myMainCamera.transform.position, myMainCameraRightPos) < 10f)
            {
                TutorialMoveLow.SetActive(true);
            }

            // Check if camera is close enough to target
            if (Vector3.Distance(myMainCamera.transform.position, myMainCameraRightPos) < 0.1f)
            {
                myMainCamera.transform.position = myMainCameraRightPos; // snap to final position
                isPositionToRight = true;

                isTransitioning = false;
            }
        }

        //Phase 2
        if (enterFinishRight.rightreached && !isPositionToRight2 && !isTransitioning2)
        {
            StartCoroutine(DisableTutorialsAfterDelay(0.2f)); // <---- delayed disable
            isTransitioning2 = true;
        }

        // Smoothly move camera to target position
        if (isTransitioning2)
        {
            myMainCamera.transform.position = Vector3.Lerp(
                myMainCamera.transform.position,
                myFinalVectorPos.transform.position,
                Time.deltaTime * transitionSpeed
            );


            myCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                myCamera.m_Lens.OrthographicSize,
                35,
                Time.deltaTime * transitionSpeed
            );

            // Check if camera is close enough to target
            if (Vector3.Distance(myMainCamera.transform.position, myMainCameraRightPos) < 0.1f)
            {
                myMainCamera.transform.position = myMainCameraRightPos; // snap to final position
                isPositionToRight2 = true;

                isTransitioning2 = false;
            }
        }
    }

    // Coroutine that waits 0.5 seconds, then disables tutorial objects
    private IEnumerator DisableTutorialsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TutorialMoveHigh2.SetActive(false);
        TutorialMoveLow2.SetActive(false);
        FinalTutorialSpeak.SetActive(true);
    }

}
