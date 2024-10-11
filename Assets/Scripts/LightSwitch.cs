using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightSwitch : MonoBehaviour
{
    public Light associatedLight;
    public bool onByDefault = false;
    public float lightIntensity = 1.0f;

    void Awake()
    {
        tag = "Switch";
        OnValidate();
    }

    void OnValidate()
    {
        associatedLight.enabled = onByDefault;
        associatedLight.intensity = lightIntensity;
    }

    public void SwitchLight()
    {
        associatedLight.enabled = !associatedLight.enabled;
    }
}
