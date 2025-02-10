using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class LightSwitch : MonoBehaviour
{
    public List<Light> associatedLights;
    public List<GameObject> associatedDecals;
    public bool onByDefault = false;
    //public float lightIntensity = 1.0f;
    public string soundEffect;


    void Awake()
    {
        tag = "Switch";

        OnValidate();
    }

    void OnValidate()
    {
        foreach (Light associatedLight in associatedLights)
        {
            associatedLight.enabled = onByDefault;
            //associatedLight[i].intensity = lightIntensity;
        }
        foreach (GameObject decal in associatedDecals)
        {
            decal.SetActive(onByDefault);
        }
    }

    public void SwitchLight()
    {
        foreach (Light associatedLight in associatedLights)
        {
            associatedLight.enabled = !associatedLight.enabled;
        }
        foreach (GameObject decal in associatedDecals)
        {
            decal.SetActive(!decal.activeSelf);
        }

        if (soundEffect != null)
            AudioManager.instance.PlaySFX(soundEffect, gameObject.transform.position);
    }
}
