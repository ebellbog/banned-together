using System;
using System.Linq;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class FocusManager : MonoBehaviour
{
    [Header("Component references")]
    public StarterAssetsInputs starterInputs;
    public Image focusCursor;
    public ParticleSystem particleEffects;
    public Volume postprocessVolume;
    public ScreenSpaceOutlines outlineManager;
    public ItemInteraction interactionManager;
    // public SelectionOutlineController selectionOutlineController;

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

    private float initialFOV;
    // private float initialOutlineWidth;
    // private float initialOutlineHardness;

    private float currentHue = 0;
    private float focusTimeDepleted;
    private float fillAmount;
    private float scaleAmount = 1.0f;
    private bool canFocus = true;
    private int doShowTutorial = 0;

    private List<InteractableItem> allFocusable;
    private bool isShowingOutlines = false;
    private string currentFocusWords = "";
    private int focusLayerIdx = -1;

    // private List<Light> allSpotLights;
    // private bool didTurnOffShadows = false;

    void Start()
    {
        particleEffects.Stop();
        allFocusable = FindObjectsByType<InteractableItem>(FindObjectsSortMode.None)
            .Where(item => item.highlightOnFocus).ToList();

        focusLayerIdx = LayerMask.NameToLayer("No post");
    }

    void OldStart() {
        // initialOutlineWidth = selectionOutlineController.OutlineWidth;
        // initialOutlineHardness = selectionOutlineController.OutlineHardness;

        // Color.RGBToHSV(selectionOutlineController.OutlineColor, out currentHue, out initialSat, out initialVal);

        // mainCamera = Camera.main;
        // initialFOV = mainCamera.fieldOfView;

        // particleEffects.Stop();

        // allFocusable = FindObjectsByType<InteractableItem>(FindObjectsSortMode.None)
        //     .Where(item => item.isFocusable)
        //     .Select(item => item.gameObject).ToList();

        // allSpotLights = FindObjectsByType<Light>(FindObjectsSortMode.None)
        //     .Where(light => light.type == LightType.Spot).ToList();
        // Debug.Log($"Number of spotlights found: {allSpotLights.Count}");
    }

    public bool HasActiveFocus()
    {
        return GS.redStickerPlacement.associatedJournalEntry != null;
    }

    void Update()
    {
        // Clear stickers
        // TODO: support multiple stickers, possibly with a confirmation modal
        if (starterInputs.delete)
        {
            GS.redStickerPlacement = new Sticker();
            starterInputs.delete = false;
        }

        string gsFocusWords = HasActiveFocus() ? GS.redStickerPlacement.associatedJournalEntry.focusWords : "";

        if ((GS.interactionMode == InteractionType.Default || GS.interactionMode == InteractionType.Monologue) && HasActiveFocus())
        {
            postprocessVolume.weight = Mathf.Min(postprocessVolume.weight + Time.deltaTime * focusSpeed, 1f);
        }
        else
        {
            postprocessVolume.weight = Mathf.Max(postprocessVolume.weight - Time.deltaTime * focusSpeed, 0);
        }

        bool modeAllowsFocus = 
            GS.interactionMode == InteractionType.Default ||
            GS.interactionMode == InteractionType.Focus ||
            GS.interactionMode == InteractionType.Monologue;

        if (currentFocusWords != gsFocusWords || canFocus != modeAllowsFocus)
        {
            foreach(InteractableItem interactableItem in allFocusable)
            {
                bool doHighlight = modeAllowsFocus && HasActiveFocus() && interactableItem.highlightOnFocus && interactableItem.MatchesCurrentFocus();
                SetLayer(interactableItem.gameObject, doHighlight ? focusLayerIdx : 0);
                if (doHighlight)
                    interactionManager.ApplyOutline(interactableItem.gameObject, Color.red, Outline.Mode.OutlineVisible);
                else
                    interactionManager.RemoveOutline(interactableItem.gameObject);
            }
            currentFocusWords = gsFocusWords;
            canFocus = modeAllowsFocus;
        }
    }

    void OldUpdate()
    {
        // // TODO: remove old focus functionality properly
        // starterInputs.focus = false; 
        // bool isFocusing = starterInputs.focus && canFocus && 
        //     (allowSpendingBodyBattery || GS.bodyBattery > 0) &&
        //     (
        //         GS.interactionMode == InteractionType.Default ||
        //         GS.interactionMode == InteractionType.Focus ||
        //         GS.interactionMode == InteractionType.Monologue
        //     );

        // bool isDefaultCursor = focusCursor.sprite.name == defaultCursorName;

        // if (!starterInputs.focus && doShowTutorial > 0)
        // {
        //     doShowTutorial = 2;
        // }
        // if (starterInputs.focus && doShowTutorial == 2)
        // {
        //     YarnDispatcher.StartTutorial("FocusDrained");
        //     doShowTutorial = -1;
        // }

        // // Update focus resource
        // if (isFocusing)
        // {
        //     focusTimeDepleted += Time.deltaTime;
        //     focusPercent = Math.Min(focusPercent + focusSpeed * Time.deltaTime, 1);
        // }
        // else if (focusTimeDepleted > 0)
        // {
        //     if (!starterInputs.focus) focusTimeDepleted = Math.Max(focusTimeDepleted - Time.deltaTime * focusRecoveryRate, 0);
        //     focusPercent = Math.Max(focusPercent - focusSpeed * Time.deltaTime, 0);

        //     canFocus = false; // Can't focus while focus is recharging
        //     if (doShowTutorial == 0) doShowTutorial++;
        // }
        // else
        // {
        //     canFocus = true;
        // }

        // if (isFocusing && !isShowingOutlines)
        // {
        //     int layerIdx = LayerMask.NameToLayer("No post");
        //     foreach(GameObject focusableObject in allFocusable)
        //     {
        //         SetLayer(focusableObject, layerIdx);
        //     }
        //     isShowingOutlines = true;
        // }
        // else if (!isFocusing && focusPercent == 0 && isShowingOutlines)
        // {
        //     foreach(GameObject focusableObject in allFocusable)
        //     {
        //         SetLayer(focusableObject, 0); // TODO: preserve previous layer?
        //     }
        //     isShowingOutlines = false;
        // }
        // // // Prevent graphic glitch with bloom effect and soft shadows
        // // if (isFocusing && !didTurnOffShadows)
        // // {
        // //     foreach(Light spotlight in allSpotLights)
        // //     {
        // //         spotlight.shadows = LightShadows.Hard;
        // //     }
        // //     didTurnOffShadows = true;
        // // }
        // // else if (!isFocusing && focusPercent == 0 && didTurnOffShadows)
        // // {
        // //     foreach(Light spotlight in allSpotLights)
        // //     {
        // //         spotlight.shadows = LightShadows.Soft;
        // //     }
        // //     didTurnOffShadows = false;
        // // }

        // fillAmount = Math.Max((focusTimeLimit - focusTimeDepleted) / focusTimeLimit, 0);
        // focusCursor.fillAmount = isDefaultCursor ? fillAmount : 1.0f;

        // if (fillAmount == 0)
        // {
        //     canFocus = false;
        //     if (doShowTutorial == 0) doShowTutorial++;
        // }

        // if (fillAmount < 1)
        // {
        //     if (scaleAmount < focusRingZoom)
        //     {
        //         scaleAmount = Math.Min(scaleAmount + Time.deltaTime * focusSpeed, focusRingZoom);
        //     }
        // }
        // else if (scaleAmount > 1)
        // {
        //     scaleAmount = Math.Max(scaleAmount - Time.deltaTime * focusSpeed, 1);
        //     if (doShowTutorial != -1) doShowTutorial = 0;
        // }
        // focusCursor.transform.localScale = isDefaultCursor ? new Vector3(scaleAmount, scaleAmount, 1) : new Vector3(1, 1, 1);

        // float cursorAlpha = isDefaultCursor ? 0.4f + 0.6f * (scaleAmount - 1f) / (focusRingZoom - 1f) : 1;
        // float cursorGB = starterInputs.focus && !canFocus ? 0 : 1;
        // focusCursor.color = new Color(1, cursorGB, cursorGB, cursorAlpha);

        // if (isFocusing && (GS.interactionMode == InteractionType.Default || GS.interactionMode == InteractionType.Monologue)) { // Enter Focus mode
        //     particleEffects.Play();
        //     AudioManager.instance.MuffleMusic();

        //     // selectionOutlineController.OutlineWidth = maxOutlineWidth;
        //     // selectionOutlineController.OutlineHardness = 0;
        //     // selectionOutlineController.OutlineType = SelectionOutlineController.OutlineMode.ColorizeOccluded;
        //     // selectionOutlineController.UpdateOutlineType();

        //     GS.interactionMode = InteractionType.Focus;
        // }
        // else if (focusPercent == 0 && particleEffects.isPlaying) { // Exit Focus mode
        //     particleEffects.Stop();
        //     AudioManager.instance.UnmuffleMusic();

        //     if (GS.interactionMode == InteractionType.Focus) GS.interactionMode = InteractionType.Default;

        //     // selectionOutlineController.OutlineWidth = initialOutlineWidth;
        //     // selectionOutlineController.OutlineHardness = initialOutlineHardness;
        //     // selectionOutlineController.OutlineType = SelectionOutlineController.OutlineMode.OnlyVisible;
        //     // selectionOutlineController.UpdateOutlineType();
        // }

        // // Update properties based on focus percent

        // if (focusPercent > 0)
        // {
        //     float newFOV = initialFOV / (1 + ((maxCameraZoom - 1) * focusPercent));
        //     mainCamera.fieldOfView = newFOV; // TODO: (optional) make sure FOV is fully restored
        //     foreach(Camera cam in additionalCameras)
        //     {
        //         cam.fieldOfView = newFOV;
        //     }
        // }
        // // TODO: scale thought bubbles down?

        // Color speedlineColor = Color.white;
        // speedlineColor.a = focusPercent;

        // ParticleSystem.MainModule main = particleEffects.main;
        // main.startColor = speedlineColor;

        // if (GS.interactionMode == InteractionType.Focus)
        // {
        //     currentHue = (currentHue + Time.deltaTime * hueRotateSpeed) % 1;
        //     Color newColor = Color.HSVToRGB(currentHue, 1, 1);
        //     outlineManager.SetOutlineColor(newColor);
        // }
    }

    private void SetLayer(GameObject targetObject, int layerIdx = 0)
    {
        targetObject.layer = layerIdx;
        foreach (Transform child in targetObject.transform)
        {
            child.gameObject.layer = layerIdx;
        }
    }

    public void SetFocused(GameObject gameObject)
    {
        SetLayer(gameObject, focusLayerIdx);
    }
    public void ClearFocused(GameObject gameObject)
    {
        SetLayer(gameObject, 0);
    }
}
