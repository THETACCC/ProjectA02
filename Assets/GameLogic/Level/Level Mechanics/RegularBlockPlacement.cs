using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularBlockPlacement : MonoBehaviour
{
    BlockAlignment myAlignmentRight;
    public BlockAlignment myAlignmentLeft;
    public bool isRegularCanPlace = false;
    public GameObject PlaceIndicator;

    // Start is called before the first frame update
    void Start()
    {
        myAlignmentRight = GetComponent<BlockAlignment>();
        if (transform.childCount > 0)
        {
            PlaceIndicator = transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogWarning(gameObject.name + " has no child at index 0");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(myAlignmentRight != null && myAlignmentLeft != null) 
        {
            if(myAlignmentLeft.isBlocked == false && myAlignmentRight.isBlocked == false)
            {
                isRegularCanPlace= true;
            }
            else
            {
                isRegularCanPlace= false;
            }

        }
    }
}
