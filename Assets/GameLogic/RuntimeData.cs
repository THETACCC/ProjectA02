using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SKCell;

public sealed class RuntimeData : SKMonoSingleton<RuntimeData>
{
    #region Scene
    public static bool isSceneLoading;
    public static SceneTitle activeSceneTitle;

    #endregion
}
