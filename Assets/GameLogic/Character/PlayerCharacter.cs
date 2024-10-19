using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Player
{
    Player1,
    Player2
}

public class PlayerCharacter : MonoBehaviour
{
    // Current player being controlled
    public Player currentPlayer = Player.Player1;


    [Header("Player Object")]
    public GameObject _player1;
    public GameObject _player2;

    //Get the reference of player's character movement sciprt
    private CharacterMovement _player1Movement;
    private CharacterMovement _player2Movement;

    public LevelController controller;
    //Reference to Outline
    public Outline outlineLeft;
    public Outline outlineRight;
    public GameObject outlineOBJLeft;
    public GameObject outlineOBJRight;
    float minThreshold = 1f;
    public float OutlineSpeed = 10f;
    void Start()
    {
        outlineOBJLeft = GameObject.FindGameObjectWithTag("PlayerLeftOutline");
        outlineLeft = outlineOBJLeft.GetComponent<Outline>();
        outlineOBJRight = GameObject.FindGameObjectWithTag("PlayerRightOutline");
        outlineRight = outlineOBJRight.GetComponent<Outline>();
        outlineRight.outlineWidth = 0f;
        outlineLeft.outlineWidth = 0f;
        //Get the Level Controller
        GameObject controllerOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        controller = controllerOBJ.GetComponent<LevelController>();

        _player1 = GameObject.FindGameObjectWithTag("Player1");
        _player1Movement = _player1.GetComponent<CharacterMovement>();
        _player2 = GameObject.FindGameObjectWithTag("Player2");
        _player2Movement = _player2.GetComponent<CharacterMovement>();
        SwitchPlayer();
    }



    public void SwitchPlayer()
    {
        switch (currentPlayer)
        {
            case Player.Player1:
                currentPlayer = Player.Player2;
                _player1Movement.canmove = true;
                _player2Movement.canmove = false;
                // Add logic here for when Player 1 is active
                break;
            case Player.Player2:
                currentPlayer = Player.Player1;
                _player1Movement.canmove = false;
                _player2Movement.canmove = true;
                // Add logic here for when Player 2 is active
                break;
        }
    }


    void Update()
    {
        if(controller.phase == LevelPhase.Running)
        {
            /*
            if (Input.GetKeyDown(KeyCode.G))
            {
                SwitchPlayer();
                Debug.Log("Switched to: " + currentPlayer);
            }
            */

            if(currentPlayer == Player.Player1)
            {
                outlineRight.needsUpdate = true;
                outlineLeft.needsUpdate = true;
                //outlineRight.outlineWidth = 0f;
                //outlineLeft.outlineWidth = 10f;
                

                outlineRight.outlineWidth = Mathf.Lerp(outlineRight.outlineWidth, 10f, OutlineSpeed * Time.deltaTime);

                // Lerp the left outline to 0
                outlineLeft.outlineWidth = Mathf.Lerp(outlineLeft.outlineWidth, 0f, OutlineSpeed * Time.deltaTime);
                if (Mathf.Abs(outlineLeft.outlineWidth - 0f) < minThreshold)
                {
                    outlineLeft.outlineWidth = 0f; // Snap to exact 0 when close enough
                }
            }
            else
            {
                outlineRight.needsUpdate = true;
                outlineLeft.needsUpdate = true;
                //outlineRight.outlineWidth = 10f;
                //outlineLeft.outlineWidth = 0f;

                outlineLeft.outlineWidth = Mathf.Lerp(outlineLeft.outlineWidth, 10f, OutlineSpeed * Time.deltaTime);
                outlineRight.outlineWidth = Mathf.Lerp(outlineRight.outlineWidth, 0f, OutlineSpeed * Time.deltaTime);
                if (Mathf.Abs(outlineRight.outlineWidth - 0f) < minThreshold)
                {
                    outlineRight.outlineWidth = 0f; // Snap to exact 0 when close enough
                }

            }

        }




    }

}
