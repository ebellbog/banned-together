using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PsychEffects : MonoBehaviour
{
    public bool drainBodyBattery = true;
    public float slowMovementToPercent = 0.50f;
    private bool isInsideTrigger = false;

    void Update()
    {
        // Only drain when not Focusing
        if (GS.interactionMode == InteractionType.Default && isInsideTrigger)
        {
            BodyBatteryManager.Main.StartDrainingBattery();
        } else
        {
            BodyBatteryManager.Main.StopDrainingBattery();
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        isInsideTrigger = true;
        GS.speedReduction = slowMovementToPercent; // Reduce speed regardless of Focus
    }
    void OnTriggerExit(Collider collision)
    {
        isInsideTrigger = false;
        GS.speedReduction = 1.0f;
    }
}
