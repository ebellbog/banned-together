using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct Sticker {
    public int startCharIdx;
    public int endCharIdx;
    public string paragraphContent;
}

public class StickerPage: BookPage
{
    public Image StickerPlaceholder;
    private List<GameObject> allPlaceholders = new List<GameObject>();

    public void LoadStickers(List<Sticker> stickers)
    {
        if (allPlaceholders.Count > 0)
        {
            foreach (GameObject placeholder in allPlaceholders) Destroy(placeholder);
            allPlaceholders.Clear();
        }

        for(int i = 0 ; i < stickers.Count; i++)
        {
            GameObject newPlaceholder = Instantiate(StickerPlaceholder.gameObject);
            newPlaceholder.transform.SetParent(StickerPlaceholder.transform.parent, false);

            Sticker currentSticker = stickers[i];
            float stickerY = GetVerticalCenterOfRange(currentSticker.startCharIdx, currentSticker.endCharIdx);
            float stickerX = Math.Abs(newPlaceholder.transform.localPosition.x);
            float stickerHeight = StickerPlaceholder.GetComponent<RectTransform>().rect.height;

            newPlaceholder.transform.localPosition = new Vector3(
                stickerX * (pageSide == PageSide.Left ? -1.0f : 1.0f),
                stickerY - stickerHeight / 4f, // TODO: figure out why stickerHeight / 4f is needed?
                0
            );

            newPlaceholder.name = $"Sticker placeholder - {i + 1}";
            newPlaceholder.SetActive(true);

            allPlaceholders.Add(newPlaceholder);
        }
    }

    public List<Sticker> GetStickerPlacements()
    {
        List<Sticker> stickers = new List<Sticker>();
        if (string.IsNullOrWhiteSpace(pageContent)) return stickers;

        int index = 0;
        while (index < pageContent.Length)
        { 
            // Find start of paragraph (skip leading newlines)
            while (index < pageContent.Length && pageContent[index] == '\n')
                index++;

            if (index >= pageContent.Length) break;

            Sticker newSticker = new Sticker();
            newSticker.startCharIdx = index;
            newSticker.paragraphContent = "";

            // Find end of paragraph
            while (index < pageContent.Length && !(pageContent[index] == '\n' && index + 1 < pageContent.Length && pageContent[index + 1] == '\n'))
            {
                newSticker.paragraphContent += pageContent[index];
                index++;
            }

            // Skip date headers
            DateTime dt;
            if (DateTime.TryParse(newSticker.paragraphContent, out dt))
                continue;

            newSticker.endCharIdx = index - 1;
            stickers.Add(newSticker);

            // Skip over the paragraph delimiter
            while (index < pageContent.Length && pageContent[index] == '\n')
                index++;
        }

        return stickers;
    }

    public float GetLineForCharacter(int charIdx)
    {
        pageTextMesh.ForceMeshUpdate();
        TMP_CharacterInfo[] charInfo = pageTextMesh.textInfo.characterInfo;
        return charInfo[charIdx].lineNumber;
    }

    public float GetLineHeight(int lineIdx)
    {
        pageTextMesh.ForceMeshUpdate();
        TMP_LineInfo[] lineInfo = pageTextMesh.textInfo.lineInfo;
        return lineInfo[lineIdx].lineHeight;
    }

    public float GetVerticalCenterOfRange(int startIdx, int endIdx)
    {
        pageTextMesh.ForceMeshUpdate();
        TMP_CharacterInfo[] charInfo = pageTextMesh.textInfo.characterInfo;
        float top = charInfo[startIdx].topLeft.y;
        float bottom = charInfo[endIdx].bottomRight.y;
        return (top + bottom) / 2f;
    }
}