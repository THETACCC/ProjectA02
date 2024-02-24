using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseSwipe : MonoBehaviour
{


    public float xStart;
    public float xEnd;

    private bool isPosCheck = true;
    private bool isStartPosInPlace = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            xStart = Input.mousePosition.x;
            CheckForDropOnUIImage();
            isPosCheck = true;

        }

        if (Input.GetMouseButtonUp(0))
        {
            xEnd = Input.mousePosition.x;
            isPosCheck = false;
        }


        if ((xStart > xEnd) && (!isPosCheck) && isStartPosInPlace)
        {

            Debug.Log("MoveLeft");
            isPosCheck = true;
            isStartPosInPlace = false;
            xStart = 0;
            xEnd = 0;
        }


        if ((xStart < xEnd) && (!isPosCheck) && isStartPosInPlace)
        {
            Debug.Log("MoveRight");
            isPosCheck = true;
            isStartPosInPlace = false;
            xStart = 0;
            xEnd = 0;
        }

    }

    void CheckForDropOnUIImage()
    {
        // Create a pointer event data using the current event system
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        // Create a list to store the raycast results
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        // Perform the raycast
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        // Check if any of the raycast results is the UI Image you're interested in
        foreach (var result in raycastResults)
        {
                if (result.gameObject.CompareTag("BlockInventory")) // Ensure your UI Image has this tag
                {
                        isStartPosInPlace = true;
                        return;
                }
        }
        isStartPosInPlace = false;
        Debug.Log("Not dropped on a UI Image.");
    }
}
