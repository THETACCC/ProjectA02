using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float maxDistance = 50f;
    public LayerMask raycastLayers;

    private void Update()
    {
        CastLightBeam();
    }

    private void CastLightBeam()
    {
        Vector3 direction = transform.right;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, raycastLayers))
        {
            HandleHit(hit);
            Debug.DrawLine(transform.position, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(
                transform.position,
                transform.position + direction * maxDistance,
                Color.green
            );
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        // Stop beam if wall is hit
        if (hit.collider.CompareTag("wall"))
        {
            return;
        }

        // Trigger LightEffector logic
        if (hit.collider.CompareTag("LightEffector"))
        {
            TriggerLightEffector(hit.collider.gameObject);
        }
    }

    private void TriggerLightEffector(GameObject target)
    {
        if (target.TryGetComponent<LightReciever>(out LightReciever receiver))
        {
            receiver.HitByLight();
        }
    }
}