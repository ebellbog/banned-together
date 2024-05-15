using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using Yarn.Unity;

public class Config : MonoBehaviour
{
    [Header("Yarn Systems")]
    public DialogueRunner internalMonologueSystem;
    public DialogueRunner tutorialDialogSystem;

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

        UI.uiManager = managerComponent;
        UI.cursorCanvas = cursorCanvas;

        Player.starterInputs = starterAssetsInputs;
        Player.playerInput = playerInput;
    }
}
