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

    void Start()
    {
        bookViewer.currentPage = GS.currentJournalPage;
        base.Start();
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

        base.Update();
    }

    void SetupStickers()
    {
        foreach(Color color in stickerColors)
        {
            GameObject newSticker = Instantiate(stickerPrefab);
            newSticker.GetComponent<DraggableElement>().OnReleaseDrag.AddListener(ReleasedSticker);
            newSticker.transform.Find("Sticker center").GetComponent<Image>().color = color;
            newSticker.transform.SetParent(stickerParent.transform, false);
        }
    }

    public void ReleasedSticker(GameObject stickerObject)
    {
        Color stickerColor = stickerObject.transform.Find("Sticker center").GetComponent<Image>().color;
        Debug.Log($"Released sticker of color: {stickerColor}");

        StickerPage stickerPage = lastMousePos.x < 0 ?
            (StickerPage)GetCurrentLeftPage() :
            (StickerPage)GetCurrentRightPage();
        if (stickerPage) {
            if (stickerPage.TryPlacingStickerAtCoords(lastMousePos, stickerColor))
            {
                stickerObject.SetActive(false);
            }
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
