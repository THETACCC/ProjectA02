using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotPos : MonoBehaviour
{

    public bool isPosEmpty = true;
    public float thresholdDistance = 5f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        FindNearestSlot();
    }


    void FindNearestSlot()
    {
        GameObject[] slots = GameObject.FindGameObjectsWithTag("Slot");
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
            }
        }

        // At this point, nearestSlot is the nearest slot within the threshold, or null if none are within the threshold
        if (nearestSlot != null)
        {
            isPosEmpty = false;
            // Implement what you want to do with the nearest slot

        }
        else
        {
            isPosEmpty = true;
        }
    }

}
