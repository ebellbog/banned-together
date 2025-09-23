using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

public class ManageNovel : ManageBook
{
    [Header("Novel metadata")]
    public string bookTitle;
    public string bookAuthor;
    public TextMeshProUGUI bookTitleText;

    private bool isFlippingRight;

    void Start() {
        if (customPagesAtStart.Count == 0 || bookTitleText == null)
        {
            Debug.LogWarning("Novel requires a title page");
            return;
        }

        LoadNovelFromGameState();

        bookTitleText.text =
            "\n\n\n\n\n" +
            "<align=\"center\">\n" +
            $"<b><size=\"38px\">{bookTitle}</size></b>\n\n" +
            $"<i>by\n<size=\"30px\">{bookAuthor}</size></i>";

        SetBookCover(GS.currentNovel.novelCoverImage);

        base.Start();

        SetCurrentPage(GS.currentNovelPage);
    }

    void LoadNovelFromGameState()
    {
        if (GS.currentNovel == null) return;

        bookTitle = GS.currentNovel.novelTitle;
        bookAuthor = GS.currentNovel.novelAuthor;

        contentSource = ContentSource.File;
        textFile = GS.currentNovel.novelFile;

        if (GS.currentNovel.useMetadataFromFile && textFile != null)
        {
            string originalText = textFile.text;
            int nonEmptyLineCount = 0;
            int currentPos = 0;

            // Find the start of the third non-empty line
            while (currentPos < originalText.Length && nonEmptyLineCount < 3)
            {
                // Find the start of the current line
                int lineStart = currentPos;

                // Find the end of the current line
                int lineEnd = currentPos;
                while (lineEnd < originalText.Length && originalText[lineEnd] != '\r' && originalText[lineEnd] != '\n')
                {
                    lineEnd++;
                }

                // Check if this line is non-empty
                string line = originalText.Substring(lineStart, lineEnd - lineStart);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    nonEmptyLineCount++;
                    if (nonEmptyLineCount == 3)
                    {
                        // Found the start of the third non-empty line
                        textFile = new TextAsset(originalText.Substring(lineStart));
                        return;
                    }
                }

                // Move to the next line
                currentPos = lineEnd;
                while (currentPos < originalText.Length && (originalText[currentPos] == '\r' || originalText[currentPos] == '\n'))
                {
                    currentPos++;
                }
            }

            // If we didn't find 3 non-empty lines, set to empty
            textFile = new TextAsset("");
        }
    }

    void SetBookCover(Sprite coverImage)
    {
        Image imageComponent = bookCover.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = coverImage;
        }
    }

    public override void HandleLeftPageTurn()
    {
        base.HandleLeftPageTurn();
        isFlippingRight = false;
    }

    public override void HandleRightPageTurn()
    {
        base.HandleRightPageTurn();
        isFlippingRight = true;
    }

    // Logic needed in addition to HandleLeftPageTurn (etc), because those handlers
    // include cases where the page does not actually flip, but rather tweens back
    // to its starting position
    public void UpdateCurrentPage()
    {
        GS.currentNovelPage = bookViewer.currentPage + (isFlippingRight ? 2 : -2);
    }
}