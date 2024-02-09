using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class MapControll : MonoBehaviour
{
    public LevelRotation rotation;
    public LevelLoader levelloader;

    GameObject[] blocks;
    GameObject[] blocks2;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!rotation.isRotating)
            {

                blocks = GameObject.FindGameObjectsWithTag(CommonReference.TAG_BLOCK);
                foreach (GameObject block in blocks)
                {
                    Block my_block = block.GetComponent<Block>();
                    if (my_block != null) // Check if the Block component is not null
                    {
                        my_block.StartMapRotation();
                    }
                    else
                    {
                        Debug.LogWarning("Block component not found on GameObject with tag " + CommonReference.TAG_BLOCK);
                    }
                }

            }

        }




        if (rotation.finishedRotation == true)
            {
                blocks = GameObject.FindGameObjectsWithTag(CommonReference.TAG_BLOCK);
                foreach (GameObject block in blocks)
                {
                    Block my_block = block.GetComponent<Block>();
                    if (my_block != null) // Check if the Block component is not null
                    {
                    my_block.EndMapRotation();
                    }
                    else
                    {
                    Debug.LogWarning("Block component not found on GameObject with tag " + CommonReference.TAG_BLOCK);
                    }
                }
                rotation.finishedRotation = false;
            }

        
    }

}
