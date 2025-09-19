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
}

public class LibraryManager : MonoBehaviour
{
    public static LibraryManager Main;

    public string BookSceneName;
    public List<NovelData> novels = new List<NovelData>();


    private bool viewingBook = false;
    private bool isReady = true;

    private AsyncOperation bookScene;
    private Dictionary<string, NovelData> novelsByTitle = new Dictionary<string, NovelData>();

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
        }
    }

    void Update()
    {
        if (bookScene == null)
        {
            bookScene = SceneManager.LoadSceneAsync(BookSceneName, LoadSceneMode.Additive);
            bookScene.allowSceneActivation = false;
        }

        StarterAssetsInputs input = FirstPersonController.Main.StarterInputs;
        if (input.anyKey && viewingBook)
        {
            input.anyKey = false;
            input.exit = false;
            CloseBook();
        }
    }

    public void OpenBook(string novelTitle) {
        if (viewingBook || !isReady) return;

        NovelData novel;
        if (novelsByTitle.TryGetValue(novelTitle, out novel))
        {
            GS.currentNovel = novel;
            Debug.Log($"Set current novel: {novel.novelTitle}, by {novel.novelAuthor}");
        } else
        {
            Debug.Log($"Could not find novel with title: {novelTitle}");
            return;
        }

        GS.interactionMode = InteractionType.Journal;
        UI.FadeInMatte();

        bookScene.allowSceneActivation = true;

        FirstPersonController.Main.LockPlayer();
        UI.UnlockCursor();

        viewingBook = true;

        AudioManager.instance.MuffleMusic();
    }

    public void CloseBook() {
        if (!viewingBook) return;

        viewingBook = false;
        isReady = false;

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

        bookScene = null;
        isReady = true;
    
        yield return null;
    }
}
