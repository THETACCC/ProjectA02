using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTutorialManager : MonoBehaviour
{
    public GameObject myDragTutorial;

    public GameObject myRotateTutorial;
    public void enableDragTutorial()
    {
        myDragTutorial.SetActive(true); 
    }

    public void enableRotateTutorial()
    {
        myDragTutorial.SetActive(false);
        myRotateTutorial.SetActive(true);
    }

    public void disableAllTutorial()
    {
        myRotateTutorial.SetActive(false);
        myDragTutorial.SetActive(false);
    }

}
