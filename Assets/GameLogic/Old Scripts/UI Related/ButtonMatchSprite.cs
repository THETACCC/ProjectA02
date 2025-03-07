using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMatchSprite : MonoBehaviour
{

    public float alphaThresold = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThresold;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
