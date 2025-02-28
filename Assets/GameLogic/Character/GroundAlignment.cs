using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundAlignment : MonoBehaviour
{
    public GameObject currentCollidingObject;
    public Rigidbody parentObject;
    public string allowedLayer = "Collision"; // Only detect objects on this layer

    private void OnTriggerEnter(Collider collision)
    {
        // Ignore objects that are NOT in the "Ground" layer
        if (collision.gameObject.layer != LayerMask.NameToLayer(allowedLayer))
        {
            Debug.Log("Ignored collision with: " + collision.gameObject.name);
            return; // Exit function early
        }

        // Ensure the object has a valid Collider component
        if (!collision.TryGetComponent(out Collider objCollider))
        {
            Debug.Log("Ignored object without a collider: " + collision.gameObject.name);
            return; // Exit function early
        }

        // Store the valid colliding object
        currentCollidingObject = collision.gameObject;
        Debug.Log("Collided with valid ground object: " + currentCollidingObject.name);
    }

    private void OnTriggerStay(Collider collision)
    {
        // Ignore objects that are NOT in the "Ground" layer
        if (collision.gameObject.layer != LayerMask.NameToLayer(allowedLayer))
            return;

        // Ensure the object has a valid Collider component
        if (!collision.TryGetComponent(out Collider objCollider))
            return;

        // Continuously update the valid colliding object
        currentCollidingObject = collision.gameObject;
    }

    private void OnTriggerExit(Collider collision)
    {
        // Ignore objects that are NOT in the "Ground" layer
        if (collision.gameObject.layer != LayerMask.NameToLayer(allowedLayer))
            return;

        // Ensure the object has a valid Collider component
        if (!collision.TryGetComponent(out Collider objCollider))
            return;

        // Clear the stored object when the player stops colliding
        if (currentCollidingObject == collision.gameObject)
        {
            Debug.Log("Stopped colliding with: " + currentCollidingObject.name);
            currentCollidingObject = null;
        }
    }

    public void AlignPlayerToCollidingObject()
    {
        if (currentCollidingObject != null)
        {
            // Ensure the colliding object has a Collider component
            if (!currentCollidingObject.TryGetComponent(out Collider objCollider))
            {
                Debug.Log("No valid collider found, skipping alignment.");
                return;
            }

            // Get the center position of the colliding object
            Vector3 objectCenter = objCollider.bounds.center;

            // Align the player's X and Z position to the object's center while keeping the current Y position
            Vector3 alignedPosition = new Vector3(objectCenter.x, parentObject.position.y, objectCenter.z);

            // Move player smoothly to the correct position
            parentObject.MovePosition(alignedPosition);

            Debug.Log("Aligned Player to Center of Ground Object: " + alignedPosition);
        }
    }

}
