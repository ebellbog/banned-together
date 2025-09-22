using System.Collections;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/*
Journal-related classes:
- ShowJournal.cs
- JournalManager.cs
- ManageBook.cs
- BookLive.cs
- BookPage.cs
*/

public class ShowJournal : MonoBehaviour
{
    public string JournalSceneName = "Journal Scene";

    private StarterAssetsInputs _input;
    private PlayerInput _playerInput;
    private bool viewingJournal = false;
    private bool isReady = true;

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
            } else if (GS.journalEnabled > 0 && GS.interactionMode == InteractionType.Default && isReady) {
                OpenJournal();
            }
            _input.journal = false;
            _input.anyKey = false;
        }
        else if (_input.anyKey && viewingJournal)
        {
            CloseJournal();
            _input.anyKey = false;
            _input.exit = false;
        }

        if (GS.interactionMode != InteractionType.Journal && viewingJournal)
            CloseJournal();
    }

    void OpenJournal() {
        GS.interactionMode = InteractionType.Journal;
        UI.FadeInMatte();

        if (!SceneManager.GetSceneByName(JournalSceneName).isLoaded) {
            SceneManager.LoadScene(JournalSceneName, LoadSceneMode.Additive);
        }

        FirstPersonController.Main.LockPlayer();
        UI.UnlockCursor();

        viewingJournal = true;

        JournalManager.Main.MarkAllEntriesRead(); // TODO: mark by page viewed
        AudioManager.instance.MuffleMusic();
    }

    public void CloseJournal() {
        if (viewingJournal)
        {
            viewingJournal = false;
            isReady = false;

            GS.interactionMode = InteractionType.Default;

            StartCoroutine(_CloseJournal());
        }
    }

    IEnumerator _CloseJournal() {
        UI.FadeOutMatte();
        AudioManager.instance.UnmuffleMusic();

        yield return new WaitForSeconds(1); // Allow time for animating out
        SceneManager.UnloadSceneAsync(JournalSceneName);

        if (GS.interactionMode == InteractionType.Paused) yield return null;

        FirstPersonController.Main.UnlockPlayer();
        UI.LockCursor();

        isReady = true;
        yield return null;
    }
}

// if (SceneManager.GetSceneByName("Journal Scene").isLoaded) {
//     SceneManager.SetActiveScene(SceneManager.GetSceneByName("Journal Scene"));
// }