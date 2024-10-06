using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookButtonPuzzleManager : MonoBehaviour
{

    public GameObject[] bookButtons;
    private string bookMasterIndex;
    private string booksPressed;
    private int buttonCount;
    public GameObject doorToOpen;
    public string soundEffect;
    public string resetSoundEffect;
    public bool useSpatialAudio = true;
    public Animator animator;

    void Start()
    {
        buttonCount = 0;
        bookMasterIndex = "";

        foreach (GameObject book in bookButtons)
        {
            bookMasterIndex += book.GetComponent<BookButtonPuzzle>().bookIndex;
            Debug.Log(bookMasterIndex);
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (bookMasterIndex == booksPressed)
        {
            Debug.Log("Correct!");
            StartCoroutine(OpenSecretDoor());
        }
        else
        {
            if (buttonCount == 5 && bookMasterIndex != booksPressed) {
                Debug.Log("Incorrect!");
                StartCoroutine(Reset());
                buttonCount = 0;
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

        animator.SetTrigger("Open");

        //Not working?
        if (soundEffect != null)
            AudioManager.instance.PlaySFX(soundEffect, useSpatialAudio && animator ? animator.gameObject.transform.position : null);
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
                AudioManager.instance.PlaySFX(resetSoundEffect, useSpatialAudio && animator ? animator.gameObject.transform.position : null);
        }

        buttonCount = 0;
        booksPressed = "";

    }
}
