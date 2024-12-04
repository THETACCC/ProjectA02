using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class EnterFinishRight : MonoBehaviour
{
    public bool rightreached = false;
    private GameObject player2;
    private PlayerController movement;

    private PlayerWinVisual playerWinVisual;
    private GameObject PlayerWinVisual2;
    private void Start()
    {

    }
    public void SerachPlayer()
    {
        player2 = GameObject.FindGameObjectWithTag("Player2");
        movement = player2.GetComponent<PlayerController>();
        PlayerWinVisual2 = GameObject.FindGameObjectWithTag("PlayerWinVisual2");
        playerWinVisual = PlayerWinVisual2.GetComponent<PlayerWinVisual>();
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player2" || collision.gameObject.tag == "Player1")
        {
            movement.canmove = false;
            movement.is_sliding = false;
            rightreached = true;
            playerWinVisual.isPlayerWin = true;
        }
    }
}
