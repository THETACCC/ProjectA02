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


    void Start()
    {
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
        if (Input.GetKeyDown(KeyCode.G)) 
        {
            SwitchPlayer();
            Debug.Log("Switched to: " + currentPlayer);
        }
    }

}
