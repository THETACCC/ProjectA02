using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SubPositionIndicator : MonoBehaviour
{
    private GameObject LevelControllerOBJ;
    private LevelController levelController;
    private MeshRenderer mr;
    private Camera mainCamera;
    public LayerMask interactableLayer; // Assign this in the Inspector
    private bool isMouseOver = false;
    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mainCamera = Camera.main; // Cache the main camera
        mr.enabled = false;
    }

    private void Start()
    {
        LevelControllerOBJ = GameObject.FindGameObjectWithTag ( "LevelPhaseControll" );
        levelController = LevelControllerOBJ.GetComponent<LevelController>();
    }


    private void Update()
    {
        if ((levelController.phase == LevelPhase.Placing) && (levelController.phase != LevelPhase.Speaking))
        {
            // Reset hover state at the beginning of each frame
            bool wasMouseOver = isMouseOver;
            isMouseOver = false;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Add a layer mask to only hit objects on the interactable layer
            // Also, specify a reasonable max distance for the raycast
            if (Physics.Raycast(ray, out hit, 1000f, interactableLayer))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red); // Draw the raycast line in the Scene view

                if (hit.collider.gameObject == gameObject)
                {

                    isMouseOver = true;
                    // Mouse is over the object
                    if (!mr.enabled)
                        mr.enabled = true;

                    if (Input.GetMouseButtonUp(0)) // Left mouse button
                    {
                        PlayerController pc = CommonReference.playerCharacters[LevelLoader.PosToMapID(transform.position)];
                        Debug.Log(pc.name);
                        pc.transform.position = new Vector3(transform.position.x, pc.transform.position.y, transform.position.z);
                    }
                }
            }


            if (wasMouseOver && !isMouseOver)
            {
                mr.enabled = false;
            }

            if (!isMouseOver)
            {
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue); // Draw the raycast line when not hitting
            }
        }
        else
        {
            mr.enabled = false;
        }

    }

    /*
    private MeshRenderer mr;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();  
        mr.enabled = true;
    }
    private void OnMouseEnter()
    {
        mr.enabled = false;
    }

    private void OnMouseExit()
    {
        mr.enabled= true;
    }

    private void OnMouseUpAsButton()
    {
        PlayerCharacter pc = CommonReference.playerCharacters[LevelLoader.PosToMapID(transform.position)];
        pc.transform.position = new Vector3(transform.position.x, pc.transform.position.y, transform.position.z);
    }
    */
}
