using UnityEngine;

namespace TechStoreSimulator
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] float moveSpeed = 4.5f;
        [SerializeField] float mouseSensitivity = 2f;
        [SerializeField] float gravity = -20f;
        [Header("Interaction")]
        [SerializeField] float interactionDistance = 3f;
        [SerializeField] Camera playerCamera;
        CharacterController controller;
        float yVelocity;
        float pitch;
        string hint = "";

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool locked = Cursor.lockState != CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !locked;
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;
            Look();
            Move();
            Interact();
        }

        void Look()
        {
            float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            transform.Rotate(Vector3.up * yaw);
            pitch = Mathf.Clamp(pitch - mouseY, -80f, 80f);
            if (playerCamera != null) playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        void Move()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 direction = (transform.right * h + transform.forward * v).normalized;
            if (controller.isGrounded && yVelocity < 0) yVelocity = -2f;
            yVelocity += gravity * Time.deltaTime;
            controller.Move((direction * moveSpeed + Vector3.up * yVelocity) * Time.deltaTime);
        }

        void Interact()
        {
            hint = "";
            if (playerCamera == null) return;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                // Interactable on hit object or its parent
                var target = hit.collider.GetComponentInParent<ProductInteraction>();
                if (target != null)
                {
                    hint = "[E] Mua " + target.DisplayName;
                    if (Input.GetKeyDown(KeyCode.E)) target.TryBuy();
                }
            }
        }

        void OnGUI()
        {
            GUI.Label(new Rect(Screen.width / 2f - 6, Screen.height / 2f - 8, 40, 30), "+");
            if (!string.IsNullOrEmpty(hint))
                GUI.Box(new Rect(Screen.width / 2f - 150, Screen.height - 95, 300, 40), hint);
            GUI.Label(new Rect(15, Screen.height - 35, 440, 25), "WASD: Di chuyen | Chuot: Nhin | E: Tuong tac | Esc: Mo chuot");
        }
    }
}
