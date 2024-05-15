using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class ShowJournal : MonoBehaviour
{
    public Texture2D ReadingCursor;

    private StarterAssetsInputs _input;
    private PlayerInput _playerInput;
    private bool viewingJournal = false;

    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        if (_input.journal)
        {
            if (viewingJournal) {
                CloseJournal();
            } else {
                OpenJournal();
            }
            _input.journal = false;
        }
        else if (_input.exit && viewingJournal)
        {
            CloseJournal();
            _input.exit = false;
        }
    }

    void OpenJournal() {
        if (!SceneManager.GetSceneByName("Journal Scene").isLoaded) {
            SceneManager.LoadScene("Journal Scene", LoadSceneMode.Additive);
        }

        GS.interactionMode = InteractionType.Journal;
        UI.FadeInMatte();

        _playerInput.actions.FindAction("Move").Disable();
        _playerInput.actions.FindAction("Interact").Disable();
        _input.cursorInputForLook = false;
        _input.look = Vector2.zero;

        UI.UnlockCursor(ReadingCursor);
        viewingJournal = true;
    }

    void CloseJournal() {
        SceneManager.UnloadSceneAsync("Journal Scene");

        GS.interactionMode = InteractionType.Default;
        UI.FadeOutMatte();

        _playerInput.actions.FindAction("Move").Enable();
        _playerInput.actions.FindAction("Interact").Enable();
        _input.cursorInputForLook = true;

        UI.LockCursor();
        viewingJournal = false;

        // YarnDispatcher.StartTutorial("TutorialTest");
    }
}

// if (SceneManager.GetSceneByName("Journal Scene").isLoaded) {
//     SceneManager.SetActiveScene(SceneManager.GetSceneByName("Journal Scene"));
// }