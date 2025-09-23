using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public GameObject bookCover;

    [Header("Text content")]
    public ContentSource contentSource;
    [Multiline]
    public string textField;
    public TextAsset textFile;
    public bool AllowBreakingParagraphs = false;

    [Tooltip("Set to 0 to allow any number of pages")]
    public int maxPages = 0;

    [Header("Custom pages")]
    public List<Texture> customPagesAtStart = new List<Texture>();
    public List<Texture> customPagesAtEnd = new List<Texture>();

    [Header("Cursors")]
    public Texture2D defaultCursor;
    public Texture2D pageRightCursor;
    public Texture2D pageLeftCursor;
    public Texture2D pageDragCursor;

    [Header("Sound effects")]
    public string pageTurnAudio;

    protected bool isVisible = true;
    protected AutoFlip autoFlip;

    protected List<BookPage> bookPages = new List<BookPage>();
    protected List<PageContent> contentByPage = new List<PageContent>();
    protected string currentContent = "";

    private bool isDragging = false;
    private const int MAX_CHARS_PER_PAGE = 800;


    protected virtual void Start()
    {
        InitPages();
        LoadContent();

        autoFlip = bookViewer.gameObject.GetComponent<AutoFlip>();

        if (pageTurnAudio != null)
        {
            bookViewer.OnReleaseEvent.AddListener(() => {
                AudioManager.instance.PlaySFX(pageTurnAudio);
            });
        }

        ResetCursor();
    }

    protected void LoadContent()
    {
        DivideTextIntoPages();
        AddPagesToBook();
        UpdateCurrentPageContent();
    }

    protected virtual void Update()
    {
        // Hide on exiting journal mode
        if (GS.interactionMode != InteractionType.None && GS.interactionMode != InteractionType.Journal && isVisible)
        {
            bookViewer.gameObject.GetComponent<Animator>().SetTrigger("Hide");
            if (bookCover != null)
                bookCover.GetComponent<Animator>().SetTrigger("Hide");

            isVisible = false;
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

    protected void DivideTextIntoPages()
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
        while (startIdx < currentContent.Length && (maxPages == 0 ||contentByPage.Count <= maxPages))
        {
            testPage.pageContent = currentContent.Substring(startIdx, Math.Min(currentContent.Length - startIdx, MAX_CHARS_PER_PAGE));
            string visibleText = testPage.GetVisibleText(AllowBreakingParagraphs);
            endIdx = startIdx + Math.Max(visibleText.Length, 1);

            PageContent newContent = new PageContent();
            newContent.startIdx = startIdx;
            newContent.endIdx = endIdx;
            newContent.content = visibleText.Trim();

            contentByPage.Add(newContent);
            startIdx = endIdx;
        }
    }

    protected void AddPagesToBook()
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

    protected BookPage GetCurrentRightPage()
    {
        int pageIdx = bookViewer.currentPage - customPagesAtStart.Count;
        if (pageIdx < 0) return null;
        return bookPages[pageIdx % bookPages.Count];
    }

    protected BookPage GetCurrentLeftPage()
    {
        int pageIdx = bookViewer.currentPage - customPagesAtStart.Count - 1;
        if (pageIdx < 0) return null;
        return bookPages[pageIdx % bookPages.Count];
    }

    public virtual void HandleRightPageTurn()
    {
        int nextLeftPageIdx = bookViewer.currentPage - customPagesAtStart.Count + 1;
        int nextRightPageIdx = bookViewer.currentPage - customPagesAtStart.Count + 2;
        UpdatePageContent(nextLeftPageIdx);
        UpdatePageContent(nextRightPageIdx);
    }

    public virtual void HandleLeftPageTurn()
    {
        int nextLeftPageIdx = bookViewer.currentPage - customPagesAtStart.Count - 3;
        int nextRightPageIdx = bookViewer.currentPage - customPagesAtStart.Count - 2;
        UpdatePageContent(nextLeftPageIdx);
        UpdatePageContent(nextRightPageIdx);
    }

    Vector2 GetCursorCenter(Texture2D texture2D)
    {
        return new Vector2(texture2D.width / 2f, texture2D.height / 2f);
    }
    public void OnHoverRight()
    {
        if (pageRightCursor != null && !isDragging && bookViewer.currentPage < bookViewer.bookPages.Length)
            Cursor.SetCursor(pageRightCursor, GetCursorCenter(pageRightCursor), CursorMode.Auto);
    }
    public void OnHoverLeft()
    {
        if (pageLeftCursor != null && !isDragging && bookViewer.currentPage > 0)
            Cursor.SetCursor(pageLeftCursor, GetCursorCenter(pageLeftCursor), CursorMode.Auto); 
    }
    public void OnDrag()
    {
        if (pageDragCursor != null && !isDragging)
        {
            Cursor.SetCursor(pageDragCursor, GetCursorCenter(pageDragCursor), CursorMode.Auto);
            isDragging = true;
        }
    }
    public void ResetCursor()
    {
        if (defaultCursor != null && !isDragging && !Input.GetMouseButton(0))
            Cursor.SetCursor(defaultCursor, GetCursorCenter(defaultCursor), CursorMode.Auto);
    }
    public void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            ResetCursor();
        }
    }
    public void HideBook()
    {
        GS.interactionMode = InteractionType.Default;
    }
    public void HideOnLeftTurn()
    {
        if (bookViewer.currentPage == 0) HideBook();
    }
    public void HideOnRightTurn()
    {
        if (bookViewer.currentPage >= bookViewer.bookPages.Length) HideBook();
    }

    // pageIdx excludes the cover, starting at 0 for the first page of text
    protected virtual BookPage UpdatePageContent(int pageIdx)
    {
        if (pageIdx < 0) return null;
        BookPage pageComponent = bookPages[pageIdx % bookPages.Count];

        pageComponent.pageNumber = pageIdx + 1;
        pageComponent.pageSide = ((pageIdx + customPagesAtStart.Count - 1) % 2 == 0) ? PageSide.Left : PageSide.Right;
        pageComponent.pageContent = (pageIdx >= 0 && pageIdx < contentByPage.Count) ? contentByPage[pageIdx].content : "";

        return pageComponent;
    }

    protected void SetCurrentPage(int pageIdx)
    {
        bookViewer.currentPage = pageIdx;
        UpdateCurrentPageContent();
    }

    protected void UpdateCurrentPageContent()
    {
        UpdatePageContent(bookViewer.currentPage - customPagesAtStart.Count - 1);
        UpdatePageContent(bookViewer.currentPage - customPagesAtStart.Count);
    }
}
