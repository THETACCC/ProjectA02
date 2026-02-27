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

    [Tooltip("Only put images here that should FADE in/out on hover. Do NOT include SideLeft.")]
    public Image[] highlightImages;

    [Tooltip("SideLeft object (RectTransform) that should NEVER disappear, only scale on hover.")]
    public RectTransform sideLeft;

    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float hoverSpeed = 0.2f;
    public float clickScale = 0.9f;
    public float clickSpeed = 0.1f;

    [Header("SideLeft Hover Scale")]
    public float sideLeftHoverScale = 1.1f;
    public float sideLeftScaleSpeed = 0.2f;

    public Color defaultColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color hoverColor = Color.white;

    Vector3 originalScale;
    Vector3 sideLeftOriginalScale;

    void Awake()
    {
        originalScale = transform.localScale;

        if (sideLeft != null)
            sideLeftOriginalScale = sideLeft.localScale;

        // zero-out ONLY highlight images (SideLeft is NOT in this array)
        if (highlightImages != null)
        {
            for (int i = 0; i < highlightImages.Length; i++)
            {
                if (highlightImages[i] == null) continue;
                var c = highlightImages[i].color;
                c.a = 0f;
                highlightImages[i].color = c;
            }
        }

        label.color = defaultColor;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        //Debug.Log($"[ENTER] handler={name}, hit={e.pointerCurrentRaycast.gameObject?.name}");

        AudioPlayer.instance.playUIHoverSound();

        switch (buttonType)
        {
            case MenuButtonType.StartGame: playerMovement.OnHoverStartGame(); break;
            case MenuButtonType.Settings: playerMovement.OnHoverSettings(); break;
            case MenuButtonType.Credits: playerMovement.OnHoverCredits(); break;
            case MenuButtonType.Exit: playerMovement.OnHoverExit(); break;
        }

        StopAllCoroutines();
        StartCoroutine(ScaleTo(transform, originalScale * hoverScale, hoverSpeed));
        StartCoroutine(ColorTo(label, defaultColor, hoverColor, hoverSpeed));
        StartCoroutine(FadeImages(0f, 1f, hoverSpeed));

        if (sideLeft != null)
            StartCoroutine(ScaleTo(sideLeft, sideLeftOriginalScale * sideLeftHoverScale, sideLeftScaleSpeed));
    }

    public void OnPointerExit(PointerEventData e)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(transform, originalScale, hoverSpeed));
        StartCoroutine(ColorTo(label, hoverColor, defaultColor, hoverSpeed));
        StartCoroutine(FadeImages(1f, 0f, hoverSpeed));

        if (sideLeft != null)
            StartCoroutine(ScaleTo(sideLeft, sideLeftOriginalScale, sideLeftScaleSpeed));
    }

    public void OnPointerClick(PointerEventData e)
    {
        StopAllCoroutines();
        StartCoroutine(ClickEffect());
    }

    IEnumerator ScaleTo(Transform targetTransform, Vector3 target, float duration)
    {
        Vector3 start = targetTransform.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            targetTransform.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        targetTransform.localScale = target;
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
        if (highlightImages == null || highlightImages.Length == 0) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);

            for (int i = 0; i < highlightImages.Length; i++)
            {
                if (highlightImages[i] == null) continue;
                var c = highlightImages[i].color;
                c.a = alpha;
                highlightImages[i].color = c;
            }

            yield return null;
        }

        for (int i = 0; i < highlightImages.Length; i++)
        {
            if (highlightImages[i] == null) continue;
            var c = highlightImages[i].color;
            c.a = to;
            highlightImages[i].color = c;
        }
    }

    IEnumerator ClickEffect()
    {
        yield return ScaleTo(transform, originalScale * clickScale, clickSpeed);

        // If you want "still hovered" logic to be accurate per-button, track a bool instead.
        bool stillHovered = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Vector3 target = stillHovered ? originalScale * hoverScale : originalScale;

        yield return ScaleTo(transform, target, clickSpeed);
    }
}