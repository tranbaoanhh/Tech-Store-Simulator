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
        [SerializeField, Min(0f)] float jumpHeight = 1.2f;

        [Header("Crouch")]
        [SerializeField, Min(0.1f)] float standingHeight = 1.8f;
        [SerializeField, Min(0.1f)] float crouchingHeight = 1f;
        [SerializeField, Min(0f)] float crouchSpeed = 2f;
        [SerializeField, Min(0.1f)] float stanceTransitionSpeed = 4f;
        [Header("Interaction")]
        [SerializeField] float interactionDistance = 3f;
        [SerializeField] Camera playerCamera;
        CharacterController controller;
        float yVelocity;
        float pitch;
        bool isCrouching;
        Vector3 standingCenter;
        Vector3 standingCameraLocalPosition;
        string hint = "";

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            // Preserve the existing capsule's foot position, including offset centers.
            standingHeight = Mathf.Max(standingHeight, controller.radius * 2f);
            crouchingHeight = Mathf.Clamp(crouchingHeight, controller.radius * 2f, standingHeight);
            standingCenter = controller.center + Vector3.up * (standingHeight - controller.height) * 0.5f;
            controller.height = standingHeight;
            controller.center = standingCenter;
            if (playerCamera != null)
                standingCameraLocalPosition = playerCamera.transform.localPosition;
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
            UpdateCrouch();
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

        void UpdateCrouch()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (!isCrouching) isCrouching = true;
                else if (CanStand()) isCrouching = false;
            }

            // Recheck while rising in case the player moves under a ceiling.
            if (!isCrouching && controller.height < standingHeight && !CanStand())
                isCrouching = true;

            float targetHeight = isCrouching ? crouchingHeight : standingHeight;
            float height = Mathf.MoveTowards(controller.height, targetHeight,
                stanceTransitionSpeed * Time.deltaTime);
            controller.height = height;
            controller.center = standingCenter + Vector3.up * (height - standingHeight) * 0.5f;

            if (playerCamera != null)
            {
                Vector3 offset = transform.TransformVector(Vector3.up * (height - standingHeight));
                Transform cameraParent = playerCamera.transform.parent;
                playerCamera.transform.localPosition = standingCameraLocalPosition +
                    (cameraParent != null ? cameraParent.InverseTransformVector(offset) : offset);
            }
        }

        bool CanStand()
        {
            Vector3 scale = transform.lossyScale;
            float radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float height = Mathf.Max(standingHeight * Mathf.Abs(scale.y), radius * 2f);
            Vector3 center = transform.TransformPoint(standingCenter);
            Vector3 endOffset = transform.up * (height * 0.5f - radius);
            // A small inset avoids treating contact with the floor as an obstruction.
            float inset = Mathf.Min(controller.skinWidth * Mathf.Abs(scale.y), radius * 0.1f);
            Collider[] overlaps = Physics.OverlapCapsule(center - endOffset, center + endOffset,
                Mathf.Max(0.001f, radius - inset), Physics.AllLayers, QueryTriggerInteraction.Ignore);
            foreach (Collider obstacle in overlaps)
            {
                if (obstacle == controller || obstacle.transform.IsChildOf(transform)) continue;
                if (Physics.GetIgnoreLayerCollision(gameObject.layer, obstacle.gameObject.layer) ||
                    Physics.GetIgnoreCollision(controller, obstacle)) continue;
                return false;
            }
            return true;
        }

        void Move()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 direction = (transform.right * h + transform.forward * v).normalized;
            if (controller.isGrounded && yVelocity < 0) yVelocity = -2f;
            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && !isCrouching &&
                controller.height >= standingHeight && gravity < 0f)
                yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            yVelocity += gravity * Time.deltaTime;
            float speed = isCrouching || controller.height < standingHeight ? crouchSpeed : moveSpeed;
            CollisionFlags flags = controller.Move((direction * speed + Vector3.up * yVelocity) * Time.deltaTime);
            if ((flags & CollisionFlags.Above) != 0 && yVelocity > 0f) yVelocity = 0f;
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
            GUI.Label(new Rect(15, Screen.height - 35, 700, 25), "WASD: Di chuyen | Chuot: Nhin | Space: Nhay | C: Ngoi/Dung | E: Tuong tac | Esc: Mo chuot");
        }
    }
}
