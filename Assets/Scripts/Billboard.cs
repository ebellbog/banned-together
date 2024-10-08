using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Billboard : MonoBehaviour
{
    public bool DynamicallyScale = true;
    public float ScaleReferenceDistance = 5.0f;

    private Vector2 startingScale;
    private Camera mainCamera;
    private GameObject player;

    void Start()
    {
        mainCamera = Camera.main;
        startingScale = transform.localScale;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void LateUpdate()
    {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);

        if (DynamicallyScale) {
            float currentDistance = Vector3.Distance(player.transform.position, transform.position); 
            transform.localScale = startingScale * (currentDistance / ScaleReferenceDistance);
        }
    }
}