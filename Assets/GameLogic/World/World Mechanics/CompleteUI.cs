using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CompleteUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 startingScale;
    private Coroutine animationCoroutine;

    // 把所有需要改透明度的组件缓存起来（自己 + 子物体）
    private Graphic[] graphics;
    private TMP_Text[] texts;

    [Header("Scale Animation Settings")]
    [SerializeField] private float initialScaleMultiplier = 1.5f;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private float bounceDuration = 1.5f;
    [SerializeField] private float bounceFrequency = 2f;
    [SerializeField] private float geometricDecayFactor = 0.5f;

    [Header("Fade Settings (alpha on ALL child graphics)")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool startHidden = true;   // 初始是否透明

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;

    float DT => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startingScale = rectTransform.localScale;

        // 缓存所有子图形与文本（包含自己）
        graphics = GetComponentsInChildren<Graphic>(true);
        texts = GetComponentsInChildren<TMP_Text>(true);

        if (startHidden)
        {
            SetAlphaAll(0f);
            rectTransform.localScale = startingScale;
        }
    }

    // === 你只要关心这四个 API，名字不变 ===
    public void InstantShow()
    {
        StopAnim();
        SetAlphaAll(1f);
        rectTransform.localScale = startingScale;
    }

    public void InstantHide()
    {
        StopAnim();
        SetAlphaAll(0f);
        rectTransform.localScale = startingScale;
    }

    public IEnumerator AnimateShow()
    {
        StopAnim();
        animationCoroutine = StartCoroutine(ShowSequence());
        yield return animationCoroutine;
        animationCoroutine = null;
    }

    public IEnumerator AnimateHide()
    {
        StopAnim();
        animationCoroutine = StartCoroutine(HideSequence());
        yield return animationCoroutine;
        animationCoroutine = null;
    }

    // === 内部实现 ===
    IEnumerator ShowSequence()
    {
        // 先从 0 → 1 淡入（只改 alpha）
        yield return FadeAll(0f, 1f, fadeDuration);
        // 再做缩放弹跳（你的原效果）
        yield return AnimateScaleBounce();
    }

    IEnumerator HideSequence()
    {
        yield return FadeAll(1f, 0f, fadeDuration * 0.6f);
        rectTransform.localScale = startingScale;
    }

    IEnumerator FadeAll(float from, float to, float duration)
    {
        // 读当前值作为起点，避免被中断后闪烁
        float current = GetAnyAlpha();
        if (!Mathf.Approximately(current, from)) from = current;

        float t = 0f;
        while (t < duration)
        {
            t += DT;
            float u = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            SetAlphaAll(Mathf.Lerp(from, to, u));
            yield return null;
        }
        SetAlphaAll(to);
    }

    IEnumerator AnimateScaleBounce()
    {
        // 上冲
        float t = 0f;
        while (t < dropDuration)
        {
            t += DT;
            float u = dropDuration > 0f ? Mathf.Sqrt(Mathf.Clamp01(t / dropDuration)) : 1f;
            rectTransform.localScale = Vector3.Lerp(startingScale, startingScale * initialScaleMultiplier, u);
            yield return null;
        }
        rectTransform.localScale = startingScale * initialScaleMultiplier;

        // 衰减回 1
        t = 0f;
        while (t < bounceDuration)
        {
            t += DT;
            float u = bounceDuration > 0f ? Mathf.Clamp01(t / bounceDuration) : 1f;
            float amp = (initialScaleMultiplier - 1f) * Mathf.Pow(geometricDecayFactor, u * bounceFrequency);
            float scaleFactor = 1f + amp * Mathf.Cos(2f * Mathf.PI * bounceFrequency * u);
            rectTransform.localScale = startingScale * scaleFactor;
            yield return null;
        }
        rectTransform.localScale = startingScale;
    }

    void StopAnim()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    // === 关键：一次性改“自己+所有子节点”的 alpha ===
    void SetAlphaAll(float a)
    {
        if (graphics != null)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                if (!graphics[i]) continue;
                var c = graphics[i].color; c.a = a; graphics[i].color = c;
            }
        }
        if (texts != null)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                if (!texts[i]) continue;
                var c = texts[i].color; c.a = a; texts[i].color = c;
            }
        }
    }

    // 拿一个当前 alpha（用于平滑续接）
    float GetAnyAlpha()
    {
        if (graphics != null && graphics.Length > 0 && graphics[0])
            return graphics[0].color.a;
        if (texts != null && texts.Length > 0 && texts[0])
            return texts[0].color.a;
        return 0f;
    }
}
