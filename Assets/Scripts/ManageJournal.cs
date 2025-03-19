using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Stickers")]
    public GameObject stickerParent;
    public GameObject stickerPrefab;
    public List<Color> stickerColors;

    private float wiggleDelay = 0;
    private Dictionary<int, List<Sticker>> stickersByPage = new Dictionary<int, List<Sticker>>();
    private Vector3 lastMousePos;
    private GameObject lastPlacedSticker; // TODO: support multiple stickers

    void Start()
    {
        bookViewer.currentPage = GS.currentJournalPage;
        base.Start();

        if (GS.stickersEnabled == 0)
        {
            bookViewer.transform.parent.localPosition = new Vector3(0, -bookViewer.transform.localPosition.y, 0);
            stickerParent.SetActive(false);
        }
        SetupStickers();
    }

    void Update()
    {
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


        foreach(DraggableElement draggable in stickerParent.GetComponentsInChildren<DraggableElement>())
        {
            // TODO: support multiple stickers by checking for corresponding sticker placement
            draggable.gameObject.SetActive(GS.redStickerPlacement.associatedJournalEntry == null);
        }

        base.Update();
    }

    void SetupStickers()
    {
        foreach(Color color in stickerColors)
        {
            GameObject newSticker = Instantiate(stickerPrefab);

            DraggableElement draggable = newSticker.GetComponent<DraggableElement>();
            draggable.OnReleaseDrag.AddListener(ReleaseSticker);

            newSticker.transform.Find("Sticker center").GetComponent<Image>().color = color;
            newSticker.transform.SetParent(stickerParent.transform, false);
            newSticker.transform.localPosition = Vector3.zero; // TODO: use layout group in the future
        }
    }

    public void ReleaseSticker(GameObject stickerObject)
    {
        Color stickerColor = stickerObject.transform.Find("Sticker center").GetComponent<Image>().color;

        StickerPage stickerPage = lastMousePos.x < 0 ?
            (StickerPage)GetCurrentLeftPage() :
            (StickerPage)GetCurrentRightPage();
        if (stickerPage && stickerPage.TryPlacingStickerAtCoords(lastMousePos, stickerColor))
        {
            stickerObject.SetActive(false);
            lastPlacedSticker = stickerObject;
        }
        else
        {
            stickerObject.GetComponent<DraggableElement>().SnapToStart();
        }
        bookViewer.interactable = true;
    }

    public void OnMouseDown(Vector3 mousePos)
    {
        StickerPage stickerPage = mousePos.x < 0 ?
            (StickerPage)GetCurrentLeftPage() :
            (StickerPage)GetCurrentRightPage();
        if (stickerPage && stickerPage.TryRemovingStickerAtCoords(mousePos))
        {
            bookViewer.interactable = false;

            // TODO: support multiple stickers by getting the corresponding sticker type
            if (lastPlacedSticker == null)
                lastPlacedSticker = stickerParent.transform.GetChild(0).gameObject;

            lastPlacedSticker.SetActive(true);
            lastPlacedSticker.GetComponent<DraggableElement>().StartDragging(true);
        }
    }

    public void OnHover(Vector3 mousePos)
    {
        lastMousePos = mousePos;
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
