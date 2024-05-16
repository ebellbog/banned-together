
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionType {
    Default,
    Examine,
    Journal,
    Tutorial,
    Focus
}

public static class GS
{
    public static string journalContent = "";
    public static InteractionType interactionMode = InteractionType.Default;
    public static bool isSitting = false;
    public static int concurrentThoughtBubbles = 0;

    public static void SetInteractionMode(InteractionType newMode) {
        interactionMode = newMode;
    }
}