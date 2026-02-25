using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEventBlockDragTutorial : MonoBehaviour
{
    public bool isReached = false;
    public BlockTutorialManager myNewManager;

    public BlockAlignment myAlignment;

    public bool isTriggered = false;

    private LevelController levelController;
    private void Start()
    {
        GameObject LevelControll = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        if (LevelControll != null)
        {
            levelController = LevelControll.GetComponent<LevelController>();
        }
    }

    public void Update()
    {

        if(myAlignment.isBlocked == true)
        {
            if (!isTriggered)
            {
                myNewManager.enableRotateTutorial();
                isTriggered = true;
            }
        }

    }

    public void OnTriggerEnter(Collider other)
    {

    }
}
