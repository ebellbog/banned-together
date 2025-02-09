using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightSwitch : MonoBehaviour
{
    public Light[] associatedLight;
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
        for (int i = 0; i < associatedLight.Length; i++)
        {
            associatedLight[i].enabled = onByDefault;
            //associatedLight[i].intensity = lightIntensity;
        }

    }

    public void SwitchLight()
    {
        for (int i = 0; i < associatedLight.Length; i++)
        {
            associatedLight[i].enabled = !associatedLight[i].enabled;
        }

        if (soundEffect != null)
            AudioManager.instance.PlaySFX(soundEffect, gameObject.transform.position);


    }
}
