using System.Collections;
using UnityEngine;

public class ImageMover : MonoBehaviour
{
    [SerializeField] private Transform startPoint;  // Starting position
    [SerializeField] private Transform endPoint;    // Ending position
    [SerializeField] private float moveDuration = 1f;  // Duration of the movement

    private bool isMoving = false;

    private void Start()
    {
        transform.position = startPoint.position;
    }

    public IEnumerator MoveImageToEnd()
    {
        Vector3 moveStart = transform.position;  // Current position as the start point
        Vector3 moveEnd = endPoint.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(moveStart, moveEnd, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it ends exactly at the end position
        transform.position = moveEnd;
    }

    public IEnumerator MoveImageToStart()
    {
        Vector3 moveStart = transform.position;  // Current position as the start point
        Vector3 moveEnd = startPoint.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(moveStart, moveEnd, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it ends exactly at the start position
        transform.position = moveEnd;
    }

    public void ResetImage()
    {
        transform.position = startPoint.position; // Resets to the start point
    }
}
