using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorMove : MonoBehaviour
{
    public float height = 1.0f; // Height of the up and down movement
    public float speed = 1.0f;  // Speed of the movement

    private Vector3 initialPosition; // Stores the initial position of the object

    void Start()
    {
        // Record the initial position of the object
        initialPosition = transform.position;
    }

    void Update()
    {
        // Smoothly move the object up and down
       // float newY = Mathf.Lerp(initialPosition.y - height, initialPosition.y + height, Mathf.PingPong(Time.time * speed, 1));
        //transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Freeze world rotation by resetting rotation to its initial state
        transform.rotation = Quaternion.identity;
    }
}
