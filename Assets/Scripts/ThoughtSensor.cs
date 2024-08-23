using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThoughtSensor : MonoBehaviour
{
    public GameObject BubbleCanvas;
    public float MaxDistance  = 12.0f;
    public float ViewAngle = 16.0f;
    public bool ShowThoughts = true;
    public bool AlwaysShowDarkThoughts = true;

    private ThoughtBubble[] allThoughts;

    void Start()
    {
        allThoughts = Object.FindObjectsByType<ThoughtBubble>(FindObjectsSortMode.None);
        Debug.Log("Found "+allThoughts.Length+" thoughts");
    }

    void Update()
    {
        ShowThoughts = (
            GS.interactionMode == InteractionType.Default ||
            GS.interactionMode == InteractionType.Focus
        ) && !GS.isSitting;

        int darkThoughtsVisible = 0;
        foreach (ThoughtBubble thought in allThoughts) {
            if (thought.isActiveAndEnabled == false) continue;
            Transform thoughtTransform = thought.gameObject.transform;

            // Hide all thoughts if disabled
            if (!ShowThoughts)
            {
                thought.FadeOut();
                continue;
            }

            // Filter for focus thoughts
            if (GS.interactionMode == InteractionType.Focus &&
                thought.thoughtType != ThoughtType.Focus)
            {
                thought.FadeOut();
                continue;
            }

            // Filter for intrusive thoughts
            if (GS.interactionMode == InteractionType.Default &&
                GS.bodyBattery > 0 &&
                (
                    thought.thoughtType == ThoughtType.Focus ||
                    (AlwaysShowDarkThoughts == false && thought.thoughtType == ThoughtType.Dark)
                )
            ){
                thought.FadeOut();
                continue;
            }

            // Filter for dark thoughts
            // TODO: reconsider how to differentiate this experience better, if dark thoughts are always showing
            if (GS.interactionMode == InteractionType.Default &&
                GS.bodyBattery == 0 &&
                thought.thoughtType != ThoughtType.Dark)
            {
                thought.FadeOut();
                continue;
            }

            // Check whether thought is within angle of view
            float angleToThought = Vector3.Angle(transform.forward, thoughtTransform.position - transform.position);
            if (angleToThought > ViewAngle) {
                thought.FadeOut();
                continue;
            }

            // Check whether thought is close enough to camera
            float distanceToThought = Vector3.Distance(transform.position, thoughtTransform.position);
            if (distanceToThought > MaxDistance) {
                thought.FadeOut();
                continue;
            }

            // Check whether the thought's date range includes the current day
            if (thought.firstDay > GS.currentDay || thought.lastDay < GS.currentDay)
            {
                thought.FadeOut();
                continue;
            }

            // Check whether thought is obscured by anything else
            RaycastHit hitInfo;
            if (Physics.Linecast(transform.position, thought.transform.position, out hitInfo)
                && hitInfo.transform.name == thought.name) {
                if (thought.bubbleCanvas == null) {
                    // Lazy instantiation of bubble
                    thought.InstantiateBubble(BubbleCanvas);
                }
                thought.FadeIn();
                if (thought.thoughtType == ThoughtType.Dark)
                    darkThoughtsVisible++;
            } else {
                thought.FadeOut();
            }
        }
        if (darkThoughtsVisible > 0 && GS.interactionMode == InteractionType.Default)
        {
            BodyBatteryManager.Main.StartDrainingBattery();
        }
        else
        {
            BodyBatteryManager.Main.StopDrainingBattery();
        }
    }
}