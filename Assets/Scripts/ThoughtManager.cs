using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThoughtManager : MonoBehaviour
{
    public GameObject BubbleCanvas;
    private ThoughtBubble[] allThoughts;

    // Start is called before the first frame update
    void Start()
    {
        allThoughts = (ThoughtBubble [])Object.FindObjectsByType(typeof(ThoughtBubble), FindObjectsSortMode.None);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject firstThought = allThoughts[0].gameObject;
        float angleToThought = Vector3.Angle(Camera.main.transform.forward, firstThought.transform.position - Camera.main.transform.position);
        Debug.Log("Angle to first thought: "+angleToThought); 
    }

    void OnTriggerEnter(Collider collision) {
        GameObject targetObject = collision.gameObject;

		RaycastHit hitInfo;
        if (Physics.Linecast(Camera.main.transform.position, targetObject.transform.position, out hitInfo)) {
            if (hitInfo.transform.name != targetObject.name) return;

            ThoughtBubble thought = targetObject.GetComponent<ThoughtBubble>();

            if (thought == null) return;
            if (thought.bubbleCanvas == null) {
                thought.InstantiateBubble(BubbleCanvas);
            }
            thought.FadeIn();
        }
    }

    void OnTriggerExit(Collider collision) {
        ThoughtBubble thought = collision.gameObject.GetComponent<ThoughtBubble>();
        if (thought?.bubbleCanvas != null) {
            thought.FadeOut();
        }
    }
}
