using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardAcquire : MonoBehaviour
{

    public bool isReached = false;
    public SKDialoguePlayer dialoguePlayer;

    private GameObject player1;
    private CharacterMovement movement;

    private LevelController levelController;
    // Start is called before the first frame update
    void Start()
    {
        GameObject LevelControll = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        if(LevelControll != null)
        {
            levelController = LevelControll.GetComponent<LevelController>();
        }

        player1 = GameObject.FindGameObjectWithTag("Player1");
        movement = player1.GetComponent<CharacterMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player1" || other.gameObject.tag == "Player2")
        {
            if (!isReached)
            {
                if (dialoguePlayer != null)
                {
                    DisablePlayer();
                    levelController.phase = LevelPhase.Speaking;
                    Debug.Log(levelController.phase);
                    dialoguePlayer.Play();

                    isReached = true;
                }
            }
        }

    }

    public void DisablePlayer()
    {
        movement.canmove = false;
        movement.is_sliding = false;
    }

    public void EnablePlayer()
    {
        levelController.phase = LevelPhase.Running;
        movement.canmove = true;
        movement.is_sliding = false;
    }

}
