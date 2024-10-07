using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookButtonPuzzle : MonoBehaviour
{

    //public int bookNumber;
    public string bookIndex;
    public bool pressed;
    public BookButtonPuzzleManager pm;
    // Start is called before the first frame update
    void Start()
    {
        pressed = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Pressed()
    {
        pressed = true;
        Debug.Log("book button pressed");
        pm.Pressed(bookIndex);

    }
}
