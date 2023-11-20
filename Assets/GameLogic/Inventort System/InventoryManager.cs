using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Start is called before the first frame update
    GameObject draggedObject;
    GameObject lastItemSlot;
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (draggedObject != null)
        {
            draggedObject.transform.position = Input.mousePosition;
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
                draggedObject = null;
            }
            else if (slot != null && slot.heldItem != null)
            {
                lastItemSlot.GetComponent<InventorySlots>().SetHeldItem(slot.heldItem);
                slot.SetHeldItem(draggedObject);
                draggedObject = null;
            }
        }
    }
    

}
