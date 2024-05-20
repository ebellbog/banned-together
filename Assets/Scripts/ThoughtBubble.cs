using UnityEngine;
using TMPro;

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
    public float InitialScale = 0.6f;
    public ThoughtType thoughtType = ThoughtType.Intrusive;

    [HideInInspector]
    public GameObject bubbleCanvas;

    private bool fadeIn = false;
    private bool fadeOut = true;

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
            }
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

        switch(thoughtType) {
            case ThoughtType.Intrusive:
                bubbleCanvas.transform.Find("ThoughtBubble/IntrusiveBg").gameObject.SetActive(true);
                break;
            case ThoughtType.Focus:
                bubbleCanvas.transform.Find("ThoughtBubble/FocusBg").gameObject.SetActive(true);
                textMesh.color = Color.white;
                textMesh.fontStyle = FontStyles.Bold;
                break;
            case ThoughtType.Dark:
                bubbleCanvas.transform.Find("ThoughtBubble/DarkBg").gameObject.SetActive(true);
                textMesh.color = Color.white;
                break;
            default:
                break;
        }

        animationParent = bubbleCanvas.transform.GetChild(0).gameObject;
        animationParent.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        canvasGroup = bubbleCanvas.GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = 0;
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
