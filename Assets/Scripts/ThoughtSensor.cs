using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThoughtSensor : MonoBehaviour
{
    public GameObject BubbleCanvas;
    public float MaxDistance  = 12.0f;
    public float ViewAngle = 16.0f;
    public bool ShowThoughts = true;

    private ThoughtBubble[] allThoughts;

    void Start()
    {
        allThoughts = Object.FindObjectsByType<ThoughtBubble>(FindObjectsSortMode.None);
        Debug.Log("Found "+allThoughts.Length+" thoughts");
    }

    void Update()
    {
        ShowThoughts = GS.interactionMode == InteractionType.Default && !GS.isSitting;

        foreach (ThoughtBubble thought in allThoughts) {
            if (thought.isActiveAndEnabled == false) continue;
            Transform thoughtTransform = thought.gameObject.transform;

            // Hide all thoughts if disabled
            if (!ShowThoughts) {
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

            // Check whether thought is obscured by anything else
            RaycastHit hitInfo;
            if (Physics.Linecast(transform.position, thought.transform.position, out hitInfo)
                && hitInfo.transform.name == thought.name) {
                if (thought.bubbleCanvas == null) {
                    // Lazy instantiation of bubble
                    thought.InstantiateBubble(BubbleCanvas);
                }
                thought.FadeIn();
            } else {
                thought.FadeOut();
            }
        }
    }
}