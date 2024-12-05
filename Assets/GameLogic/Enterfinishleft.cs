using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enterfinishleft : MonoBehaviour
{
    public bool leftreached = false;
    private GameObject player1;
    private PlayerController movement;

    private PlayerWinVisual playerWinVisual;
    private GameObject PlayerWin;
    private Collider PlayerCollider;
    private Collider PlayerColliderCLD;
    private Rigidbody Rigidbody;
    private void Start()
    {

    }

    public void SerachPlayer()
    {
        player1 = GameObject.FindGameObjectWithTag("Player1");
        movement = player1.GetComponent<PlayerController>();
        PlayerWin = GameObject.FindGameObjectWithTag("PlayerWinVisual");
        playerWinVisual = PlayerWin.GetComponent<PlayerWinVisual>();
        PlayerCollider = player1.GetComponent<Collider>();
        Rigidbody = player1.GetComponent<Rigidbody>();

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player1" || collision.gameObject.tag == "Player2")
        {
            movement.canmove = false;
            movement.is_sliding = false;
            leftreached = true;
            playerWinVisual.isPlayerWin = true;
            PlayerCollider.enabled = false;
            Rigidbody.useGravity = false;
        }
    }
}
