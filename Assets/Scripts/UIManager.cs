using UnityEngine;

public class UIManager : MonoBehaviour {
    public CanvasGroup backgroundMatte;
    public CanvasGroup itemInteractionUI;
    public float FadeSpeed = 1f;

    private bool isFadingInMatte = false;
    private bool isFadingInUI = false;

    public void FadeInMatte()
    {
        isFadingInMatte = true;
    }
    public void FadeOutMatte()
    {
        isFadingInMatte = false;
    }

    public void FadeInInteractionUI()
    {
        isFadingInUI = true;
    }
    public void FadeOutInteractionUI()
    {
        isFadingInUI = false;
    }

    void Awake()
    {
        backgroundMatte.alpha = 0;
        itemInteractionUI.alpha = 0;
    }

    void Update()
    {
        incrementFade(backgroundMatte, isFadingInMatte);
        incrementFade(itemInteractionUI, isFadingInUI);
    }

    private void incrementFade(CanvasGroup canvasGroup, bool isFadingIn)
    {
        if (isFadingIn)
        {
            if (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += FadeSpeed * Time.deltaTime;
            }
            else
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
        }
        else
        {
            if (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= FadeSpeed * Time.deltaTime;
            }
            else
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
    }
}