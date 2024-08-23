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
    private bool _oldHideBooks = false;

    [Header("Yarn Systems")]
    public DialogueRunner internalMonologueSystem;
    public DialogueRunner tutorialDialogSystem;
    public TextMeshProUGUI monologueTextMesh;

    [Header("UI Management")]
    public UIManager managerComponent;
    public Canvas cursorCanvas;

    [Header("Player Control")]
    public StarterAssetsInputs starterAssetsInputs;
    public PlayerInput playerInput;

    void Start()
    {
        YarnDispatcher.internalMonologueSystem = internalMonologueSystem;
        YarnDispatcher.tutorialDialogSystem = tutorialDialogSystem;
        YarnDispatcher.monologueTextMesh = monologueTextMesh;

        UI.uiManager = managerComponent;
        UI.cursorCanvas = cursorCanvas;

        Player.starterInputs = starterAssetsInputs;
        Player.playerInput = playerInput;
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

        if (hideBooks != _oldHideBooks)
        {
            _oldHideBooks = hideBooks;
            SetActiveForAllBooks();
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
}
