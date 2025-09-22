using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using Yarn.Unity;
using TMPro;

public class Config : MonoBehaviour
{
    [Header("Game State")]
    public int currentDay = 1;
    public bool hideBooks = false;
    public bool makeBooksReadable = false;
    public bool journalStartsEnabled = false;
    private bool _oldHideBooks = false;
    private bool _oldMakeBooksReadable = false;

    [Header("Yarn Systems")]
    public DialogueRunner internalMonologueSystem;
    public DialogueRunner tutorialDialogSystem;
    public TextMeshProUGUI monologueTextMesh;

    [Header("UI Management")]
    public UIManager managerComponent;
    public Canvas cursorCanvas;

    [Header("Player Control")]
    public GameObject player;
    public StarterAssetsInputs starterAssetsInputs;
    public PlayerInput playerInput;
    public GameObject playerStartPosition;
    public bool startPlayerAtPosition = false;

    void Start()
    {
        YarnDispatcher.internalMonologueSystem = internalMonologueSystem;
        YarnDispatcher.tutorialDialogSystem = tutorialDialogSystem;
        YarnDispatcher.monologueTextMesh = monologueTextMesh;

        UI.uiManager = managerComponent;
        UI.cursorCanvas = cursorCanvas;

        Player.starterInputs = starterAssetsInputs;
        Player.playerInput = playerInput;

        if (startPlayerAtPosition && playerStartPosition) {
            player.transform.position = playerStartPosition.transform.position;
            player.transform.rotation = playerStartPosition.transform.rotation;
        }
    }

    #if UNITY_EDITOR
    private void OnValidate() {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }
    private void _OnValidate()
    {
        UnityEditor.EditorApplication.delayCall -= _OnValidate;
        if(this == null) return;

        if (GS.interactionMode == InteractionType.Paused) GS.currentDay = currentDay;

        if (journalStartsEnabled)
        {
            GS.journalEnabled = 1;
            GS.stickersEnabled = 1;
        }

        if (hideBooks != _oldHideBooks)
        {
            _oldHideBooks = hideBooks;
            SetActiveForAllBooks();
        }

        if (makeBooksReadable != _oldMakeBooksReadable)
        {
            _oldMakeBooksReadable = makeBooksReadable;
            MakeAllBooksReadable();
        }
    }
    #endif

    void Update()
    {
        currentDay = GS.currentDay;
    }

    void SetActiveForAllBooks()
    {
        List<ReplicatorExtras> allReplicators = GameObject.FindGameObjectsWithTag("Bookshelf")
            .Select(x => x.transform.GetChild(0).gameObject.GetComponent<ReplicatorExtras>())
            .Where(x => x)
            .ToList();
        foreach(ReplicatorExtras extra in allReplicators)
        {
            extra.gameObject.SetActive(!hideBooks);
        }
    }

    void MakeAllBooksReadable()
    {
        // Find the layer index for "Books"
        int booksLayer = LayerMask.NameToLayer("Books");
        if (booksLayer == -1) return; // Layer not found, exit

        // Find all objects with the "Bookshelf" tag
        GameObject[] bookshelves = GameObject.FindGameObjectsWithTag("Bookshelf");

        int modifiedBookCount = 0;
        foreach (GameObject bookshelf in bookshelves)
        {
            // Traverse all descendants of the bookshelf
            foreach (Transform child in bookshelf.GetComponentsInChildren<Transform>(true))
            {
                GameObject obj = child.gameObject;
                if (obj.layer == booksLayer)
                {
                    InteractableItem interactable = obj.GetComponent<InteractableItem>();
                    if (interactable != null)
                    {
                        ReadableBook readable = obj.GetComponent<ReadableBook>();
                        if (makeBooksReadable)
                        {
                            if (readable == null)
                            {
                                obj.AddComponent<ReadableBook>();
                            }
                        }
                        else
                        {
                            if (readable != null)
                            {
                                readable.ResetInteractableItem();

                                #if UNITY_EDITOR
                                Object.DestroyImmediate(readable, true);
                                #else
                                Destroy(readable);
                                #endif
                            }
                        }
                        modifiedBookCount++;
                    }
                }
            }
        }
        Debug.Log($"Set {modifiedBookCount} book{(modifiedBookCount == 1 ? "" : "s")} to be {(makeBooksReadable ? "readable" : "non-readable")}.");
    }
}
