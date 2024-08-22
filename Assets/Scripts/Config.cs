using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using Yarn.Unity;
using TMPro;

public class Config : MonoBehaviour
{
    [Header("Game State")]
    public int currentDay = 1;

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

        GS.currentDay = currentDay;
    }

    void OnValidate()
    {
        GS.currentDay = currentDay;
    }

    void Update()
    {
        currentDay = GS.currentDay;
    }
}
