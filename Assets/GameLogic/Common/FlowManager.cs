using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.IO;
using System;
using static Unity.VisualScripting.Icons;

public class FlowManager : MonoSingleton<FlowManager>
{
    public static SceneTitle scenetitle;
    public static int t_spawnPoint;
    private void Start()
    {

        scenetitle = (SceneTitle)PlayerPrefs.GetInt("StartScene");

        CommonUtils.InvokeAction(0.2f, () =>
        {


            LoadScene(new SceneInfo()
            {
                index = scenetitle,
            });
        });
    }
    public void LoadScene(SceneInfo info)
    {
        Scenecontroller.instance.LoadSceneAsset(info);
    }

}
