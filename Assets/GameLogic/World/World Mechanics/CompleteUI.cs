using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CompleteUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 startingScale;
    private Image image;
    private Color startingColor;

    [Header("Scale Animation Settings")]
    [SerializeField] private float initialScaleMultiplier = 1.5f;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private float bounceDuration = 1.5f;
    [SerializeField] private float bounceFrequency = 2f;
    [SerializeField] private float geometricDecayFactor = 0.5f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isAnimating = false;
    private bool isVisible = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startingScale = rectTransform.localScale;

        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.Log("No Image component attached!");
        }
        else
        {
            startingColor = image.color;
            // Start hidden.
            startingColor.a = 0f;
            image.color = startingColor;
        }
    }

    // Public trigger methods to be called from LevelTrigger.
    public void TriggerShow()
    {
        if (!isAnimating && !isVisible)
        {
            StartCoroutine(ShowAndBounce());
        }
    }

    public void TriggerHide()
    {
        if (!isAnimating && isVisible)
        {
            StartCoroutine(Hide());
        }
    }

    // Removed Update() input check since LevelTrigger now triggers the animations.

    IEnumerator ShowAndBounce()
    {
        isAnimating = true;
        // Fade in the UI element.
        yield return StartCoroutine(FadeIn());
        yield return StartCoroutine(AnimateScaleBounce());
        isVisible = true;
        isAnimating = false;
    }

    IEnumerator Hide()
    {
        isAnimating = true;
        // Fade out the UI element.
        yield return StartCoroutine(FadeOut());
        // Reset the scale to its starting value.
        rectTransform.localScale = startingScale;
        isVisible = false;
        isAnimating = false;
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            Color col = image.color;
            col.a = t;
            image.color = col;
            yield return null;
        }
        Color finalColor = image.color;
        finalColor.a = 1f;
        image.color = finalColor;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            Color col = image.color;
            col.a = 1 - t;
            image.color = col;
            yield return null;
        }
        Color finalColor = image.color;
        finalColor.a = 0f;
        image.color = finalColor;
    }

    IEnumerator AnimateScaleBounce()
    {
        float elapsed = 0f;
        // Phase 1: Quick scale up to the overshot value.
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);
            float tFast = Mathf.Sqrt(t);
            rectTransform.localScale = Vector3.Lerp(startingScale, startingScale * initialScaleMultiplier, tFast);
            yield return null;
        }
        rectTransform.localScale = startingScale * initialScaleMultiplier;

        // Phase 2: Scale bounce oscillations.
        elapsed = 0f;
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounceDuration);
            float amplitude = (initialScaleMultiplier - 1) * Mathf.Pow(geometricDecayFactor, t * bounceFrequency);
            float phase = 2 * Mathf.PI * bounceFrequency * t;
            float scaleFactor = 1 + amplitude * Mathf.Cos(phase);
            rectTransform.localScale = startingScale * scaleFactor;
            yield return null;
        }
        rectTransform.localScale = startingScale;
    }
}
