using System;
using StarterAssets;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [NonSerialized]
    public static PauseManager instance;
    public CanvasGroup pauseCanvas;
    public StarterAssetsInputs starterInputs;

    private bool isFadingIn = false;
    private float FadeSpeed = 3.0f;

    void Start()
    {
        pauseCanvas.alpha = 0;
    }
    void Awake()
    {
        if (!instance) instance = this;
    }

    void Update() {
        if (isFadingIn)
        {
            if (pauseCanvas.alpha < 1)
            {
                pauseCanvas.alpha += FadeSpeed * Time.deltaTime;
            }
            else
            {
                pauseCanvas.blocksRaycasts = true;
                pauseCanvas.interactable = true;
            }
        }
        else
        {
            if (pauseCanvas.alpha > 0)
            {
                pauseCanvas.alpha -= FadeSpeed * Time.deltaTime;
            }
            else
            {
                pauseCanvas.blocksRaycasts = false;
                pauseCanvas.interactable = false;
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        if (GS.interactionMode == InteractionType.Paused) return;
        Debug.Log("Pausing game");

        YarnDispatcher.Stop();
        GS.interactionMode = InteractionType.Paused;

        Player.LockPlayer();
    
        AudioManager.instance.MuffleMusic();
        UI.FadeInMatte();
        FadeInPauseScreen();

        UI.UnlockCursor(null);
    }

    public void ResumeGame()
    {
        if (GS.interactionMode != InteractionType.Paused) return;
        Debug.Log("Resuming game");

        starterInputs.interact = false;

        UI.LockCursor();

        UI.FadeOutMatte();
        FadeOutPauseScreen();

        AudioManager.instance.UnmuffleMusic();

        Player.UnlockPlayer();

        GS.interactionMode = InteractionType.Default;
        YarnDispatcher.RetryInterrupted();
    }

    private void FadeInPauseScreen() {
        isFadingIn = true;
    }

    private void FadeOutPauseScreen() {
        isFadingIn = false;
    }
}
