using System.Collections;
using UnityEngine;

public class BookButtonPuzzleManager : MonoBehaviour
{

    public GameObject[] bookButtons;
    private string bookMasterIndex;
    private string safeMasterIndex;
    private string booksPressed;
    private int buttonCount;
    private bool doorIsOpen = false;
    public GameObject doorToOpen;
    public string soundEffect;
    public string resetSoundEffect;
    public bool useSpatialAudio = true;
    public Animator secretDoorAnimator;
    public Animator safeAnimator;

    void Start()
    {
        buttonCount = 0;
        safeMasterIndex = "ACBDE";
        bookMasterIndex = "CBDEA"; // not currently in use
    }

    // Update is called once per frame
    void Update()
    {

        if (!doorIsOpen && (bookMasterIndex == booksPressed || (Application.isEditor && Input.GetKeyDown(KeyCode.Space))))
        {
            booksPressed = "";
            buttonCount = 0;
            StartCoroutine(OpenSecretDoor());
        }
        else if (safeMasterIndex == booksPressed)
        {
            booksPressed = "";
            buttonCount = 0;
            StartCoroutine(OpenSafe());
        }
        else if (buttonCount == 5)
        {
            buttonCount = 0;
            StartCoroutine(ResetBooks());
        }
    }

    public void Pressed(string bookIndex)
    {  
        booksPressed += bookIndex;
        buttonCount++;
    }

    IEnumerator OpenSecretDoor()
    {
        yield return new WaitForSeconds(.4f);

        secretDoorAnimator.SetTrigger("Open");
        doorIsOpen = true;

        if (soundEffect != null)
        {
            AudioManager.instance.PlaySFX(soundEffect, secretDoorAnimator.gameObject.transform.position);
        }

        //StartCoroutine("ResetBooks", false);
    }

    IEnumerator OpenSafe()
    {
        yield return new WaitForSeconds(.6f);

        safeAnimator.SetTrigger("Open");

        // For demo only - remove in full game
        JournalManager.Main.DisableJournalEntry("workOrder");
        JournalManager.Main.DisableJournalEntry("hiddenMechanisms");

        if (soundEffect != null)
        {
            AudioManager.instance.PlaySFX(soundEffect, safeAnimator.gameObject.transform.position);
        }

        //StartCoroutine("ResetBooks", false);
    }

    IEnumerator ResetBooks(bool playSFX = true)
    {
        yield return new WaitForSeconds(1.12f);

        foreach (GameObject book in bookButtons)
        {
            book.GetComponent<Animator>().SetTrigger("Reset");
            book.GetComponent<Door>().isOpen = false;
            book.GetComponent<BookButtonPuzzle>().pressed = false;
            book.tag = "Door";

            if (playSFX && resetSoundEffect != null)
                AudioManager.instance.PlaySFX(resetSoundEffect);
        }

        buttonCount = 0;
        booksPressed = "";
    }
}
