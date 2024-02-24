using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.IO;
using System;
using static Unity.VisualScripting.Icons;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

public class FlowManager : SKMonoSingleton<FlowManager>
{
    public static SceneTitle scenetitle;
    public static int t_spawnPoint;
    private void Start()
    {

        scenetitle = (SceneTitle)PlayerPrefs.GetInt("StartScene");

        SKUtils.InvokeAction(0.2f, () =>
        {


            LoadScene(new SceneInfo()
            {
                index = scenetitle,
            });
        });

        InitializeConsoleCommands();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SKConsole.Toggle();
        }
    }
    public void LoadScene(SceneInfo info)
    {
        Scenecontroller.instance.LoadSceneAsset(info);
    }

    public void InitializeConsoleCommands()
    {
        SKConsole.AddCommand("loadscene", "Load a scene.", (x) =>
        {
            SceneInfo info = new SceneInfo()
            {
                index = (SceneTitle)(x)
            };
            Scenecontroller.instance.LoadSceneAsset(info);
        });
        SKConsole.AddCommand("reload", "Reload a scene.", () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
       SKConsoleCommand cmd =  SKConsole.AddCommand("set", "Set value");
        cmd.AddCommand("ply", "Set player value...", () =>
        {

        });
        cmd.AddCommand("obj", "Set obj value...", () =>
        {

        });
    }

}
