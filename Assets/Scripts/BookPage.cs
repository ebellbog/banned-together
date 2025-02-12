using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PageSide {
    Left, Right
}

public class BookPage : MonoBehaviour
{
    public int pageNumber = 1;
    public bool setNumberAutomatically = true;

    public PageSide pageSide = PageSide.Left;
    public bool setSideAutomatically = true;

    private TextMeshProUGUI pageNumberMesh;
    private TextMeshProUGUI pageTextMesh;
    private Image pageBackground;
    private Camera pageCamera;
    private float textOffset;

    private const float PAGE_OFFSET = 100f;

    void OnValidate()
    {
        GetSubcomponents();
        AutoUpdatePages();
    }

    private void GetSubcomponents()
    {
        if (!pageNumberMesh) pageNumberMesh = transform.Find("Canvas/Page number").GetComponent<TextMeshProUGUI>();
        if (!pageTextMesh) {
            pageTextMesh = transform.Find("Canvas/Page text").GetComponent<TextMeshProUGUI>();
        }
        if (!pageBackground) {
            pageBackground = transform.GetComponentInChildren<Image>();
        }
        if (!pageCamera) {
            pageCamera = GetComponent<Camera>();
        }
    }

    bool IsOverflowing()
    {
        pageTextMesh.ForceMeshUpdate();
        return pageTextMesh ? pageTextMesh.isTextOverflowing : false;
    }

    // void GeneratePages()
    // {
    //     if (IsOverflowing() && pageTextMesh.overflowMode != TextOverflowModes.Linked)
    //     {
    //         Debug.Log("Generating new page");

    //         GameObject newPage = Instantiate(gameObject);
    //         newPage.transform.localPosition = new Vector3(
    //             transform.localPosition.x + PAGE_OFFSET,
    //             transform.localPosition.y,
    //             transform.localPosition.z
    //         );
    //         pageTextMesh.overflowMode = TextOverflowModes.Linked;
    //         pageTextMesh.linkedTextComponent = newPage.GetComponentInChildren<TMP_Text>();

    //         BookPage newPageComponent = newPage.GetComponent<BookPage>();
    //         newPageComponent.maxPages = maxPages - 1;

    //         newPageComponent.OnValidate();
    //     }
    // }

    void AutoUpdatePages()
    {
        List<BookPage> siblingPages = FindObjectsOfType<BookPage>()
            .Where(page => page.transform.parent == transform.parent)
            .OrderBy(page => page.transform.GetSiblingIndex())
            .ToList();

        // Allow the first page to apply this logic for all pages
        if (siblingPages[0] != this)
        {
            siblingPages[0].AutoUpdatePages();
            return;
        }

        int currentNumber = 0;
        bool isLeftPage = false;
        foreach(BookPage bookPage in siblingPages) {
            if (bookPage.gameObject == null) continue;
            // if (bookPage.GetPageText().Length == 0 && currentNumber > 0) {
            //     UnityEditor.EditorApplication.delayCall += () =>
            //     {
            //         if (bookPage) DestroyImmediate(bookPage.gameObject);
            //     };
            //     continue;
            // }

            if (bookPage.setNumberAutomatically) {
                currentNumber++;
                bookPage.pageNumber = currentNumber;
                bookPage.gameObject.name = $"Live Book Page ({currentNumber})";
                bookPage.UpdateRenderTexture();
            }

            isLeftPage = !isLeftPage;
            if (bookPage.setSideAutomatically)
            {
                bookPage.pageSide = isLeftPage ? PageSide.Left: PageSide.Right;
            }

            bookPage.SyncState();
        }
    }

    void UpdateRenderTexture()
    {
        RenderTexture targetTexture = Resources.Load<RenderTexture>($"Page {pageNumber} texture");
        if (pageCamera && targetTexture)
        {
            pageCamera.targetTexture = targetTexture;
        } 
    }

    string GetPageText()
    {
        pageTextMesh.ForceMeshUpdate();

        string fullText = pageTextMesh.GetParsedText();
        TMP_TextInfo textInfo = pageTextMesh.textInfo;

        int lastVisibleCharIndex = -1;
        int firstVisibleCharIndex = -1;
        
        if (textInfo == null) return "";
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (textInfo.characterInfo[i].isVisible)
            {
                if (firstVisibleCharIndex == -1) firstVisibleCharIndex = i;
                lastVisibleCharIndex = i;
            }
        }

        if (lastVisibleCharIndex == -1) return "";
        return fullText.Substring(firstVisibleCharIndex, lastVisibleCharIndex - firstVisibleCharIndex + 1);
    }

    void SyncState()
    {
        pageNumberMesh.text = pageNumber.ToString();

        Transform backgroundTransform = pageBackground.gameObject.transform;
        backgroundTransform.localScale = new Vector3(
            pageSide == PageSide.Left ? 1f : -1f,
            backgroundTransform.localScale.y, backgroundTransform.localScale.z
        );

        Transform textTransform = pageTextMesh.gameObject.transform;
        textOffset = Mathf.Abs(textTransform.localPosition.x);

        textTransform.localPosition = new Vector3(
            pageSide == PageSide.Left ? textOffset : -textOffset,
            textTransform.localPosition.y, textTransform.localPosition.z
        );
    }
}
