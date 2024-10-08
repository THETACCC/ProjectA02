using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using SKCell;



public class Scenecontroller : SKMonoSingleton<Scenecontroller>
{
    public SceneInfo sceneInfo;


    // Start is called before the first frame update
    void Start()
    {
        SKSceneManager.instance.onNextSceneLoaded.AddListener(() =>
        {
            SKUtils.InvokeAction(0.1f, () =>
            {
                LoadSceneSetup();
            });
        });
    }


    public void LoadSceneSetup()
    {

        RuntimeData.activeSceneTitle = sceneInfo.index;
        RuntimeData.isSceneLoading = false;

    }
    public bool LoadSceneAsset(SceneInfo info)
    {
        sceneInfo = info;
        if (SKSceneManager.instance.LoadSceneAsync(GlobalLibrary.G_SCENE_LOADING_ASSET_NAME, GlobalLibrary.G_SCENE_ASSET_NAME[info.index]))
        {
            RuntimeData.isSceneLoading = true;
            EventDispatcher.Dispatch(EventDispatcher.Common, EventRef.CM_ON_SCENE_EXIT);
            return true;
        }
        return false;
    }
}


[System.Serializable]

public struct SceneInfo
{
    public SceneTitle index;
    public SceneTeleportType teleportType;
    public Vector3 position;
}

public enum SceneTeleportType
{
    SpawnPoint,
    CheckPoint,
    CustomPosition
}
public enum SceneTitle
{
    
    MainMenu = 0,
    Level1_1 = 1,
    Level1_2 = 2,
    Level1_3 = 3, 
    Level1_4 = 4, 
    Level1_5 = 5,
    Level1_6 = 6,
    Level1_7 = 7,
    Level1_8 = 8,
    Level1_9 = 9,
    Level1_10 = 10,
    Chapter1World = 11,
    Level1_1_MapTest = 12,
    Chapter1Base3x3 = 14,
    Chapter1Base4x4 = 15,
    Chapter1BaseVisualGym = 16,
    Chapter1_Level1 = 17,
    Chapter1_Level2 = 18,
    //Common Scenes
    Main_Menu = 13,


}

public enum SceneCategory
{
    Common = 0,
    Chapter1 = 100

}
