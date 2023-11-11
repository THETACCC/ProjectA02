using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SKCell;
public class CharacterManager : MonoSingleton<CharacterManager>
{
    public bool[] isSliding =  new bool[2];

    /// <summary>
    /// Is either of the two characters sliding?
    /// </summary>
    public bool IsSlidingEither()
    {
        return isSliding[0] || isSliding[1];  
    }
}
