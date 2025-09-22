using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class NovelData
{
    public string novelTitle;
    public string novelAuthor;
    public TextAsset novelFile;
    public bool useMetadataFromFile = true;
}

public class LibraryManager : MonoBehaviour
{
    public static LibraryManager Main;

    public string BookSceneName;
    public List<NovelData> novels = new List<NovelData>();
    public Color hoverOutlineColor;
    public Sprite hoverCursor;

    private bool viewingBook = false;
    private bool isReady = true;

    // private AsyncOperation bookScene;
    private Dictionary<string, NovelData> novelsByTitle = new Dictionary<string, NovelData>();
    private ReadableBook currentBook;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (!Main) Main = this;

        foreach(NovelData novel in novels)
        {
            novelsByTitle[novel.novelTitle] = novel;

            if (novel.useMetadataFromFile && novel.novelFile != null)
            {
                string[] lines = novel.novelFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1) {
                    novel.novelTitle = lines[0];
                    novel.novelAuthor = lines[1];
                }
            }
        }
    }

    void Update()
    {
        // if (bookScene == null)
        // {
        //     bookScene = SceneManager.LoadSceneAsync(BookSceneName, LoadSceneMode.Additive);
        //     bookScene.allowSceneActivation = false;
        // }

        StarterAssetsInputs input = FirstPersonController.Main.StarterInputs;
        if (viewingBook && (input.anyKey || GS.interactionMode != InteractionType.Journal))
        {
            input.anyKey = false;
            input.exit = false;
            CloseBook();
        }
    }

    public void OpenBook(ReadableBook book) {
        if (viewingBook || !isReady) return;
        currentBook = book;

        NovelData novel;
        if (novelsByTitle.TryGetValue(book.bookTitle, out novel))
        {
            GS.currentNovel = novel;
            GS.currentNovelPage = book.currentPage;
        } else
        {
            Debug.Log($"Couldn't find book with title: {book.bookTitle}");
            return;
        }

        GS.interactionMode = InteractionType.Journal;
        UI.FadeInMatte();

        if (!SceneManager.GetSceneByName(BookSceneName).isLoaded) {
            SceneManager.LoadSceneAsync(BookSceneName, LoadSceneMode.Additive);
        }
        // bookScene.allowSceneActivation = true;

        FirstPersonController.Main.LockPlayer();
        UI.UnlockCursor();

        viewingBook = true;

        AudioManager.instance.MuffleMusic();
    }

    public void CloseBook() {
        if (!viewingBook) return;

        viewingBook = false;
        isReady = false;

        if (currentBook != null)
        {
            currentBook.currentPage = GS.currentNovelPage;
            currentBook = null;
        }

        GS.interactionMode = InteractionType.Default;

        StartCoroutine(_CloseBook());
    }

    IEnumerator _CloseBook() {
        UI.FadeOutMatte();
        AudioManager.instance.UnmuffleMusic();

        yield return new WaitForSeconds(1); // Allow time for animating out
        SceneManager.UnloadSceneAsync(BookSceneName);

        if (GS.interactionMode == InteractionType.Paused) yield return null;

        FirstPersonController.Main.UnlockPlayer();
        UI.LockCursor();

        // bookScene = null;
        isReady = true;
    
        yield return null;
    }
}
