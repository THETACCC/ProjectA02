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
    [SerializeField] private float geometricDecayFactor = 0.5f;     // Decay factor (0 < factor < 1) applied each cycle.

    private bool isAnimating = false;
    private bool isAtTarget = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startingPosition = rectTransform.anchoredPosition;
        startingRotationZ = rectTransform.rotation.eulerAngles.z;
    }

    void Update()
    {
        // For testing!!!!!!!!!!
        if (Input.GetKeyDown(KeyCode.Space) && !isAnimating)
        {
            if (!isAtTarget)
                StartCoroutine(AnimateToTarget());
            else
                StartCoroutine(AnimateToStart());
        }
    }

    IEnumerator AnimateToTarget()
    {
        isAnimating = true;
        float elapsed = 0f;

        // Phase 1: Fast drop.
        // Move quickly to the target position while rotating to (targetRotationZ - initialOvershoot),
        // so that the rotation starts below the desired target.
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);
            // Using square-root interpolation for a rapid drop effect.
            float tFast = Mathf.Sqrt(t);
            rectTransform.anchoredPosition = Vector2.Lerp(startingPosition, targetPosition, tFast);
            float currentRotation = Mathf.Lerp(startingRotationZ, targetRotationZ - initialOvershoot, tFast);
            rectTransform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            yield return null;
        }
        // Ensure the drop phase ends exactly at the target position and with rotation at targetRotationZ - initialOvershoot.
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, targetRotationZ - initialOvershoot);

        // Phase 2: Bounce phase.
        // Oscillate the rotation around the target value.
        elapsed = 0f;
        while (elapsed < rotationBounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationBounceDuration);
            // The amplitude decays geometrically.
            float amplitude = initialOvershoot * Mathf.Pow(geometricDecayFactor, t * bounceFrequency);
            // Phase starts at -π/2 (matching the drop phase, where sin(-π/2) = -1) and completes several oscillations.
            float phase = -Mathf.PI / 2 + 2 * Mathf.PI * bounceFrequency * t;
            float offset = amplitude * Mathf.Sin(phase);
            float currentRotation = targetRotationZ + offset;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            yield return null;
        }
        // Finalize rotation exactly at the target.
        rectTransform.rotation = Quaternion.Euler(0f, 0f, targetRotationZ);
        isAtTarget = true;
        isAnimating = false;
    }

    // Animate back to the starting state with a smoothly.
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
