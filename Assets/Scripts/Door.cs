using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator animator;
    public string soundEffect;
    public bool isButton;

    private bool isOpen;

    void Start()
    {
        gameObject.tag = "Door";
    }

    public void Open()
    {
        if (isOpen) return;
        animator.SetTrigger("Open");
        
        if (isButton)
            gameObject.GetComponent<Animator>().SetTrigger("Press");

        if (soundEffect != null)
            AudioManager.instance.PlaySFX(soundEffect);

        gameObject.tag = "Untagged";
        isOpen = true;
    }
}
