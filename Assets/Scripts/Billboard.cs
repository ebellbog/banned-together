using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Billboard : MonoBehaviour
{
    public bool DynamicallyScale = true;
    public float ScaleReferenceDistance = 5.0f;

    private Camera mainCamera;
    private Vector2 startingScale;

    void Start()
    {
        mainCamera = Camera.main;
        startingScale = transform.localScale;
    }

    void LateUpdate()
    {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);

        if (DynamicallyScale) {
            float currentDistance = Vector3.Distance(mainCamera.transform.position, transform.position);
            transform.localScale = startingScale * (currentDistance / ScaleReferenceDistance);
        }
    }
}