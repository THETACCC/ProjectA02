using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOnBlock : MonoBehaviour
{
    // get the player and the Block sciprt
    public Block blockRef;
    
    void Start()
    {
        blockRef = GetComponentInParent<Block>();   
    }

    public void OnTriggerEnter(Collider other)
    {
        if( other.gameObject.tag == "Player1" ||  other.gameObject.tag == "Player2")
        {
            if(blockRef != null)
            {
                if(blockRef.type == BlockType.Regular || blockRef.type == BlockType.Free) 
                {
                    if (!blockRef.isDragging)
                    {
                        blockRef.draggable = false;
                    }


                }

            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player1" || other.gameObject.tag == "Player2")
        {
            if (blockRef != null)
            {
                if (blockRef.type == BlockType.Regular || blockRef.type == BlockType.Free)
                {

                    if (!blockRef.isDragging)
                    {
                        blockRef.draggable = true;
                    }

                }
            }
        }
    }

}
