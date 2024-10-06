using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator animator; // For buttons, set this to the associated door's animator
    public string soundEffect;
    public string closeSoundEffect;
    public bool isButton;
    public bool isLocked;
    public string statePropertyName;
    public bool useSpatialAudio = true;
    public List<Door> connectedDoors;
    public bool closeable;

    [HideInInspector] public bool isOpen;

    void Start()
    {
        gameObject.tag = "Door";
    }

    public void Open()
    {
        if (isOpen) { 
            if (closeable) {
                animator.SetTrigger("Close");
                if (closeSoundEffect != null)
                    AudioManager.instance.PlaySFX(closeSoundEffect, useSpatialAudio && animator ? animator.gameObject.transform.position : null);
                isOpen = false;
                return;
            }
        }

        if (!isLocked)
        {
            animator.SetTrigger("Open");
            isOpen = true;

            if (!closeable)
            {
                gameObject.tag = "Untagged";
            }

            if (isButton)
                gameObject.GetComponent<Animator>().SetTrigger("Press");
        }

        if (soundEffect != null)
            AudioManager.instance.PlaySFX(soundEffect, useSpatialAudio && animator ? animator.gameObject.transform.position : null);

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

        if (connectedDoors != null)
        {
            foreach(Door door in connectedDoors)
            {
                door.Open();
            }
        }
    }
}
