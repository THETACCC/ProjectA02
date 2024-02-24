using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlots : MonoBehaviour
{
    public GameObject heldItem;
    public GameObject emptyItemDisplay;

    public void Start()
    {

    }


    public void SetHeldItem(GameObject item)
    {
        heldItem = item;
        heldItem.transform.position = transform.position;
    }


    public void Update()
    {
        if (heldItem == null)
        {
            emptyItemDisplay.SetActive(true);
        }
        else
        {
            emptyItemDisplay.SetActive(false);
        }

    }

}
