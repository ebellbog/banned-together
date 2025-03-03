using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DraggableElement : MonoBehaviour
{
    public bool snapBackToStart = true;
    [Header("Cursors")]
    public Texture2D SelectionCursor;
    public Texture2D DraggingCursor;
    public Texture2D DefaultCursor;

    [Header("Events")]
    public UnityEvent<GameObject> OnReleaseDrag;

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

        UI.SetCursor(DraggingCursor);
    }
    public void StopDragging()
    {
        isDragging = false;

        if (snapBackToStart)
        {
            transform.localPosition = elementStartPosition;
        }

        if (OnReleaseDrag != null)
            OnReleaseDrag.Invoke(gameObject);

        ResetCursor();
    }

    public void OnHover()
    {
        if (!isDragging)
            UI.SetCursor(SelectionCursor);
    }

    public void ResetCursor()
    {
        if (!isDragging)
            UI.SetCursor(DefaultCursor);
    }
}
