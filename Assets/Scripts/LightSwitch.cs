using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightSwitch : MonoBehaviour
{
    public Light associatedLight;
    public bool onByDefault = false;

    void Awake()
    {
        tag = "Switch";
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
