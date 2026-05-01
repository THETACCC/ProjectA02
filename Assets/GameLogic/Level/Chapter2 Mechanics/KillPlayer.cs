using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    private LevelController controller;

    public LevelFail myLevelFail;
    public bool isCheckPlayer1 = true;

    private void Start()
    {
        GameObject level_controller = GameObject.Find("LevelController");
        if (level_controller != null)
            controller = level_controller.GetComponent<LevelController>();

        GameObject myLevelFailOBJ = GameObject.FindGameObjectWithTag("LevelFail");
        if (myLevelFailOBJ != null)
            myLevelFail = myLevelFailOBJ.GetComponent<LevelFail>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Do not kill while dragging blocks
        if (controller != null)
        {
            if (controller.phase == LevelPhase.Draging ||
                controller.curDraggedblock != null ||
                controller.isAnyBlockDragging ||
                controller.isAnyBlockBeingDragged)
            {
                return;
            }
        }

        if (myLevelFail == null) return;

        if (other.CompareTag("Player1") && isCheckPlayer1)
        {
            Debug.Log("Touched Player1!!!!!!!!!!!!");
            myLevelFail.FailLevel();
        }
        else if (other.CompareTag("Player2") && !isCheckPlayer1)
        {
            Debug.Log("Touched Player2!!!!!!!!!!!!");
            myLevelFail.FailLevel();
        }
    }
}
