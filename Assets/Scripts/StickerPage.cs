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
    public float stickerCenterY;
    public string paragraphContent;
}

public class StickerPage: BookPage
{
    public Image StickerPlaceholder;
    public Sprite StickerSprite;
    private Sprite placeholderSprite;
    private float placeholderHeight;
    private List<GameObject> allPlaceholders = new List<GameObject>();
    private List<Sticker> stickerData;

    void Start()
    {
        placeholderHeight = StickerPlaceholder.GetComponent<RectTransform>().rect.height;
        placeholderSprite = StickerPlaceholder.sprite;
    }

    public void OnMouseDown(Vector3 mousePos)
    {
        int clickedStickerIdx = -1;
        for (int i = 0; i < stickerData.Count; i++)
        {
            Sticker currentSticker = stickerData[i];
            if (Math.Abs(currentSticker.stickerCenterY - mousePos.y) < placeholderHeight / 2f)
            {
                clickedStickerIdx = i;
                break;
            }
        }

        if (clickedStickerIdx > -1)
        {
            Sticker s = stickerData[clickedStickerIdx];
            Debug.Log($"Clicked sticker on {pageSide} page: {s.paragraphContent}");
    
            Image stickerImage = allPlaceholders[clickedStickerIdx].GetComponent<Image>();
            if (stickerImage.sprite != StickerSprite)
            {
                stickerImage.sprite = StickerSprite;
                pageTextMesh.text = $"{pageTextMesh.text.Substring(0, s.startCharIdx)}<color=\"red\">{s.paragraphContent}</color>{pageTextMesh.text.Substring(s.endCharIdx + 1)}";
            }
            else
            {
                stickerImage.sprite = placeholderSprite;
                pageTextMesh.text = pageTextMesh.GetParsedText();
            }
            // TODO: persist sticker placement
            // TODO: clear existing stickers (of same type)
        }
    }

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
            float stickerCenterX = Math.Abs(newPlaceholder.transform.localPosition.x) * (pageSide == PageSide.Left ? -1.0f : 1.0f);

            newPlaceholder.transform.localPosition = new Vector3(
                stickerCenterX,
                currentSticker.stickerCenterY,
                0
            );

            newPlaceholder.name = $"Sticker placeholder - {i + 1}";
            newPlaceholder.SetActive(true);

            allPlaceholders.Add(newPlaceholder);
        }
        stickerData = stickers;
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
            newSticker.stickerCenterY = GetVerticalCenterOfRange(newSticker.startCharIdx, newSticker.endCharIdx) - placeholderHeight / 4f; // TODO: figure out why stickerHeight / 4f is needed?

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