using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalLibrary
{
    #region SceneControl
    public static readonly string G_SCENE_TAG_SPAWNPOINT = "SpawnPoint";
    public static readonly string G_SCENE_TAG_CHECKPOINT = "CheckPoint";
    public static readonly string G_SCENE_ZOOMIN_PRID = "SceneZoomIn";


    public static readonly Dictionary<SceneTitle, SceneSpecifics> G_SCENE_SPECIFICS = new Dictionary<SceneTitle, SceneSpecifics>()
    {
        {SceneTitle.Level1_1, new SceneSpecifics()
        { 


        }},
        {SceneTitle.Level1_2, new SceneSpecifics()
        {


        }},
        {SceneTitle.Level1_3, new SceneSpecifics()
        {


        }},
        {SceneTitle.Level1_4, new SceneSpecifics()
        {


        }},
        {SceneTitle.Level1_5, new SceneSpecifics()
        {


        }},
        {SceneTitle.Level1_6, new SceneSpecifics()
        {


        }},
        {SceneTitle.Level1_7, new SceneSpecifics()
        {


        }},
        {SceneTitle.Level1_8, new SceneSpecifics()
        {


        }},
        {SceneTitle.Level1_9, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1World, new SceneSpecifics()
        {


        }},
    };


    public static readonly string G_SCENE_LOADING_ASSET_NAME = "Loading";
    public static readonly string G_SCENE_PREPARE_ASSET_NAME = "Prepare";
    //Add level to here to load different levels

    public static readonly Dictionary<SceneCategory, SceneTitle[]> G_SCENE_CATEGORY_DICT = new Dictionary<SceneCategory, SceneTitle[]>
    {
        {SceneCategory.Common, new SceneTitle[]{SceneTitle.MainMenu,SceneTitle.Level1_1} },
        {SceneCategory.Chapter1, new SceneTitle[]{SceneTitle.Level1_1, SceneTitle.Level1_2, SceneTitle.Level1_3, SceneTitle.Level1_4, SceneTitle.Level1_5, SceneTitle.Level1_6, SceneTitle.Level1_7, SceneTitle.Level1_8, SceneTitle.Level1_9, SceneTitle.Chapter1World } },

    };

    public static readonly Dictionary<SceneTitle, string> G_SCENE_ASSET_NAME = new Dictionary<SceneTitle, string>()
    {
        {SceneTitle.Level1_1, "1-1" },
        {SceneTitle.Level1_2, "1-2" },
        {SceneTitle.Level1_3, "1-3" },
        {SceneTitle.Level1_4, "1-4" },
        {SceneTitle.Level1_5, "1-5" },
        {SceneTitle.Level1_6, "1-6" },
        {SceneTitle.Level1_7, "1-7" },
        {SceneTitle.Level1_8, "1-8" },
        {SceneTitle.Level1_9, "1-9" },
        {SceneTitle.Chapter1World, "Chapter1World" }
    };

    public static readonly Dictionary<int, SceneTitle> G_SCENE_INDEX = new Dictionary<int, SceneTitle>()
    {

    };


    public static readonly float G_SCENE_PARALLEX_INTENSITY = 0.5f;
    #endregion
}
