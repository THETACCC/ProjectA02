using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewTutorialManager : MonoBehaviour
{
    public GameObject mySpaceBar;

    public GameObject myRotationTutorial;

    public GameObject myBackGround;
    public void enableSpaceBarIndicate()
    {
        mySpaceBar.SetActive(true);
    }

    public void enableRotationTutorial()
    {
        mySpaceBar.SetActive(false);
        myRotationTutorial.SetActive(true);
    }

    public void disableTutorial()
    {
        mySpaceBar.SetActive(false);
        myRotationTutorial.SetActive(false);
    }

}
