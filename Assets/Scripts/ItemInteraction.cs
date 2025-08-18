using DG.Tweening;
using OccaSoftware.Outlines.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

namespace StarterAssets
{
    public class ItemInteraction : MonoBehaviour
    {
        [Header("General settings")]
        public float RaycastReach;
        public FocusManager FocusManager;

        [Header("Outline settings")]
        public Color DefaultExaminableColor = Color.white;
        public Color DefaultSittableColor = Color.white;
        public OutlinesRenderfeature outlinesRenderFeature;
        public Shader spriteOutliner;
        public UniversalRendererData rendererData;

        [Header("Examination")]
        public Camera ExamineCamera;
        public Transform ExamineTarget;
        public GameObject TakeItemButton;
        public string TakeItemSFX;
        public float rotationSpeed = 7.5f;
        public float pickUpDuration = 0.6f;
        public float putDownDuration = .35f;
        public float PanSpeed = 0.02f;

        [Header("Sitting")]
        public float SitDistance = 1.75f;
        public float sittingHeight = 1.25f;

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
        public Texture2D HandOpen;
        public Texture2D HandGrabbing;
        public Texture2D DefaultUnlocked;

        private StarterAssetsInputs _input;
        private PlayerInput _playerInput;
		private RaycastHit hitInfo;
        private int examineLayerIdx;
        private int outlineLayerIdx;

        private GameObject currentObject;
        private InteractableItem currentInteractable;
        private GameObject outlinedObject;
        private int prevLayerIdx;
        private Shader prevShader = null;
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
        private bool inertialRotation = false;
        private Vector2 lastLook = Vector2.zero;
        private bool useGrabCursor = false;
        private bool affectsBodyBattery = false;

        private bool readyToStand = false;
        private GameObject currentSeat;
        private float characterHeight;
        private FirstPersonController _playerController;

        private Action examineCallback;

        void Start()
        {
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();
            _playerController = GetComponent<FirstPersonController>();

            examineLayerIdx = LayerMask.NameToLayer("Examine Object");
            outlineLayerIdx = LayerMask.NameToLayer("No post");

            characterHeight = GetComponent<CharacterController>().height;
        }

        void Update()
        {
            Interact();

            if (inertialRotation && (Mathf.Abs(lastLook.x) + Mathf.Abs(lastLook.y) > .05f) && !isDragging)
            {
                lastLook *= Mathf.Pow(.4f, Time.deltaTime);
                ExamineTarget.Rotate(Vector3.down * lastLook.x * rotationSpeed, rotationSpace);
            }
        }

        private bool ModeSupportsInteraction()
        {
            return GS.interactionMode == InteractionType.Default ||
                GS.interactionMode == InteractionType.Focus ||
                GS.interactionMode == InteractionType.Monologue;
        }

		private void Interact() {
            if (GS.interactionMode == InteractionType.Examine)
            {
                Physics.Raycast(ExamineCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, RaycastReach);
            } else
            {
                Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, RaycastReach);
            }

            string hitTag = hitInfo.transform?.tag;
            currentObject = hitInfo.transform?.gameObject;

            // Handle hover and hover exit behaviors

            #nullable enable

            InteractableItem? interactableItem = currentObject?.GetComponent<InteractableItem>();

            // Set hit (aka hover) flags, in modes that allow interaction
            bool hitExaminable = false, hitSittable = false, hitDoor = false, hitSwitch = false;
            hitRotatable = false;
            if (ModeSupportsInteraction())
            {
                hitExaminable = interactableItem && interactableItem.isExaminable;
                hitSittable = hitTag == "Sittable" && Vector3.Distance(hitInfo.transform.position, transform.position) < SitDistance;
                hitDoor = hitTag == "Door";
                hitSwitch = hitTag == "Switch";
            }

            if (currentInteractable && currentInteractable != interactableItem)
            {
                currentInteractable.ApplyCustomEffects(ActionTiming.onHoverExit);
            }
            currentInteractable = interactableItem; // TODO: figure out the appropriate place for this

