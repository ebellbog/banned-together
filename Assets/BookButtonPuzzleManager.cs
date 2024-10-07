using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookButtonPuzzleManager : MonoBehaviour
{

    public GameObject[] bookButtons;
    private string bookMasterIndex;
    private string safeMasterIndex;
    private string booksPressed;
    private int buttonCount;
    public GameObject doorToOpen;
    public string soundEffect;
    public string resetSoundEffect;
    public bool useSpatialAudio = true;
    public Animator secretDoorAnimator;
    public Animator safeAnimator;

    void Start()
    {
        buttonCount = 0;
        bookMasterIndex = "ABCDE";
        safeMasterIndex = "CBDEA";
    }

    // Update is called once per frame
    void Update()
    {

        if (bookMasterIndex == booksPressed)
        {
            Debug.Log("Correct");
            StartCoroutine(OpenSecretDoor());
            booksPressed = "";
        }
        if (safeMasterIndex == booksPressed)
        {

        }
        else
        {
            if (buttonCount == 5 && bookMasterIndex != booksPressed) {
                buttonCount = 0;
                StartCoroutine(Reset());

            }
        }
    }

    public void Pressed(string bookIndex)
    {  
        booksPressed += bookIndex;
        Debug.Log("Bookspressed: " + booksPressed);
        buttonCount++;
    }

    IEnumerator OpenSecretDoor()
    {

        yield return new WaitForSeconds(1.12f);

        secretDoorAnimator.SetTrigger("Open");

        //Not working?
        if (soundEffect != null)
        {
            AudioManager.instance.PlaySFX(soundEffect);
            Debug.Log(soundEffect);
        }

        StartCoroutine(Reset());
    }

    IEnumerator OpenSafe()
    {
        yield return new WaitForSeconds(1.12f);
        safeAnimator.SetTrigger("Open");

        if (soundEffect != null)
            AudioManager.instance.PlaySFX(soundEffect, useSpatialAudio && secretDoorAnimator ? secretDoorAnimator.gameObject.transform.position : null);

        StartCoroutine(Reset());
    }

        IEnumerator Reset()
    {
        yield return new WaitForSeconds(1.12f);

        foreach (GameObject book in bookButtons)
        {
            book.GetComponent<Animator>().SetTrigger("Reset");
            book.GetComponent<Door>().isOpen = false;
            book.GetComponent<BookButtonPuzzle>().pressed = false;
            book.tag = "Door";

            if (resetSoundEffect != null)
                AudioManager.instance.PlaySFX(resetSoundEffect);
        }

        buttonCount = 0;
        booksPressed = "";

    }
}
