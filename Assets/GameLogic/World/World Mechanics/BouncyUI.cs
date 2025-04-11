using UnityEngine;
using System.Collections;

public class BouncyUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 startingPosition;
    private float startingRotationZ;
    private Coroutine animationCoroutine;

    [Header("Position Animation")]
    [SerializeField] private Vector2 targetPosition = Vector2.zero;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private float returnDuration = 0.3f;

    [Header("Rotation Animation")]
    [SerializeField] private float targetRotationZ = 0f;
    [SerializeField] private float initialOvershoot = 30f;
    [SerializeField] private float rotationBounceDuration = 1.0f;
    [SerializeField] private float bounceFrequency = 2f;
    [SerializeField] private float geometricDecayFactor = 0.5f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startingPosition = rectTransform.anchoredPosition;
        startingRotationZ = rectTransform.rotation.eulerAngles.z;
    }

    public void InstantShow()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        rectTransform.anchoredPosition = targetPosition;
        rectTransform.rotation = Quaternion.Euler(0, 0, targetRotationZ);
    }

    public void InstantHide()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        rectTransform.anchoredPosition = startingPosition;
        rectTransform.rotation = Quaternion.Euler(0, 0, startingRotationZ);
    }

    public IEnumerator AnimateShow()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateToTarget());
        yield return animationCoroutine;
    }

    public IEnumerator AnimateHide()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateToStart());
        yield return animationCoroutine;
    }

    IEnumerator AnimateToTarget()
    {
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float tFast = Mathf.Sqrt(elapsed / dropDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startingPosition, targetPosition, tFast);
            rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(startingRotationZ, targetRotationZ - initialOvershoot, tFast));
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        rectTransform.rotation = Quaternion.Euler(0, 0, targetRotationZ - initialOvershoot);

        elapsed = 0f;
        while (elapsed < rotationBounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationBounceDuration;
            float amplitude = initialOvershoot * Mathf.Pow(geometricDecayFactor, t * bounceFrequency);
            float offset = amplitude * Mathf.Sin(-Mathf.PI / 2 + 2 * Mathf.PI * bounceFrequency * t);
            rectTransform.rotation = Quaternion.Euler(0, 0, targetRotationZ + offset);
            yield return null;
        }

        rectTransform.rotation = Quaternion.Euler(0, 0, targetRotationZ);
    }

    IEnumerator AnimateToStart()
    {
        float elapsed = 0f;
        Vector2 currentPosition = rectTransform.anchoredPosition;
        float currentRotationZ = rectTransform.rotation.eulerAngles.z;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(currentPosition, startingPosition, t);
            rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(currentRotationZ, startingRotationZ, t));
            yield return null;
        }

        rectTransform.anchoredPosition = startingPosition;
        rectTransform.rotation = Quaternion.Euler(0, 0, startingRotationZ);
    }
}
