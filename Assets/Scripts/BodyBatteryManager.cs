using System;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class BodyBatteryManager : MonoBehaviour
{
    public StarterAssetsInputs starterInputs;
    public Image energizedImage;
    public Animator watchAnimator;
    public float exhaustionSpeed = .15f;
    public float restingSpeed = .3f;

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

    private bool isPulsing = false;
    private bool isAlarming = false;
    private bool showTutorial = false;
    private bool didShowTutorial = false;

    void Start()
    {
        _energyLevel = energyLevel;
    }

    void Update()
    {
        energizedImage.fillAmount = GS.bodyBattery;

        if (starterInputs.focus)
        {
            GS.bodyBattery = Math.Max(GS.bodyBattery - exhaustionSpeed * Time.deltaTime, 0);
            if (GS.bodyBattery == 0)
            {
                starterInputs.focus = false;
                StartDrainedAlarm();

                // Show tutorial the next time the player tries focusing
                if (!didShowTutorial) {
                    if (showTutorial) {
                        YarnDispatcher.StartTutorial("EnergyDrained");
                        didShowTutorial = true;
                    }
                    showTutorial = true;
                }
            } else
            {
                StartPulsing();
            }
        } else
        {
            StopPulsing();
        }

        if (GS.isSitting)
        {
            StopDrainedAlarm();
            GS.bodyBattery = Math.Min(GS.bodyBattery + restingSpeed * Time.deltaTime, 1);
        }
    }

    private void OnValidate()
    {
        energyLevel = _energyLevel;
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
            isPulsing = false;
            isAlarming = true;
        }
    }

    public void StopDrainedAlarm()
    {
        if (isAlarming) {
            watchAnimator.SetTrigger("Default");
            isAlarming = false;
        }
    }
}
