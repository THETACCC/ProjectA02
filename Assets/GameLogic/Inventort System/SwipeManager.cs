using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeManager : MonoBehaviour
{
    public MouseSwipe mouseSwipe;

    public InventorySlots[] inventorySlots;
    
    public SlotPos[] slotPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (mouseSwipe.isSwipeRight && slotPos[slotPos.Length - 1].isPosEmpty)
        {
            MoveSlotsRight();
        }
        else if (mouseSwipe.isSwipeLeft && slotPos[0].isPosEmpty)
        {
            MoveSlotsLeft();
        }
    }

    void MoveSlotsRight()
    {
        Debug.Log("SwipeRight");
        // Check if there's an empty slot to move into before shifting slots right

        // 6  7  8 Are magic numbers as we are using 9 slot pos
        if (slotPos[6].isPosEmpty) // Assuming isOccupied is a way to check if slotPos 5 is empty
        {
            for (int i = inventorySlots.Length - 1; i >= 0; i--) // Start from slot 4 (5th slot), as the last slot (6th slot) moves into slotPos[5]
            {
                MoveSlotToPosition(inventorySlots[i], i + 1);
            }

        }
        else if (slotPos[7].isPosEmpty)
        {
            for (int i = inventorySlots.Length - 1; i >= 0; i--) // Start from slot 4 (5th slot), as the last slot (6th slot) moves into slotPos[5]
            {
                MoveSlotToPosition(inventorySlots[i], i + 2);
            }
        }
        else if (slotPos[8].isPosEmpty)
        {
            for (int i = inventorySlots.Length - 1; i >= 0; i--) // Start from slot 4 (5th slot), as the last slot (6th slot) moves into slotPos[5]
            {
                MoveSlotToPosition(inventorySlots[i], i + 3);
            }
        }
    }

    void MoveSlotsLeft()
    {
        Debug.Log("SwipeLeft");
        // Start moving from the first slot
        if (slotPos[2].isPosEmpty) // Assuming isOccupied is a way to check if slotPos 5 is empty
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                int newPosIndex = i + 2;
                MoveSlotToPosition(inventorySlots[i], newPosIndex);
            }
        }
        else if (slotPos[1].isPosEmpty) // Assuming isOccupied is a way to check if slotPos 5 is empty
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                int newPosIndex = i + 1;
                MoveSlotToPosition(inventorySlots[i], newPosIndex);
            }
        }
        else if (slotPos[0].isPosEmpty) // Assuming isOccupied is a way to check if slotPos 5 is empty
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                int newPosIndex = i;
                MoveSlotToPosition(inventorySlots[i], newPosIndex);
            }
        }
    }

    private void MoveSlotToPosition(InventorySlots slot, int newPosIndex)
    {
        RectTransform slotRect = slot.GetComponent<RectTransform>();
        RectTransform newPosRect = slotPos[newPosIndex].GetComponent<RectTransform>();

        if (slotRect != null && newPosRect != null)
        {
            slot.StartMovingPosition(newPosRect.anchoredPosition);
        }
    }

    // This function updates the internal tracking of slots' positions after a move.
    // You might need to adjust this logic based on how you're tracking slots' positions.

}
