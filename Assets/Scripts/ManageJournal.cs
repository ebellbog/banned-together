using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


public class ManageJournal : ManageBook
{
    [Header("Tutorial")]
    public bool showPageTurnHint;
    public float delayUntilHint = 1.5f;

    private float wiggleDelay = 0;
    private Dictionary<int, List<Sticker>> stickersByPage = new Dictionary<int, List<Sticker>>();

    void Update()
    {
        // Hide journal on exiting journal mode
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
            stickersByPage.Clear();
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

    public void OnMouseDown(Vector3 mousePos)
    {
        PageSide sideClicked = mousePos.x < 0 ? PageSide.Left : PageSide.Right;
        StickerPage clickedPage = mousePos.x < 0 ?
            (StickerPage)GetCurrentLeftPage() :
            (StickerPage)GetCurrentRightPage();
        if (clickedPage) {
            clickedPage.OnMouseDown(mousePos);
        }
    }

    protected override BookPage UpdatePageContent(int pageIdx)
    {
        StickerPage updatedPage = (StickerPage)base.UpdatePageContent(pageIdx);
        if (updatedPage == null) return null;

        if (!stickersByPage.ContainsKey(pageIdx))
            stickersByPage[pageIdx] = updatedPage.GetStickerPlacements();

        updatedPage.PlaceStickers(stickersByPage[pageIdx]);
        return updatedPage;
    }

    override public void HandleRightPageTurn()
    {
        GS.didTutorializeJournal = true;
        base.HandleRightPageTurn();
    } 

    // TODO: remember page for all books, not just journal?
    public void SyncGameState()
    {
        GS.currentJournalPage = bookViewer.currentPage;
    }
}
