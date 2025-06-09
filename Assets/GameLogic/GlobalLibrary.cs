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
        {SceneTitle.MainMenu, new SceneSpecifics()
        {


        }},
        {SceneTitle.Main_Menu, new SceneSpecifics()
        {


        }},

        {SceneTitle.Chapter1Base3x3, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1Base4x4, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1BaseVisualGym, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level1, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level2, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level3, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level4, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level5, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level6, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level7, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level8, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level9, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level10, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level11, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level12, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level13, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level14, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1_Level15, new SceneSpecifics()
        {


        }},



        {SceneTitle.Chapter1MoveTest, new SceneSpecifics()
        {


        }},

        /*
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
        {SceneTitle.Level1_10, new SceneSpecifics()
        {


        }},
        */
        {SceneTitle.Chapter0World, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter1World, new SceneSpecifics()
        {


        }},
        {SceneTitle.Level1_1_MapTest, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter0_GYM, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter0_Level1, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter0_Level2, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter0_Level3, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter0_Level4, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter0_Level5, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter0_Level6, new SceneSpecifics()
        {


        }},
        {SceneTitle.Chapter0_Level7, new SceneSpecifics()
        {


        }}

    };


    public static readonly string G_SCENE_LOADING_ASSET_NAME = "Loading";
    public static readonly string G_SCENE_PREPARE_ASSET_NAME = "Prepare";
    //Add level to here to load different levels

    public static readonly Dictionary<SceneCategory, SceneTitle[]> G_SCENE_CATEGORY_DICT = new Dictionary<SceneCategory, SceneTitle[]>
    {
        {SceneCategory.Common, new SceneTitle[]{SceneTitle.MainMenu, SceneTitle.Main_Menu } },
        {SceneCategory.Chapter0, new SceneTitle[]{ SceneTitle.Chapter0World, SceneTitle.Chapter0_GYM, SceneTitle.Chapter0_Level1, SceneTitle.Chapter0_Level2, SceneTitle.Chapter0_Level3, SceneTitle.Chapter0_Level4, SceneTitle.Chapter0_Level5 , SceneTitle.Chapter0_Level6 , SceneTitle.Chapter0_Level7 } },
        {SceneCategory.Chapter1, new SceneTitle[]{ SceneTitle.Chapter1Base3x3, SceneTitle.Chapter1BaseVisualGym, SceneTitle.Chapter1_Level1, SceneTitle.Chapter1_Level2, SceneTitle.Chapter1_Level3, SceneTitle.Chapter1_Level4, SceneTitle.Chapter1_Level5, SceneTitle.Chapter1_Level6, SceneTitle.Chapter1_Level7, SceneTitle.Chapter1_Level8, SceneTitle.Chapter1_Level9, SceneTitle.Chapter1_Level10, SceneTitle.Chapter1_Level11, SceneTitle.Chapter1_Level12, SceneTitle.Chapter1_Level13, SceneTitle.Chapter1_Level14, SceneTitle.Chapter1_Level15, 
            SceneTitle.Chapter1Base4x4, SceneTitle.Level1_1_MapTest,  SceneTitle.Chapter1World , SceneTitle.Chapter1MoveTest } },

    };

    public static readonly Dictionary<SceneTitle, string> G_SCENE_ASSET_NAME = new Dictionary<SceneTitle, string>()
    {
        //Common
        {SceneTitle.MainMenu, "MainMenu" },
        {SceneTitle.Main_Menu, "Main_Menu" },


        //Chapter 0
       {SceneTitle.Chapter0World, "Chapter0World" },
       {SceneTitle.Chapter0_GYM, "Chapter0_GYM" },
       {SceneTitle.Chapter0_Level1, "Chapter0_Level1" },
       {SceneTitle.Chapter0_Level2, "Chapter0_Level2" },
       {SceneTitle.Chapter0_Level3, "Chapter0_Level3" },
       {SceneTitle.Chapter0_Level4, "Chapter0_Level4" },
       {SceneTitle.Chapter0_Level5, "Chapter0_Level5" },
       {SceneTitle.Chapter0_Level6, "Chapter0_Level6" },
       {SceneTitle.Chapter0_Level7, "Chapter0_Level7" },
        //chapter1
        {SceneTitle.Chapter1MoveTest, "Chapter1MoveTest" },
        {SceneTitle.Chapter1Base3x3, "Chapter1Base3x3" },
        {SceneTitle.Chapter1Base4x4, "Chapter1Base4x4" },
        {SceneTitle.Chapter1BaseVisualGym, "Chapter1BaseVisualGym" },
        {SceneTitle.Chapter1_Level1, "Chapter1_Level1" },
        {SceneTitle.Chapter1_Level2, "Chapter1_Level2" },
        {SceneTitle.Chapter1_Level3, "Chapter1_Level3" },
        {SceneTitle.Chapter1_Level4, "Chapter1_Level4" },
        {SceneTitle.Chapter1_Level5, "Chapter1_Level5" },
        {SceneTitle.Chapter1_Level6, "Chapter1_Level6" },
        {SceneTitle.Chapter1_Level7, "Chapter1_Level7" },
        {SceneTitle.Chapter1_Level8, "Chapter1_Level8" },
        {SceneTitle.Chapter1_Level9, "Chapter1_Level9" },
        {SceneTitle.Chapter1_Level10, "Chapter1_Level10" },
        {SceneTitle.Chapter1_Level11, "Chapter1_Level11" },
        {SceneTitle.Chapter1_Level12, "Chapter1_Level12" },
        {SceneTitle.Chapter1_Level13, "Chapter1_Level13" },
        {SceneTitle.Chapter1_Level14, "Chapter1_Level14" },
        {SceneTitle.Chapter1_Level15, "Chapter1_Level15" },
        
        /*
        {SceneTitle.Level1_1, "1-1" },
        {SceneTitle.Level1_2, "1-2" },
        {SceneTitle.Level1_3, "1-3" },
        {SceneTitle.Level1_4, "1-4" },
        {SceneTitle.Level1_5, "1-5" },
        {SceneTitle.Level1_6, "1-6" },
        {SceneTitle.Level1_7, "1-7" },
        {SceneTitle.Level1_8, "1-8" },
        {SceneTitle.Level1_9, "1-9" },
        {SceneTitle.Level1_10, "1-10" },
        */
        {SceneTitle.Level1_1_MapTest, "1_1_MapTest" },

        {SceneTitle.Chapter1World, "Chapter1World" }
    };

    public static readonly Dictionary<int, SceneTitle> G_SCENE_INDEX = new Dictionary<int, SceneTitle>()
    {

    };


    public static readonly float G_SCENE_PARALLEX_INTENSITY = 0.5f;
    #endregion
}
