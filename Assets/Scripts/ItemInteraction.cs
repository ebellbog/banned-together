using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;

namespace StarterAssets
{
    public class ItemInteraction : MonoBehaviour
    {
        [Header("Examine mode")]
        public Camera ExamineCamera;
        public Transform ExamineTarget;
        public float RaycastReach;
        public float rotationSpeed = 7.5f;
        public float pickUpDuration = 0.6f;
        public float putDownDuration = .35f;
        public float SitDistance = 1.5f;
        public float PanSpeed = 0.02f; 

        [Header("Sitting mode")]
        private GameObject weirdLibraryParent; // private until we actually implement this
        public Transform standingUpPosition;
        public float sittingHeight;

        [Header("Cursor settings")]
        public Image CursorImage;
		public Sprite DefaultIcon;
		public Sprite InspectIcon;
        public Sprite SitIcon;
        public Sprite DoorIcon;
        public Sprite HoverIcon;

        [Header("Unlocked cursors")]
        public Texture2D RotateAllDirections;
        public Texture2D RotateLeftRight;
        public Texture2D RotateUpDown;
        public Texture2D DefaultUnlocked;

        [Header("Outline colors")]
        public Color InteractableOutlineColor;
        public Color InteractableOccludedColor;
        public Color SittableOutlineColor;
        public Color SittableOccludedColor;

        private StarterAssetsInputs _input;
        private PlayerInput _playerInput;
        private SelectionOutlineController _selectionOutlineController;
		private RaycastHit hitInfo;
        private int layerNumber;

        private GameObject currentObject;
        private GameObject activeObject;
        private GameObject activeParent;
        private Vector3 startPosition;
        private Vector3 startScale;
        private Quaternion startRotation;
        private Quaternion targetStartRotation;
        
        [SerializeField]
        private Vector3? targetStartPosition;
        private Space rotationSpace;
        private RotationAxis rotationAxis;
        private bool isDragging = false;
        private bool hitRotatable = false;
        private bool isReadyToInspect = false;
        private bool isAnimating = false;
        private bool panningEnabled = false;
        private bool affectsBodyBattery = false;

        private bool readyToStand = false;
        private GameObject currentSeat;
        private float characterHeight;
        private FirstPersonController _playerController;

        private Action examineCallback;


        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();
            _playerController = GetComponent<FirstPersonController>();
            _selectionOutlineController = Camera.main.GetComponent<SelectionOutlineController>();

            layerNumber = LayerMask.NameToLayer("Examine Object");
            characterHeight = GetComponent<CharacterController>().height;

            if (weirdLibraryParent) {
                weirdLibraryParent.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            Interact();
        }

		private void Interact() {
            // TODO: replace raycaster in SelectionOutlineController with this one
            if (GS.interactionMode == InteractionType.Examine)
            {
                Physics.Raycast(ExamineCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, RaycastReach);
            } else
            {
                Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, RaycastReach);
            }

            string hitTag = hitInfo.transform?.tag;

            // Set hit (aka hover) flags, in modes that allow interaction
            bool hitInteractable = false, hitSittable = false, hitDoor = false, hitSwitch = false;
            hitRotatable = false;
            if (GS.interactionMode == InteractionType.Default ||
                GS.interactionMode == InteractionType.Focus ||
                GS.interactionMode == InteractionType.Monologue)
            {
                hitInteractable = hitTag == "Interactable";
                hitSittable = hitTag == "Sittable" && Vector3.Distance(hitInfo.transform.position, transform.position) < SitDistance;
                hitDoor = hitTag == "Door";
                hitSwitch = hitTag == "Switch";
            }

            if (!(hitInteractable || hitSittable || hitDoor || hitSwitch)) // Reset if nothing hit
            {
                CursorImage.sprite = DefaultIcon;
                _selectionOutlineController.FilterByTag = "None";
                hitInteractable = false;
                hitSittable = false;
            }
            else if (hitInteractable) // Highlight for examinable objects
            {
                CursorImage.sprite = InspectIcon;
                _selectionOutlineController.FilterByTag = "Interactable";
                _selectionOutlineController.OutlineColor = InteractableOutlineColor;
                _selectionOutlineController.OccludedColor = InteractableOccludedColor;
                _selectionOutlineController.OutlineType = SelectionOutlineController.OutlineMode.OnlyVisible;
                _selectionOutlineController.UpdateOutlineType();
            }
            else if (hitSittable)
            { // Highlight for sittable objects
                CursorImage.sprite = SitIcon;
                _selectionOutlineController.FilterByTag = "Sittable";
                _selectionOutlineController.OutlineColor = SittableOutlineColor;
                _selectionOutlineController.OccludedColor = SittableOccludedColor;
                _selectionOutlineController.OccludedColor.a = 0;
                _selectionOutlineController.OutlineType = SelectionOutlineController.OutlineMode.Whole;
                _selectionOutlineController.UpdateOutlineType();
            }
            else if (hitDoor) { // Set cursor for doors
                CursorImage.sprite = DoorIcon;
            }
            else if (hitSwitch)
            {
                CursorImage.sprite = HoverIcon;
            }

