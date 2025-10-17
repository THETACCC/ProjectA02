using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterFinishLeftTutorial : MonoBehaviour
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
        player1 = GameObject.FindGameObjectWithTag("Player1Tutorial");
        movement = player1.GetComponent<PlayerController>();
        PlayerWin = GameObject.FindGameObjectWithTag("Player1WinVisualTutorial");
        playerWinVisual = PlayerWin.GetComponent<PlayerWinVisual>();
        PlayerCollider = player1.GetComponent<Collider>();
        Rigidbody = player1.GetComponent<Rigidbody>();

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player1Tutorial" || collision.gameObject.tag == "Player2Tutorial")
        {
            Rigidbody.useGravity = false;
            Rigidbody.velocity = Vector3.zero;
            PlayerCollider.enabled = false;
            movement.is_sliding = false;
            movement.canmove = false;

            movement.enabled = false;
            leftreached = true;
            playerWinVisual.isPlayerWin = true;


        }
    }
}
