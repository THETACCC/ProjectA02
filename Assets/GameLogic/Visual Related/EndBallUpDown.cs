using System.Collections;
using UnityEngine;

public class EndBallUpDown : MonoBehaviour
{
    public float duration = 3.0f;     // Time (in seconds) for one-way movement
    public float heightOffset = 1.0f; // Vertical distance to move

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        // Use localPosition so the parent's movement (e.g., along X) is preserved.
        startPosition = transform.localPosition;
        targetPosition = startPosition + new Vector3(0, heightOffset, 0);

        // Start the continuous up-and-down movement coroutine
        StartCoroutine(MoveUpDown());
    }

    IEnumerator MoveUpDown()
    {
        while (true)
        {
            // Move upward with easing
            yield return StartCoroutine(MoveObject(startPosition, targetPosition, duration));
            // Move downward with easing
            yield return StartCoroutine(MoveObject(targetPosition, startPosition, duration));
        }
    }

    IEnumerator MoveObject(Vector3 start, Vector3 end, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            // Smooth interpolation with easing
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            // Only update the Y component of the localPosition
            Vector3 newLocalPos = transform.localPosition;
            newLocalPos.y = Mathf.Lerp(start.y, end.y, t);
            transform.localPosition = newLocalPos;

            time += Time.deltaTime;
            yield return null;
        }
        // Ensure the final Y position is set precisely
        Vector3 finalLocalPos = transform.localPosition;
        finalLocalPos.y = end.y;
        transform.localPosition = finalLocalPos;
    }
}
