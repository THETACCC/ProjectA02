using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enterfinishleft : MonoBehaviour
{
    public bool leftreached = false;
    private GameObject player1;
    private CharacterMovement movement;
    private void Start()
    {
        player1 = GameObject.FindGameObjectWithTag("Player1");
        movement = player1.GetComponent<CharacterMovement>();
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player1" || collision.gameObject.tag == "Player2")
        {
            movement.canmove = false;
            movement.is_sliding = false;
            leftreached = true;
        }
    }
}
