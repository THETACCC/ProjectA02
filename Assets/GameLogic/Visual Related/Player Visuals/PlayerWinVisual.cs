using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;


public class PlayerWinVisual : MonoBehaviour
/*{

    public MeshRenderer myRenderer;
    Material myMaterial;

    public bool isPlayerWin = false;     // Set this to true when the player wins
    private bool isEffectPlayed = false; // This becomes true after the effect is completed
    private bool isEffectReversed = false;

    public float dissolveOverTime = 0.5f; // Speed of the effect transition
    private float effectTime = 2f;


    public GameObject PlayerVisual;

    // Start is called before the first frame update
    void Start()
    {
        myMaterial = myRenderer.material;
        //print(myMaterial);
        myMaterial.SetFloat("_Effect_Time", 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            isPlayerWin = true;

        }

        // Check if the player has won and the effect has not yet been played
        if (isPlayerWin && !isEffectPlayed)
        {
            // Transition the Effect_Time value
            if (!isEffectReversed)
            {
                // Gradually decrease the Effect_Time from 2f to -2f
                effectTime -= dissolveOverTime * Time.deltaTime;
                myMaterial.SetFloat("_Effect_Time", effectTime);

                // If Effect_Time reaches -2f, start reversing the effect
                if (effectTime <= -2f)
                {
                    isEffectReversed = true;
                    PlayerVisual.SetActive(false);
                }
            }
            else
            {
                // Gradually increase the Effect_Time back to 2f
                effectTime += dissolveOverTime * Time.deltaTime;
                myMaterial.SetFloat("_Effect_Time", effectTime);

                // If Effect_Time reaches 2f again, mark the effect as played
                if (effectTime >= 2f)
                {
                    isEffectPlayed = true;
                }
            }
        }
    }
}
*/
{ 
public MeshRenderer myRenderer;
private Material myMaterial;

public bool isPlayerWin = false;     // Set this to true when the player wins
private bool isEffectPlayed = false; // This becomes true after the effect is completed
private bool isEffectReversed = false;
    private bool isFeedbackPlayed = false;


public AnimationCurve effectCurve;   // The curve controlling the transition
public float transitionDuration = 1f; // Total time to complete the effect (forward or backward)
private float transitionTimer = 0f;

public GameObject PlayerVisual;

private bool isForward = true;  // To control the direction of the curve
public MMFeedbacks playerWin;

void Start()
{
    myMaterial = myRenderer.material;
    // Set initial Effect_Time value
    myMaterial.SetFloat("_Effect_Time", 1f);
}

void Update()
{
    if (Input.GetKeyDown(KeyCode.U))
    {
        isPlayerWin = true;
    }

    // Check if the player has won and the effect has not yet been played
    if (isPlayerWin && !isEffectPlayed)
    {
            if(playerWin!= null && !isFeedbackPlayed) 
            {
                playerWin?.PlayFeedbacks();
                isFeedbackPlayed = true;
            }



        // Update transition timer within the given duration
        transitionTimer += Time.deltaTime / transitionDuration;

        // Ensure the transition timer remains within [0, 1] bounds for Evaluate() usage
        transitionTimer = Mathf.Clamp01(transitionTimer);

        // Evaluate the curve based on the normalized timer
        if (isForward)
        {
            // Forward direction: Curve from 2f to -2f
            float effectTime = Mathf.Lerp(1f, -1f, effectCurve.Evaluate(transitionTimer));
            myMaterial.SetFloat("_Effect_Time", effectTime);

            // If the curve has reached the end (i.e., -2f), start the reverse effect
            if (transitionTimer >= 1f)
            {
                isForward = false;  // Start reversing the effect
                transitionTimer = 0f;  // Reset the timer
                PlayerVisual.SetActive(false);  // Deactivate PlayerVisual
            }
        }
        else
        {
            // Reverse direction: Curve from -2f to 2f
            float effectTime = Mathf.Lerp(-1f, 1f, effectCurve.Evaluate(transitionTimer));
            myMaterial.SetFloat("_Effect_Time", effectTime);

            // If the curve has reached the end (i.e., 2f), mark the effect as played
            if (transitionTimer >= 1f)
            {
                isEffectPlayed = true;  // Mark that the effect is completed
            }
        }
    }
}
}
