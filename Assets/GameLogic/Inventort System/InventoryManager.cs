using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //References
    [SerializeField] Camera cam;

    //Inventory systems

    [SerializeField] GameObject inventoryParent;
    bool isInventoryOpened = true;

    GameObject draggedObject;
    GameObject lastItemSlot;
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        inventoryParent.SetActive(isInventoryOpened);


        if (draggedObject != null)
        {
            draggedObject.transform.position = Input.mousePosition;
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

            Debug.Log(eventData.pointerCurrentRaycast.gameObject);
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
                Vector3 position = ray.GetPoint(3);

                GameObject newItem = Instantiate(draggedObject.GetComponent<InventoryItem>().itemScriptableObject.prefab, position, new Quaternion());
                lastItemSlot.GetComponent<InventorySlots>().heldItem = null;

                Destroy(draggedObject);

            }
            draggedObject = null;
        }
    }
    

}
