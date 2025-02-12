using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Auto-hides journal and preserves current page
/// </summary>
public class ManageBook : MonoBehaviour
{
    [Header("References")]
    public BookLive bookViewer;
    public GameObject pagePrefab;
    public TextMeshProUGUI firstPage;

    [Header("Content")]
    [Multiline(10)]
    public string bookContent;

    bool isVisible = true;

    void Start()
    {
        firstBookPage.text = GS.journalContent;
        // firstPage.text = bookContent;
        bookViewer.currentPage = GS.currentJournalPage;
    }

    void OnValidate()
    {
        firstPage.text = bookContent;
    }

    void Update()
    {
        if (firstPage.text != GS.journalContent) {
            firstPage.text = GS.journalContent;
        }
        if (GS.interactionMode != InteractionType.Journal && isVisible)
        {
            book.gameObject.GetComponent<Animator>().SetTrigger("Hide");
            isVisible = false;
        }
    }

    public void SyncGameState()
    {
        GS.currentJournalPage = bookViewer.currentPage;
    }
}
