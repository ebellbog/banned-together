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
    private Vector3 elementStartPosition;

    void Update()
    {
        if (isDragging)
        {
            transform.position = Input.mousePosition;

            if (!Input.GetMouseButton(0))
                StopDragging();
        }
    }

    Vector3 GetNormalizedMouse()
    {
        return new Vector3(1920f * Input.mousePosition.x / Screen.width, 1080f * Input.mousePosition.y / Screen.height, 0);
    }

    public void StartDragging(bool fromMousePosition = true) // TODO: either support this parameter or drop it
    {
        isDragging = true;

        if (elementStartPosition == null)
            elementStartPosition = transform.localPosition;

        UI.SetCursor(DraggingCursor);
    }
    public void StopDragging()
    {
        isDragging = false;

        if (OnReleaseDrag != null)
            OnReleaseDrag.Invoke(gameObject);

        ResetCursor();
    }

    public void SnapToStart()
    {
        transform.localPosition = elementStartPosition;
    }

    public void OnHover()
    {
        if (!isDragging)
            UI.SetCursor(SelectionCursor);
    }

    public void ResetCursor()
    {
        if (!isDragging)
        {
            UI.SetCursor(DefaultCursor);
        }
    }
}
