using System.Collections;
using UnityEngine;

public class ImageMover : MonoBehaviour
{
    [SerializeField] private Transform startPoint; // Starting position
    [SerializeField] private Transform endPoint;   // Ending position
    [SerializeField] private float moveDuration = 1f; // Duration of the movement

    private Vector3 startPos;
    private Vector3 endPos;
    private bool isMoving = false;

    private void Start()
    {
        // Set the initial positions from the assigned transforms
        startPos = startPoint.position;
        endPos = endPoint.position;

        // Initially set the image to the starting position
        transform.position = startPos;
    }

    // Coroutine to move the image from start to end position
    public IEnumerator MoveImageForward()
    {
        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it ends exactly at the end position
        transform.position = endPos;
        isMoving = false;
    }

    // Coroutine to move the image from end to start position (reverse)
    public IEnumerator MoveImageBackward()
    {
        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(endPos, startPos, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it ends exactly at the start position
        transform.position = startPos;
        isMoving = false;
    }

    // Reset the image to its starting position
    public void ResetImage()
    {
        transform.position = startPos;
    }
}
