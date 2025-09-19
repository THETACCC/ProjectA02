using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnterFinishRight : MonoBehaviour
{
    public bool rightreached = false;
    private GameObject player2;
    private PlayerController movement;

    private PlayerWinVisual playerWinVisual;
    private GameObject PlayerWinVisual2;
    private Collider PlayerCollider;
    private Rigidbody Rigidbody;
    private void Start()
    {

    }
    public void SerachPlayer()
    {
        player2 = GameObject.FindGameObjectWithTag("Player2");
        movement = player2.GetComponent<PlayerController>();
        PlayerWinVisual2 = GameObject.FindGameObjectWithTag("PlayerWinVisual2");
        playerWinVisual = PlayerWinVisual2.GetComponent<PlayerWinVisual>();
        PlayerCollider = player2.GetComponent<Collider>();
        Rigidbody = player2.GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player2" || collision.gameObject.tag == "Player1")
        {
            Rigidbody.useGravity = false;
            Rigidbody.velocity = Vector3.zero;
            PlayerCollider.enabled = false;
            movement.is_sliding = false;
            movement.canmove = false;

            movement.enabled= false;
            rightreached = true;
            playerWinVisual.isPlayerWin = true;

        }
    }
}
