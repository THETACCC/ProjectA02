using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardAcquire : MonoBehaviour
{

    public bool isReached = false;
    public SKDialoguePlayer dialoguePlayer;

    private GameObject player1;
    private PlayerController movement;
    private GameObject player2;
    private PlayerController movement_player2;


    private LevelController levelController;
    private MeshRenderer myRenderer;

    private bool isPlayer1 = false;
    // Start is called before the first frame update
    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();
        GameObject LevelControll = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        if(LevelControll != null)
        {
            levelController = LevelControll.GetComponent<LevelController>();
        }

        player1 = GameObject.FindGameObjectWithTag("Player1");
        movement = player1.GetComponent<PlayerController>();
        player2 = GameObject.FindGameObjectWithTag("Player2");
        movement_player2 = player2.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
        if (other.gameObject.tag == "Player1" )
        {
            if (!isReached)
            {
                if (dialoguePlayer != null)
                {

                    DisablePlayer();
                    levelController.phase = LevelPhase.Speaking;
                    Debug.Log(levelController.phase);
                    dialoguePlayer.Play();
                    //This renderer makes the visual of the reward disappear
                    myRenderer.enabled= false;
                    isReached = true;
                    isPlayer1 = true;
                }
            }
        }
        else if (other.gameObject.tag == "Player2")
        {
            if (!isReached)
            {
                if (dialoguePlayer != null)
                {

                    DisablePlayer2();
                    levelController.phase = LevelPhase.Speaking;
                    Debug.Log(levelController.phase);
                    dialoguePlayer.Play();
                    //This renderer makes the visual of the reward disappear
                    myRenderer.enabled = false;
                    isReached = true;
                    isPlayer1 = false;
                }
            }
        }

    }

    public void DisablePlayer()
    {
        movement.canmove = false;
        movement.is_sliding = false;
        movement_player2.canmove = false;
        movement_player2.is_sliding = false;
    }

    public void EnablePlayer()
    {
        levelController.phase = LevelPhase.Running;
        movement.canmove = true;
        movement.is_sliding = false;
        movement_player2.canmove = true;
        movement_player2.is_sliding = false;


    }


    public void DisablePlayer2()
    {
        movement.canmove = false;
        movement.is_sliding = false;
        movement_player2.canmove = false;
        movement_player2.is_sliding = false;
    }

    public void EnablePlayer2()
    {
        levelController.phase = LevelPhase.Running;
        movement.canmove = true;
        movement.is_sliding = false;
        movement_player2.canmove = true;
        movement_player2.is_sliding = false;
    }
}
