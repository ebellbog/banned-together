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
    Paused,
    None
}

public static class GS
{
    public static bool isReady = false;

    public static bool didTutorializeJournal = false;
    public static string journalContent = "";
    public static int currentJournalPage;
    public static int journalEnabled; // using int instead of bool for compatibility with InteractableItem
    public static int stickersEnabled; // (same as above)
    public static Sticker redStickerPlacement;
    public static Sticker blueStickerPlacement;

    public static InteractionType interactionMode = InteractionType.None;

    public static bool isSitting;
    public static int concurrentThoughtBubbles;
    public static float bodyBattery;
    public static float speedReduction;

    public static int tutorialItems;
    public static int fidgetSpinnerSeen;
    public static int genderQueerSeen;
    public static int paper1Seen;
    public static int paper2Seen;
    public static int poetryBookSeen;
    public static int lockedDoorSeen;
    public static int lostBooksSeen;

    public static List<StateTrigger> yarnStateTriggers;
    public static Dictionary<string, JournalEntry> journalEntryByKey;
    public static Dictionary<string, JournalEntry> journalEntryByContent;

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

        currentJournalPage = 0;
        didTutorializeJournal = false;
        journalEntryByKey = null;
        journalEntryByContent = null;
        journalContent = "10/22/23";
        journalEnabled = 0;
        stickersEnabled = 0;

        RemoveAllStickers();

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
        speedReduction = 1.0f;
    }

    public static void RemoveAllStickers()
    {
        redStickerPlacement = new Sticker();
        blueStickerPlacement = new Sticker();
    }
}