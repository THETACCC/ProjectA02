using UnityEngine;
using System.Collections;

public class BouncyUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 startingPosition;
    private float startingRotationZ;

    [Header("Position Animation")]
    [SerializeField] private Vector2 targetPosition = Vector2.zero;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private float returnDuration = 1f;

    [Header("Rotation Animation (Forward Only)")]
    [SerializeField] private float targetRotationZ = 0f;
    [SerializeField] private float initialOvershoot = 30f;
    [SerializeField] private float rotationBounceDuration = 1.5f;
    [SerializeField] private float bounceFrequency = 2f;
    [SerializeField] private float geometricDecayFactor = 0.5f;

    private bool isAnimating = false;
    private bool isAtTarget = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startingPosition = rectTransform.anchoredPosition;
        startingRotationZ = rectTransform.rotation.eulerAngles.z;
    }

    // Public trigger methods.
    public void TriggerToTarget()
    {
        if (!isAnimating && !isAtTarget)
        {
            StartCoroutine(AnimateToTarget());
        }
    }

    public void TriggerToStart()
    {
        if (!isAnimating && isAtTarget)
        {
            StartCoroutine(AnimateToStart());
        }
    }

    // Removed Update() testing code.

    IEnumerator AnimateToTarget()
    {
        isAnimating = true;
        float elapsed = 0f;

        // Phase 1: Fast drop.
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);
            float tFast = Mathf.Sqrt(t);
            rectTransform.anchoredPosition = Vector2.Lerp(startingPosition, targetPosition, tFast);
            float currentRotation = Mathf.Lerp(startingRotationZ, targetRotationZ - initialOvershoot, tFast);
            rectTransform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, targetRotationZ - initialOvershoot);

        // Phase 2: Bounce phase.
        elapsed = 0f;
        while (elapsed < rotationBounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationBounceDuration);
            float amplitude = initialOvershoot * Mathf.Pow(geometricDecayFactor, t * bounceFrequency);
            float phase = -Mathf.PI / 2 + 2 * Mathf.PI * bounceFrequency * t;
            float offset = amplitude * Mathf.Sin(phase);
            float currentRotation = targetRotationZ + offset;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            yield return null;
        }
        rectTransform.rotation = Quaternion.Euler(0f, 0f, targetRotationZ);
        isAtTarget = true;
        isAnimating = false;
    }

    IEnumerator AnimateToStart()
    {
        isAnimating = true;
        float elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(targetPosition, startingPosition, t);
            float currentRotation = Mathf.Lerp(targetRotationZ, startingRotationZ, t);
            rectTransform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            yield return null;
        }
        rectTransform.anchoredPosition = startingPosition;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, startingRotationZ);
        isAtTarget = false;
        isAnimating = false;
    }
}
