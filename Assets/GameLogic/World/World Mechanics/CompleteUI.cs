using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CompleteUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 startingScale;
    private Image image;
    private Coroutine animationCoroutine;

    [Header("Scale Animation Settings")]
    [SerializeField] private float initialScaleMultiplier = 1.5f;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private float bounceDuration = 1.5f;
    [SerializeField] private float bounceFrequency = 2f;
    [SerializeField] private float geometricDecayFactor = 0.5f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startingScale = rectTransform.localScale;
        image = GetComponent<Image>();
        SetAlpha(0f);
    }

    public void InstantShow()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        SetAlpha(1f);
        rectTransform.localScale = startingScale;
    }

    public void InstantHide()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        SetAlpha(0f);
        rectTransform.localScale = startingScale;
    }

    public IEnumerator AnimateShow()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(ShowSequence());
        yield return animationCoroutine;
    }

    public IEnumerator AnimateHide()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(HideSequence());
        yield return animationCoroutine;
    }

    IEnumerator ShowSequence()
    {
        yield return Fade(0f, 1f, fadeDuration);
        yield return AnimateScaleBounce();
    }

    IEnumerator HideSequence()
    {
        yield return Fade(1f, 0f, fadeDuration * 0.6f);
        rectTransform.localScale = startingScale;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    IEnumerator AnimateScaleBounce()
    {
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sqrt(elapsed / dropDuration);
            rectTransform.localScale = Vector3.Lerp(startingScale, startingScale * initialScaleMultiplier, t);
            yield return null;
        }
        rectTransform.localScale = startingScale * initialScaleMultiplier;

        elapsed = 0f;
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            float amplitude = (initialScaleMultiplier - 1) * Mathf.Pow(geometricDecayFactor, t * bounceFrequency);
            float scaleFactor = 1 + amplitude * Mathf.Cos(2 * Mathf.PI * bounceFrequency * t);
            rectTransform.localScale = startingScale * scaleFactor;
            yield return null;
        }
        rectTransform.localScale = startingScale;
    }

    void SetAlpha(float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
