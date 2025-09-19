using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Debug")]
    public bool showDebugLines = true; // toggle in Inspector

    private Collider sensor;                       // this object¡¯s collider (must be IsTrigger = true)
    private Collider currentGround;                // last valid ground we¡¯re overlapping
    private bool alignRequested;
    private Vector3 targetPosition;

    private void Awake()
    {
        sensor = GetComponent<Collider>();

        if (parentObject == null)
            parentObject = GetComponentInParent<Rigidbody>();

        if (sensor != null && !sensor.isTrigger)
            Debug.LogWarning("[GroundAlignment] Set this collider to IsTrigger = true to receive OnTrigger callbacks.");

        if (parentObject == null)
            Debug.LogError("[GroundAlignment] No Rigidbody assigned/found for 'parentObject'.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidGround(other))
        {
            currentGround = other;
            if (showDebugLines)
                Debug.Log("[GroundAlignment] Enter ground: " + other.name);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsValidGround(other))
            currentGround = other;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == currentGround)
        {
            if (showDebugLines)
                Debug.Log("[GroundAlignment] Exit ground: " + other.name);
            currentGround = null;
        }
    }

    private bool IsValidGround(Collider other)
    {
        // LayerMask filter
        if (((1 << other.gameObject.layer) & groundMask) == 0) return false;
        // Trigger filter
        if (ignoreTriggerGround && other.isTrigger) return false;
        return true;
    }

    /// <summary> Public API: request an align to whatever valid ground we¡¯re overlapping. </summary>
    public bool AlignPlayerToCollidingObject()
    {
        if (parentObject == null || currentGround == null)
        {
            if (showDebugLines)
                Debug.LogWarning("[GroundAlignment] Align requested but no valid ground detected.");
            return false;
        }

        Vector3 refPos = parentObject.position;
        Vector3 xz;

        if (useClosestPoint)
        {
            Vector3 cp = currentGround.ClosestPoint(refPos);
            xz = new Vector3(cp.x, 0f, cp.z);
            if (showDebugLines)
                Debug.DrawRay(cp, Vector3.up * 2, Color.green, 1.0f); // show hit point
        }
        else
        {
            Vector3 c = currentGround.bounds.center;
            xz = new Vector3(c.x, 0f, c.z);
            if (showDebugLines)
                Debug.DrawRay(c, Vector3.up * 2, Color.yellow, 1.0f); // show center point
        }

        targetPosition = new Vector3(xz.x, parentObject.position.y, xz.z);
        alignRequested = true;

        if (showDebugLines)
        {
            Debug.Log("[GroundAlignment] Target position set to: " + targetPosition);
            Debug.DrawLine(parentObject.position, targetPosition, Color.cyan, 1.0f);
        }

        return true;
    }

    private void FixedUpdate()
    {
        if (!alignRequested || parentObject == null) return;

        if (snapSpeed <= 0f)
        {
            parentObject.position = targetPosition; // instant set
            alignRequested = false;

            if (showDebugLines)
                Debug.Log("[GroundAlignment] Teleported to target position.");
            return;
        }

        Vector3 current = parentObject.position;
        Vector3 next = Vector3.MoveTowards(current, targetPosition, snapSpeed * Time.fixedDeltaTime);

        parentObject.MovePosition(next);

        if (showDebugLines)
        {
            Debug.DrawLine(current, next, Color.magenta, Time.fixedDeltaTime);
            Debug.Log("[GroundAlignment] Moving player from " + current + " to " + next);
        }

        if ((next - targetPosition).sqrMagnitude <= (stopThreshold * stopThreshold))
        {
            parentObject.MovePosition(targetPosition);
            alignRequested = false;

            if (showDebugLines)
            {
                Debug.Log("[GroundAlignment] Arrived at target position: " + targetPosition);
                Debug.DrawRay(targetPosition, Vector3.up * 2, Color.blue, 1.0f);
            }
        }
    }

    // Optional: show gizmos always in Scene view for debug
    private void OnDrawGizmos()
    {
        if (!showDebugLines) return;

        Gizmos.color = Color.red;
        if (currentGround != null)
        {
            Gizmos.DrawWireCube(currentGround.bounds.center, currentGround.bounds.size);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        if (alignRequested)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}