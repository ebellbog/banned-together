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
    public Image backgroundImage;
    public Sprite defaultSprite;
    public Sprite drainedSprite;
    public Animator watchAnimator;
    public Animator goToBedAnimator;
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

    public float sittingRecoverySpeed = .08f;
    public float maxSittingRecovery = .4f;
    private float remainingSittingRecovery;

    public float fidgetRecoverySpeed = .04f;
    public float maxFidgetRecovery = .4f;
    private float remainingFidgetRecovery;

    public static BodyBatteryManager Main { get; private set; }

    private bool isDraining = false;
    private bool isPulsing = false;
    private bool isAlarming = false;
    private bool isVisible = false;
    private bool isReady = false;
    private bool isShowingBed = false;

    private float exhaustionEffectLevel = 0;
    private float effectTransitionSpeed = 1.0f;

    void Awake()
    {
        Main = this;
    }

    void Start()
    {
        _energyLevel = energyLevel;
        remainingSittingRecovery = maxSittingRecovery;
        remainingFidgetRecovery = maxFidgetRecovery;
    }

    public void FidgetToRecover()
    {
        if (energyLevel < 1 && remainingFidgetRecovery > 0)
        {
            energyLevel = Math.Min(energyLevel + fidgetRecoverySpeed * Time.deltaTime, 1.0f);
            remainingFidgetRecovery -= fidgetRecoverySpeed * Time.deltaTime;
        }
    }

    void Update()
    {
        energizedImage.fillAmount = GS.bodyBattery;

        if (isDraining && isReady)
        {
            energyLevel = Math.Max(energyLevel - exhaustionSpeed * Time.deltaTime, 0);
        } else if (GS.isSitting && energyLevel < 1 && remainingSittingRecovery > 0)
        {
            energyLevel = Math.Min(energyLevel + sittingRecoverySpeed * Time.deltaTime, 1.0f);
            remainingSittingRecovery -= sittingRecoverySpeed * Time.deltaTime;
        }

        // Show & hide watch based on mode, even when not pressing focus
        if (
            GS.interactionMode == InteractionType.Paused ||
            GS.interactionMode == InteractionType.Monologue ||
            GS.interactionMode == InteractionType.Tutorial)
        {
            StopPulsing();
            isAlarming = false; // TODO: handle these transitions better
            HideWatch();
        }
        else if (isDraining || energyLevel < 1) {
            ShowWatch();
            if (energyLevel < .8) YarnDispatcher.StartTutorial("EnergyDraining");
        }
        else if (energyLevel >= 1)
        {
            HideWatch();
        }

        // Animate postprocessing effect
        if (energyLevel == 0) {
            StartDrainedAlarm();
            if (GS.interactionMode == InteractionType.Default)
            {
                ShowGoToBed();
            } else
            {
                HideGoToBed();
            }
            exhaustionEffectLevel = Math.Min(exhaustionEffectLevel + effectTransitionSpeed * Time.deltaTime, 1);
        } else {
            exhaustionEffectLevel = Math.Max(exhaustionEffectLevel - effectTransitionSpeed * Time.deltaTime, 0);
            StopDrainedAlarm();
            HideGoToBed();
        }
        exhaustionEffects.weight = exhaustionEffectLevel;

        // Go to sleep
        if (starterInputs.sleep)
        {
            if (isShowingBed)
            {
                LevelLoader.current.StartNextDay();
            }
            else
            {
                Debug.Log("Not time to sleep yet!");
            }

            starterInputs.sleep = false;
            starterInputs.anyKey = false;
        }
    }

    public void StartDrainingBattery()
    {
        if (!isDraining)
        {
            isDraining = true;
            if (energyLevel > 0) StartPulsing();
            StartCoroutine(SetReadyWithDelay(1.5f));
        }
    }
    public void StopDrainingBattery()
    {
        if (isDraining)
        {
            StopPulsing();
            isDraining = false;
            isReady = false;
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

    private void StartPulsing()
    {
        if (!isPulsing) {
            watchAnimator.SetTrigger("Pulse");
            isPulsing = true;
        }
    }
    private void StopPulsing()
    {
        if (isPulsing) {
            watchAnimator.SetTrigger("Default");
            isPulsing = false;
        }
    }

    private void ShowGoToBed()
    {
        if (isShowingBed) return;
        goToBedAnimator.SetTrigger("Show");
        isShowingBed = true;
    }
    private void HideGoToBed()
    {
        if (!isShowingBed) return;
        goToBedAnimator.SetTrigger("Hide");
        isShowingBed = false;
    }

    private void StartDrainedAlarm()
    {
        if (!isAlarming) {
            backgroundImage.sprite = drainedSprite;
            watchAnimator.SetTrigger("Drained");
            AudioManager.instance.SlowMusic();
            firstPersonController.MoveSpeed = 2f;

            isPulsing = false;
            isAlarming = true;
        }
    }
    private void StopDrainedAlarm()
    {
        if (isAlarming) {
            backgroundImage.sprite = defaultSprite;
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
