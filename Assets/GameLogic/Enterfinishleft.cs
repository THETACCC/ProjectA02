using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enterfinishleft : MonoBehaviour
{
    public bool leftreached = false;
    private GameObject player1;
    private CharacterMovement movement;

    private PlayerWinVisual playerWinVisual;
    private GameObject PlayerWin;

    private void Start()
    {

    }

    public void SerachPlayer()
    {
        player1 = GameObject.FindGameObjectWithTag("Player1");
        movement = player1.GetComponent<CharacterMovement>();
        PlayerWin = GameObject.FindGameObjectWithTag("PlayerWinVisual");
        playerWinVisual = PlayerWin.GetComponent<PlayerWinVisual>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player1" || collision.gameObject.tag == "Player2")
        {
            movement.canmove = false;
            movement.is_sliding = false;
            leftreached = true;
            playerWinVisual.isPlayerWin = true;
        }
    }
}