            bool canHover = false, canExamine = false, canOutline = false;
            if (currentInteractable != null && ModeSupportsInteraction())
            {
                currentInteractable.ApplyCustomEffects(ActionTiming.onHover);

                // Manage outline
                Color? outlineColor = null;
                canHover = !currentInteractable.focusAffectsHover || currentInteractable.MatchesCurrentFocus();
                canOutline = canHover && currentInteractable.showOutline;
                canExamine = !currentInteractable.focusAffectsExamine || currentInteractable.MatchesCurrentFocus();

                if (currentInteractable.isExaminable && canExamine && canOutline) outlineColor = DefaultExaminableColor;
                if (currentInteractable.outlineColorOverride != Color.clear && canOutline)
                    outlineColor = currentInteractable.outlineColorOverride;
                if (outlineColor != null) {
                    if (GS.interactionMode != InteractionType.Focus)
                    {
                        SetOutlined(currentObject, (Color)outlineColor, currentInteractable.useSpriteOutline);
                    }
                } else {
                    ClearOutlined();
                }

                // Manage cursor
                if (currentInteractable.setCursor){
                    if (currentInteractable.cursorOverride && canHover) CursorImage.sprite = currentInteractable.cursorOverride;
                    else if (currentInteractable.isExaminable)
                    {
                        if (canExamine && canHover) CursorImage.sprite = InspectIcon;
                    }
                    else CursorImage.sprite = canHover ? HoverIcon : DefaultIcon;
                }
            } else
            {
                if (!hitSittable) ClearOutlined();
                CursorImage.sprite = DefaultIcon;
            }

            #nullable disable

            // TODO: refactor some of this logic to merge with the code block above, for better clarity
            if (!(hitExaminable || hitSittable || hitDoor || hitSwitch)) // Reset if nothing hit
            {
                hitExaminable = false;
                hitSittable = false;
            }
            else if (hitSittable)
            { // Highlight for sittable objects
                CursorImage.sprite = SitIcon;
                SetOutlined(hitInfo.transform.gameObject, DefaultSittableColor);
            }
            else if (hitDoor) { // Set cursor for doors
                CursorImage.sprite = DoorIcon;
            }
            else if (hitSwitch) { // Set cursor for light switches
                CursorImage.sprite = HoverIcon;
            }

            // Set cursor and hit flag for rotating objects during examination
            if (GS.interactionMode == InteractionType.Examine)
            {
                int? hitLayer = hitInfo.transform?.gameObject.layer;
                hitRotatable = hitLayer == examineLayerIdx;
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
                else if (hitExaminable && canExamine)
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

                currentInteractable?.ApplyCustomEffects(ActionTiming.onClick); // Update during examination

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
                else if (_input.exit && GS.interactionMode != InteractionType.Journal)
                {
                    _input.exit = false;
                    _input.anyKey = false;
                    PauseManager.instance.PauseGame();
                }
            }
            else if (_playerInput.actions["interact"].IsPressed() == false)
            {
                isDragging = false;
            }
		}

        /* HELPER METHODS */

        private void SetLayer(GameObject targetObject, int layerIdx = 0)
        {
            targetObject.layer = layerIdx;
            foreach (Transform child in targetObject.transform)
            {
                child.gameObject.layer = layerIdx;
            }
        }

