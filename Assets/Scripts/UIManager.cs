using UnityEngine;

public class UIManager : MonoBehaviour {
    public CanvasGroup backgroundMatte;
    public float FadeSpeed = 1f;

    private bool isFadingIn = false;

    public void FadeInMatte() {
        isFadingIn = true;
    }

    public void FadeOutMatte() {
        isFadingIn = false;
    }

    void Update() {
        if (isFadingIn)
        {
            if (backgroundMatte.alpha < 1)
            {
                backgroundMatte.alpha += FadeSpeed * Time.deltaTime;
            }
            else
            {
                backgroundMatte.blocksRaycasts = true;
                backgroundMatte.interactable = true;
            }
        }
        else
        {
            if (backgroundMatte.alpha > 0)
            {
                backgroundMatte.alpha -= FadeSpeed * Time.deltaTime;
            }
            else
            {
                backgroundMatte.blocksRaycasts = false;
                backgroundMatte.interactable = false;
            }
        }
    }
}