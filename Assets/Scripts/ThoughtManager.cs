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
            Collider targetCollider = targetObject.GetComponent<Collider>(); 
            Vector3 size = targetCollider.bounds.size;
            Vector3 position = targetCollider.bounds.center;
    
            thought.bubbleCanvas = Instantiate(
                BubbleCanvas,
                new Vector3 (position.x + size.x / 2, position.y + size.y / 2 - 0.2f, position.z),
                Quaternion.identity
            );
            TextMeshProUGUI bubbleText = thought.bubbleCanvas.GetComponentInChildren<TextMeshProUGUI>();
            bubbleText.SetText(thought.ThoughtText);
        }
    }
    void OnTriggerExit(Collider collision) {
        ThoughtBubble thought = collision.gameObject.GetComponent<ThoughtBubble>();
        if (thought?.bubbleCanvas != null) {
            Destroy(thought.bubbleCanvas);
            thought.bubbleCanvas = null;
        }
    }
}
