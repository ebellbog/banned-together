using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class FocusManager : MonoBehaviour
{
    [Header("Component references")]
    public StarterAssetsInputs starterInputs;
    public Image focusCursor;
    public ParticleSystem particleEffects;
    public PostProcessVolume postprocessVolume;

    [Space(10)]
    [Header("Resource management")]
    public float focusTimeLimit;
    public float focusRingZoom = 1.5f;
    public float focusRecoveryRate = 1.0f;
    public bool allowSpendingBodyBattery = false;
    public float bodyBatteryDepletionRate;

    [Space(10)]
    [Header("Camera settings")]
    public float maxCameraZoom = 1.25f;
    public List<Camera> additionalCameras;

    [Space(10)]
    [Header("Animation settings")]
    public float maxOutlineWidth = .6f;
    public float hueRotateSpeed = .5f;
    public float focusSpeed = 1.5f;
    public float focusPercent = 0;

    [Space(10)]
    [Header("Miscellaneous")]
    public string defaultCursorName;

    private Camera mainCamera;
    private SelectionOutlineController selectionOutlineController;

    private float initialFOV;
    private float initialOutlineWidth;
    private float initialOutlineHardness;

    private float currentHue;
    private float initialSat;
    private float initialVal;

    private float focusTimeDepleted;
    private float fillAmount;
    private float scaleAmount = 1.0f;
    private bool canFocus = true;
    private int doShowTutorial = 0;

    void Start() {
        selectionOutlineController = GetComponent<SelectionOutlineController>();
        initialOutlineWidth = selectionOutlineController.OutlineWidth;
        initialOutlineHardness = selectionOutlineController.OutlineHardness;

        Color.RGBToHSV(selectionOutlineController.OutlineColor, out currentHue, out initialSat, out initialVal);

        mainCamera = Camera.main;
        initialFOV = mainCamera.fieldOfView;

        particleEffects.Stop();
    }

    void Update()
    {
        bool isFocusing = starterInputs.focus && canFocus && 
            (GS.interactionMode == InteractionType.Default || GS.interactionMode == InteractionType.Focus);
        bool isDefaultCursor = focusCursor.sprite.name == defaultCursorName;  // TODO: don't hard code default sprite name

        if (!starterInputs.focus && doShowTutorial > 0)
        {
            doShowTutorial = 2;
        }
        else if (starterInputs.focus && doShowTutorial == 2)
        {
            YarnDispatcher.StartTutorial("FocusDrained");
            doShowTutorial = -1;
        }

        // Update focus resource
        if (isFocusing)
        {
            focusTimeDepleted += Time.deltaTime;
            focusPercent = Math.Min(focusPercent + focusSpeed * Time.deltaTime, 1);
        }
        else if (focusTimeDepleted > 0)
        {
            if (!starterInputs.focus) focusTimeDepleted = Math.Max(focusTimeDepleted - Time.deltaTime * focusRecoveryRate, 0);
            focusPercent = Math.Max(focusPercent - focusSpeed * Time.deltaTime, 0);

            canFocus = false;
            if (doShowTutorial == 0) doShowTutorial++;
        }
        else
        {
            canFocus = true;
        }

        fillAmount = Math.Max((focusTimeLimit - focusTimeDepleted) / focusTimeLimit, 0);
        focusCursor.fillAmount = isDefaultCursor ? fillAmount : 1.0f;

        if (fillAmount == 0)
        {
            canFocus = false;
            if (doShowTutorial == 0) doShowTutorial++;
        }

        if (fillAmount < 1)
        {
            if (scaleAmount < focusRingZoom)
            {
                scaleAmount = Math.Min(scaleAmount + Time.deltaTime * focusSpeed, focusRingZoom);
            }
        }
        else if (scaleAmount > 1)
        {
            scaleAmount = Math.Max(scaleAmount - Time.deltaTime * focusSpeed, 1);
        }
        focusCursor.transform.localScale = isDefaultCursor ? new UnityEngine.Vector3(scaleAmount, scaleAmount, 1) : new UnityEngine.Vector3(1, 1, 1);

        float cursorAlpha = isDefaultCursor ? 0.4f + 0.6f * (scaleAmount - 1f) / (focusRingZoom - 1f) : 1;
        float cursorGB = starterInputs.focus && !canFocus ? 0 : 1;
        focusCursor.color = new Color(1, cursorGB, cursorGB, cursorAlpha);

        if (isFocusing && GS.interactionMode == InteractionType.Default) { // Enter Focus mode
            particleEffects.Play();
            AudioManager.instance.MuffleMusic();

            GS.interactionMode = InteractionType.Focus;
            selectionOutlineController.OutlineWidth = maxOutlineWidth;
            selectionOutlineController.OutlineHardness = 0;
        }
        else if (focusPercent == 0 && particleEffects.isPlaying) { // Exit Focus mode
            particleEffects.Stop();
            AudioManager.instance.UnmuffleMusic();

            if (GS.interactionMode == InteractionType.Focus) GS.interactionMode = InteractionType.Default;

            selectionOutlineController.OutlineWidth = initialOutlineWidth;
            selectionOutlineController.OutlineHardness = initialOutlineHardness;
        }

        // Update properties based on focus percent
        postprocessVolume.weight = focusPercent;

        if (focusPercent > 0)
        {
            float newFOV = initialFOV / (1 + ((maxCameraZoom - 1) * focusPercent));
            mainCamera.fieldOfView = newFOV; // TODO: (optional) make sure FOV is fully restored 
            foreach(Camera cam in additionalCameras)
            {
                cam.fieldOfView = newFOV;
            }
        }
        // TODO: scale thought bubbles down?

        Color speedlineColor = Color.white;
        speedlineColor.a = focusPercent;

        ParticleSystem.MainModule main = particleEffects.main;
        main.startColor = speedlineColor;

        if (GS.interactionMode == InteractionType.Focus)
        {
            currentHue = (currentHue + Time.deltaTime * hueRotateSpeed) % 1;
            Color newColor = Color.HSVToRGB(currentHue, initialSat, initialVal);
            selectionOutlineController.OutlineColor = newColor;
            selectionOutlineController.OccludedColor = newColor;
        }
    }
}
