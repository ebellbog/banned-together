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

    public void Reset()
    {
        alreadyTriggered = false;
    }
}

public class YarnStateTriggers : MonoBehaviour
{
    public List<StateTrigger> triggers;

    void Update()
    {
        if (GS.yarnStateTriggers == null)
        {
            GS.yarnStateTriggers = triggers;
        }

        for(int i = GS.yarnStateTriggers.Count - 1; i >= 0; i--)
        {
            StateTrigger trigger = GS.yarnStateTriggers[i];
            if (trigger.triggerOnlyOnce && trigger.alreadyTriggered) continue;

            if (CheckTriggerCondition(trigger)) {
                StartCoroutine(DispatchWithDelay(trigger));
            }
        }
    }

    private bool CheckTriggerCondition(StateTrigger trigger)
    {
        #nullable enable
        FieldInfo? fieldInfo = typeof(GS).GetField(trigger.propertyName);
        #nullable disable

        if (fieldInfo == null) {
            Debug.Log($"The game state does not include the specified property: {trigger.propertyName}");
            return false;
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

        return doTrigger;
    }

    IEnumerator DispatchWithDelay(StateTrigger trigger)
    {
        if (trigger.triggerAfterDelay > 0) yield return new WaitForSeconds(trigger.triggerAfterDelay);

        if (trigger.alreadyTriggered) yield break; // in case this happened in parallel, while waiting
        if (!CheckTriggerCondition(trigger)) yield break; // in case the trigger condition no longer applies after the delay

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
