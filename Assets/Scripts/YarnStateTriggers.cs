using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Yarn.Unity;

public enum ComparatorType {
    Equals,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

[System.Serializable]
public class StateTrigger {
    public string yarnNode;
    public DialogueType dialogueType;
    public string propertyName;
    public ComparatorType comparatorType;
    public int comparisonValue;
    public bool triggerOnlyOnce;
    internal bool alreadyTriggered;
    public float triggerAfterDelay;
}

public class YarnStateTriggers : MonoBehaviour
{
    public List<StateTrigger> triggers;

    void Update()
    {
        for(int i = triggers.Count - 1; i >= 0; i--)
        {
            StateTrigger trigger = triggers[i];
            if (trigger.triggerOnlyOnce && trigger.alreadyTriggered) continue;

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

            if (doTrigger) {
                StartCoroutine(DispatchWithDelay(trigger));
            }
        }
    }

    IEnumerator DispatchWithDelay(StateTrigger trigger)
    {
        if (trigger.triggerAfterDelay > 0) yield return new WaitForSeconds(trigger.triggerAfterDelay);

        bool didTrigger;
        if (trigger.dialogueType == DialogueType.InternalMonologue) {
            didTrigger = YarnDispatcher.StartInternalMonologue(trigger.yarnNode);
        } else {
            didTrigger = YarnDispatcher.StartTutorial(trigger.yarnNode);
        }

        if (didTrigger) trigger.alreadyTriggered = true;
        yield return null;
    }

}
