using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightReciever : MonoBehaviour
{
    public bool isHit = false;

    [Header("Reappear Delay")]
    [Tooltip("Seconds after light stops hitting before reappearing")]
    public float reenableDelay = 1f;

    private MeshRenderer myRenderer;

    [Header("Collision Child (auto-assigned)")]
    public GameObject myColliderLight;
    public GameObject myCOlliderDark;

    private Coroutine reenableRoutine;

    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();

        if (transform.childCount > 0)
        {
            myColliderLight = transform.GetChild(0).gameObject;
            myCOlliderDark = transform.GetChild(1).gameObject;
        }
        else
        {
            Debug.LogWarning(
                $"LightReciever on {gameObject.name} has no child to use as collider."
            );
        }
    }

    void Update()
    {
        if (myRenderer != null)
            myRenderer.enabled = !isHit;

        if (myColliderLight != null)
            myColliderLight.SetActive(!isHit);

        if (myCOlliderDark != null)
            myCOlliderDark.SetActive(isHit);
    }

    /// <summary>
    /// Called by the light when hit this frame.
    /// Immediately disables the receiver and cancels any pending re-enable.
    /// </summary>
    public void HitByLight()
    {
        isHit = true;

        // If we were waiting to re-enable, cancel it
        if (reenableRoutine != null)
        {
            StopCoroutine(reenableRoutine);
            reenableRoutine = null;
        }
    }

    /// <summary>
    /// Called when the light is NOT hitting this receiver this frame.
    /// Starts a delayed re-enable if not already running.
    /// </summary>
    public void NotifyNotHit()
    {
        if (isHit && reenableRoutine == null)
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