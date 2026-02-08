using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightReciever : MonoBehaviour
{
    [Header("Activation")]
    [Tooltip("Seconds of continuous light required to activate")]
    public float requiredHitTime = 0.5f;

    public bool isHit = false;

    [Header("Reappear Delay")]
    [Tooltip("Seconds after light stops hitting before reappearing")]
    public float reenableDelay = 1f;

    private MeshRenderer myRenderer;

    [Header("Collision Child (auto-assigned)")]
    public GameObject myColliderLight;
    public GameObject myColliderDark;

    private Coroutine reenableRoutine;

    // --- NEW ---
    private float hitTimer = 0f;
    private bool hitThisFrame = false;

    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();

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
        // ----- VISUAL / COLLIDER STATE -----
        if (myRenderer != null)
            myRenderer.enabled = !isHit;

        if (myColliderLight != null)
            myColliderLight.SetActive(!isHit);

        if (myColliderDark != null)
            myColliderDark.SetActive(isHit);

        // ----- HIT TIMER LOGIC -----
        if (hitThisFrame)
        {
            hitTimer += Time.deltaTime;

            if (!isHit && hitTimer >= requiredHitTime)
            {
                Activate();
            }
        }
        else
        {
            // Light not hitting ¡ú reset accumulation
            hitTimer = 0f;

            if (isHit)
                NotifyNotHit();
        }

        // Reset for next frame (emitter must call HitByLight again)
        hitThisFrame = false;
    }

    /// <summary>
    /// Called EVERY FRAME the beam hits this receiver.
    /// </summary>
    public void HitByLight()
    {
        hitThisFrame = true;

        // Cancel pending re-enable if light comes back
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

    public void NotifyNotHit()
    {
        if (reenableRoutine == null)
        {
            reenableRoutine = StartCoroutine(ReenableAfterDelay());
        }
    }

    private IEnumerator ReenableAfterDelay()
    {
        yield return new WaitForSeconds(reenableDelay);

        isHit = false;
        reenableRoutine = null;
    }
}