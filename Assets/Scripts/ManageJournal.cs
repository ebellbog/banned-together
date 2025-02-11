using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Auto-hides journal and preserves current page
/// </summary>
public class ManageJournal : MonoBehaviour
{
    public TextMeshProUGUI firstPage;
    public BookLive book;

    bool isVisible = true;

    void Start()
    {
        // firstBookPage.text = GS.journalContent;
        book.currentPage = GS.currentJournalPage;
    }

    void Update()
    {
        // if (firstPage.text != GS.journalContent) {
        //     firstPage.text = GS.journalContent;
        // }
        // if (GS.interactionMode != InteractionType.Journal && isVisible)
        // {
        //     book.gameObject.GetComponent<Animator>().SetTrigger("Hide");
        //     isVisible = false;
        // }
    }

    public void SyncGameState()
    {
        GS.currentJournalPage = book.currentPage;
    }
}
