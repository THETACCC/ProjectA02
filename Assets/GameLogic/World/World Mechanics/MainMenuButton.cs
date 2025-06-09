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
    public TextMeshProUGUI label;       // assign your TextMeshProUGUI child
    public Image highlightImage;        // assign your overlay Image child

    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float hoverSpeed = 0.2f;
    public float clickScale = 0.9f;
    public float clickSpeed = 0.1f;
    public Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color hoverColor = Color.white;

    Vector3 originalScale;
    Color originalTextColor;
    float originalImgAlpha;

    void Awake()
    {
        // cache originals
        originalScale = transform.localScale;
        originalTextColor = label.color;
        if (highlightImage != null)
            originalImgAlpha = highlightImage.color.a;

        // initialize to default
        label.color = defaultColor;
        if (highlightImage != null)
        {
            var c = highlightImage.color;
            c.a = 0f;
            highlightImage.color = c;
        }
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
        if (highlightImage != null)
            StartCoroutine(FadeImage(0f, 1f, hoverSpeed));
    }

    public void OnPointerExit(PointerEventData e)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale, hoverSpeed));
        StartCoroutine(ColorTo(label, hoverColor, defaultColor, hoverSpeed));
        if (highlightImage != null)
            StartCoroutine(FadeImage(1f, 0f, hoverSpeed));
    }

    public void OnPointerClick(PointerEventData e)
    {
        StopAllCoroutines();
        StartCoroutine(ClickEffect());
        // ? hook up your actual “StartGame/Settings…” actions via
        //   UI-Button OnClick events or here.
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

    IEnumerator FadeImage(float from, float to, float duration)
    {
        float t = 0f;
        Color c = highlightImage.color;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            highlightImage.color = c;
            yield return null;
        }
        c.a = to;
        highlightImage.color = c;
    }

    IEnumerator ClickEffect()
    {
        // shrink
        yield return ScaleTo(originalScale * clickScale, clickSpeed);
        // bounce back to hover-scale if still hovered, else to normal
        bool stillHovered = EventSystem.current.IsPointerOverGameObject();
        Vector3 target = stillHovered ? originalScale * hoverScale : originalScale;
        yield return ScaleTo(target, clickSpeed);
    }
}
