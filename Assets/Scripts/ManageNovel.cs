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

public class ManageNovel : ManageBook
{
    [Header("Novel metadata")]
    public string bookTitle;
    public string bookAuthor;
    public TextMeshProUGUI bookTitleText;

    void Start() {
        if (customPagesAtStart.Count == 0)
        {
            Debug.LogWarning("Novel requires a title page");
        }

        bookTitleText.text =
            "\n\n\n\n\n" +
            "<align=\"center\">\n" +
            $"<b><size=\"48px\">{bookTitle}</size></b>\n\n" +
            $"<i>by\n{bookAuthor}</i>";

        base.Start();
    }
}