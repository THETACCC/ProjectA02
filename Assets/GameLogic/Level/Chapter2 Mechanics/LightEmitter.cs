using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    public float maxDistance = 50f;
    public LayerMask raycastLayers;

    [Header("Safety")]
    public int maxSegments = 32;      // total segments processed per frame across all branches
    public float surfaceEpsilon = 0.02f;

    private readonly HashSet<LightReciever> hitThisFrame = new HashSet<LightReciever>();

    private struct BeamSegment
    {
        public Vector3 origin;
        public Vector3 dir;
        public float remaining;

        public BeamSegment(Vector3 o, Vector3 d, float r)
        {
            origin = o;
            dir = d.normalized;
            remaining = r;
        }
    }

    void Update()
    {
        CastBeamBranched();
    }

    private void CastBeamBranched()
    {
        // Reset receivers hit last frame
        foreach (var r in hitThisFrame)
        {
            if (r != null)
                r.NotifyNotHit();
        }
        hitThisFrame.Clear();

        var queue = new Queue<BeamSegment>();
        queue.Enqueue(new BeamSegment(transform.position, transform.right, maxDistance));

        int processed = 0;

        while (queue.Count > 0 && processed < maxSegments)
        {
            processed++;
            BeamSegment seg = queue.Dequeue();

            RaycastHit[] hits = Physics.RaycastAll(
                seg.origin,
                seg.dir,
                seg.remaining,
                raycastLayers,
                QueryTriggerInteraction.Collide
            );

            if (hits.Length == 0)
            {
                Debug.DrawLine(seg.origin, seg.origin + seg.dir * seg.remaining, Color.green);
                continue;
            }

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool branchedOrRedirected = false;

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                // 1) Wall blocks
                if (hit.collider.CompareTag("Wall"))
                {
                    Debug.DrawLine(seg.origin, hit.point, Color.red);
                    branchedOrRedirected = true; // this branch ends
                    break;
                }

                // 2) Receiver: mark hit, pass through
                if (hit.collider.CompareTag("LightEffector"))
                {
                    var receiver = hit.collider.GetComponentInParent<LightReciever>();
                    if (receiver != null)
                    {
                        receiver.HitByLight();
                        hitThisFrame.Add(receiver);
                    }
                    continue;
                }

                // 3) Splitter: two new branches
                var splitter = hit.collider.GetComponentInParent<MirrorBothSides>();
                if (splitter != null)
                {
                    Debug.DrawLine(seg.origin, hit.point, Color.yellow);

                    float newRemaining = seg.remaining - hit.distance;
                    if (newRemaining <= 0f) { branchedOrRedirected = true; break; }

                    Vector3 center = GetColliderWorldCenter(hit.collider);

                    // NEW: ask the mirror for outputs (1 or 2)
                    Vector3[] outs = splitter.GetOutputDirections(seg.dir);

                    for (int i = 0; i < outs.Length; i++)
                    {
                        Vector3 d = outs[i];
                        queue.Enqueue(new BeamSegment(center + d * surfaceEpsilon, d, newRemaining));
                    }

                    branchedOrRedirected = true;
                    break;
                }

                // 4) Normal 90бу mirror: one redirected branch
                var mirror90 = hit.collider.GetComponentInParent<Mirror90Degree>();
                if (mirror90 != null)
                {
                    Debug.DrawLine(seg.origin, hit.point, Color.cyan);

                    float newRemaining = seg.remaining - hit.distance;
                    if (newRemaining <= 0f) { branchedOrRedirected = true; break; }

                    Vector3 newDir = mirror90.RedirectDirection(seg.dir);
                    Vector3 center = GetColliderWorldCenter(hit.collider);

                    queue.Enqueue(new BeamSegment(center + newDir * surfaceEpsilon, newDir, newRemaining));

                    branchedOrRedirected = true;
                    break;
                }

                // 5) Other objects: ignore (or block if you prefer)
            }

            // If we didn't hit a wall/mirror/splitter (only receivers/ignored), draw the rest
            if (!branchedOrRedirected)
            {
                Debug.DrawLine(seg.origin, seg.origin + seg.dir * seg.remaining, Color.magenta);
            }
        }
    }

    private static Vector3 GetColliderWorldCenter(Collider col)
    {
        if (col is BoxCollider box)
            return box.transform.TransformPoint(box.center);

        return col.bounds.center;
    }
}