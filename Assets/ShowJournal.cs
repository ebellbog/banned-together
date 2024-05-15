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
    public Canvas BackgroundMatte;
    public Image CursorImage;
    public Texture2D ReadingCursor;
    public ThoughtSensor thoughtSensor;

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

        thoughtSensor.ShowThoughts = false;
        BackgroundMatte.enabled = true;

        _playerInput.actions.FindAction("Move").Disable();
        _playerInput.actions.FindAction("Interact").Disable();
        _input.cursorInputForLook = false;
        _input.look = Vector2.zero;

        Cursor.SetCursor(ReadingCursor, new Vector2(32, 32), CursorMode.ForceSoftware);
        Cursor.lockState = CursorLockMode.None;
        CursorImage.enabled = false;

        viewingJournal = true;
    }

    void CloseJournal() {
        SceneManager.UnloadSceneAsync("Journal Scene");

        thoughtSensor.ShowThoughts = true;
        BackgroundMatte.enabled = false;

        _playerInput.actions.FindAction("Move").Enable();
        _playerInput.actions.FindAction("Interact").Enable();
        _input.cursorInputForLook = true;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        Cursor.lockState = CursorLockMode.Locked;
        CursorImage.enabled = true;

        viewingJournal = false;
    }
}

// if (SceneManager.GetSceneByName("Journal Scene").isLoaded) {
//     SceneManager.SetActiveScene(SceneManager.GetSceneByName("Journal Scene"));
// }