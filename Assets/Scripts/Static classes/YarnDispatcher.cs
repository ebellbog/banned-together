using TMPro;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

public static class YarnDispatcher
{
    public static DialogueRunner internalMonologueSystem;
    public static DialogueRunner tutorialDialogSystem;
    public static TextMeshProUGUI monologueTextMesh;

    public static bool StartTutorial(string tutorialNode) {
        if (!tutorialDialogSystem)
        {
            Debug.LogWarning("No tutorial dialog system is set");
            return false;
        }
        if (YarnSpinnerIsActive())
        {
            Debug.LogWarning($"Skipping tutorial for {tutorialNode} because dialogue is already active");
            return false;
        }
        if (GS.interactionMode != InteractionType.Default)
        {
            Debug.LogWarning($"Skipping tutorial for {tutorialNode} because the current interaction mode is {GS.interactionMode}");
            return false;
        }

        UI.FadeInMatte();
        UI.HideCursor();
        Player.LockPlayer();

        GS.interactionMode = InteractionType.Tutorial;

        tutorialDialogSystem.StartDialogue(tutorialNode);
        tutorialDialogSystem.onDialogueComplete.AddListener(OnDialogueComplete);

        return true;
    }

    public static void EndTutorial() {
        if (GS.interactionMode == InteractionType.Tutorial)
            tutorialDialogSystem.Stop();
    }

    private static void OnDialogueComplete() {
        GS.interactionMode = InteractionType.Default;
        UI.FadeOutMatte();
        UI.ShowCursor();
        Player.UnlockPlayer();
    }

    public static bool StartInternalMonologue(string monologueNode) {
        if (!tutorialDialogSystem)
        {
            Debug.LogWarning("No internal monologue system is set");
            return false;
        }
        if (YarnSpinnerIsActive())
        {
            Debug.Log($"Skipping monologue for {monologueNode} because dialogue is already active");
            return false;
        }
        if (GS.interactionMode != InteractionType.Default && GS.interactionMode != InteractionType.Examine)
        {
            Debug.LogWarning($"Skipping tutorial for {monologueNode} because the current interaction mode is {GS.interactionMode}");
            return false;
        }

        GS.interactionMode = InteractionType.Monologue;

        internalMonologueSystem.onDialogueComplete.RemoveAllListeners();
        internalMonologueSystem.onDialogueComplete.AddListener(OnMonologueEnd);

        internalMonologueSystem.StartDialogue(monologueNode);

        return true;
    }

    public static bool YarnSpinnerIsActive()
    {
        return internalMonologueSystem.Dialogue.IsActive || tutorialDialogSystem.Dialogue.IsActive;
    }

    private static void OnMonologueEnd() {
        GS.journalContent += $"{(GS.journalContent.Length > 0 ? "\n\n" : "")}{monologueTextMesh.text}";
        GS.interactionMode = InteractionType.Default;
    }
}