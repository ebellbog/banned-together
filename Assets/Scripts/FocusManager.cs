using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class FocusManager : MonoBehaviour
{
    public StarterAssetsInputs starterInputs;
    public ParticleSystem particleEffects;
    public PostProcessVolume postprocessVolume;
    public List<Camera> additionalCameras;
    public float maxCameraZoom = 1.25f;
    public float maxOutlineWidth = .6f;
    public float hueRotateSpeed = .5f;
    public float focusSpeed = 1.5f;
    public float focusPercent = 0;

    private Camera mainCamera;

    private SelectionOutlineController selectionOutlineController;
    private float initialFOV;
    private float initialOutlineWidth;
    private float initialOutlineHardness;

    private float currentHue;
    private float initialSat;
    private float initialVal;

    void Start() {
        selectionOutlineController = GetComponent<SelectionOutlineController>();
        initialOutlineWidth = selectionOutlineController.OutlineWidth;
        initialOutlineHardness = selectionOutlineController.OutlineHardness;

        Color.RGBToHSV(selectionOutlineController.OutlineColor, out currentHue, out initialSat, out initialVal);

        mainCamera = GetComponent<Camera>();
        initialFOV = mainCamera.fieldOfView;

        particleEffects.Stop();
    }

    void Update()
    {
        // Update focus percent
        if (starterInputs.focus && (GS.interactionMode == InteractionType.Default || GS.interactionMode == InteractionType.Focus))
        {
            focusPercent = Math.Min(focusPercent + focusSpeed * Time.deltaTime, 1);
        } else
        {
            focusPercent = Math.Max(focusPercent - focusSpeed * Time.deltaTime, 0);
        }

        // Logic for entering & exiting focus mode
        if (focusPercent > 0 && GS.interactionMode == InteractionType.Default) {
            particleEffects.Play();

            GS.interactionMode = InteractionType.Focus;
            selectionOutlineController.OutlineWidth = maxOutlineWidth;
            selectionOutlineController.OutlineHardness = 0;
        }
        else if (focusPercent == 0 && particleEffects.isPlaying) {
            particleEffects.Stop();

            if (GS.interactionMode == InteractionType.Focus) GS.interactionMode = InteractionType.Default;

            selectionOutlineController.OutlineWidth = initialOutlineWidth;
            selectionOutlineController.OutlineHardness = initialOutlineHardness;
        }

        // Update properties based on focus percent
        postprocessVolume.weight = focusPercent;

        float newFOV = initialFOV / (1 + ((maxCameraZoom - 1) * focusPercent));
        mainCamera.fieldOfView = newFOV;
        foreach(Camera cam in additionalCameras)
        {
            cam.fieldOfView = newFOV;
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
