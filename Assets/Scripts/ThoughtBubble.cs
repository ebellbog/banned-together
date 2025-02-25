using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public enum ThoughtType {
    Intrusive,
    Focus,
    Dark
}

[RequireComponent(typeof(Collider))]
public class ThoughtBubble : MonoBehaviour
{
    public string ThoughtText;
    public float FadeSpeed = 2.5f;
    public float scale = 1.0f;
    public ThoughtType thoughtType = ThoughtType.Intrusive;
    [Tooltip("Separate terms by commas or spaces")]
    public string focusOnWords;
    public int firstDay = 1;
    public int lastDay = 999;

    [HideInInspector]
    public GameObject bubbleCanvas;

    private float InitialScale = 0.6f;
    private bool fadeIn = false;
    private bool fadeOut = true;
    private List<string> focusList;

    private CanvasGroup canvasGroup;
    private GameObject animationParent;
    private TextMeshProUGUI textMesh;

    private float currentScale = 0.6f;

    void Update()
    {
        if (bubbleCanvas == null) return;
        if (fadeIn) {
            if (canvasGroup.alpha < 1.0f) {
                canvasGroup.alpha += FadeSpeed * Time.deltaTime;
            }
            if (currentScale < 1.0f) {
                currentScale += FadeSpeed * Time.deltaTime * (1.0f - InitialScale);
                animationParent.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
        } else if (fadeOut) {
            if (canvasGroup.alpha > 0) {
                canvasGroup.alpha -= FadeSpeed * Time.deltaTime;
            } // TODO: once fully faded, stop billboarding, for efficiency?
            if (currentScale > InitialScale) {
                currentScale -= FadeSpeed * Time.deltaTime * (1.0f - InitialScale);
                animationParent.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
        }
    }

    public void InstantiateBubble(GameObject bubblePrefab)
    {
        Collider myCollider = gameObject.GetComponent<Collider>(); 
        Vector3 size = myCollider.bounds.size;
        Vector3 position = myCollider.bounds.center;

        bubbleCanvas = Instantiate(
            bubblePrefab,
            new Vector3 (position.x, position.y + size.y / 2 - 0.1f, position.z),
            Quaternion.identity
        );

        textMesh = bubbleCanvas.GetComponentInChildren<TextMeshProUGUI>();
        textMesh.SetText(ThoughtText);

        Image bgImage = bubbleCanvas.GetComponentInChildren<Image>();

        switch(thoughtType) {
            case ThoughtType.Intrusive:
                break;
            case ThoughtType.Focus:
                textMesh.color = Color.white;
                // textMesh.fontStyle = FontStyles.Bold;

                // TODO: set colors based on sticker
                bgImage.color = new Color(.8f, .2f, .2f);
                bgImage.material = Instantiate(bgImage.material); // copy material to set independently for each thought bubble
                bgImage.material.SetColor("_SolidOutline", new Color(.4f, 0, 0));
                break;
            case ThoughtType.Dark:
                textMesh.color = Color.white;
                break;
            default:
                break;
        }

        if (scale != 1) {
            Billboard billboardComponent = bubbleCanvas.GetComponent<Billboard>();
            billboardComponent.ScaleReferenceDistance /= scale;
        }

        animationParent = bubbleCanvas.transform.GetChild(0).gameObject;
        animationParent.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        canvasGroup = bubbleCanvas.GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    public bool MatchesCurrentFocus()
    {
        if (!(GS.redStickerPlacement.filterWords?.Count > 0)) return false;
        if (focusList == null)
        {
            focusList = new List<string>();
            if (focusOnWords?.Length > 0)
                focusList.AddRange(focusOnWords.Split(new[] {",", " "}, StringSplitOptions.RemoveEmptyEntries));
        }
        return GS.redStickerPlacement.filterWords.Intersect(focusList).Count() > 0;
    }

    public void FadeIn() {
        if (!fadeIn) {
            currentScale = InitialScale;
            GS.concurrentThoughtBubbles++;
        }
        fadeIn = true;
        fadeOut = false;
    }

    public void FadeOut() {
        if (!fadeOut) {
            GS.concurrentThoughtBubbles--;
        }
        fadeIn = false;
        fadeOut = true;
    }

    // note: WIP; does not work yet
    public void Explode() {
        Vector3[] vertices = textMesh.mesh.vertices;
        int characterCount = textMesh.textInfo.characterCount;

        for (int i = 0; i < characterCount; i++) {
            TMP_CharacterInfo charInfo = textMesh.textInfo.characterInfo[i];
            int vertexIndex = charInfo.vertexIndex;
        }
    }
}
