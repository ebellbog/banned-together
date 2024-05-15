using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
public static class Player
{
    public static PlayerInput playerInput;
    public static StarterAssetsInputs starterInputs;

    public static void LockPlayer()
    {
        LockMovement();

        starterInputs.look = Vector2.zero;
        starterInputs.cursorInputForLook = false;
    }

    public static void UnlockPlayer()
    {
        playerInput.enabled = true;
        playerInput.actions.FindAction("Move").Enable();
        starterInputs.cursorInputForLook = true;
    }

    // Lock traversal without locking camera movement
    public static void LockMovement()
    {
        playerInput.actions.FindAction("Move").Disable();
    }
}