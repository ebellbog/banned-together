using System;
using UnityEngine;

public class ReadableBook : InteractableItem
{
    [Header("Book settings")]
    public int bookIdx;

    void Start() {
        // isExaminable = true;
        base.Start();
    }

    public void ApplyCustomEffects(ActionTiming currentTiming)
    {
        Debug.Log("Applying custom effects for book");
        base.ApplyCustomEffects(currentTiming);

        if (currentTiming == ActionTiming.onClick)
        {
            GS.currentNovelIdx = bookIdx;
            LibraryManager.Main.LoadBookFromLibrary();
        }
    }
}