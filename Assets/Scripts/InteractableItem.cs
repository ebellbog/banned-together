using System.Reflection;
using UnityEngine;

[System.Serializable]
public class StateUpdate {
    public string propertyToIncrement;
    public bool incrementOnlyOnce = true;
    public bool incrementDuringMonologue = false;
    internal bool alreadyIncremented = false;
}

public class InteractableItem : MonoBehaviour
{
    public bool highlightInFocusMode = true;
    public bool orientToCamera = false;
    public float scaleOnInteraction = 1.0f;
    public StateUpdate[] gameStateUpdates;

    public void Awake()
    {
        gameObject.tag = "Interactable";
    }

    public void UpdateGameState(bool duringMonologue = false)
    {
        foreach (StateUpdate stateUpdate in gameStateUpdates)
        {
            if (stateUpdate.incrementOnlyOnce && stateUpdate.alreadyIncremented) continue;
            if (duringMonologue != stateUpdate.incrementDuringMonologue) continue;

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
    }
}
