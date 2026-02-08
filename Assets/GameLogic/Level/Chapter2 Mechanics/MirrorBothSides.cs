using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorBothSides : MonoBehaviour
{
    [Header("Read yaw from this transform (set if you rotate a child/parent)")]
    public Transform yawSource; // if null, uses transform

    [Header("Yaw snapping")]
    public float yawSnapTolerance = 6f;

    private Transform YawT => yawSource != null ? yawSource : transform;

    public enum Cardinal { PosX, NegX, PosZ, NegZ }
    public enum Action { Split, TurnLeft, TurnRight, Stop }

    /// <summary>
    /// Main API: returns 0 (stop), 1 (turn), or 2 (split) output directions in WORLD space.
    /// </summary>
    public Vector3[] GetOutputDirections(Vector3 incomingWorldDirection)
    {
        incomingWorldDirection.y = 0f;
        if (incomingWorldDirection.sqrMagnitude < 0.0001f)
            return System.Array.Empty<Vector3>();

        incomingWorldDirection.Normalize();

        int yaw = SnapYawTo90(YawT.eulerAngles.y, yawSnapTolerance);
        Cardinal inc = ClassifyCardinal(incomingWorldDirection);

        Action act = GetAction(inc, yaw);

        switch (act)
        {
            case Action.Stop:
                return System.Array.Empty<Vector3>();

            case Action.TurnRight:
                return new[] { TurnRelativeToMirror(incomingWorldDirection, right: true) };

            case Action.TurnLeft:
                return new[] { TurnRelativeToMirror(incomingWorldDirection, right: false) };

            case Action.Split:
            default:
                {
                    Vector3 left = TurnRelativeToMirror(incomingWorldDirection, right: false);
                    Vector3 right = TurnRelativeToMirror(incomingWorldDirection, right: true);
                    return new[] { right, left };
                }
        }
    }

    // ---------------- YOUR RULE TABLE ----------------
    // Incoming +X:
    //  yaw 0   -> Split
    //  yaw 90  -> TurnRight
    //  yaw 180 -> Stop
    //  yaw 270 -> TurnLeft
    //
    // Incoming -Z:
    //  yaw 90  -> Split
    //  yaw 180 -> TurnRight
    //  yaw 270 -> Stop
    //  yaw 0   -> TurnLeft
    //
    // Incoming +Z:
    //  yaw 270 -> Split
    //  yaw 0   -> TurnRight
    //  yaw 90  -> Stop
    //  yaw 180 -> TurnLeft
    //
    // Incoming -X:
    //  yaw 180 -> Split
    //  yaw 270 -> TurnRight
    //  yaw 0   -> Stop
    //  yaw 90  -> TurnLeft   // CHANGE HERE if you meant TurnRight instead
    private static Action GetAction(Cardinal incoming, int yaw)
    {
        switch (incoming)
        {
            case Cardinal.PosX:
                return yaw switch
                {
                    0 => Action.Split,
                    90 => Action.TurnRight,
                    180 => Action.Stop,
                    270 => Action.TurnLeft,
                    _ => Action.Stop
                };

            case Cardinal.NegZ:
                return yaw switch
                {
                    90 => Action.Split,
                    180 => Action.TurnRight,
                    270 => Action.Stop,
                    0 => Action.TurnLeft,
                    _ => Action.Stop
                };

            case Cardinal.PosZ:
                return yaw switch
                {
                    270 => Action.Split,
                    0 => Action.TurnRight,
                    90 => Action.Stop,
                    180 => Action.TurnLeft,
                    _ => Action.Stop
                };

            case Cardinal.NegX:
                return yaw switch
                {
                    180 => Action.Split,
                    270 => Action.TurnRight,
                    0 => Action.Stop,
                    90 => Action.TurnLeft, // CHANGE HERE if needed
                    _ => Action.Stop
                };

            default:
                return Action.Stop;
        }
    }

    // Turn ¡À90 in MIRROR-LOCAL space so rotating the mirror changes left/right behavior.
    private Vector3 TurnRelativeToMirror(Vector3 incomingWorldDirection, bool right)
    {
        Vector3 localIn = transform.InverseTransformDirection(incomingWorldDirection);
        localIn.y = 0f;

        if (localIn.sqrMagnitude < 0.0001f)
            localIn = Vector3.forward;

        localIn.Normalize();

        float angle = right ? -90f : 90f;
        Vector3 localOut = Quaternion.Euler(0f, angle, 0f) * localIn;

        Vector3 worldOut = transform.TransformDirection(localOut);
        worldOut.y = 0f;
        return worldOut.normalized;
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

    private static int SnapYawTo90(float yaw, float tolerance)
    {
        int[] candidates = { 0, 90, 180, 270 };
        float best = 999f;
        int bestYaw = 0;

        foreach (int c in candidates)
        {
            float d = Mathf.Abs(Mathf.DeltaAngle(yaw, c));
            if (d < best)
            {
                best = d;
                bestYaw = c;
            }
        }

        // Always snap to the nearest cardinal yaw (stable puzzle logic)
        return bestYaw;
    }
}