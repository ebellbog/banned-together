using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamingoDoor : MonoBehaviour
{

    public Animator animator;
    public bool useSpatialAudio = true;
    public string soundEffect;

    public bool button1 = false;
    public bool button2 = false;

    // Update is called once per frame
    void Update()
    {
        if (button1 == true && button2 == true)
        {
            animator.SetTrigger("Open");
            AudioManager.instance.PlaySFX(soundEffect, useSpatialAudio && animator ? animator.gameObject.transform.position : null);

        }
    }

    public void ActivateButton1()
    {
        button1 = true;
    }

    public void ActivateButton2()
    {
        button2 = true;
    }
}
