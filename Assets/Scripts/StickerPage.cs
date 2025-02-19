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
    public List<string> filterWords;

    public static bool operator ==(Sticker first, Sticker second)
    {
        return first.paragraphContent == second.paragraphContent;
    }
    public static bool operator !=(Sticker first, Sticker second)
    {
        return !(first == second);
    }
}

public class StickerPage: BookPage
{
    public Image StickerPlaceholder;
    public Sprite StickerSprite;
    public Sprite PlaceholderSprite;
    private float placeholderHeight;
    private List<GameObject> allPlaceholders = new List<GameObject>();
    private List<Sticker> stickerData;
    private Sticker prevRedStickerPlacement;
    private string originalText;

    override public string pageContent {
        get {
            return originalText;
        }
        set {
            base.pageContent = value;
            originalText = value;
        }
    }

    void Start()
    {
        placeholderHeight = StickerPlaceholder.GetComponent<RectTransform>().rect.height;
        prevRedStickerPlacement = GS.redStickerPlacement;
    }

    void Update()
    {
        if (prevRedStickerPlacement != GS.redStickerPlacement && stickerData != null)
        {
            for (int i = 0; i < stickerData.Count; i++) UpdateSticker(i);
            UpdateTextHighlights();
            prevRedStickerPlacement = GS.redStickerPlacement;
        }
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
            ToggleSticker(clickedStickerIdx);
        }
    }

    // TODO: support other sticker types
    bool StickerIsActive(int stickerIdx)
    {
        return GS.redStickerPlacement == stickerData[stickerIdx];
    }

    void ToggleSticker(int stickerIdx)
    {
        if (StickerIsActive(stickerIdx)) GS.redStickerPlacement = new Sticker();
        else
        {
            GS.redStickerPlacement = stickerData[stickerIdx];
            Debug.Log("Current filter words: "+String.Join("-", GS.redStickerPlacement.filterWords.ToArray()));
        }
        UpdateSticker(stickerIdx);
        UpdateTextHighlights();
    }

    void UpdateSticker(int stickerIdx)
    {
        Image stickerImage = allPlaceholders[stickerIdx].GetComponent<Image>();
        stickerImage.sprite = StickerIsActive(stickerIdx) ? StickerSprite : PlaceholderSprite;
    }

    void UpdateTextHighlights()
    {
        string highlightedText = "";
        int startIdx = 0;

        for (int i = 0; i < stickerData.Count; i++)
        {
            Sticker s = stickerData[i];
            if (StickerIsActive(i))
            {
                highlightedText += $"{pageContent.Substring(startIdx, s.startCharIdx - startIdx)}<color=#ff0000>{s.paragraphContent}</color>";
                startIdx = s.endCharIdx + 1;
            }
        }
        if (startIdx < pageContent.Length) highlightedText += pageContent.Substring(startIdx);
        pageTextMesh.text = highlightedText;
    }

    public void PlaceStickers(List<Sticker> stickers)
    {
        stickerData = stickers;

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
            UpdateSticker(i);
        }

        UpdateTextHighlights();
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

            List<string> filterWords;
            if (GS.filterWordsByEntry.TryGetValue(newSticker.paragraphContent.Trim(), out filterWords))
            {
                newSticker.filterWords = filterWords;
            }

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