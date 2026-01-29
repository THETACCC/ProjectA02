using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightReciever : MonoBehaviour
{
    public bool isHit = false;

    private MeshRenderer myRenderer;

    [Header("Collision Child (auto-assigned)")]
    public GameObject myCollider;

    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();

        // Auto-get first child as collider object
        if (transform.childCount > 0)
        {
            myCollider = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogWarning(
                $"LightReciever on {gameObject.name} has no child to use as collider."
            );
        }
    }

    void Update()
    {
        if (myRenderer != null)
            myRenderer.enabled = !isHit;

        if (myCollider != null)
            myCollider.SetActive(!isHit);
    }

    public void HitByLight()
    {
        isHit = true;
    }
}
