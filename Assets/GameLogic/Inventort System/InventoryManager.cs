using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //References
    [SerializeField] Camera cam;
    [SerializeField] GameObject dropItemUI;
    //Inventory systems

    [SerializeField] GameObject[] slots = new GameObject[4];
    [SerializeField] GameObject inventoryParent;
    [SerializeField] GameObject itemPrefab;
    bool isInventoryOpened = true;

    public GameObject draggedObject;
    GameObject lastItemSlot;
    

    void Start()
    {
        GameObject cameraGameObject = GameObject.FindGameObjectWithTag("MainCamera");
        cam = cameraGameObject.GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Cinemachine camera not found!");
        }

    }

    // Update is called once per frame
    void Update()
    {

        inventoryParent.SetActive(isInventoryOpened);


        if (draggedObject != null)
        {
            draggedObject.transform.position = Input.mousePosition;
           // Debug.Log(draggedObject.transform.position.y);
            if (draggedObject.transform.position.y > 100)
            {
                //When the object is throw out of the inventory
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 position = ray.GetPoint(200);
                //The magic number 110 is to fix the issue where the block will mismatch the position it should be in orthographic CAm

                //Use this if the camera is not orthographic
                //GameObject newItem = Instantiate(draggedObject.GetComponent<InventoryItem>().ActualObject, position, new Quaternion());

                //This code is for the instanitation of the dragged blocks, the magic numbers for position is to make sure it align correctly
                GameObject newItem = Instantiate(draggedObject.GetComponent<InventoryItem>().ActualObject, new Vector3(position.x - 40,0, position.z - 40), new Quaternion());
                Destroy(draggedObject);
                //Instantiate draggable blocks in to the scene, make sure the block register the mouse and is moving
                lastItemSlot.GetComponent<InventorySlots>().heldItem = null;
                Block block = newItem.GetComponent<Block>();
                block.mouse_drag = true;


                block._OnStartDrag();
                //block.instantiateBlocks();
                //block.instantiateBlocks();


                draggedObject = null;

            }


        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if(isInventoryOpened)
            {
                isInventoryOpened = false;
            }
            else
            {
                isInventoryOpened = true;
            }
        }



    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlots slot = clickedObject.GetComponent<InventorySlots>();

            if (slot != null && slot.heldItem != null)
            {
                draggedObject = slot.heldItem;
                //change this for a idle object
                slot.heldItem = null;
                lastItemSlot = clickedObject;
            }


        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {

            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlots slot = clickedObject.GetComponent<InventorySlots>();
            Debug.Log(clickedObject.name);

        }

    }




    public void OnPointerUp(PointerEventData eventData)
    {
        if (draggedObject != null && eventData.pointerCurrentRaycast.gameObject != null && eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedobject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlots slot = clickedobject.GetComponent<InventorySlots>();

            if (slot != null && slot.heldItem == null)
            {
                slot.SetHeldItem(draggedObject);

            }
            else if (slot != null && slot.heldItem != null)
            {
                lastItemSlot.GetComponent<InventorySlots>().SetHeldItem(slot.heldItem);
                slot.SetHeldItem(draggedObject);

            }
            else if (clickedobject.name != "DropItem")
            {
                lastItemSlot.GetComponent<InventorySlots>().SetHeldItem(draggedObject);
            }
            else
            {
                //When the object is throw out of the inventory
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 position = ray.GetPoint(50);

                GameObject newItem = Instantiate(draggedObject.GetComponent<InventoryItem>().ActualObject, new Vector3(position.x, 0, position.z), new Quaternion());
                lastItemSlot.GetComponent<InventorySlots>().heldItem = null;

                Destroy(draggedObject);

            }
            draggedObject = null;
        }
    }


    //code for putting blocks back to the inventory
    /*
    public void ItemPicked(GameObject Item)
    {
        GameObject emptyslot = null;

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlots slot = slots[i].GetComponent<InventorySlots>();

            if (slot.heldItem == null)
            {
                emptyslot = slots[i];
                break;
            }

        }

        if (emptyslot != null)
        {
            GameObject newItem = Instantiate(itemPrefab);
            newItem.GetComponent<InventoryItem>().itemScriptableObject = Item.GetComponent<ItemPickable>().itemScriptableOBJ;
            //newItem.GetComponent<InventoryItem>().ActualObject = Item.GetComponent<Block>().blocka;

            newItem.transform.SetParent(emptyslot.transform.parent.parent.GetChild(1));
            //This line of code is to set the scale of the blocks in UI
            newItem.transform.localScale = new Vector3 (.8f, .8f, .8f);
            emptyslot.GetComponent<InventorySlots>().SetHeldItem(newItem); 

            Destroy(Item);
        }


    }
    */
    public void ItemPicked(GameObject Item)
    {
        GameObject nearestEmptySlot = FindNearestEmptySlotToMouse();

        if (nearestEmptySlot != null)
        {
            GameObject newItem = Instantiate(itemPrefab);
            newItem.GetComponent<InventoryItem>().itemScriptableObject = Item.GetComponent<ItemPickable>().itemScriptableOBJ;

            newItem.transform.SetParent(nearestEmptySlot.transform.parent.parent.GetChild(1), false);
            newItem.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f); // Set the scale of the item in the UI
            nearestEmptySlot.GetComponent<InventorySlots>().SetHeldItem(newItem);

            Destroy(Item);
        }
    }

    private GameObject FindNearestEmptySlotToMouse()
    {
        GameObject nearestEmptySlot = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject slot in slots)
        {
            InventorySlots slotInfo = slot.GetComponent<InventorySlots>();
            if (slotInfo.heldItem == null)
            {
                Vector2 slotPosition = RectTransformUtility.WorldToScreenPoint(null, slot.transform.position);
                float distance = Vector2.Distance(slotPosition, Input.mousePosition);

                if (distance < nearestDistance)
                {
                    nearestEmptySlot = slot;
                    nearestDistance = distance;
                }
            }
        }

        return nearestEmptySlot;
    }
}
