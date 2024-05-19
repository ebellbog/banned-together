
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
    public static string journalContent = "10/22/24 \n \nI’m taking poetry workshop this semester, and I haven’t told mom about it. She’s always been so excited that I’m majoring in journalism. But sometimes I get off track, and I can’t explain it to her, or anyone. Especially when I’m supposed to be doing research, my mind seems to want to go off somewhere else. It would mean a lot if I could really break this “disappearing books” story for the Birchwood Daily. Even if sometimes I just want to hide away and write a poem. \n 10/23/24";
    public static InteractionType interactionMode = InteractionType.Default;
    public static bool isSitting = false;
    public static int concurrentThoughtBubbles = 0;
    public static float bodyBattery = 1.0f;
    public static int paper1Seen = 0;
    public static int paper2Seen = 0;
    public static int poetryBookSeen = 0;
    public static int dangerousLibrarySeen = 0;
    public static int tutorialItems = 0;
    public static int fidgetSpinnerSeen = 0;
    public static int genderQueerSeen = 0;

    public static void SetInteractionMode(InteractionType newMode) {
        interactionMode = newMode;
    }

}