using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StarterAssets
{
    public class ItemInteraction : MonoBehaviour
    {
        public Transform ExamineTarget;
        public float RaycastReach;
        public Canvas BackgroundMatte;

        [Header("Cursor settings")]
        public Image CursorImage;
		public Sprite DefaultIcon;
		public Sprite InspectIcon;
        public Texture2D RotateIcon;

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

        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<StarterAssetsInputs>();
            _playerInput = GetComponent<PlayerInput>();
            selectionOutlineController = Camera.main.GetComponent<SelectionOutlineController>();
            layerNumber = LayerMask.NameToLayer("Examine Object");
        }

        // Update is called once per frame
        void Update()
        {
            if (_input.focus) {

            }
            Interact();
        }

		private void Interact() {
            bool hitInteractable = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitInfo, RaycastReach) &&
                    hitInfo.transform.tag == "Interactable";
            CursorImage.sprite = hitInteractable ? InspectIcon : DefaultIcon;

			if (_input.interact) {
				if (
                    !isInspecting && hitInteractable
                ){
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

                    currentObject.layer = layerNumber;
                    foreach (Transform child in currentObject.transform)
                    {
                        child.gameObject.layer = layerNumber;
                    }
                    GameObject parentObject = currentObject.transform.parent?.gameObject;
                    if (parentObject) {
                        parentObject.layer = layerNumber;
                    }

                    BackgroundMatte.enabled = true;

                    isReadyToInspect = false;
                    isInspecting = true;

                    StartCoroutine(MoveToTarget(currentObject, ExamineTarget.position, startRotation, 6.0f));

                    // hitInfo.transform.gameObject.GetComponent<YarnInteractable>().StartConversation();
                }
                else if (currentObject) {
                    Debug.Log("Exiting examination...");
                    StopAllCoroutines();

                    currentObject.layer = 0;
                    foreach (Transform child in currentObject.transform)
                    {
                        child.gameObject.layer = 0;
                    }
                    GameObject parentObject = currentObject.transform.parent?.gameObject;
                    if (parentObject) {
                        parentObject.layer = 0;
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
                _input.interact = false;
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
