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

    public static void FadeInInteractionUI()
    {
        uiManager.FadeInInteractionUI();
    }
    public static void FadeOutInteractionUI()
    {
        uiManager.FadeOutInteractionUI();
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
            UI.SetCursor(mouseTexture);
        }
        Cursor.lockState = CursorLockMode.None;
    }

    public static void SetCursor(Texture2D mouseTexture)
    {
        Cursor.SetCursor(mouseTexture, new Vector2(32, 32), CursorMode.ForceSoftware);
    }
}