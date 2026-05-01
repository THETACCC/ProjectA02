using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightReciever : MonoBehaviour
{
    [Header("Activation")]
    public float requiredHitTime = 0.5f;
    public bool isHit = false;

    [Header("Reappear Delay")]
    public float reenableDelay = 1f;

    [Header("Fail-safe")]
    [Tooltip("If no HitByLight call is received within this time, light is considered gone.")]
    public float lostLightTimeout = 0.08f;

    private MeshRenderer myRenderer;

    [Header("Collision Child")]
    public GameObject myColliderLight;
    public GameObject myColliderDark;

    private Coroutine reenableRoutine;

    private float hitTimer = 0f;
    private float lastHitTime = -999f;

    void Start()
    {
        if (transform.childCount >= 2)
        {
            myColliderLight = transform.GetChild(0).gameObject;
            myColliderDark = transform.GetChild(1).gameObject;
        }
        else
        {
            Debug.LogWarning($"LightReciever on {gameObject.name} needs 2 children.");
        }
    }

    void Update()
    {
        bool currentlyHit = Time.time - lastHitTime <= lostLightTimeout;

        if (currentlyHit)
        {
            hitTimer += Time.deltaTime;

            if (!isHit && hitTimer >= requiredHitTime)
            {
                Activate();
            }
        }
        else
        {
            hitTimer = 0f;

            if (isHit)
                NotifyNotHit();
        }

        UpdateVisualState();
    }

    public void HitByLight()
    {
        lastHitTime = Time.time;

        if (reenableRoutine != null)
        {
            StopCoroutine(reenableRoutine);
            reenableRoutine = null;
        }
    }

    private void Activate()
    {
        isHit = true;
        hitTimer = requiredHitTime;
    }

    public void ForceNotHit()
    {
        lastHitTime = -999f;
        hitTimer = 0f;

        if (reenableRoutine != null)
        {
            StopCoroutine(reenableRoutine);
            reenableRoutine = null;
        }

        isHit = false;
        UpdateVisualState();
    }

    public void NotifyNotHit()
    {
        if (reenableRoutine == null)
            reenableRoutine = StartCoroutine(ReenableAfterDelay());
    }

    private IEnumerator ReenableAfterDelay()
    {
        yield return new WaitForSeconds(reenableDelay);

        isHit = false;
        hitTimer = 0f;
        reenableRoutine = null;

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (myRenderer != null)
            myRenderer.enabled = !isHit;

        if (myColliderLight != null)
            myColliderLight.SetActive(!isHit);

        if (myColliderDark != null)
            myColliderDark.SetActive(isHit);
    }

    private void OnDisable()
    {
        ForceNotHit();
    }

    private void OnEnable()
    {
        lastHitTime = -999f;
        hitTimer = 0f;
    }

    private void OnTransformParentChanged()
    {
        ForceNotHit();
    }

    private void OnTransformChildrenChanged()
    {
        ForceNotHit();
    }
}