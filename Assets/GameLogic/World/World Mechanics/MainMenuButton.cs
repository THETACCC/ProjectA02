using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum MenuButtonType { StartGame, Settings, Credits, Exit }

public class MainMenuButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("References")]
    public MenuButtonType buttonType;
    public MainMenuPlayerMovement playerMovement;
    public TextMeshProUGUI label;
    public Image[] highlightImages;     // now an array!

    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float hoverSpeed = 0.2f;
    public float clickScale = 0.9f;
    public float clickSpeed = 0.1f;
    public Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color hoverColor = Color.white;

    Vector3 originalScale;
    Color originalTextColor;
    float[] originalImgAlphas;

    void Awake()
    {
        originalScale = transform.localScale;
        originalTextColor = label.color;

        // cache and zero-out all highlight images
        originalImgAlphas = new float[highlightImages.Length];
        for (int i = 0; i < highlightImages.Length; i++)
        {
            originalImgAlphas[i] = highlightImages[i].color.a;
            var c = highlightImages[i].color;
            c.a = 0f;
            highlightImages[i].color = c;
        }

        // initialize label
        label.color = defaultColor;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        // tell manager which target to use
        switch (buttonType)
        {
            case MenuButtonType.StartGame: playerMovement.OnHoverStartGame(); break;
            case MenuButtonType.Settings: playerMovement.OnHoverSettings(); break;
            case MenuButtonType.Credits: playerMovement.OnHoverCredits(); break;
            case MenuButtonType.Exit: playerMovement.OnHoverExit(); break;
        }

        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale * hoverScale, hoverSpeed));
        StartCoroutine(ColorTo(label, defaultColor, hoverColor, hoverSpeed));
        StartCoroutine(FadeImages(0f, 1f, hoverSpeed));
    }

    public void OnPointerExit(PointerEventData e)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale, hoverSpeed));
        StartCoroutine(ColorTo(label, hoverColor, defaultColor, hoverSpeed));
        StartCoroutine(FadeImages(1f, 0f, hoverSpeed));
    }

    public void OnPointerClick(PointerEventData e)
    {
        StopAllCoroutines();
        StartCoroutine(ClickEffect());
        // add your click logic here or via the Button OnClick in the Inspector
    }

    IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.localScale = target;
    }

    IEnumerator ColorTo(TextMeshProUGUI txt, Color from, Color to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            txt.color = Color.Lerp(from, to, t / duration);
            yield return null;
        }
        txt.color = to;
    }

    IEnumerator FadeImages(float from, float to, float duration)
    {
        float t = 0f;
        // capture starting alphas in case you need non-zero originals
        float[] starts = new float[highlightImages.Length];
        for (int i = 0; i < highlightImages.Length; i++)
            starts[i] = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            for (int i = 0; i < highlightImages.Length; i++)
            {
                var c = highlightImages[i].color;
                c.a = alpha;
                highlightImages[i].color = c;
            }
            yield return null;
        }
        // ensure final
        for (int i = 0; i < highlightImages.Length; i++)
        {
            var c = highlightImages[i].color;
            c.a = to;
            highlightImages[i].color = c;
        }
    }

    IEnumerator ClickEffect()
    {
        // shrink
        yield return ScaleTo(originalScale * clickScale, clickSpeed);
        // bounce back to hover‐scale if still hovered, else to normal
        bool stillHovered = EventSystem.current.IsPointerOverGameObject();
        Vector3 target = stillHovered ? originalScale * hoverScale : originalScale;
        yield return ScaleTo(target, clickSpeed);
    }
}
