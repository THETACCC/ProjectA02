using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlots : MonoBehaviour
{
    public GameObject heldItem;
    public GameObject emptyItemDisplay;
    private RectTransform rectTransform;


    //reference to the slot Pos
    public GameObject slotPos;
    public float thresholdDistance = 5f;
    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }


    public void SetHeldItem(GameObject item)
    {
        heldItem = item;
        heldItem.transform.position = transform.position;
    }


    public void Update()
    {
        FindNearestSlot();
        if (heldItem != null)
        {
            heldItem.transform.position = transform.position;
        }


        if (heldItem == null)
        {
            emptyItemDisplay.SetActive(true);
        }
        else
        {
            emptyItemDisplay.SetActive(false);
        }

    }
    void FindNearestSlot()
    {
        GameObject[] slots = GameObject.FindGameObjectsWithTag("SlotPos");
        GameObject nearestSlot = null;
        float minDistance = Mathf.Infinity;
        Vector2 currentPosition = this.GetComponent<RectTransform>().anchoredPosition;

        foreach (GameObject slot in slots)
        {
            Vector2 slotPosition = slot.GetComponent<RectTransform>().anchoredPosition;
            float distance = Vector2.Distance(currentPosition, slotPosition);
            if (distance < minDistance && distance <= thresholdDistance)
            {
                nearestSlot = slot;
                minDistance = distance;
                slotPos = nearestSlot;
            }
        }
    }

    public void movePosition(Vector2 StartPos, Vector2 EndPos)
    {
        // SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
        // {
        //     rectTransform.anchoredPosition = Vector2.Lerp(StartPos, EndPos, f);
        // }, null, gameObject.GetInstanceID() + "Swipe Success");
        rectTransform.anchoredPosition = Vector2.Lerp(StartPos, EndPos, .2f);
    }
    public void StartMovingPosition(Vector2 endPos)
    {
        // Assuming rectTransform is already initialized
        StartCoroutine(MovePosition(rectTransform.anchoredPosition, endPos, 1f)); // Adjust the duration as needed
    }
    IEnumerator MovePosition(Vector2 startPos, Vector2 endPos, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration; // Normalized time
            float easeT = EaseOutCubic(t);
            // Optionally, you can use easing functions for a smoother effect
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
            yield return null; // Wait for the next frame
        }

        rectTransform.anchoredPosition = endPos; // Ensure it ends exactly at the endPos
    }

    float EaseOutCubic(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }
}
