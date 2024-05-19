
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public enum InteractionType {
    Default,
    Examine,
    Journal,
    Tutorial,
    Monologue,
    Focus
}

public static class GS
{
    public static string journalContent = "";
    public static InteractionType interactionMode = InteractionType.Default;
    public static bool isSitting = false;
    public static int concurrentThoughtBubbles = 0;
    public static float bodyBattery = 1.0f;
    public static int paper1Seen = 0;
    public static int dangerousLibrarySeen = 0;
    public static int tutorialItems = 0;

    public static void SetInteractionMode(InteractionType newMode) {
        interactionMode = newMode;
    }

}