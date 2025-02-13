using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct PageContent {
    public int startIdx, endIdx;
    public string content;
}

public class ManageBook : MonoBehaviour
{
    [Header("References")]
    public BookLive bookViewer;
    public GameObject pagePrefab;

    [Header("Images")]
    public Texture bookCover;
    public Texture bookBack;
    public Sprite pageBackground;

    [Header("Content")]
    [Multiline]
    public string bookContent;
    public TextAsset textAsset;

    private bool isVisible = true;
    private List<BookPage> bookPages = new List<BookPage>();
    private List<PageContent> contentByPage = new List<PageContent>();

    private const int MAX_CHARS_PER_PAGE = 800;


    void Start()
    {
        InitPages();
        DivideTextIntoPages();
        AddPagesToBook();

        bookViewer.currentPage = GS.currentJournalPage;
    }

    void InitPages()
    {
        bookPages.Clear();
        for (int i = 0; i < 4; i++)
        {
            GameObject newPage = Instantiate(pagePrefab);
            newPage.transform.SetParent(transform, false);

            BookPage pageComponent = newPage.GetComponent<BookPage>();
            pageComponent.InitPage(
                i + 1,
                i % 2 == 0 ? PageSide.Left : PageSide.Right,
                pageBackground
            );

            bookPages.Add(pageComponent);
        }
    }

    void DivideTextIntoPages()
    {
        contentByPage.Clear();
        BookPage testPage = bookPages[0];

        int startIdx = 0, endIdx = 0;
        string fullText = textAsset ? textAsset.text : bookContent;

        while (startIdx < fullText.Length)
        {
            testPage.pageContent = fullText.Substring(startIdx, Math.Min(fullText.Length - startIdx, MAX_CHARS_PER_PAGE));
            endIdx = startIdx + testPage.GetVisibleText().Length;

            PageContent newContent = new PageContent();
            newContent.startIdx = startIdx;
            newContent.endIdx = endIdx;
            newContent.content = fullText.Substring(startIdx, endIdx - startIdx).Trim();

            contentByPage.Add(newContent);
            startIdx = endIdx;
        }
    }

    void AddPagesToBook()
    {
        List<Texture> bookTextures = new List<Texture>();
        bookTextures.Add(bookCover);

        int pageCount = Math.Max(contentByPage.Count, 2);
        pageCount += pageCount % 2; // Back cover won't display right if page count isn't even

        for (int i = 0; i < pageCount; i++)
        {
            bookTextures.Add(bookPages[i % bookPages.Count].renderTexture);
        }
        bookTextures.Add(bookBack);
        bookViewer.bookPages = bookTextures.ToArray();
    }

    void Update()
    {
        // if (GS.interactionMode != InteractionType.Journal && isVisible)
        // {
        //     book.gameObject.GetComponent<Animator>().SetTrigger("Hide");
        //     isVisible = false;
        // }
    }

    public void SyncGameState()
    {
        GS.currentJournalPage = bookViewer.currentPage;
    }

    public void HandleRightPageTurn()
    {
        int nextLeftPageIdx = bookViewer.currentPage;
        int nextRightPageIdx = bookViewer.currentPage + 1;
        UpdatePageContent(nextLeftPageIdx);
        UpdatePageContent(nextRightPageIdx);
    }

    public void HandleLeftPageTurn()
    {
        int nextLeftPageIdx = bookViewer.currentPage - 4;
        int nextRightPageIdx = bookViewer.currentPage - 3;
        UpdatePageContent(nextLeftPageIdx);
        UpdatePageContent(nextRightPageIdx);
    }

    // pageIdx excludes the cover, starting at 0 for the first page of text
    void UpdatePageContent(int pageIdx)
    {
        if (pageIdx < 0) return;
        BookPage pageComponent = bookPages[pageIdx % bookPages.Count];

        pageComponent.pageNumber = pageIdx + 1;
        pageComponent.pageSide = (pageIdx % 2 == 0) ? PageSide.Left : PageSide.Right;
        pageComponent.pageContent = (pageIdx >= 0 && pageIdx < contentByPage.Count) ? contentByPage[pageIdx].content : "";
    }
}
