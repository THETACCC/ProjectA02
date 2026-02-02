using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorBothSides : MonoBehaviour
{
    [Header("Read yaw from this transform (set if you rotate a child/parent)")]
    public Transform yawSource; // if null, uses transform

    [Header("Turn-mode detection")]
    [Tooltip("How close (degrees) to +90 or -90 to count as turn mode.")]
    public float turnModeTolerance = 3f;

    [Header("Turn-mode mapping")]
    [Tooltip("If true: +90 yaw turns right, -90 yaw turns left. If false, swapped.")]
    public bool plus90TurnsRight = true;

    private Transform YawT => yawSource != null ? yawSource : transform;

    /// <summary>
    /// Main API: returns 1 direction (turn mode) or 2 directions (split mode).
    /// </summary>
    public Vector3[] GetOutputDirections(Vector3 incomingDirection)
    {
        float y = YawT.eulerAngles.y;

        bool nearPlus90 = Mathf.Abs(Mathf.DeltaAngle(y, 90f)) <= turnModeTolerance;
        bool nearMinus90 = Mathf.Abs(Mathf.DeltaAngle(y, -90f)) <= turnModeTolerance; // DeltaAngle supports -90

        // TURN MODE: only one beam
        if (nearPlus90 || nearMinus90)
        {
            bool turnRight = nearPlus90 ? plus90TurnsRight : !plus90TurnsRight;
            Vector3 turned = Turn90(incomingDirection, turnRight);
            return new[] { turned };
        }

        // SPLIT MODE: two beams
        Vector3 a, b;
        GetSplitDirections(incomingDirection, out a, out b);
        return new[] { a, b };
    }

    /// <summary>
    /// Split into +90 and -90 around mirror local Y (XZ plane).
    /// </summary>
    public void GetSplitDirections(Vector3 incomingDirection, out Vector3 dirPlus90, out Vector3 dirMinus90)
    {
        Vector3 localIn = transform.InverseTransformDirection(incomingDirection);
        localIn.y = 0f;

        if (localIn.sqrMagnitude < 0.0001f)
            localIn = Vector3.forward;

        localIn.Normalize();

        Vector3 localPlus = Quaternion.Euler(0f, 90f, 0f) * localIn;
        Vector3 localMinus = Quaternion.Euler(0f, -90f, 0f) * localIn;

        dirPlus90 = transform.TransformDirection(localPlus).normalized;
        dirMinus90 = transform.TransformDirection(localMinus).normalized;
    }

    private Vector3 Turn90(Vector3 incomingDirection, bool right)
    {
        Vector3 localIn = transform.InverseTransformDirection(incomingDirection);
        localIn.y = 0f;

        if (localIn.sqrMagnitude < 0.0001f)
            localIn = Vector3.forward;

        localIn.Normalize();

        float angle = right ? 90f : -90f;
        Vector3 localOut = Quaternion.Euler(0f, angle, 0f) * localIn;

        return transform.TransformDirection(localOut).normalized;
    }
}
