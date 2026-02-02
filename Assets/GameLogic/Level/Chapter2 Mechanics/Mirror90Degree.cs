using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror90Degree : MonoBehaviour
{
    [Tooltip("How far to offset the new ray origin to avoid immediately re-hitting the mirror.")]
    public float surfaceEpsilon = 0.02f;

    public Vector3 GetRedirectOrigin(Collider col)
    {
        // World-space center of the collider's bounds
        return col.bounds.center;
    }

    public Vector3 RedirectDirection(Vector3 incomingDirection)
    {
        incomingDirection.y = 0f;
        incomingDirection = incomingDirection.normalized;

        // Turn right 90 degrees around world up (Y)
        Vector3 redirected = Quaternion.AngleAxis(-90f, Vector3.up) * incomingDirection;
        redirected.y = 0f;
        return redirected.normalized;
    }
}