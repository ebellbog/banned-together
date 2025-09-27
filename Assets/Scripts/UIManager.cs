using UnityEngine;

public class UIManager : MonoBehaviour {
    public CanvasGroup backgroundMatte;
    public CanvasGroup itemInteractionUI;
    public float FadeSpeed = 1f;

    private bool isFadingInMatte = false;
    private bool isFadingInUI = false;
    private float maxMatteOpacity = .5f;
    private float originalFadeSpeed = 0;

    public void FadeInMatte()
    {
        if (originalFadeSpeed != 0)
        {
            FadeSpeed = originalFadeSpeed;
            originalFadeSpeed = 0;
        }
        maxMatteOpacity = .5f;
        isFadingInMatte = true;
    }
    public void FadeOutMatte()
    {
        if (originalFadeSpeed != 0)
        {
            FadeSpeed = originalFadeSpeed;
            originalFadeSpeed = 0;
        }
        isFadingInMatte = false;
    }
    public void FadeToBlack()
    {
        originalFadeSpeed = FadeSpeed;
        FadeSpeed *= .3f;

        maxMatteOpacity = 1f;
        isFadingInMatte = true;
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
            if (canvasGroup.alpha < maxMatteOpacity)
            {
                canvasGroup.alpha += FadeSpeed * maxMatteOpacity * Time.deltaTime;
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
                canvasGroup.alpha -= FadeSpeed * maxMatteOpacity * Time.deltaTime;
            }
            else
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
    }
}