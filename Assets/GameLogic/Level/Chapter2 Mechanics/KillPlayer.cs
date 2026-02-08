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
        controller = level_controller.GetComponent<LevelController>();

        GameObject myLevelFailOBJ = GameObject.FindGameObjectWithTag("LevelFail");
        if(myLevelFailOBJ != null )
        {
            myLevelFail = myLevelFailOBJ.GetComponent<LevelFail>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") && isCheckPlayer1 && (controller.phase != LevelPhase.Draging))
        {

            myLevelFail.FailLevel();
        }
        else if (other.CompareTag("Player2") && !isCheckPlayer1 && (controller.phase != LevelPhase.Draging))
        {
            myLevelFail.FailLevel();
        }
    }

}
