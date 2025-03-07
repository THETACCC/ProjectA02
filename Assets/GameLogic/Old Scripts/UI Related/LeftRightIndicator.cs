using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftRightIndicator : MonoBehaviour
{
    public RectTransform RightSideGradient;
    public RectTransform LeftSideGradient;
    public LevelController levelController;
    public float lerpSpeed = 5f; // Lerp speed for smooth transition

    private Vector2 rightStartPosition = new Vector2(2000, 0); // Target position for RightSideGradient
    private Vector2 leftTargetPosition = new Vector2(0, 0);    // Target position for LeftSideGradient
    private Vector2 rightTargetPosition = new Vector2(0, 0);    // Start position for RightSideGradient
    private Vector2 leftStartPosition = new Vector2(-2000, 0);
    // Start is called before the first frame update
    void Start()
    {
        GameObject controllerOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        levelController = controllerOBJ.GetComponent<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(levelController.phase == LevelPhase.Running)
        {
            if(levelController.isPlayingRight)
            {
                // Lerp RightSideGradient to 2000 and LeftSideGradient to 0
                RightSideGradient.anchoredPosition = Vector2.Lerp(RightSideGradient.anchoredPosition, rightTargetPosition, Time.deltaTime * lerpSpeed);
                LeftSideGradient.anchoredPosition = Vector2.Lerp(LeftSideGradient.anchoredPosition, leftStartPosition, Time.deltaTime * lerpSpeed);
            }
            else
            {
                RightSideGradient.anchoredPosition = Vector2.Lerp(RightSideGradient.anchoredPosition, rightStartPosition, Time.deltaTime * lerpSpeed);
                LeftSideGradient.anchoredPosition = Vector2.Lerp(LeftSideGradient.anchoredPosition, leftTargetPosition, Time.deltaTime * lerpSpeed);
                // Lerp RightSideGradient to 0 and LeftSideGradient to 2000

            }
        }
    }
}