        private void SetOutlined(GameObject gameObject, Color outlineColor, bool isSprite = false)
        {
            if (outlinedObject != gameObject) ClearOutlined();
            ApplyOutline(gameObject, outlineColor, isSprite);
            outlinedObject = gameObject;
        }
        public void ApplyOutline(GameObject gameObject, Color outlineColor, bool isSprite = false)
        {
            if (FocusManager.HasActiveFocus()) FocusManager.SetFocused(gameObject);

            if (isSprite)
            {
                Material objectMaterial = gameObject.GetComponent<MeshRenderer>().materials[0];
                if (spriteOutliner != null && objectMaterial.shader != spriteOutliner)
                {
                    prevShader = objectMaterial.shader;
                    objectMaterial.shader = spriteOutliner;
                    objectMaterial.SetColor("_SolidOutline", outlineColor);
                    gameObject.transform.localScale *= 1.1f;
                    SetLayer(gameObject, outlineLayerIdx);
                }
            }
            else
            {
                Outline outlineComponent = gameObject.GetComponent<Outline>();
                if (outlineComponent == null) outlineComponent = gameObject.AddComponent<Outline>();
                else if (outlineComponent.enabled == true) return;

                outlineComponent.OutlineColor = outlineColor;
                outlineComponent.OutlineMode = Outline.Mode.OutlineVisible;
                outlineComponent.OutlineWidth = 8f;

                outlineComponent.enabled = true;

                // Render object outline on top of postprocessing outline
                // if (outlinesRenderFeature.renderPassEvent != RenderPassEvent.BeforeRenderingTransparents)
                // {
                //     outlinesRenderFeature.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
                //     rendererData.SetDirty();
                // }
            }

        }
        private void ClearOutlined()
        {
            if (outlinedObject != null)
            {
                InteractableItem interactable = outlinedObject.GetComponent<InteractableItem>();
                if (interactable != null && interactable.MatchesCurrentFocus()) return;

                RemoveOutline(outlinedObject, (interactable != null) ? interactable.useSpriteOutline : false);
                outlinedObject = null;

                // Restore to setting that works better for decal lighting effects
                // renderFeature.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
                // rendererData.SetDirty();
            }
        }
        public void RemoveOutline(GameObject gameObject, bool isSprite = false)
        {
            FocusManager.ClearFocused(gameObject);

            if (isSprite && prevShader != null)
            {
                MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
                Material objectMaterial = renderer.materials[0];
                objectMaterial.shader = prevShader;
                prevShader = null;

                gameObject.transform.localScale /= 1.1f;

                if (GS.interactionMode != InteractionType.Examine)
                    SetLayer(gameObject, 0);
            }
            else
            {
                Outline outlineComponent = gameObject.GetComponent<Outline>();
                if (outlineComponent) outlineComponent.enabled = false;
            }
        }
        public void ClearAllOutlines(List<GameObject> gameObjects)
        {
            foreach (GameObject outlinedObject in gameObjects)
            {
                Outline outlineComponent = outlinedObject.GetComponent<Outline>();
                if (outlineComponent) outlineComponent.enabled = false;
            }
        }

        /* EXAMINE METHODS */

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

            activeObject = currentInteractable.interactionParent;
            if (!activeObject) activeObject = currentObject;

            activeParent = activeObject.transform.parent?.gameObject;
            activeObject.transform.SetParent(ExamineTarget);

            startPosition = activeObject.transform.position;
            startScale = activeObject.transform.localScale;
            startRotation = activeObject.transform.rotation;

            targetStartRotation = ExamineTarget.transform.rotation;
            targetStartPosition = ExamineTarget.transform.position;

            ClearOutlined();
            SetLayer(activeObject, examineLayerIdx);

            currentInteractable.ExpandHotspot(false);

            UI.FadeInMatte();
            UI.FadeInInteractionUI();

            isReadyToInspect = false;
            GS.interactionMode = InteractionType.Examine;

            rotationSpace = currentInteractable.orientToCamera ? Space.Self : Space.World;
            rotationAxis = currentInteractable.rotationAxis;
            panningEnabled = currentInteractable.enablePanning;
            inertialRotation = currentInteractable.inertialRotation;
            useGrabCursor = currentInteractable.useGrabCursor;
            affectsBodyBattery = currentInteractable.affectsBodyBattery;

            TakeItemButton.SetActive(currentInteractable.keepAfterExamining);

            Player.LockPlayer();
            UI.UnlockCursor(GetRotationIcon());

            examineCallback = null;
            examineCallback += () => {
                isReadyToInspect = true;
                activeObject.transform.SetParent(ExamineTarget);
            };

