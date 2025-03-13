using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum ActionTiming {
    onHover,
    onHoverExit,
    onClick,
    afterExamine
}

public enum EnabledState {
    setEnabled,
    setDisabled,
    toggleEnabled,
}

[System.Serializable]
public class StateUpdate {
    public ActionTiming timing;
    public string propertyToIncrement;
    public bool incrementOnlyOnce = true;
    internal bool alreadyIncremented = false;
}

[System.Serializable]
public class ComponentAction {
    public ActionTiming timing;
    public MonoBehaviour targetComponent;

    // TODO: replace with a single EnabledState property
    public bool setEnabled;
    public bool setDisabled;
    public bool toggleEnabled;
    public string callFunctionByName;
    public bool callOnlyOnce = false;
    internal bool alreadyCalled = false;
}

[System.Serializable]
public class GameObjectAction {
    public ActionTiming timing;
    public GameObject gameObject;
    public EnabledState action;
}

[System.Serializable]
public class JournalUpdate {
    public ActionTiming timing;
    [Tooltip("Make sure this entry is present in the JournalManager component")]
    public string journalEntryName;
}

[System.Serializable]
public class AudioAction {
    public ActionTiming timing;
    public string sfxName;
    public bool playOnlyOnce = false;
    internal bool alreadyPlayed = false;
}

public enum RotationAxis {
    All, LeftRight, UpDown,
}

[RequireComponent(typeof(Collider))]
public class InteractableItem : MonoBehaviour
{
    public GameObject interactionParent;

    [Header("Focus settings")]
    public bool isFocusable = false;

    [Header("Examination settings")]
    public bool isExaminable = false;
    public bool orientToCamera = false;
    public bool pivotAroundVisualCenter = true;
    public RotationAxis rotationAxis = RotationAxis.All;
    public float scaleOnInteraction = 1.0f;
    public bool enablePanning = false;
    public bool affectsBodyBattery = false;
    public bool inertialRotation = false;
    public bool useGrabCursor = false;
    public bool keepAfterExamining = false;

    [Header("UI settings")]
    public bool showOutline = false;
    public Color outlineColorOverride;
    public Sprite cursorOverride;

    [Header("Custom effects")]
    public List<GameObjectAction> gameObjectActions;
    public List<ComponentAction> componentActions;
    public List <AudioAction> audioActions;
    public List <JournalUpdate> journalUpdates;
    public List <StateUpdate> gameStateUpdates;

    public void Awake()
    {
        gameObject.tag = "Interactable";
        // if (isFocusable) gameObject.layer = LayerMask.NameToLayer("No post");
    }

    // TODO: maybe DRY up some of this code?
    public void ApplyCustomEffects(ActionTiming currentTiming)
    {
        foreach(AudioAction audioAction in audioActions)
        {
            if (currentTiming != audioAction.timing) continue;
            if (audioAction.playOnlyOnce && audioAction.alreadyPlayed) continue;
            AudioManager.instance.PlaySFX(audioAction.sfxName);
            audioAction.alreadyPlayed = true;
        }

        foreach(StateUpdate stateUpdate in gameStateUpdates)
        {
            if (stateUpdate.incrementOnlyOnce && stateUpdate.alreadyIncremented) continue;
            if (currentTiming != stateUpdate.timing) continue;

            string propertyName = stateUpdate.propertyToIncrement;

            #nullable enable
            FieldInfo? fieldInfo = typeof(GS).GetField(propertyName);
            #nullable disable

            if (fieldInfo == null) {
                Debug.Log($"The game state does not include the specified property: {propertyName}");
                return;
            }

            int currentValue = (int)fieldInfo.GetValue(null);
            fieldInfo.SetValue(null, currentValue + 1);
            stateUpdate.alreadyIncremented = true;

            Debug.Log($"Incremented property {propertyName} of game state to {currentValue + 1}");
        }

        foreach(ComponentAction action in componentActions)
        {
            if (action.callOnlyOnce && action.alreadyCalled) continue;
            if (currentTiming != action.timing) continue;

            if (action.toggleEnabled) {
                action.targetComponent.enabled = !action.targetComponent.enabled;
            }
            else if (action.setEnabled)
            {
                action.targetComponent.enabled = true;
            }
            else if (action.setDisabled)
            {
                action.targetComponent.enabled = false;
            }
            else if (action.callFunctionByName != null)
            {
                action.targetComponent.Invoke(action.callFunctionByName, 0);
            }

            action.alreadyCalled = true;
        }

        foreach(GameObjectAction action in gameObjectActions)
        {
            if (currentTiming != action.timing) continue;

            switch(action.action)
            {
                case EnabledState.setEnabled:
                    action.gameObject.SetActive(true);
                    break;
                case EnabledState.setDisabled:
                    action.gameObject.SetActive(false);
                    break;
                case EnabledState.toggleEnabled:
                    action.gameObject.SetActive(!action.gameObject.activeSelf);
                    break;
            }
        }

        foreach(JournalUpdate journalUpdate in journalUpdates)
        {
            if (currentTiming != journalUpdate.timing) continue;
            JournalManager.Main.AddToJournal(journalUpdate.journalEntryName);
        }
    }
}
