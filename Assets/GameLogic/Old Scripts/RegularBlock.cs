using SKCell;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RegularBlock : MonoBehaviour
{
    public Block block;
    public GameObject ConnectedBlock;

    private Vector3 distanceBetweenBlocks = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {




        var mouseinfo = ConnectedBlock.GetComponent<Block>();
        if (mouseinfo.mouse_drag == true)
        {
            var info = ConnectedBlock.GetComponent<Transform>();
            transform.position = new float3(info.position.x - 42, info.position.y, info.position.z);
            
        }
        else if (mouseinfo.mouse_drag == false)
        {

        }
        //else if (Input.GetMouseButtonUp(0))
        //{
             
        //    block._OnEndDrag();
        //}



        if (block.rotated)
        {
            var info2 = ConnectedBlock.GetComponent<Block>();
            info2.rotate += new Vector3(0, 90, 0);
        }

        distanceBetweenBlocks = gameObject.GetComponent<Vector3>() - ConnectedBlock.GetComponent<Vector3>();
    }
}
