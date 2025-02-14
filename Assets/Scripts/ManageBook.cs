using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
Journal-related classes:
- ShowJournal.cs
- JournalManager.cs
- ManageBook.cs
- BookLive.cs
- BookPage.cs
*/

public struct PageContent {
    public int startIdx, endIdx;
    public string content;
}

public enum ContentSource {
    TextField, File, GameState
}

public class ManageBook : MonoBehaviour
{
    [Header("References")]
    public BookLive bookViewer;
    public GameObject pagePrefab;

    [Header("Images")]
    public Texture bookCover;
    public Texture bookBack;

    [Header("Content")]
    public ContentSource contentSource;
    [Multiline]
    public string textField;
    public TextAsset textFile;

    private bool isVisible = true;
    private List<BookPage> bookPages = new List<BookPage>();
    private List<PageContent> contentByPage = new List<PageContent>();
    private string currentContent = "";

    private const int MAX_CHARS_PER_PAGE = 800;


    void Start()
    {
        InitPages();
        DivideTextIntoPages();
        AddPagesToBook();

        bookViewer.currentPage = GS.currentJournalPage;
        UpdatePageContent(bookViewer.currentPage - 2);
        UpdatePageContent(bookViewer.currentPage - 1);
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
                i % 2 == 0 ? PageSide.Left : PageSide.Right
            );

            bookPages.Add(pageComponent);
        }
    }

    void DivideTextIntoPages()
    {
        contentByPage.Clear();
        BookPage testPage = bookPages[0];

        switch(contentSource)
        {
            case ContentSource.TextField:
                currentContent = textField;
                break;
            case ContentSource.File:
                currentContent = textFile.text;
                break;
            case ContentSource.GameState:
                currentContent = GS.journalContent;
                break;
        }

        int startIdx = 0, endIdx = 0;
        while (startIdx < currentContent.Length)
        {
            testPage.pageContent = currentContent.Substring(startIdx, Math.Min(currentContent.Length - startIdx, MAX_CHARS_PER_PAGE));
            string visibleText = testPage.GetVisibleText();
            endIdx = startIdx + Math.Max(visibleText.Length, 1);

            PageContent newContent = new PageContent();
            newContent.startIdx = startIdx;
            newContent.endIdx = endIdx;
            newContent.content = visibleText.Trim();//currentContent.Substring(startIdx, endIdx - startIdx).Trim();

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
        if (GS.interactionMode != null && GS.interactionMode != InteractionType.Journal && isVisible)
        {
            bookViewer.gameObject.GetComponent<Animator>().SetTrigger("Hide");
            isVisible = false;
        }

        if (contentSource == ContentSource.GameState &&
            GS.journalContent != null &&
            currentContent != GS.journalContent)
        {
            // TODO: maybe optimize by only rebuilding the last page and beyond?
            DivideTextIntoPages();
            AddPagesToBook();
        }
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