            // Set cursor and hit flag for rotating objects during examination
            if (GS.interactionMode == InteractionType.Examine)
            {
                int? hitLayer = hitInfo.transform?.gameObject.layer;
                hitRotatable = hitLayer == layerNumber;
                UI.SetCursor(hitRotatable || isDragging ? GetRotationIcon() : DefaultUnlocked);

                if (ZoomManager.GetZoom() >= 1 && !isAnimating)
                {
                    RestoreTargetPosition();
                }
            }

            // Handle clicks, based on mode and hover status
            if (_input.interact && GS.interactionMode != InteractionType.Paused) {
                // Skip code if clicking UI element
                if (EventSystem.current.currentSelectedGameObject != null) {
                    _input.interact = false;
                    return;
                } 

                if (hitRotatable)
                {
                    isDragging = true;
                }
                else if (hitInteractable)
                {
                    BeginExamination();
                }
                else if (hitSittable && !GS.isSitting)
                {
                    _playerInput.enabled = false;

                    currentSeat = hitInfo.transform.gameObject;
                    currentSeat.GetComponent<Collider>().enabled = false;

                    _playerController.SeatAngle = (currentSeat.transform.eulerAngles.y + 90f) % 360f;

                    transform.DOMove(hitInfo.transform.GetChild(0).position, 1.5f);
                    StartCoroutine(RotatePlayerToChair(4f, hitInfo.transform.GetChild(0).rotation));
                    StartCoroutine(HeightChange("shrink", 0.5f));
                    StartCoroutine(SitShake(1.1f));
                    StartCoroutine(ReenableControls(2.3f, "sit"));

                    //Show weird library
                    if (weirdLibraryParent) {
                        weirdLibraryParent.SetActive(true);
                    }
                }
                else if (GS.isSitting)
                {
                    StandUp();
                    readyToStand = true;
                }
                else if (hitDoor)
                {
                    Door doorComponent = hitInfo.transform.gameObject.GetComponent<Door>();
                    doorComponent.Open();
                }
                else if (hitSwitch)
                {
                    LightSwitch switchComponent = hitInfo.transform.gameObject.GetComponent<LightSwitch>();
                    switchComponent.SwitchLight();
                }
                else if (GS.interactionMode == InteractionType.Tutorial)
                {
                    YarnDispatcher.SkipToEnd();
                }
                else
                {
                    ExitExamination();
                }

                _input.interact = false;
                _input.anyKey = false;
			}
            else if (_input.anyKey)
            {
                if (GS.interactionMode == InteractionType.Examine)
                {
                    ExitExamination();
                    _input.anyKey = false;
                    _input.exit = false;
                } 
                else if (GS.interactionMode == InteractionType.Tutorial)
                {
                    YarnDispatcher.SkipToEnd();
                    _input.anyKey = false;
                    _input.exit = false;
                }
                else if (GS.interactionMode == InteractionType.Paused)
                {
                    PauseManager.instance.ResumeGame();
                    _input.anyKey = false;
                    _input.exit = false;
                }
                else if (_input.exit)
                {
                    _input.exit = false;
                    _input.anyKey = false;
                    if (GS.interactionMode != InteractionType.Journal) PauseManager.instance.PauseGame();
                }
            }
            else if (_playerInput.actions["interact"].IsPressed() == false)
            {
                isDragging = false;
            }
		}

        private void RestoreTargetPosition(float duration = 0.6f)
        {
            if (targetStartPosition == null || targetStartPosition == ExamineTarget.position) return;

            Debug.Log("Restoring target position");
            StartCoroutine(MoveForDuration(
                ExamineTarget.gameObject,
                targetStartPosition,
                null,
                null,
                duration
            ));

            targetStartPosition = null;
        }

