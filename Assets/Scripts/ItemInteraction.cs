using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

namespace StarterAssets
{
    public class ItemInteraction : MonoBehaviour
    {
        public Transform ExamineTarget;
        public float RaycastReach;
        public Canvas BackgroundMatte;
        public GameObject weirdLibraryParent;
        public Camera playerCamera;
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
        private SelectionOutlineController selectionOutlineController;
		private RaycastHit hitInfo;
        private int layerNumber;

        private GameObject currentObject;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool isInspecting = false;
        private bool isReadyToInspect = false;

        private bool sitting = false;
        private GameObject currentSeat;
        private float characterHeight;

        private float timeCount = 0.0f;

        private const float _threshold = 0.01f;

        private float _rotationVelocity;

        private float _yaw;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();
            selectionOutlineController = Camera.main.GetComponent<SelectionOutlineController>();
            layerNumber = LayerMask.NameToLayer("Examine Object");
            characterHeight = GetComponent<CharacterController>().height;

            if (weirdLibraryParent) {
                weirdLibraryParent.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_input.focus) {

            }
            Interact();
        }

		private void Interact() {
            Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, RaycastReach);

            string hitTag = hitInfo.transform?.tag;
            bool hitInteractable = hitTag == "Interactable";
            bool hitSittable = hitTag == "Sittable";

            if (hitInteractable)
            {
                CursorImage.sprite = InspectIcon;
                selectionOutlineController.FilterByTag = "Interactable";
                selectionOutlineController.OutlineColor = InteractableOutlineColor;
                selectionOutlineController.OccludedColor = InteractableOccludedColor;
            }
            else if (hitSittable) {
                CursorImage.sprite = SitIcon;
                selectionOutlineController.FilterByTag = "Sittable";
                selectionOutlineController.OutlineColor = SittableOutlineColor;
                selectionOutlineController.OccludedColor = SittableOccludedColor;
            }
            else
            {
                CursorImage.sprite = DefaultIcon;
            }

            if (_input.interact) {
				if (hitInteractable && !isInspecting)
                {
                    Debug.Log("Initiating examine object...");
                    StopAllCoroutines();
                    if (currentObject) {
                        currentObject.transform.position = startPosition;
                        currentObject.transform.rotation = startRotation;
                    }

                    currentObject = hitInfo.transform.gameObject;
                    startPosition = hitInfo.transform.position;
                    startRotation = hitInfo.transform.rotation;

                    _playerInput.actions.FindAction("Move").Disable();

                    _input.cursorInputForLook = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.SetCursor(RotateIcon, Vector2.zero, CursorMode.ForceSoftware);

                    CursorImage.enabled = false;
                    
                    if (selectionOutlineController) {
                        selectionOutlineController.enabled = false;
                    }

                    currentObject.GetComponent<Collider>().enabled = false;
                    currentObject.layer = layerNumber;
                    foreach (Transform child in currentObject.transform)
                    {
                        child.gameObject.layer = layerNumber;
                    }

                    BackgroundMatte.enabled = true;

                    isReadyToInspect = false;
                    isInspecting = true;

                    StartCoroutine(MoveToTarget(currentObject, ExamineTarget.position, startRotation, 6.0f));

                    // hitInfo.transform.gameObject.GetComponent<YarnInteractable>().StartConversation();
                }
                else if (isInspecting) {
                    Debug.Log("Exiting examination...");
                    StopAllCoroutines();

                    currentObject.GetComponent<Collider>().enabled = true;
                    currentObject.layer = 0;
                    foreach (Transform child in currentObject.transform)
                    {
                        child.gameObject.layer = 0;
                    }

                    BackgroundMatte.enabled = false;

                    StartCoroutine(MoveToTarget(currentObject, startPosition, startRotation, 8.0f));

                    _playerInput.actions.FindAction("Move").Enable();

                    _input.cursorInputForLook = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

                    CursorImage.enabled = true;

                    if (selectionOutlineController) {
                        selectionOutlineController.enabled = true;
                    }

                    currentObject = null;
                    isInspecting = false;
                }
                else if (hitSittable)
                {
                    if (!sitting)
                    {
                        Debug.Log("Clicked to sit");

                        currentSeat = hitInfo.transform.gameObject;
                        currentSeat.GetComponent<BoxCollider>().enabled = false;
                        _playerInput.enabled = false;

                        StartCoroutine(ReenableControls(2.2f, "sit"));
                        transform.DOMove(hitInfo.transform.GetChild(0).position, 1.5f);
                        StartCoroutine(RotatePlayerToChair(3f, hitInfo.transform.GetChild(0).rotation));
                        StartCoroutine(HeightChange("shrink", 0.5f));
                        StartCoroutine(SitShake(1.1f));

                        //Show weird library

                        if (weirdLibraryParent) {
                            weirdLibraryParent.SetActive(true);
                        }
                        sitting = true;
                        gameObject.GetComponent<FirstPersonController>().sitting = true;
                    }
                }
                _input.interact = false;
			}
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

            _playerInput.enabled = true;
            
            if (action == "sit") { 
            _playerInput.SwitchCurrentActionMap("Sitting");

            }
            else if (action == "stand")
            {
                _playerInput.SwitchCurrentActionMap("Player");

            }


        }
        
        public void OnMove(InputValue value)
        {
            if (sitting)
            {

                _playerInput.enabled = false;

                StartCoroutine(HeightChange("grow", 0.25f));
                if (weirdLibraryParent) {
                    weirdLibraryParent.SetActive(false);
                }

                StartCoroutine(WaitAndReactivateChair(1.5f));
                StartCoroutine(ReenableControls(0.5f, "stand"));
            }

            sitting = false;
            gameObject.GetComponent<FirstPersonController>().sitting = false;


        }

    IEnumerator WaitAndReactivateChair(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        currentSeat.GetComponent<BoxCollider>().enabled = true;
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
			if (isInspecting && isReadyToInspect)
			{
				Vector2 look = value.Get<Vector2>();
                float rotationSpeed = 9.0f;

                currentObject.transform.Rotate(Vector3.down * look.x * rotationSpeed, Space.World);
                currentObject.transform.Rotate(Vector3.left * look.y * rotationSpeed, Space.World);
			}
		}

        IEnumerator MoveToTarget(GameObject movedObject, Vector3 targetPosition, Quaternion targetRotation, float speed, float rotationSpeed = 200.0f) {
            while (movedObject.transform.position != targetPosition || movedObject.transform.rotation != targetRotation) {
                movedObject.transform.position = Vector3.MoveTowards(movedObject.transform.position, targetPosition, Time.deltaTime * speed);
                movedObject.transform.rotation = Quaternion.RotateTowards(movedObject.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                yield return null;
            }
            isReadyToInspect = true;
        }
    }
}
