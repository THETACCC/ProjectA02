using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror90Degree : MonoBehaviour
{
    [Tooltip("How far to offset the new ray origin to avoid immediately re-hitting the mirror.")]
    public float surfaceEpsilon = 0.02f;

    [Header("Yaw Handling")]
    [Tooltip("If true, adds world Y rotation to the 90-degree turn")]
    public bool addWorldYaw = true;

    [Tooltip("Optional baseline yaw (use if your default mirror yaw should add 0)")]
    public bool useYawOffset = false;
    public float yawZero = 0f;

    public Vector3 GetRedirectOrigin(Collider col)
    {
        return col.bounds.center;
    }

    public Vector3 RedirectDirection(Vector3 incomingDirection)
    {
        // Flatten to XZ plane
        incomingDirection.y = 0f;
        incomingDirection.Normalize();

        // Base 90-degree turn (right turn)
        float turnAngle = -90f;

        if (addWorldYaw)
        {
            float worldY = transform.eulerAngles.y;

            if (useYawOffset)
                worldY = Mathf.DeltaAngle(yawZero, worldY);

            turnAngle += worldY;
        }

        // Rotate around world Y
        Vector3 redirected = Quaternion.AngleAxis(turnAngle, Vector3.up) * incomingDirection;

        redirected.y = 0f;
        return redirected.normalized;
    }
}