using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SKCell;

public class CommonReference : SKMonoSingleton<CommonReference>
{

    public static PlayerCharacter[] playerCharacters;

    public static string TAG_BLOCK = "Block";
    public static string TAG_CHARACTER = "Character";
    public static string TAG_BLOCK_REF = "BlockReference";
    public static int LAYER_MAP_0 = 7;
    public static int LAYER_MAP_1 = 8;
    public static int LAYER_GROUND = 9;

    public static Camera mainCam;
    private void Start()
    {
        mainCam = Camera.main;
        Debug.Log(mainCam.name);
    }
}
