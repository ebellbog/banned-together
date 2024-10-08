using System.Collections;
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
            StartCoroutine(Reset());
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

        if (soundEffect != null)
        {
            AudioManager.instance.PlaySFX(soundEffect);
        }

        yield return new WaitForSeconds(1.12f);

        StartCoroutine("Reset", false);
    }

    IEnumerator OpenSafe()
    {
        yield return new WaitForSeconds(1.12f);
        safeAnimator.SetTrigger("Open");

        if (soundEffect != null)
        {
            AudioManager.instance.PlaySFX(soundEffect);
        }

        yield return new WaitForSeconds(1.12f);

        StartCoroutine("Reset", false);
    }

    IEnumerator Reset(bool playSFX = true)
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