        private void BeginExamination()
        {
            StopAllCoroutines();
            if (activeObject) {
                activeObject.transform.position = startPosition;
                activeObject.transform.rotation = startRotation;
                activeObject.transform.SetParent(activeParent.transform);

                if (targetStartPosition != null)
                    ExamineTarget.transform.position = (Vector3)targetStartPosition;
                if (targetStartRotation != null)
                    ExamineTarget.transform.rotation = targetStartRotation;
            }

            currentObject = hitInfo.transform.gameObject;

            InteractableItem interactableItem = currentObject.GetComponent<InteractableItem>();
            activeObject = interactableItem.interactionParent;
            if (!activeObject) activeObject = currentObject;

            activeParent = activeObject.transform.parent?.gameObject;

            startPosition = activeObject.transform.position;
            startScale = activeObject.transform.localScale;
            startRotation = activeObject.transform.rotation;

            targetStartRotation = ExamineTarget.transform.rotation;
            targetStartPosition = ExamineTarget.transform.position;

            if (_selectionOutlineController) {
                _selectionOutlineController.enabled = false;
            }

            activeObject.layer = layerNumber;
            foreach (Transform child in activeObject.transform)
            {
                child.gameObject.layer = layerNumber;
            }

            UI.FadeInMatte();
            UI.FadeInInteractionUI();

            isReadyToInspect = false;
            GS.interactionMode = InteractionType.Examine;

            rotationSpace = interactableItem.orientToCamera ? Space.Self : Space.World;
            rotationAxis = interactableItem.rotationAxis;
            panningEnabled = interactableItem.enablePanning;
            affectsBodyBattery = interactableItem.affectsBodyBattery;

            Player.LockPlayer();
            UI.UnlockCursor(GetRotationIcon());

            examineCallback = null;
            examineCallback += () => {
                isReadyToInspect = true;
                activeObject.transform.SetParent(ExamineTarget);
                interactableItem.UpdateGameState(true);
            };

            // Compensate for visually off-center objects
            Vector3 targetPosition;
            if (interactableItem.useRenderPivot)
            {
                Renderer currentRenderer = currentObject.GetComponent<Renderer>();
                Vector3 offCenterAdjustment = currentRenderer.bounds.center - currentObject.transform.position;
                targetPosition = ExamineTarget.position - offCenterAdjustment;
            }
            else {
                targetPosition = ExamineTarget.position;
            }

            StartCoroutine(MoveForDuration(
                activeObject,
                targetPosition,
                interactableItem.orientToCamera ? ExamineTarget.rotation : startRotation,
                startScale * interactableItem.scaleOnInteraction,
                pickUpDuration
            ));
        }

        private void ExitExamination()
        {
            if (GS.interactionMode != InteractionType.Examine) return;
            StopAllCoroutines();

            RestoreTargetPosition(putDownDuration); // Important that this duration <= putDownDuration
            ZoomManager.ResetZoom();

            UI.FadeOutMatte();
            UI.FadeOutInteractionUI();

            examineCallback = null;
            examineCallback += () => {
                activeObject.layer = 0;
                foreach (Transform child in activeObject.transform)
                {
                    child.gameObject.layer = 0;
                }

                activeObject.transform.SetParent(activeParent.transform);
                ExamineTarget.transform.rotation = targetStartRotation;
                
                if (GS.interactionMode != InteractionType.Journal && GS.interactionMode != InteractionType.Paused)
                    Player.UnlockPlayer();

                if (_selectionOutlineController) {
                    _selectionOutlineController.enabled = true;
                }

                InteractableItem interactableItem = currentObject.GetComponent<InteractableItem>();
                interactableItem.UpdateGameState();

                JournalManager.Main.AddToJournal(interactableItem.journalEntry);

                activeObject = null;
                examineCallback = null;
            };

            StartCoroutine(MoveForDuration(activeObject, startPosition, startRotation, startScale, putDownDuration));

            UI.LockCursor();
            GS.interactionMode = YarnDispatcher.YarnSpinnerIsActive() ? InteractionType.Monologue : InteractionType.Default;
        }

        IEnumerator SitShake(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            transform.DOShakeRotation(0.6f, new Vector3(0, 0, 3f));
        }

        IEnumerator RotatePlayerToChair(float duration, Quaternion destination)
        {

            float timer = 0.0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                t = t * t * t * (t * (6f * t - 15f) + 10f);

                transform.rotation = Quaternion.Slerp(transform.rotation, destination, t);

                yield return null;
            }

