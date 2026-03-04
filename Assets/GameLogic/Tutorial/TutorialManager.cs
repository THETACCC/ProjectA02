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
    public float transitionSpeed = 2f;

    private bool isTransitioning = false;

    public GameObject TutorialMoveLow;

    // Phase 2
    public EnterFinishRight enterFinishRight;
    public bool isPositionToRight2 = false;
    private bool isTransitioning2 = false;

    public GameObject myFinalVectorPos;
    public CinemachineVirtualCamera myCamera;

    public GameObject TutorialMoveHigh2;
    public GameObject TutorialMoveLow2;
    public GameObject FinalTutorialSpeak;

    public GameObject TutorialUI;

    [Header("Snap Protection")]
    [Tooltip("If camera gets within this distance, force snap to the target.")]
    public float snapDistance = 0.25f;

    [Tooltip("If transition runs longer than this, force snap to the target.")]
    public float maxTransitionTime = 3f;

    private float t1StartTime;
    private float t2StartTime;

    void Update()
    {
        // Phase 1 trigger
        if (enterfinishleft.leftreached && !isPositionToRight && !isTransitioning)
        {
            if (myRightCollider != null) myRightCollider.SetActive(false);
            isTransitioning = true;
            t1StartTime = Time.time;
        }

        // Phase 1 movement
        if (isTransitioning)
        {
            Vector3 target = myMainCameraRightPos;

            myMainCamera.transform.position = Vector3.Lerp(
                myMainCamera.transform.position,
                target,
                Time.deltaTime * transitionSpeed
            );

            if (Vector3.Distance(myMainCamera.transform.position, target) < 10f && TutorialMoveLow != null)
                TutorialMoveLow.SetActive(true);

            // Extra protection: snap if near OR if taking too long
            ForceSnapIfNearOrStuck(
                target,
                ref isTransitioning,
                ref isPositionToRight,
                t1StartTime
            );
        }

        // Phase 2 trigger
        if (enterFinishRight.rightreached && !isPositionToRight2 && !isTransitioning2)
        {
            StartCoroutine(DisableTutorialsAfterDelay(0.2f));
            isTransitioning2 = true;
            t2StartTime = Time.time;
        }

        // Phase 2 movement
        if (isTransitioning2 && myFinalVectorPos != null)
        {
            Vector3 target = myFinalVectorPos.transform.position;

            myMainCamera.transform.position = Vector3.Lerp(
                myMainCamera.transform.position,
                target,
                Time.deltaTime * transitionSpeed
            );

            if (myCamera != null)
            {
                myCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                    myCamera.m_Lens.OrthographicSize,
                    35f,
                    Time.deltaTime * transitionSpeed
                );
            }

            // FIX: compare to FINAL target, and snap to FINAL target
            ForceSnapIfNearOrStuck(
                target,
                ref isTransitioning2,
                ref isPositionToRight2,
                t2StartTime
            );
        }
    }

    private void ForceSnapIfNearOrStuck(
        Vector3 target,
        ref bool isTransitioningFlag,
        ref bool reachedFlag,
        float startTime
    )
    {
        float dist = Vector3.Distance(myMainCamera.transform.position, target);

        if (dist <= snapDistance || (Time.time - startTime) >= maxTransitionTime)
        {
            myMainCamera.transform.position = target; // snap
            reachedFlag = true;
            isTransitioningFlag = false;
        }
    }

    private IEnumerator DisableTutorialsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (TutorialMoveHigh2 != null) TutorialMoveHigh2.SetActive(false);
        if (TutorialMoveLow2 != null) TutorialMoveLow2.SetActive(false);
        if (FinalTutorialSpeak != null) FinalTutorialSpeak.SetActive(true);
    }
}