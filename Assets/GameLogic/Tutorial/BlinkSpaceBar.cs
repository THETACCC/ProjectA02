using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkSpaceBar : MonoBehaviour
{
    [Header("Blink Settings")]
    [Tooltip("How fast the image fades in and out.")]
    public float blinkSpeed = 2f;

    [Tooltip("Minimum alpha (0 = fully transparent).")]
    [Range(0f, 1f)]
    public float minAlpha = 0f;

    [Tooltip("Maximum alpha (1 = fully visible).")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    private Image img;
    private Color baseColor;

    void Awake()
    {
        img = GetComponent<Image>();

        // Force base color to white (so only alpha changes)
        baseColor = Color.white;
        img.color = baseColor;
    }

    void Update()
    {
        // Smooth oscillation between 0 and 1
        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;

        // Lerp alpha between min and max
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = baseColor;
        c.a = alpha;
        img.color = c;
    }
}