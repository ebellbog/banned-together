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
    public JournalEntry associatedJournalEntry;
    public Vector2 stickerCenter;
    public Color stickerColor;

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
    public Sprite PlaceholderSprite;
    public GameObject stickerPrefab;
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
        // Clear old stickers after a new one has been placed
        if (prevRedStickerPlacement != GS.redStickerPlacement && stickerData != null)
        {
            for (int i = 0; i < stickerData.Count; i++) UpdateSticker(i);
            UpdateTextHighlights();
            prevRedStickerPlacement = GS.redStickerPlacement;
        }
    }

    public void OnMouseDown(Vector3 mousePos)
    {

        int clickedStickerIdx = GetStickerByCoords(mousePos);
        if (clickedStickerIdx > -1)
        {
            ToggleSticker(clickedStickerIdx);
        }
    }

    int GetStickerByCoords(Vector2 coords, float maxDist = 0)
    {
        if (maxDist == 0) maxDist = placeholderHeight / 2f;
        for (int i = 0; i < stickerData.Count; i++)
        {
            Sticker currentSticker = stickerData[i];

            float distanceToSticker = Mathf.Abs(currentSticker.stickerCenter.y - coords.y);//Vector2.Distance(coords, currentSticker.stickerCenter);
            if (distanceToSticker < maxDist && Mathf.Abs(coords.x) > 500f) // TODO: incorporate x coord correctly
                return i;
        }
        return -1;
    }

    public bool TryPlacingStickerAtCoords(Vector2 coords, Color stickerColor)
    {
        int stickerIdx = GetStickerByCoords(coords);
        if (stickerIdx < 0) return false;

        Sticker targetSticker = stickerData[stickerIdx];
        targetSticker.stickerColor = stickerColor;
        stickerData[stickerIdx] = targetSticker;

        ToggleSticker(stickerIdx);
        return true;
    }

    // TODO: support other sticker types
    bool StickerIsActive(int stickerIdx)
    {
        return GS.redStickerPlacement == stickerData[stickerIdx];
    }

    public void ToggleSticker(int stickerIdx)
    {
        if (StickerIsActive(stickerIdx)) GS.redStickerPlacement = new Sticker();
        else
        {
            GS.redStickerPlacement = stickerData[stickerIdx];
        }
        UpdateSticker(stickerIdx);
        UpdateTextHighlights();
    }

    void UpdateSticker(int stickerIdx)
    {
        GameObject stickerPlaceholder = allPlaceholders[stickerIdx];
        if (StickerIsActive(stickerIdx))
        {
            stickerPlaceholder.GetComponentInChildren<Image>().enabled = false;
            if (stickerPlaceholder.transform.childCount == 0)
            {
                GameObject newSticker = Instantiate(stickerPrefab);

                newSticker.transform.Find("Sticker border shadow").gameObject.SetActive(false);
                newSticker.transform.Find("Sticker border plain").gameObject.SetActive(true);

                Color stickerColor = stickerData[stickerIdx].stickerColor;
                newSticker.transform.Find("Sticker center").GetComponent<Image>().color = stickerColor;

                newSticker.transform.SetParent(stickerPlaceholder.transform, false);
                newSticker.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                newSticker.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            stickerPlaceholder.GetComponentInChildren<Image>().enabled = true;
            if (stickerPlaceholder.transform.childCount > 0)
                Destroy(stickerPlaceholder.transform.GetChild(0).gameObject);
        }
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
                highlightedText += $"{pageContent.Substring(startIdx, s.startCharIdx - startIdx)}<color=#{ColorUtility.ToHtmlStringRGB(s.stickerColor * .9f)}>{s.paragraphContent}</color>";
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
            Vector2 stickerCenter = new Vector2(
                Math.Abs(newPlaceholder.transform.localPosition.x) * (pageSide == PageSide.Left ? -1.0f : 1.0f),
                currentSticker.stickerCenter.y
            );

            currentSticker.stickerCenter = stickerCenter;
            stickers[i] = currentSticker;

            newPlaceholder.transform.localPosition = stickerCenter;

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

            // Recover color data
            if (GS.redStickerPlacement == newSticker)
                newSticker.stickerColor = GS.redStickerPlacement.stickerColor;

            // Skip date headers
            DateTime dt;
            if (DateTime.TryParse(newSticker.paragraphContent, out dt))
                continue;

            JournalEntry journalEntry;
            if (GS.journalEntryByContent != null && GS.journalEntryByContent.TryGetValue(newSticker.paragraphContent.Trim(), out journalEntry))
            {
                newSticker.associatedJournalEntry = journalEntry;
            }

            newSticker.endCharIdx = index - 1;
            newSticker.stickerCenter = new Vector2();
            newSticker.stickerCenter.y = GetVerticalCenterOfRange(newSticker.startCharIdx, newSticker.endCharIdx) - placeholderHeight / 4f; // TODO: figure out why stickerHeight / 4f is needed?

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