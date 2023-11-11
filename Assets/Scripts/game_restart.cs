using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class game_restart : MonoBehaviour
{
    [HideInInspector]
    public bool isActive = true;

    public SceneInfo sceneInfo;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (Scenecontroller.instance.LoadSceneAsset(sceneInfo))
                isActive = false;
        }
    }
}
