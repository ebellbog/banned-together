using UnityEngine;
using Yarn.Unity;

public static class YarnDispatcher
{
    public static DialogueRunner internalMonologueSystem;
    public static DialogueRunner tutorialDialogSystem;

    public static void StartTutorial(string tutorialNode) {
        if (!tutorialDialogSystem)
        {
            Debug.LogWarning("No tutorial dialog system is set");
            return;
        }

        Debug.Log($"Starting tutorial: {tutorialNode}");

        UI.FadeInMatte();
        UI.HideCursor();
        Player.LockPlayer();

        GS.interactionMode = InteractionType.Tutorial;

        tutorialDialogSystem.StartDialogue(tutorialNode);
        tutorialDialogSystem.onDialogueComplete.AddListener(OnDialogueComplete);
    }

    public static void EndTutorial() {
        tutorialDialogSystem.Stop();
    }

    private static void OnDialogueComplete() {
        GS.interactionMode = InteractionType.Default;
        UI.FadeOutMatte();
        UI.ShowCursor();
        Player.UnlockPlayer();
    }

    public static void StartInternalMonologue(string monologueNode) {
        if (!tutorialDialogSystem)
        {
            Debug.LogWarning("No internal monologue system is set");
            return;
        }

        Debug.Log($"Starting monologue: {monologueNode}");
        internalMonologueSystem.StartDialogue(monologueNode);
        // TODO: add to journal
        // TODO: play dialog
    }
}