using System;
using System.Collections;
using StarterAssets;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class BodyBatteryManager : MonoBehaviour
{
    [Header("Component references")]
    public StarterAssetsInputs starterInputs;
    public FirstPersonController firstPersonController;
    public Image energizedImage;
    public Animator watchAnimator;
    public PostProcessVolume exhaustionEffects;
    
    [Space(10)]
    [Header("Resource management")]
    [SerializeField]
    private float _energyLevel = 1.0f;
    public float energyLevel
    {
        set {
            GS.bodyBattery = value;
        }
        get {
            return GS.bodyBattery;
        }
    }
    public float exhaustionSpeed = .15f;
    public float restingSpeed = .3f;


    private bool isPulsing = false;
    private bool isAlarming = false;
    private bool isVisible = false;
    private bool isReady = false;
    private bool didShowTutorial = false;
    private float exhaustionEffectLevel = 0;
    private float effectTransitionSpeed = 1.0f;

    void Start()
    {
        _energyLevel = energyLevel;
    }

    void Update()
    {
        energizedImage.fillAmount = GS.bodyBattery;

        if (GS.bodyBattery == 0) {
            exhaustionEffectLevel = Math.Min(exhaustionEffectLevel + effectTransitionSpeed * Time.deltaTime, 1);
        } else {
            exhaustionEffectLevel = Math.Max(exhaustionEffectLevel - effectTransitionSpeed * Time.deltaTime, 0);
        }
        exhaustionEffects.weight = exhaustionEffectLevel;

        // Show & hide watch based on mode, even when not pressing focus
        if (
            GS.interactionMode == InteractionType.Journal ||
            GS.interactionMode == InteractionType.Examine ||
            GS.interactionMode == InteractionType.Tutorial ||
            GS.interactionMode == InteractionType.Paused) {
            // TODO: think through how to handle this in drained state
            HideWatch();
        } else if (GS.bodyBattery < 1) {
            ShowWatch();
        }

        // if (
        //     starterInputs.focus &&
        //     GS.interactionMode == InteractionType.Default &&
        //     GS.bodyBattery == 0 &&
        //     !didShowTutorial
        // ){
        //     starterInputs.anyKey = false;
        //     YarnDispatcher.StartTutorial("EnergyDrained");
        //     didShowTutorial = true;
        // }
        // else if (starterInputs.focus && GS.interactionMode == InteractionType.Focus)
        // {
        //     if (isReady) {
        //         GS.bodyBattery = Math.Max(GS.bodyBattery - exhaustionSpeed * Time.deltaTime, 0);
        //     }

        //     if (GS.bodyBattery == 0)
        //     {
        //         starterInputs.focus = false;
        //         StartDrainedAlarm();
        //     }
        //     else if (!isVisible)
        //     {
        //         ShowWatch();
        //         StartPulsing();
        //         StartCoroutine(SetReadyWithDelay(2.0f));
        //     }
        //     else {
        //         StartPulsing();
        //     }
        // } else
        // {
        //     StopPulsing();
        // }

        if (GS.isSitting)
        {
            StopDrainedAlarm();
            GS.bodyBattery = Math.Min(GS.bodyBattery + restingSpeed * Time.deltaTime, 1);

            if (GS.bodyBattery == 1) {
                if (isReady) {
                    StartCoroutine(HideAfterDelay(1.5f));
                }
                isReady = false;
            }

            didShowTutorial = true; // don't tutorialize if player has already figured it out
        }
    }

    IEnumerator SetReadyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isReady = true;
        yield return null;
    }

    IEnumerator HideAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        HideWatch();
        yield return null;
    }

    private void OnValidate()
    {
        energyLevel = _energyLevel;
    }

    public bool IsReady() {
        return isReady;
    }

    public void StartPulsing()
    {
        if (!isPulsing) {
            watchAnimator.SetTrigger("Pulse");
            isPulsing = true;
        }
    }

    public void StopPulsing()
    {
        if (isPulsing) {
            watchAnimator.SetTrigger("Default");
            isPulsing = false;
        }
    }

    public void StartDrainedAlarm()
    {
        if (isPulsing && !isAlarming) {
            watchAnimator.SetTrigger("Drained");
            AudioManager.instance.SlowMusic();
            firstPersonController.MoveSpeed = 2f;

            isPulsing = false;
            isAlarming = true;
        }
    }

    public void StopDrainedAlarm()
    {
        if (isAlarming) {
            watchAnimator.SetTrigger("Default");
            AudioManager.instance.ResetMusicEffects();
            firstPersonController.MoveSpeed = 4f;

            isAlarming = false;
        }
    }

    public void ShowWatch()
    {
        if (!isVisible)
        {
            watchAnimator.SetTrigger("Show");
            isVisible = true;
        }
    }

    public void HideWatch()
    {
        if (isVisible)
        {
            watchAnimator.SetTrigger("Hide");
            isVisible = false;
        }
    }
}
