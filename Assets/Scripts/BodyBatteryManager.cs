using System;
using System.Collections;
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
    private bool isVisible = false;
    private bool isReady = false;
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
            if (isReady) {
                GS.bodyBattery = Math.Max(GS.bodyBattery - exhaustionSpeed * Time.deltaTime, 0);
            }

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
            }
            else if (!isVisible)
            {
                ShowWatch();
                StartPulsing();
                StartCoroutine(SetReadyWithDelay(2.0f));
            }
            else {
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
