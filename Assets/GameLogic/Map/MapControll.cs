using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapControll : MonoBehaviour
{
    public LevelRotation rotation;
    public LevelLoader levelloader;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!rotation.isRotating)
            {
                levelloader.LoadMap();
            }

        }
    }

}
