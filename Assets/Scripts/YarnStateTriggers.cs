using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum ComparatorType {
    Equals,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

[System.Serializable]
public struct StateTrigger {
    public string yarnNode;
    public DialogueType dialogueType;
    public string propertyName;
    public ComparatorType comparatorType;
    public int comparisonValue;
    public bool triggerOnlyOnce;
}

public class YarnStateTriggers : MonoBehaviour
{
    public List<StateTrigger> triggers;

    void Update()
    {
        for(int i = triggers.Count - 1; i >= 0; i--)
        {
            StateTrigger trigger = triggers[i];

            #nullable enable
            FieldInfo? fieldInfo = typeof(GS).GetField(trigger.propertyName);
            #nullable disable

            if (fieldInfo == null) {
                Debug.Log($"The game state does not include the specified property: {trigger.propertyName}");
                return;
            }

            int currentValue = (int)fieldInfo.GetValue(null);
            int comparisonValue = trigger.comparisonValue;

            bool doTrigger = false;
            switch(trigger.comparatorType) {
                case ComparatorType.Equals:
                    doTrigger = currentValue == trigger.comparisonValue;
                    break;
                case ComparatorType.LessThan:
                    doTrigger = currentValue < comparisonValue;
                    break;
                case ComparatorType.LessThanOrEqual:
                    doTrigger = currentValue <= comparisonValue;
                    break;
                case ComparatorType.GreaterThan:
                    doTrigger = currentValue > comparisonValue;
                    break;
                case ComparatorType.GreaterThanOrEqual:
                    doTrigger = currentValue >= comparisonValue;
                    break;
            }

            bool didTrigger;
            if (doTrigger) {
                if (trigger.dialogueType == DialogueType.InternalMonologue) {
                    didTrigger = YarnDispatcher.StartInternalMonologue(trigger.yarnNode);
                } else {
                    didTrigger = YarnDispatcher.StartTutorial(trigger.yarnNode);
                }

                if (didTrigger && trigger.triggerOnlyOnce) {
                    triggers.RemoveAt(i);
                }
            }
        }
    }
}
