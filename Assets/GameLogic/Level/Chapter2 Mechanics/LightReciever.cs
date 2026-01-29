using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightReciever : MonoBehaviour
{

    public bool isHit = false;
    private BoxCollider myCollider;
    private MeshRenderer myRenderer;
    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
        myRenderer = GetComponent<MeshRenderer>();  
    }

    // Update is called once per frame
    void Update()
    {
        if(isHit)
        {
            myCollider.enabled = false;
            myRenderer.enabled = false;
        }
        else
        {
            myCollider.enabled = true;
            myRenderer.enabled = true;
        }
    }

    public void HitByLight()
    {
        isHit = true;
    }
}
