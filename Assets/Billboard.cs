using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Billboard : MonoBehaviour
{
    public bool DynamicallyScale = false;
    
    private Vector2 startingScale;
    private float startingDistance;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        startingScale = transform.localScale;
        startingDistance = Vector3.Distance(mainCamera.transform.position, transform.position);
    }

    void LateUpdate()
    {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);

        if (DynamicallyScale) {
            float currentDistance = Vector3.Distance(mainCamera.transform.position, transform.position); 
            transform.localScale = startingScale * (currentDistance / startingDistance);
        }
    }
}