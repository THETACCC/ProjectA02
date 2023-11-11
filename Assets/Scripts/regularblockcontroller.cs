using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class regularblockcontroller : MonoBehaviour
{
    public GameObject blockPrefab; // The block prefab to be cloned
    private GameObject leftBlock;
    private GameObject rightBlock;
    private bool isDraggingLeft = false;
    private bool isDraggingRight = false;

    void Start()
    {
        // Instantiate the initial block (leftBlock) from the inventory
        leftBlock = Instantiate(blockPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        // Clone the block to create rightBlock
        rightBlock = Instantiate(leftBlock, new Vector3(leftBlock.transform.position.x + 2, leftBlock.transform.position.y, 0), Quaternion.identity);
    }



}
