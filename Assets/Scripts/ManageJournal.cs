using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManageJournal : MonoBehaviour
{
    public TextMeshProUGUI firstPage;

    void Start()
    {
        // firstBookPage.text = GS.journalContent;
    }

    void Update()
    {
        if (firstPage.text != GS.journalContent) {
            firstPage.text = GS.journalContent;
        }
    }
}
