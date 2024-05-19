using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Unity.VisualScripting;

namespace StarterAssets
{
    public class ItemInteraction : MonoBehaviour
    {
        [Header("Examine mode")]
        public Transform ExamineTarget;
        public float RaycastReach;
        public float rotationSpeed = 7.5f;
        public float pickUpDuration = 0.6f;
        public float putDownDuration = .35f;
        public float SitDistance = 1.5f;

        [Header("Sitting mode")]
        public GameObject weirdLibraryParent; // private until we actually implement this
        public Transform standingUpPosition;
        public float sittingHeight;

        [Header("Cursor settings")]
        public Image CursorImage;
		public Sprite DefaultIcon;
		public Sprite InspectIcon;
        public Sprite SitIcon;
        public Texture2D RotateIcon;

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
        private GameObject currentParent;
        private Vector3 startPosition;
        private Vector3 startScale;
        private Quaternion startRotation;
        private Quaternion targetStartRotation;
        private Space rotationSpace; // TODO: consider removing
        private bool isReadyToInspect = false;

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
            Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, RaycastReach);

            string hitTag = hitInfo.transform?.tag;

            bool hitInteractable = false, hitSittable = false;
            if (GS.interactionMode == InteractionType.Default)
            {
                hitInteractable = hitTag == "Interactable";
                hitSittable = hitTag == "Sittable" && Vector3.Distance(hitInfo.transform.position, transform.position) < SitDistance;
            }

            if (GS.interactionMode != InteractionType.Default || !(hitInteractable || hitSittable))
            {
                CursorImage.sprite = DefaultIcon;
                _selectionOutlineController.FilterByTag = "None";
                hitInteractable = false;
                hitSittable = false;
            }
            else if (hitInteractable)
            {
                CursorImage.sprite = InspectIcon;
                _selectionOutlineController.FilterByTag = "Interactable";
                _selectionOutlineController.OutlineColor = InteractableOutlineColor;
                _selectionOutlineController.OccludedColor = InteractableOccludedColor;
                _selectionOutlineController.OutlineType = SelectionOutlineController.OutlineMode.OnlyVisible;
            }
            else if (hitSittable) {
                CursorImage.sprite = SitIcon;
                _selectionOutlineController.FilterByTag = "Sittable";
                _selectionOutlineController.OutlineColor = SittableOutlineColor;
                _selectionOutlineController.OccludedColor = SittableOccludedColor;
                _selectionOutlineController.OutlineType = SelectionOutlineController.OutlineMode.Whole;
            }

            if (_input.interact) {
				if (hitInteractable)
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
                else if (GS.interactionMode == InteractionType.Tutorial)
                {
                    YarnDispatcher.EndTutorial();
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
                ExitExamination();
                YarnDispatcher.EndTutorial();
            }
		}

        private void BeginExamination()
        {
            StopAllCoroutines();
            if (currentObject) {
                currentObject.transform.position = startPosition;
                currentObject.transform.rotation = startRotation;
            }

            currentObject = hitInfo.transform.gameObject;
            currentParent = hitInfo.transform.parent?.gameObject;
            startPosition = hitInfo.transform.position;
            startScale = hitInfo.transform.localScale;
            startRotation = hitInfo.transform.rotation;
            targetStartRotation = ExamineTarget.transform.rotation;

            Player.LockPlayer();
            UI.UnlockCursor(RotateIcon);

            if (_selectionOutlineController) {
                _selectionOutlineController.enabled = false;
            }

            Collider currentCollider = currentObject.GetComponent<Collider>();
            currentCollider.enabled = false;
            currentObject.layer = layerNumber;
            foreach (Transform child in currentObject.transform)
            {
                child.gameObject.layer = layerNumber;
                Collider childCollider = child.gameObject.GetComponent<Collider>();
                if (childCollider) childCollider.enabled = false;
            }

            UI.FadeInMatte();

            isReadyToInspect = false;
            GS.interactionMode = InteractionType.Examine;


            InteractableItem interactableItem = currentObject.GetComponent<InteractableItem>();
            rotationSpace = interactableItem.orientToCamera ? Space.Self : Space.World;
    
            examineCallback = null;
            examineCallback += () => currentObject.transform.SetParent(ExamineTarget);

            // Compensate for visually off-center objects
            Renderer currentRenderer = currentObject.GetComponent<Renderer>();
            Vector3 offCenterAdjustment = currentRenderer.bounds.center - currentObject.transform.position;

            StartCoroutine(MoveForDuration(
                currentObject,
                ExamineTarget.position - offCenterAdjustment,
                interactableItem.orientToCamera ? ExamineTarget.rotation : startRotation,
                startScale * interactableItem.scaleOnInteraction,
                pickUpDuration
            ));
        }

        private void ExitExamination()
        {
            if (GS.interactionMode != InteractionType.Examine) return;
            StopAllCoroutines();

            UI.FadeOutMatte();

            examineCallback = null;
            examineCallback += () => {
                currentObject.GetComponent<Collider>().enabled = true;
                currentObject.layer = 0;
                foreach (Transform child in currentObject.transform)
                {
                    child.gameObject.layer = 0;
                    // TODO: would we ever actually want to reenable a child collider?
                }

                currentObject.transform.SetParent(currentParent.transform);
                ExamineTarget.transform.rotation = targetStartRotation;

                Player.UnlockPlayer();

                if (_selectionOutlineController) {
                    _selectionOutlineController.enabled = true;
                }


                InteractableItem interactableItem = currentObject.GetComponent<InteractableItem>();
                interactableItem.UpdateGameState();

                currentObject = null;
            };

            StartCoroutine(MoveForDuration(currentObject, startPosition, startRotation, startScale, putDownDuration));

            UI.LockCursor();
            GS.interactionMode = InteractionType.Default;
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
                // Transform rotationTarget = (rotationSpace == Space.Self) ? ExamineTarget : currentObject.transform;
                ExamineTarget.Rotate(Vector3.down * look.x * rotationSpeed, Space.Self);
                ExamineTarget.transform.Rotate(Vector3.left * look.y * rotationSpeed, Space.Self);
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
            Vector3 targetPosition,
            Quaternion targetRotation,
            Vector3 targetScale,
            float duration
        ) {

            float counter = 0;
            Vector3 oldPos = movedObject.transform.position;
            Vector3 oldScale = movedObject.transform.localScale;
            Quaternion oldRot = movedObject.transform.rotation;

            while (counter < duration)
            {
                counter += Time.deltaTime;
                float timePercent = counter / duration;

                movedObject.transform.position = Vector3.Lerp(oldPos, targetPosition, timePercent);
                movedObject.transform.localScale = Vector3.Lerp(oldScale, targetScale, timePercent);
                movedObject.transform.rotation = Quaternion.Lerp(oldRot, targetRotation, timePercent);
                yield return null;
            }

            isReadyToInspect = true;
            if (examineCallback != null) examineCallback();
            yield return null;
        }
    }
}
