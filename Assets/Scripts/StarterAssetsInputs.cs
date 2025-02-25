using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool interact;
		public bool focus;
		public bool journal;
		public bool sleep;
		public bool exit;
		public bool delete;
		public bool anyKey;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		public void Start() {
			SetCursorState(true);
		}
#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
			anyKey = false;
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

		public void OnFocus(InputValue value)
		{
			FocusInput(value.isPressed);
		}

		public void OnJournal(InputValue value)
		{
			JournalInput(value.isPressed);
		}

		public void OnExit(InputValue value)
		{
			ExitInput(value.isPressed);
		}

		public void OnDelete(InputValue value)
		{
			DeleteInput(value.isPressed);
		}

		public void OnSleep(InputValue value) {
			SleepInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnAny(InputValue value)
		{
			AnyInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void InteractInput(bool newInteractState)
		{
			interact = newInteractState;
		}

		public void FocusInput(bool newFocusState)
		{
			focus = newFocusState;
		}

		public void JournalInput(bool newJournalState)
		{
			journal = newJournalState;
		}

		public void ExitInput(bool newExitState)
		{
			exit = newExitState;
		}

		public void DeleteInput(bool newDeleteState)
		{
			delete = newDeleteState;
		}

		public void SleepInput(bool newSleepState)
		{
			sleep = newSleepState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void AnyInput(bool newAnyState)
		{
			anyKey = newAnyState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}