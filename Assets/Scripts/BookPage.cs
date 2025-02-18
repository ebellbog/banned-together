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
    private int _pageNumber = 1;
    public int pageNumber {
        get { return _pageNumber; }
        set { 
            _pageNumber = value; 
            pageNumberMesh.text = _pageNumber.ToString();
            UpdateRenderTexture();
        }
    }

    private PageSide _pageSide = PageSide.Left;
    public PageSide pageSide {
        get { return _pageSide; }
        set {
            _pageSide = value;

            Transform backgroundTransform = pageBackground.gameObject.transform;
            backgroundTransform.localScale = new Vector3(
                pageSide == PageSide.Left ? 1f : -1f,
                backgroundTransform.localScale.y, backgroundTransform.localScale.z
            );

            Transform textTransform = pageTextMesh.gameObject.transform;
            float textOffset = Mathf.Abs(textTransform.localPosition.x);

            textTransform.localPosition = new Vector3(
                pageSide == PageSide.Left ? textOffset : -textOffset,
                textTransform.localPosition.y, textTransform.localPosition.z
            );
        }
    }

    public string pageContent {
        get { return pageTextMesh.text; }
        set {
            GetSubcomponents();
            pageTextMesh.text = value;
        }
    }

    public float pageHeight = 861;
    public float aspectRatio = 8.5f / 11f;

    protected TextMeshProUGUI pageNumberMesh;
    protected TextMeshProUGUI pageTextMesh;
    protected Image pageBackground;
    protected Camera pageCamera;

    [System.NonSerialized]
    public RenderTexture renderTexture;


    public void InitPage(int pNum, PageSide pSide)
    {
        GetSubcomponents();
        pageNumber = pNum;
        pageSide = pSide;
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

    void UpdateRenderTexture()
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture((int)(pageHeight * aspectRatio), (int)pageHeight, 8);
            renderTexture.name = $"Dynamic texture ({pageNumber})";
            pageCamera.targetTexture = renderTexture;
        }
    }

    public string GetVisibleText()
    {
        pageTextMesh.ForceMeshUpdate();

        string fullText = pageTextMesh.text;//GetParsedText();

        int overflowIdx = pageTextMesh.firstOverflowCharacterIndex;
        if (overflowIdx == -1) return fullText;
        else return fullText.Substring(0, overflowIdx);
    }

    bool IsOverflowing()
    {
        pageTextMesh.ForceMeshUpdate();
        return pageTextMesh ? pageTextMesh.isTextOverflowing : false;
    }
}
