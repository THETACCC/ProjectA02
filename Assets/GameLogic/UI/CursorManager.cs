using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public bool hideCursorInGame = false;
    public bool lockCursorInGame = false;

    private void Start()
    {
        ApplyCursorState();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ApplyCursorState();
        }
    }

    private void ApplyCursorState()
    {
        Cursor.visible = !hideCursorInGame;

        if (lockCursorInGame)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }
}