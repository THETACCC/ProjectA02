using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBlockSelector : MonoBehaviour
{
    [Header("Placement")]
    public float elevation = 2f;

    [Header("References")]
    [SerializeField] private Collider blockCld;
    [SerializeField] private GameObject indicatorPrefab;
    public GameObject Parent;

    // Optional fine-tune if your prefab mesh/pivot is offset
    public Vector3 manualOffsetWorld = Vector3.zero;

    private void Start()
    {
        if (!blockCld || !indicatorPrefab)
        {
            Debug.LogError("[StartBlockSelector] Missing references.");
            return;
        }

        // World-space bounds
        Bounds bounds = blockCld.bounds;

        // Centered top plane
        Vector3 centerTop = new Vector3(
            bounds.center.x,
            bounds.max.y + elevation,
            bounds.center.z
        );

        // Use per-axis spacing (in case X != Z)
        float stepX = 4f;
        float stepZ = 4f;

        // Build a symmetric 3x3 around the center: indices -1, 0, +1
        for (int ix = -1; ix <= 1; ix++)
        {
            for (int iz = -1; iz <= 1; iz++)
            {
                Vector3 worldPos = centerTop + new Vector3(ix * stepX - 0.45f, 0f, iz * stepZ) + manualOffsetWorld;

                // Place at exact world position, then parent (keeps world pos)
                Transform parentT = Parent ? Parent.transform : null;
                Instantiate(indicatorPrefab, worldPos, Quaternion.identity, parentT);
            }
        }
    }
}