using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using SKCell;
using UnityEditor.SearchService;

public class LevelController : MonoSingleton<LevelController>
{
    public int Level { get; private set; }
    public LevelPhase phase;


    public Block curDraggedblock = null;
    //private void Start()
    //{
    //   InitLevel();
    // }

    //Get level loader
    private LevelLoader levelLoader;


    private void Start()
    {
        GameObject level_loader = GameObject.Find("LevelLoader");
        levelLoader = level_loader.GetComponent<LevelLoader>();
    }

    private void Update()
    {

        //CHEAT CODE
        
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            levelLoader.AlignBlockSelection();
            if (levelLoader.checkindex == 0)
            {
                Debug.Log("pressed- Go");
                phase = LevelPhase.Running;
            }


        }
        
    }
    public void InitLevel()
    {
        phase = LevelPhase.Placing;
        LevelLoader.instance.Load();
        //there is a bug here where the phase will not change to place
    }
}

public enum LevelPhase
{
    Loading,
    Placing,
    Running
}
