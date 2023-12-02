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

    [SerializeField] GameObject inventoryParent;
    bool isInventoryOpened = true;

    GameObject draggedObject;
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
            if (draggedObject.transform.position.y > 170)
            {
                //When the object is throw out of the inventory
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Vector3 position = ray.GetPoint(200);

                GameObject newItem = Instantiate(draggedObject.GetComponent<InventoryItem>().ActualObject, position, new Quaternion());
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

        if (Input.GetKeyDown(KeyCode.A))
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
                slot.heldItem = null;
                lastItemSlot = clickedObject;
            }


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
                Vector3 position = ray.GetPoint(100);

                GameObject newItem = Instantiate(draggedObject.GetComponent<InventoryItem>().ActualObject, position, new Quaternion());
                lastItemSlot.GetComponent<InventorySlots>().heldItem = null;

                Destroy(draggedObject);

            }
            draggedObject = null;
        }
    }

}
