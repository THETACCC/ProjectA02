using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror90Degree : MonoBehaviour
{
    public float surfaceEpsilon = 0.02f;

    private const float TURN_LEFT = 90f;
    private const float TURN_RIGHT = 270f; // -90

    private enum Cardinal { PosX, NegX, PosZ, NegZ }

    public Vector3 GetRedirectOrigin(Collider col) => col.bounds.center;

    /// <summary>
    /// Returns redirected direction. Returns Vector3.zero when this mirror should NOT reflect
    /// for the given incoming direction + mirror yaw state.
    /// </summary>
    public Vector3 RedirectDirection(Vector3 incomingWorldDirection)
    {
        incomingWorldDirection.y = 0f;
        if (incomingWorldDirection.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        incomingWorldDirection.Normalize();

        int yaw = SnapYawTo90(transform.eulerAngles.y); // 0,90,180,270
        Cardinal c = ClassifyCardinal(incomingWorldDirection);

        if (!TryGetTurn(yaw, c, out float turnDeg))
            return Vector3.zero; // no reflection for this case

        Vector3 outDir = Quaternion.AngleAxis(turnDeg, Vector3.up) * incomingWorldDirection;
        outDir.y = 0f;
        return outDir.normalized;
    }

    // ----------------- YOUR RULES -----------------
    // Incoming +Z:
    //  - yaw 270 (-90) -> RIGHT
    //  - yaw 0         -> LEFT
    //
    // Incoming -Z:
    //  - yaw 90        -> RIGHT
    //  - yaw 180 (-180)-> LEFT
    //
    // Incoming -X:
    //  - yaw 180 (-180)-> RIGHT
    //  - yaw 270 (-90) -> LEFT
    //
    // (Plus earlier rule you stated: incoming +X, yaw0 -> +90 ; incoming +X, yaw90 -> -90)
    private static bool TryGetTurn(int yawSnap, Cardinal incoming, out float turnDeg)
    {
        turnDeg = 0f;

        switch (incoming)
        {
            case Cardinal.PosX:
                if (yawSnap == 0) { turnDeg = TURN_LEFT; return true; }  // +X @0  -> left
                if (yawSnap == 90) { turnDeg = TURN_RIGHT; return true; }  // +X @90 -> right
                return false;

            case Cardinal.PosZ:
                if (yawSnap == 0) { turnDeg = TURN_RIGHT; return true; } // +Z @0   -> left
                if (yawSnap == 270) { turnDeg = TURN_LEFT; return true; } // +Z @270 -> right
                return false;

            case Cardinal.NegZ:
                if (yawSnap == 90) { turnDeg = TURN_LEFT; return true; } // -Z @90  -> right
                if (yawSnap == 180) { turnDeg = TURN_RIGHT; return true; } // -Z @180 -> left
                return false;

            case Cardinal.NegX:
                if (yawSnap == 180) { turnDeg = TURN_LEFT; return true; } // -X @180 -> right
                if (yawSnap == 270) { turnDeg = TURN_RIGHT; return true; } // -X @270 -> left
                return false;
        }

        return false;
    }

    // ----------------- helpers -----------------
    private static int SnapYawTo90(float yaw)
    {
        int snapped = Mathf.RoundToInt(yaw / 90f) * 90;
        snapped %= 360;
        if (snapped < 0) snapped += 360;
        return snapped;
    }

    private static Cardinal ClassifyCardinal(Vector3 dir)
    {
        float ax = Mathf.Abs(dir.x);
        float az = Mathf.Abs(dir.z);

        if (ax >= az)
            return (dir.x >= 0f) ? Cardinal.PosX : Cardinal.NegX;
        else
            return (dir.z >= 0f) ? Cardinal.PosZ : Cardinal.NegZ;
    }
}