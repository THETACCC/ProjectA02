using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform; // Reference to the player's transform
    public Vector3 offset; // Offset of the camera from the player

    // Update is called once per frame
    void Update()
    {
        if (playerTransform != null)
        {
            // Set the camera's position to follow the player, but only on the X and Z axes
            Vector3 targetPosition = new Vector3(playerTransform.position.x + offset.x, transform.position.y, playerTransform.position.z + offset.z);
            transform.position = targetPosition;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }
}
