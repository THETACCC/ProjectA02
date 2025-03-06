using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map_Level_Icon_Emission : MonoBehaviour
{
    private Material materialInstance;

    void Start()
    {
        // Use material instead of sharedMaterial to create an instance for this object only
        materialInstance = GetComponent<Renderer>().material;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            // Enable emission
            materialInstance.EnableKeyword("_EMISSION");
            //materialInstance.SetColor("_EmissionColor", Color.white * 5f); // Set emission color and intensity
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Disable emission
            materialInstance.DisableKeyword("_EMISSION");
        }
    }
}
