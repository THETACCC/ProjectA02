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
        if (isReached) return; // already collected

        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            // Hide the 3D reward visual
            if (myRenderer != null)
                myRenderer.enabled = false;

            // Mark as collected so RewardManager sees it
            isReached = true;

            // OPTIONAL: if you previously paused the level on pickup, make sure we stay in Running
            // if (levelController != null) levelController.phase = LevelPhase.Running;

            // OPTIONAL: if you DON'T want to stop or align players anymore, remove these:
            // Stop any pending coroutines and skip alignment
            // StopAllCoroutines();
            // (do nothing else)
        }
    }


    public void DisablePlayer()
    {
        movement.canmove = false;
        movement.is_sliding = false;
        movement_player2.canmove = false;
        movement_player2.is_sliding = false;
    }


    private IEnumerator WaitAndAlignPlayers()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("REWARD STOPPPPP");
        // after 1s align players
        DisablePlayer();
        DisablePlayer2();
        movement.AlignPlayerFunction();
        movement_player2.AlignPlayerFunction();


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
