using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThoughtManager : MonoBehaviour
{
    public GameObject BubbleCanvas;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collision) {
        GameObject targetObject = collision.gameObject;
        ThoughtBubble thought = targetObject.GetComponent<ThoughtBubble>();

        if (thought != null && thought.bubbleCanvas == null) {
            thought.InstantiateBubble(BubbleCanvas);
        }
        thought.FadeIn();
    }

    void OnTriggerExit(Collider collision) {
        ThoughtBubble thought = collision.gameObject.GetComponent<ThoughtBubble>();
        if (thought?.bubbleCanvas != null) {
            thought.FadeOut();
        }
    }
}
