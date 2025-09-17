using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public List<NovelData> novels = new List<NovelData>();

    void Awake()
    {
        if (!Main) Main = this;
    }

    void Start()
    {
        LoadBookFromLibrary();
    }

    public void LoadBookFromLibrary()
    {
        Debug.Log("Loading novel with index: " + GS.currentNovelIdx);
        // NovelData novel = novels[bookIdx];
        // GS.currentNovel = novel;
        // SceneManager.LoadScene("Novel Scene");
    }
}
