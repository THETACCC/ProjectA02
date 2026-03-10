using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class CursorManager : MonoBehaviour
{
    public bool hideCursorInGame = false;
    public bool lockCursorInGame = false;
    public float interval = 3f;




    private void Start()
    {
        InvokeRepeating(nameof(EnableCursor), 0f, interval);
        ApplyCursorState();
    }

    void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
            Cursor.visible = true;
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

}