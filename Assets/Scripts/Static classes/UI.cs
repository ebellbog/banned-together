using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UI
{
    public static UIManager uiManager;
    public static Canvas cursorCanvas;

    public static void FadeInMatte() {
        uiManager.FadeInMatte();
    }

    public static void FadeOutMatte()
    {
        uiManager.FadeOutMatte();
    }

    public static void ShowCursor()
    {
        cursorCanvas.enabled = true;
    }

    public static void HideCursor()
    {
        cursorCanvas.enabled = false;
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        ShowCursor();
    }

    public static void UnlockCursor(Texture2D mouseTexture)
    {
        HideCursor();
        if (mouseTexture) {
            Cursor.SetCursor(mouseTexture, new Vector2(32, 32), CursorMode.ForceSoftware);
        }
        Cursor.lockState = CursorLockMode.None;
    }
}