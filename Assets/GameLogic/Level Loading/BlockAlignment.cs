using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Beautify.Universal.Beautify;

public class BlockAlignment : MonoBehaviour
{
    public bool isInventoryBrick = false;
    public bool isBlocked = false;




    public GameObject PlaceIndicator;
    // The distance threshold within which the object will align with the block
    public float proximityThreshold = 1.0f;

    // The layer or tag to detect
    public string targetTag = "Block";

    public LevelController levelController;

    private float Timer = 1;
    private float time = 0;
    // Start is called before the first frame update
    void Start()
    {
        GameObject level_controller = GameObject.Find("LevelController");
        levelController = level_controller.GetComponent<LevelController>();
        AlignWithNearestBlockInRange();
    }

    private void Update()
    {
        if (levelController.isAnyBlockBeingDragged == false) 
        {

            time += Time.deltaTime;
            if(time > Timer)
            {
                time = 0;
                AlignWithNearestBlockInRangeDuringGame();
            }


        }
    }

    // Update is called once per frame
    private void AlignWithNearestBlockInRange()
    {
        // Find all objects with the "Block" tag
        GameObject[] blocks = GameObject.FindGameObjectsWithTag(targetTag);

        GameObject nearestBlock = null;
        float nearestDistance = Mathf.Infinity;

        // Loop through each block to find the nearest one within the threshold
        foreach (GameObject block in blocks)
        {
            float distance = Vector3.Distance(transform.position, block.transform.position);

            // Check if this block is closer than the previously found ones and within the threshold
            if (distance < nearestDistance && distance <= proximityThreshold)
            {
                nearestDistance = distance;
                nearestBlock = block;
            }
        }

        // Align with the nearest block if it's within range
        if (nearestBlock != null)
        {
            AlignPosition(nearestBlock.transform);
            Block mBlock = nearestBlock.GetComponent<Block>();
            if(mBlock != null && isInventoryBrick) 
            {

                mBlock.isInventory = true;
            }

            isBlocked = true;
        }
    }
    private void AlignWithNearestBlockInRangeDuringGame()
    {
        if(isBlocked)
        {
            return;
        }
        // Find all objects with the "Block" tag
        GameObject[] blocks = GameObject.FindGameObjectsWithTag(targetTag);

        GameObject nearestBlock = null;
        float nearestDistance = Mathf.Infinity;

        // Loop through each block to find the nearest one within the threshold
        foreach (GameObject block in blocks)
        {
            float distance = Vector3.Distance(transform.position, block.transform.position);

            // Check if this block is closer than the previously found ones and within the threshold
            if (distance < nearestDistance && distance <= proximityThreshold)
            {
                nearestDistance = distance;
                nearestBlock = block;
            }
        }

        // Align with the nearest block if it's within range
        if (nearestBlock != null)
        {
            AlignPosition(nearestBlock.transform);
            Block mBlock = nearestBlock.GetComponent<Block>();
            if (mBlock != null && isInventoryBrick)
            {

                mBlock.isInventory = true;
            }

            isBlocked = true;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter Trigger");
        if(other.gameObject.tag == targetTag) 
        {

            AlignWithNearestBlockInRange();

        }
    }

    private void AlignPosition(Transform target)
    {
        // Align the position of this object to the target's position
        target.position = transform.position;
    }

}
