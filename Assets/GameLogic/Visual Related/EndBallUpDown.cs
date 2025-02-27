/*using System.Collections;
using UnityEngine;

public class EndBallUpDown : MonoBehaviour
{
    public float duration = 3.0f;      // Time (in seconds)
    public float heightOffset = 1.0f;  // How high the object moves

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + new Vector3(0, heightOffset, 0);

        StartCoroutine(MoveUpDown());
    }

    IEnumerator MoveUpDown()
    {
        while (true)
        {
            // Move up
            yield return StartCoroutine(MoveObject(startPosition, targetPosition, duration));
            // Move down
            yield return StartCoroutine(MoveObject(targetPosition, startPosition, duration));
        }
    }

    IEnumerator MoveObject(Vector3 start, Vector3 end, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            transform.position = Vector3.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }
}
*/
using System.Collections;
using UnityEngine;

public class EndBallUpDown : MonoBehaviour
{
    public float duration = 3.0f;     // Time (in seconds) 
    public float heightOffset = 1.0f; // Vertical distance to move

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + new Vector3(0, heightOffset, 0);

        // Start the continuous up-down movement coroutine
        StartCoroutine(MoveUpDown());
    }

    IEnumerator MoveUpDown()
    {
        while (true)
        {
            // Move up
            yield return StartCoroutine(MoveObject(startPosition, targetPosition, duration));
            // Move down
            yield return StartCoroutine(MoveObject(targetPosition, startPosition, duration));
        }
    }

    IEnumerator MoveObject(Vector3 start, Vector3 end, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            // Smooth interpolation
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            transform.position = Vector3.Lerp(start, end, t);
            time += Time.deltaTime;
            yield return null;
        }
        // Set final position precisely
        transform.position = end;
    }
}
