using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomManager : MonoBehaviour
{
    public float[] zoomSteps = new float[] {0.5f, 1.0f, 1.5f};
    public int currentStep = 1;
    public float zoomSpeed = 35.0f;
    private float originalZoomSpeed;

    private Camera thisCamera;
    private float initialFOV;
    private int initialZoomStep;

    public static ZoomManager Main { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        thisCamera = gameObject.GetComponent<Camera>();
        initialFOV = thisCamera.fieldOfView;
        initialZoomStep = currentStep;
        thisCamera.fieldOfView = initialFOV * zoomSteps[currentStep];
    }

    void Awake()
    {
        if (!Main) Main = this;
        originalZoomSpeed = zoomSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        float targetFOV = initialFOV * zoomSteps[currentStep];
        float currentFOV = thisCamera.fieldOfView;

        if (currentFOV > targetFOV)
        {
            thisCamera.fieldOfView  = Math.Max(currentFOV - Time.deltaTime * zoomSpeed, targetFOV);
        }
        else if (currentFOV < targetFOV)
        {
           thisCamera.fieldOfView = Math.Min(currentFOV + Time.deltaTime * zoomSpeed, targetFOV); 
        }
    }

    public void ZoomOut()
    {
        EventSystem.current.SetSelectedGameObject(null);
        zoomSpeed = originalZoomSpeed;
        if (currentStep < zoomSteps.Length - 1) currentStep++;
    }
    public void ZoomIn()
    {
        EventSystem.current.SetSelectedGameObject(null);
        zoomSpeed = originalZoomSpeed;
        if (currentStep > 0) currentStep--;
    }

    public static float GetZoom()
    {
        return ZoomManager.Main.zoomSteps[ZoomManager.Main.currentStep];
    }
    public static void ResetZoom()
    {
        ZoomManager.Main.zoomSpeed = ZoomManager.Main.originalZoomSpeed * 2.5f; // TODO: more precise value
        ZoomManager.Main.currentStep = ZoomManager.Main.initialZoomStep;
    }
}
