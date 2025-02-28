using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableElement : MonoBehaviour
{
    public bool snapBackToStart = true;
    [Header("Cursors")]
    public Texture2D selectionCursor;
    public Texture2D draggingCursor;
    public Texture2D defaultCursor;

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
        if (isDragging)
        {
            Vector3 currentPosition = Input.mousePosition;
            transform.localPosition += currentPosition - mouseStartPosition;
            mouseStartPosition = currentPosition;
        }
    }

    public void StartDragging()
    {
        isDragging = true;
        mouseStartPosition = Input.mousePosition;
        elementStartPosition = transform.localPosition;

        UI.SetCursor(draggingCursor);
    }
    public void StopDragging()
    {
        isDragging = false;
        if (snapBackToStart)
        {
            transform.localPosition = elementStartPosition;
        }

        ResetCursor();
    }

    public void OnHover()
    {
        UI.SetCursor(selectionCursor);
    }

    public void ResetCursor()
    {
        UI.SetCursor(defaultCursor);
    }
}
