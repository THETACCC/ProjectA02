using System.Collections;
using UnityEngine;

public class EndMiddleRotate : MonoBehaviour
{
    // Degrees to rotate each time
    public float rotationAngle = 45f;
    // Rotation speed in degrees per second
    public float rotationSpeed = 180f;
    // Pause duration in seconds
    public float pauseDuration = 0.5f;

    void Start()
    {
        StartCoroutine(RotateCoroutine());
    }

    IEnumerator RotateCoroutine()
    {
        while (true)
        {
            float targetAngle = transform.eulerAngles.y + rotationAngle;

            // Rotate
            while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.1f)
            {
                float step = rotationSpeed * Time.deltaTime;
                // Move towards the target angle
                float newAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, step);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, newAngle, transform.eulerAngles.z);
                yield return null;
            }
            // Ensure final rotation is exact
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z);

            // Pause before the next rotation
            yield return new WaitForSeconds(pauseDuration);
        }
    }
}
