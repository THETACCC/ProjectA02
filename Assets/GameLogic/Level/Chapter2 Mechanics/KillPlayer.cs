using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayer : MonoBehaviour
{

    public LevelFail myLevelFail;

    private void Start()
    {
        GameObject myLevelFailOBJ = GameObject.FindGameObjectWithTag("LevelFail");
        if(myLevelFailOBJ != null )
        {
            myLevelFail = myLevelFailOBJ.GetComponent<LevelFail>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            myLevelFail.FailLevel();
        }
    }

}
