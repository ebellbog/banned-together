using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableElement : MonoBehaviour
{
    public bool snapBackToStart = true;
    private bool isDragging = false;
    private Vector3 mouseStartPosition;
    private Vector3 elementStartPosition;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartDragging()
    {
        isDragging = true;
        mouseStartPosition = Input.mousePosition;
        elementStartPosition = transform.localPosition;
    }
    public void StopDragging()
    {
        isDragging = false;
        if (snapBackToStart)
        {
            transform.localPosition = elementStartPosition;
        }
    }

    public void OnMouseMove()
    {
        Vector3 currentPosition = Input.mousePosition;
        Debug.Log(currentPosition);
        transform.localPosition += currentPosition - mouseStartPosition;
        mouseStartPosition = currentPosition;
    }
}
