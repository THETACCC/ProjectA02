using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class GroundAlignment : MonoBehaviour
{
    [Header("References")]
    public Rigidbody parentObject;                 // Player RB to move

    [Header("Detection")]
    [Tooltip("Layers considered 'ground' (e.g., Ground, Tile, Platform).")]
    public LayerMask groundMask;
    [Tooltip("If true, ignore ground that is itself a Trigger collider.")]
    public bool ignoreTriggerGround = false;

    [Header("Alignment")]
    [Tooltip("Use collider bounds center (grid tiles) or nearest point (irregular ground).")]
    public bool useClosestPoint = true;
    [Tooltip("If > 0, smoothly approach target; if 0, teleport in one FixedUpdate step.")]
    public float snapSpeed = 20f;
    [Tooltip("Stop when within this distance of the target.")]
    public float stopThreshold = 0.01f;

    [Header("Robustness")]
    [Tooltip("Keep last ground alive for this many seconds after OnTriggerExit (debounce).")]
    public float groundGraceSeconds = 0.08f;
    [Tooltip("When aligning but ground is null, try a small OverlapSphere to re-acquire.")]
    public bool fallbackOverlapIfLost = true;
    [Tooltip("Radius for the overlap fallback search.")]
    public float fallbackRadius = 0.4f;

    [Header("Debug")]
    public bool showDebugLines = true; // toggle in Inspector

    private Collider sensor;                 // this object¡¯s collider (must be IsTrigger = true)
    private Collider currentGround;          // last valid ground we¡¯re overlapping
    private float lastGroundSeenTime = -999f;

    private bool alignRequested;
    private Vector3 targetPosition;

    private void Awake()
    {
        sensor = GetComponent<Collider>();
        if (parentObject == null) parentObject = GetComponentInParent<Rigidbody>();

        if (sensor != null && !sensor.isTrigger)
            Debug.LogWarning("[GroundAlignment] Set this collider to IsTrigger = true to receive OnTrigger callbacks.");
        if (parentObject == null)
            Debug.LogError("[GroundAlignment] No Rigidbody assigned/found for 'parentObject'.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidGround(other)) return;
        currentGround = other;
        lastGroundSeenTime = Time.time;
        if (showDebugLines) Debug.Log("[GroundAlignment] Enter ground: " + other.name);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsValidGround(other)) return;
        currentGround = other;
        lastGroundSeenTime = Time.time;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != currentGround) return;
        // Do NOT null immediately; we use a grace period in IsGroundAvailable()
        if (showDebugLines) Debug.Log("[GroundAlignment] Exit ground: " + other.name);
    }

    private bool IsValidGround(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundMask) == 0) return false;
        if (ignoreTriggerGround && other.isTrigger) return false;
        return true;
    }

    private bool IsGroundAvailable()
    {
        // still valid if we¡¯ve seen ground within grace window
        return currentGround != null && (Time.time - lastGroundSeenTime) <= groundGraceSeconds;
    }

    /// <summary> Public API: request an align to whatever valid ground we¡¯re overlapping. </summary>
    public bool AlignPlayerToCollidingObject()
    {
        if (parentObject == null)
        {
            if (showDebugLines) Debug.LogWarning("[GroundAlignment] Align requested but parentObject is null.");
            return false;
        }

        // If ground appears stale, attempt a quick local re-acquire
        if (!IsGroundAvailable())
        {
            if (fallbackOverlapIfLost)
                TryReacquireGroundViaOverlap();
            if (!IsGroundAvailable())
            {
                if (showDebugLines) Debug.LogWarning("[GroundAlignment] Align requested but no valid ground (even after fallback).");
                return false;
            }
        }

        Vector3 refPos = parentObject.position;
        Vector3 xz;

        if (useClosestPoint)
        {
            Vector3 cp = currentGround.ClosestPoint(refPos);
            xz = new Vector3(cp.x, 0f, cp.z);
            if (showDebugLines) Debug.DrawRay(cp, Vector3.up * 2f, Color.green, 0.5f);
        }
        else
        {
            Vector3 c = currentGround.bounds.center;
            xz = new Vector3(c.x, 0f, c.z);
            if (showDebugLines) Debug.DrawRay(c, Vector3.up * 2f, Color.yellow, 0.5f);
        }

        targetPosition = new Vector3(xz.x, parentObject.position.y, xz.z);
        alignRequested = true;

        if (showDebugLines)
        {
            Debug.Log("[GroundAlignment] Target position set to: " + targetPosition);
            Debug.DrawLine(parentObject.position, targetPosition, Color.cyan, 0.5f);
        }
        return true;
    }

    private void TryReacquireGroundViaOverlap()
    {
        var hits = Physics.OverlapSphere(transform.position, fallbackRadius, groundMask,
                                         ignoreTriggerGround ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
        float bestDist = float.PositiveInfinity;
        Collider best = null;
        foreach (var h in hits)
        {
            if (!IsValidGround(h)) continue;
            float d = Vector3.SqrMagnitude(h.ClosestPoint(transform.position) - transform.position);
            if (d < bestDist) { bestDist = d; best = h; }
        }
        if (best != null)
        {
            currentGround = best;
            lastGroundSeenTime = Time.time;
            if (showDebugLines) Debug.Log("[GroundAlignment] Re-acquired ground via overlap: " + best.name);
        }
    }

    private void FixedUpdate()
    {
        if (!alignRequested || parentObject == null) return;

        if (snapSpeed <= 0f)
        {
            // Instant, but in physics step
            parentObject.position = targetPosition;
            alignRequested = false;
            if (showDebugLines) Debug.Log("[GroundAlignment] Teleported to target position.");
            return;
        }

        // Smooth, physics-friendly move
        Vector3 current = parentObject.position;
        Vector3 next = Vector3.MoveTowards(current, targetPosition, snapSpeed * Time.fixedDeltaTime);
        parentObject.MovePosition(next);

        if (showDebugLines)
        {
            Debug.DrawLine(current, next, Color.magenta, Time.fixedDeltaTime);
            // mark destination
            Debug.DrawRay(targetPosition, Vector3.up * 2f, Color.blue, 0.2f);
        }

        if ((next - targetPosition).sqrMagnitude <= (stopThreshold * stopThreshold))
        {
            parentObject.MovePosition(targetPosition);
            alignRequested = false;
            if (showDebugLines) Debug.Log("[GroundAlignment] Arrived at target position: " + targetPosition);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugLines) return;
        Gizmos.color = Color.red;
        if (currentGround != null)
            Gizmos.DrawWireCube(currentGround.bounds.center, currentGround.bounds.size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        if (fallbackOverlapIfLost)
        {
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, fallbackRadius);
        }

        if (alignRequested)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}