            yield return null;
        }

        IEnumerator ReenableControls(float seconds, string action)
        {
            yield return new WaitForSeconds(seconds);

            if (action == "sit") {
                _playerInput.enabled = true;
                _playerInput.SwitchCurrentActionMap("Sitting");
                GS.isSitting = true;
            }
            else if (action == "stand")
            {
                _playerInput.enabled = true;
                _playerInput.SwitchCurrentActionMap("Player");
                GS.isSitting = false;
            }
        }
        
        public void OnMove(InputValue value)
        {
            if (GS.isSitting) {
                StandUp();
            }
            if (GS.isSitting || readyToStand) {
                StartCoroutine(WaitAndReactivateChair(1.5f, currentSeat));
                readyToStand = false;
            }
        }

        public void StandUp() {
            _playerInput.enabled = false;

            StartCoroutine(HeightChange("grow", 0.25f));
            StartCoroutine(ReenableControls(0.5f, "stand"));

            if (weirdLibraryParent)
            {
                weirdLibraryParent.SetActive(false);
            }
        }

        IEnumerator WaitAndReactivateChair(float seconds, GameObject targetSeat)
        {
            yield return new WaitForSeconds(seconds);
            targetSeat.GetComponent<Collider>().enabled = true;
        }

        IEnumerator HeightChange(string operation, float duration)
        {
            if (operation == "grow")
            {
                float pointInTime = 0.0f;
                while (pointInTime <= duration)
                {
                    GetComponent<CharacterController>().height = Mathf.Lerp(sittingHeight, characterHeight, pointInTime / duration);
                    pointInTime += Time.deltaTime;
                    yield return null;
                }
            }
            else if (operation == "shrink")
            {
                float pointInTime = 0.0f;
                while (pointInTime <= duration)
                {
                    GetComponent<CharacterController>().height = Mathf.Lerp(characterHeight, sittingHeight, pointInTime / duration);
                    pointInTime += Time.deltaTime;
                    yield return null;
                }

            }

        }

        public void OnLook(InputValue value)
		{
			if (GS.interactionMode == InteractionType.Examine && isReadyToInspect)
			{
                Vector2 look = value.Get<Vector2>();
                if (isDragging)
                {
                    if (rotationAxis == RotationAxis.LeftRight || rotationAxis == RotationAxis.All)
                    {
                        ExamineTarget.Rotate(Vector3.down * look.x * rotationSpeed, rotationSpace);
                    } 
                    if (rotationAxis == RotationAxis.UpDown || rotationAxis == RotationAxis.All)
                    {
                        ExamineTarget.Rotate(-Camera.main.transform.right * look.y * rotationSpeed, Space.World);
                    }
                    if (affectsBodyBattery) BodyBatteryManager.Main.FidgetToRecover();
                } else if (panningEnabled && !isAnimating && hitRotatable && ZoomManager.GetZoom() < 1)
                {
                    ExamineTarget.transform.position = ExamineTarget.transform.position
                        - Camera.main.transform.right * look.x * PanSpeed
                        + Camera.main.transform.up * look.y * PanSpeed;
                }
			}
		}

        private Texture2D GetRotationIcon() {
            switch(rotationAxis) {
                case RotationAxis.LeftRight:
                    return RotateLeftRight;
                case RotationAxis.UpDown:
                    return RotateUpDown;
                default:
                    return RotateAllDirections;
            }
        }

        IEnumerator MoveToTarget(
            GameObject movedObject,
            Vector3 targetPosition,
            Quaternion targetRotation,
            Vector3 targetScale,
            float speed,
            float rotationSpeed = 300f,
            float scaleSpeed = 2.0f
        ) {
            while (
                movedObject.transform.position != targetPosition ||
                movedObject.transform.rotation != targetRotation ||
                movedObject.transform.localScale != targetScale
            ) {
                movedObject.transform.position = Vector3.MoveTowards(movedObject.transform.position, targetPosition, Time.deltaTime * speed);
                movedObject.transform.rotation = Quaternion.RotateTowards(movedObject.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                movedObject.transform.localScale = Vector3.MoveTowards(movedObject.transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
                yield return null;
            }
            isReadyToInspect = true;
            examineCallback();
            yield return null;
        }

        IEnumerator MoveForDuration(
            GameObject movedObject,
            Vector3? targetPosition,
            Quaternion? targetRotation,
            Vector3? targetScale,
            float duration
        ) {
            float counter = 0;
            Vector3 oldPos = movedObject.transform.position;
            Vector3 oldScale = movedObject.transform.localScale;
            Quaternion oldRot = movedObject.transform.rotation;

            if (targetPosition != null || targetRotation != null || targetScale != null)
            {
                isAnimating = true;
                while (counter < duration)
                {
                    counter += Time.deltaTime;
                    float timePercent = counter / duration;

                    if (targetPosition != null) movedObject.transform.position = Vector3.Lerp(oldPos, (Vector3)targetPosition, timePercent);
                    if (targetScale != null) movedObject.transform.localScale = Vector3.Lerp(oldScale, (Vector3)targetScale, timePercent);
                    if (targetRotation != null) movedObject.transform.rotation = Quaternion.Lerp(oldRot, (Quaternion)targetRotation, timePercent);
                    yield return null;
                }
                isAnimating = false;
            }

            if (examineCallback != null) examineCallback();
            yield return null;
        }
    }
}
