using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockPlaceTutorial : MonoBehaviour
{
    public RectTransform position1;
    public RectTransform position2;

    [Header("Timing")]
    [Tooltip("Seconds to fade in (transparent -> white).")]
    public float fadeInDuration = 0.35f;

    [Tooltip("Seconds to move from position1 -> position2.")]
    public float moveDuration = 1.0f;

    [Tooltip("Seconds to fade out (white -> transparent).")]
    public float fadeOutDuration = 0.35f;

    [Tooltip("Optional pause at the end before fading out.")]
    public float endHoldDuration = 0.0f;

    [Tooltip("Optional pause at the start before fading in.")]
    public float startHoldDuration = 0.0f;

    private RectTransform selfRect;
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        selfRect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // Initialize at position1 and transparent
        if (position1 != null)
            selfRect.position = position1.position;

        SetAlpha(0f);
    }

    private void Update()
    {
        if (position1 == null || position2 == null) return;

        float t = Time.time;

        float cycle =
            startHoldDuration +
            fadeInDuration +
            moveDuration +
            endHoldDuration +
            fadeOutDuration;

        if (cycle <= 0.0001f) return;

        float local = t % cycle;

        // Phase A: start hold (stay at pos1, transparent)
        if (local < startHoldDuration)
        {
            selfRect.position = position1.position;
            SetAlpha(0f);
            return;
        }
        local -= startHoldDuration;

        // Phase B: fade in (pos1, alpha 0->1)
        if (local < fadeInDuration)
        {
            float u = Smooth01(local / fadeInDuration);
            selfRect.position = position1.position;
            SetAlpha(u);
            return;
        }
        local -= fadeInDuration;

        // Phase C: move (pos1->pos2, alpha stays 1)
        if (local < moveDuration)
        {
            float u = Smooth01(local / moveDuration);
            selfRect.position = Vector3.Lerp(position1.position, position2.position, u);
            SetAlpha(1f);
            return;
        }
        local -= moveDuration;

        // Phase D: end hold (stay at pos2, alpha 1)
        if (local < endHoldDuration)
        {
            selfRect.position = position2.position;
            SetAlpha(1f);
            return;
        }
        local -= endHoldDuration;

        // Phase E: fade out (pos2, alpha 1->0)
        if (local < fadeOutDuration)
        {
            float u = Smooth01(local / fadeOutDuration);
            selfRect.position = position2.position;
            SetAlpha(1f - u);
            return;
        }

        // End of cycle: will naturally wrap back to start next frame
    }

    private void SetAlpha(float a)
    {
        Color c = img.color;
        c.r = 1f; c.g = 1f; c.b = 1f; // ensure white
        c.a = Mathf.Clamp01(a);
        img.color = c;
    }

    // Smoothstep for nicer easing
    private float Smooth01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}
