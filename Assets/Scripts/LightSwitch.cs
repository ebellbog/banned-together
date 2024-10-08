using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightSwitch : MonoBehaviour
{
    public Light associatedLight;
    public bool onByDefault = false;

    void Awake()
    {
        tag = "Switch";
        associatedLight.intensity = 1;
        OnValidate();
    }

    void OnValidate()
    {
        associatedLight.enabled = onByDefault;
    }

    public void SwitchLight()
    {
        associatedLight.enabled = !associatedLight.enabled;
    }
}
