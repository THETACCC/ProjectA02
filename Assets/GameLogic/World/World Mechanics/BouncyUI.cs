using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;    // RectMask2D
using TMPro;
using System.Collections;

public class BouncyUI : MonoBehaviour
{
    public event System.Action<BouncyUI> OnShowFinished;  // ← 播完入场时发事件

    private RectTransform rectTransform;
    private Vector2 startingPosition;
    private float startingRotationZ;
    private Coroutine animationCoroutine;

    [Header("Position Animation")]
    [SerializeField] private Vector2 targetPosition = Vector2.zero;
    [Tooltip("true 表示 targetPosition 为相对起点的偏移；false 表示绝对 anchoredPosition")]
    [SerializeField] private bool targetIsOffset = false;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private float returnDuration = 0.3f;

    [Header("Rotation Animation")]
    [SerializeField] private float targetRotationZ = 0f;
    [SerializeField] private float initialOvershoot = 30f;
    [SerializeField] private float rotationBounceDuration = 1.0f;
    [SerializeField] private float bounceFrequency = 2f;
    [SerializeField] private float geometricDecayFactor = 0.5f;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;

    private bool warmed = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startingPosition = rectTransform.anchoredPosition;
        startingRotationZ = rectTransform.rotation.eulerAngles.z;
    }

    float DT => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    // 供协调器计算“何时播完”
    public float GetShowDuration()
    {
        return Mathf.Max(0f, dropDuration) + Mathf.Max(0f, rotationBounceDuration);
    }
    public float GetHideDuration()
    {
        return Mathf.Max(0f, returnDuration);
    }

    public void InstantShow()
    {
        StopAnim();
        StartCoroutine(StabilizeIfNeeded());

        var tgtPos = targetIsOffset ? startingPosition + targetPosition : targetPosition;
        rectTransform.anchoredPosition = tgtPos;
        rectTransform.rotation = Quaternion.Euler(0, 0, targetRotationZ);
    }

    public void InstantHide()
    {
        StopAnim();
        ClearSelectedIfMine();
        rectTransform.anchoredPosition = startingPosition;
        rectTransform.rotation = Quaternion.Euler(0, 0, startingRotationZ);
    }

    public IEnumerator AnimateShow()
    {
        StopAnim();
        yield return StabilizeIfNeeded();
        animationCoroutine = StartCoroutine(AnimateToTarget());
        yield return animationCoroutine;
        animationCoroutine = null;

        // 关键：入场动画完整结束 → 通知协调器
        OnShowFinished?.Invoke(this);
    }

    public IEnumerator AnimateHide()
    {
        StopAnim();
        ClearSelectedIfMine();
        animationCoroutine = StartCoroutine(AnimateToStart());
        yield return animationCoroutine;
        animationCoroutine = null;
    }

    private void StopAnim()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    IEnumerator AnimateToTarget()
    {
        Vector2 fromPos = rectTransform.anchoredPosition;
        Vector2 tgtPos = targetIsOffset ? startingPosition + targetPosition : targetPosition;

        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += DT;
            float tFast = Mathf.Sqrt(Mathf.Clamp01(elapsed / dropDuration));
            rectTransform.anchoredPosition = Vector2.Lerp(fromPos, tgtPos, tFast);
            rectTransform.rotation = Quaternion.Euler(0, 0,
                Mathf.Lerp(startingRotationZ, targetRotationZ - initialOvershoot, tFast));
            yield return null;
        }

        rectTransform.anchoredPosition = tgtPos;
        rectTransform.rotation = Quaternion.Euler(0, 0, targetRotationZ - initialOvershoot);

        elapsed = 0f;
        while (elapsed < rotationBounceDuration)
        {
            elapsed += DT;
            float t = Mathf.Clamp01(elapsed / rotationBounceDuration);
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
            elapsed += DT;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(currentPosition, startingPosition, t);
            rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(currentRotationZ, startingRotationZ, t));
            yield return null;
        }

        rectTransform.anchoredPosition = startingPosition;
        rectTransform.rotation = Quaternion.Euler(0, 0, startingRotationZ);
    }

    // —— 首帧稳定化：两帧布局 + TMP 预热 + 临时关父链 RectMask2D —— //
    IEnumerator StabilizeIfNeeded()
    {
        if (warmed) yield break;

        ClearSelectedIfMine();

        var masks = GetComponentsInParent<RectMask2D>(true);
        var maskOn = new bool[masks.Length];
        for (int i = 0; i < masks.Length; i++) { maskOn[i] = masks[i].enabled; masks[i].enabled = false; }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        yield return new WaitForEndOfFrame();

        foreach (var tmp in GetComponentsInChildren<TMP_Text>(true))
            tmp.ForceMeshUpdate();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < masks.Length; i++) masks[i].enabled = maskOn[i];

        warmed = true;
    }

    private void ClearSelectedIfMine()
    {
        var es = EventSystem.current;
        if (es && es.currentSelectedGameObject &&
            es.currentSelectedGameObject.transform.IsChildOf(transform))
            es.SetSelectedGameObject(null);
    }
}
