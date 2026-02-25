using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEventTutorial : MonoBehaviour
{
    public bool isReached = false;
    public NewTutorialManager myNewManager;


    private LevelController levelController;
    private void Start()
    {
        GameObject LevelControll = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        if (LevelControll != null)
        {
            levelController = LevelControll.GetComponent<LevelController>();
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (isReached) return; // already collected

        if (other.CompareTag("Player1") || other.CompareTag("Player2") || other.CompareTag("Player2Tutorial") || other.CompareTag("Player1Tutorial"))
        {
            isReached = true;
            myNewManager.enableSpaceBarIndicate();
            levelController.phase = LevelPhase.Running;
        }

    }
}
