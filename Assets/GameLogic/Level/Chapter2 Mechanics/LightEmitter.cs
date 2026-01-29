using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    public float maxDistance = 50f;
    public LayerMask raycastLayers;

    private readonly HashSet<LightReciever> lastFrameHit = new HashSet<LightReciever>();

    void Update()
    {
        CastLightBeam();
    }

    private void CastLightBeam()
    {
        // Reset: anything hit last frame is assumed not hit this frame until proven otherwise
        foreach (var r in lastFrameHit)
            r.isHit = false;
        lastFrameHit.Clear();

        Vector3 origin = transform.position;
        Vector3 direction = transform.right;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, raycastLayers);

        // No hits: draw full beam and done
        if (hits.Length == 0)
        {
            Debug.DrawLine(origin, origin + direction * maxDistance, Color.green);
            return;
        }

        // Sort by distance so we process from near to far
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Process hits until we hit a wall
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            // Beam stops at wall
            if (hit.collider.CompareTag("wall"))
            {
                Debug.DrawLine(origin, hit.point, Color.red);
                return;
            }

            // Hit receivers before the wall
            if (hit.collider.CompareTag("LightEffector"))
            {
                if (hit.collider.TryGetComponent(out LightReciever receiver))
                {
                    receiver.HitByLight();
                    lastFrameHit.Add(receiver);
                }
            }
        }

        // If we never hit a wall, beam goes full distance
        Debug.DrawLine(origin, origin + direction * maxDistance, Color.yellow);
    }
}