            // Compensate for visually off-center objects
            Vector3 targetPosition;
            if (currentInteractable.pivotAroundVisualCenter)
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
                currentInteractable.orientToCamera ? ExamineTarget.rotation : startRotation,
                startScale * currentInteractable.scaleOnInteraction,
                pickUpDuration
            ));
        }

        public void TakeItem()
        {
            ExitExamination(true);
            AudioManager.instance.PlaySFX(TakeItemSFX);
        }

        private void ExitExamination(bool doTakeItem = false)
        {
            if (GS.interactionMode != InteractionType.Examine) return;
            StopAllCoroutines();

            RestoreTargetPosition(putDownDuration); // Important that this duration <= putDownDuration
            ZoomManager.ResetZoom();

            UI.FadeOutMatte();
            UI.FadeOutInteractionUI();

            InteractableItem activeInteractable = activeObject.GetComponent<InteractableItem>();
            if (activeInteractable == null)
                activeInteractable = activeObject.GetComponentInChildren<InteractableItem>();

            lastLook = Vector2.zero;

            examineCallback = null;
            examineCallback += () => {
                // TOOD: consider not waiting for animation to complete, at least for journal entries
                activeInteractable.ApplyCustomEffects(doTakeItem ? ActionTiming.afterTaking : ActionTiming.afterExamine); // Update after examination
    
                if (activeInteractable.journalUpdates.Count == 0)
                {
                    // Add default entry
                    JournalManager.Main.AddToJournal("");
                }

                if (doTakeItem)
                {
                    Destroy(activeObject);
                } else
                {
                    SetLayer(activeObject, 0);
                    activeObject.transform.SetParent(activeParent.transform);
                    activeInteractable.ExpandHotspot(true);
                }

                ExamineTarget.transform.rotation = targetStartRotation;
                
                if (GS.interactionMode != InteractionType.Journal && GS.interactionMode != InteractionType.Paused)
                    Player.UnlockPlayer();

                activeObject = null;
                activeParent = null;
                examineCallback = null;
            };

            if (doTakeItem)
            {
                StartCoroutine(MoveForDuration(
                    activeObject,
                    activeObject.transform.position + Vector3.down * 2f,
                    activeObject.transform.rotation,
                    activeObject.transform.localScale * 3.5f,
                    putDownDuration * 2f));
            }
            else
            {
                StartCoroutine(MoveForDuration(activeObject, startPosition, startRotation, startScale, putDownDuration));
            }

            UI.LockCursor();
            GS.interactionMode = YarnDispatcher.YarnSpinnerIsActive() ? InteractionType.Monologue : InteractionType.Default;
        }

        public void OnLook(InputValue value)
		{
			if (GS.interactionMode == InteractionType.Examine && isReadyToInspect)
			{
                Vector2 look = value.Get<Vector2>();
                if (isDragging)
                {
                    lastLook = look;
                    if (rotationAxis == RotationAxis.LeftRight || rotationAxis == RotationAxis.All)
                    {
                        ExamineTarget.Rotate(Vector3.down * look.x * rotationSpeed, rotationSpace);
                    } 
                    if (rotationAxis == RotationAxis.UpDown || rotationAxis == RotationAxis.All)
                    {
                        ExamineTarget.Rotate(-Camera.main.transform.right * look.y * rotationSpeed, Space.World);
                    }
                    if (affectsBodyBattery) BodyBatteryManager.Main.FidgetToRecover();
                }
                else if (panningEnabled && !isAnimating && hitRotatable && ZoomManager.GetZoom() < 1)
                {
                    ExamineTarget.transform.position = ExamineTarget.transform.position
                        - Camera.main.transform.right * look.x * PanSpeed
                        + Camera.main.transform.up * look.y * PanSpeed;
                }
			}
		}

        private Texture2D GetRotationIcon() {
            if (useGrabCursor)
            {
                return isDragging ? HandGrabbing : HandOpen;
            }
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

        /* SITTING METHODS */

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
    }
}
