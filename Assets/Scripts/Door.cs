using System;
using System.Reflection;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator animator;
    public string soundEffect;
    public bool isButton;
    public bool isLocked;
    public string statePropertyName;

    private bool isOpen;

    void Start()
    {
        gameObject.tag = "Door";
    }

    public void Open()
    {
        if (isOpen) return;

        if (!isLocked)
        {
            animator.SetTrigger("Open");

            gameObject.tag = "Untagged";
            isOpen = true;

            if (isButton)
                gameObject.GetComponent<Animator>().SetTrigger("Press");
        }        

        if (soundEffect != null)
            AudioManager.instance.PlaySFX(soundEffect);

        if (statePropertyName != null && statePropertyName.Length > 0)
        { 
            Type staticClassType = typeof(GS); 
            FieldInfo fieldInfo = staticClassType.GetField(statePropertyName, BindingFlags.Static | BindingFlags.Public);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(null, 1);
                Debug.Log($"Set property {statePropertyName} of game state to 1");
            }
            else
            {
                Debug.Log($"Couldn't find property {statePropertyName} on game state");
            }
        }
        
    }
}
