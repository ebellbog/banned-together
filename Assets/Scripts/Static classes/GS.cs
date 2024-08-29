using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InteractionType {
    Default,
    Examine,
    Journal,
    Tutorial,
    Monologue,
    Focus,
    Paused
}

public static class GS
{
    public static bool isReady = false;
    public static string journalContent;
    public static InteractionType interactionMode;
    public static bool isSitting;
    public static int concurrentThoughtBubbles;
    public static float bodyBattery;
    public static int currentJournalPage;

    public static int tutorialItems;

    public static int fidgetSpinnerSeen;
    public static int genderQueerSeen;
    public static int paper1Seen;
    public static int paper2Seen;
    public static int poetryBookSeen;
    public static int lockedDoorSeen;
    public static int lostBooksSeen;

    public static List<StateTrigger> yarnStateTriggers;
    public static Dictionary<string, JournalEntry> journalDict;

    public static int currentDay;

    private static int _currentLevelIdx = -1;
    public static int currentSceneIdx {
        get { return _currentLevelIdx; }
        set {
            _prevLevelIdx = _currentLevelIdx;
            _currentLevelIdx = value;
        }
    }

    private static int _prevLevelIdx;
    public static int prevLevelIdx {
        get { return _prevLevelIdx; }
        private set {
            _prevLevelIdx = value;
        }
    }

    public static void SetInteractionMode(InteractionType newMode) {
        interactionMode = newMode;
    }

    public static void ResetAll()
    {
        ResetDaily();

        currentJournalPage = 0;

        currentDay = 1;

        concurrentThoughtBubbles = 0;
        tutorialItems = 0;

        fidgetSpinnerSeen = 0;
        genderQueerSeen = 0;
        paper1Seen = 0;
        paper2Seen = 0;
        poetryBookSeen = 0;
        lockedDoorSeen = 0;
        lostBooksSeen = 0;

        journalDict = null;
        journalContent = "<b>10/22/23</b> \n \nI'm taking poetry workshop this semester, and I haven't told mom about it. She's always been so excited that I'm majoring in journalism. But sometimes I get off track, and I can't explain it to her, or anyone. Especially when I'm supposed to be doing research, my mind seems to want to go off somewhere else. It would mean a lot if I could really break this disappearing books story for the Birchwood Chronicle. Even if sometimes I just want to hide away and write a poem. \n <b>10/23/23</b>";
        
        currentSceneIdx = SceneManager.GetActiveScene().buildIndex;
        prevLevelIdx = 0;

        yarnStateTriggers = null;
        YarnDispatcher.Reset();
    }

    public static void ResetDaily()
    {

        interactionMode = InteractionType.Default;
        isSitting = false;
        bodyBattery = 1.0f;
    }

}