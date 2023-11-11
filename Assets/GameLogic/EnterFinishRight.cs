using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterFinishRight : MonoBehaviour
{
    public bool rightreached = false;
    private GameObject player2;
    private CharacterMovement movement;

    private void Start()
    {
        player2 = GameObject.FindGameObjectWithTag("Player2");
        movement = player2.GetComponent<CharacterMovement>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player2" || collision.gameObject.tag == "Player1")
        {
            movement.canmove = false;
            movement.is_sliding = false;
            rightreached = true;
        }
    }
}
