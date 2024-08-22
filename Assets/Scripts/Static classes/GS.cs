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
    public static string journalContent;
    public static InteractionType interactionMode;
    public static bool isSitting;
    public static int concurrentThoughtBubbles;
    public static float bodyBattery;
    public static int tutorialItems;

    public static int fidgetSpinnerSeen;
    public static int genderQueerSeen;
    public static int paper1Seen;
    public static int paper2Seen;
    public static int poetryBookSeen;

    public static int currentDay = 0;

    private static int _currentLevelIdx = -1;
    public static int currentLevelIdx {
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

    public static void Reset()
    {
        interactionMode = InteractionType.Default;
        isSitting = false;

        concurrentThoughtBubbles = 0;
        bodyBattery = 1.0f;
        tutorialItems = 0;
        fidgetSpinnerSeen = 0;

        journalContent = "10/22/24 \n \nI'm taking poetry workshop this semester, and I haven't told mom about it. She's always been so excited that I'm majoring in journalism. But sometimes I get off track, and I can't explain it to her, or anyone. Especially when I'm supposed to be doing research, my mind seems to want to go off somewhere else. It would mean a lot if I could really break this disappearing books story for the Birchwood Daily. Even if sometimes I just want to hide away and write a poem. \n 10/23/24";
    }

}