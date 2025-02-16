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

    [Header("Text content")]
    public ContentSource contentSource;
    [Multiline]
    public string textField;
    public TextAsset textFile;

    [Header("Custom pages")]
    public List<Texture> customPagesAtStart = new List<Texture>();
    public List<Texture> customPagesAtEnd = new List<Texture>();

    [Header("Sound effects")]
    public string pageTurnAudio;

    [Header("Tutorial")]
    public bool showPageTurnHint;
    public float delayUntilHint = 1.5f;
    private float wiggleDelay = 0;

    private bool isVisible = true;
    private AutoFlip autoFlip;
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
        UpdatePageContent(bookViewer.currentPage - customPagesAtStart.Count - 1);
        UpdatePageContent(bookViewer.currentPage - customPagesAtStart.Count);

        autoFlip = bookViewer.gameObject.GetComponent<AutoFlip>();

        if (pageTurnAudio != null)
        {
            bookViewer.OnReleaseEvent.AddListener(() => {
                AudioManager.instance.PlaySFX(pageTurnAudio);
            });
        }
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
        bookTextures.AddRange(customPagesAtStart);

        int customPageCount = customPagesAtStart.Count + customPagesAtEnd.Count;
        int pageCount = Math.Max(contentByPage.Count, 4 - customPageCount); // Ensure a minimum of four pages
        if ((pageCount + customPageCount) % 2 == 1) pageCount++; // Back cover won't display right if page count isn't even

        for (int i = 0; i < pageCount; i++)
        {
            bookTextures.Add(bookPages[i % bookPages.Count].renderTexture);
        }

        bookTextures.AddRange(customPagesAtEnd);
        bookViewer.bookPages = bookTextures.ToArray();
    }

    void Update()
    {
        if (GS.interactionMode != InteractionType.None && GS.interactionMode != InteractionType.Journal && isVisible)
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

        if (showPageTurnHint && !GS.didTutorializeJournal)
        {
            if (wiggleDelay < delayUntilHint) wiggleDelay += Time.deltaTime;
            else {
                autoFlip.StartWiggling();
                GS.didTutorializeJournal = true;
            }
        } 
    }

    public void SyncGameState()
    {
        GS.currentJournalPage = bookViewer.currentPage;
    }

    public void HandleRightPageTurn()
    {
        showPageTurnHint = false;
        int nextLeftPageIdx = bookViewer.currentPage - customPagesAtStart.Count + 1;
        int nextRightPageIdx = bookViewer.currentPage - customPagesAtStart.Count + 2;
        UpdatePageContent(nextLeftPageIdx);
        UpdatePageContent(nextRightPageIdx);
    }

    public void HandleLeftPageTurn()
    {
        int nextLeftPageIdx = bookViewer.currentPage - customPagesAtStart.Count - 3;
        int nextRightPageIdx = bookViewer.currentPage - customPagesAtStart.Count - 2;
        UpdatePageContent(nextLeftPageIdx);
        UpdatePageContent(nextRightPageIdx);
    }

    // pageIdx excludes the cover, starting at 0 for the first page of text
    void UpdatePageContent(int pageIdx)
    {
        if (pageIdx < 0) return;
        BookPage pageComponent = bookPages[pageIdx % bookPages.Count];

        pageComponent.pageNumber = pageIdx + 1;
        pageComponent.pageSide = ((pageIdx + customPagesAtStart.Count - 1) % 2 == 0) ? PageSide.Left : PageSide.Right;
        pageComponent.pageContent = (pageIdx >= 0 && pageIdx < contentByPage.Count) ? contentByPage[pageIdx].content : "";
    }
}
