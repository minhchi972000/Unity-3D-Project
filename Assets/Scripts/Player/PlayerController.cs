using UnityEngine;
using FarmValley.Events;

namespace FarmValley.Player
{
    /// <summary>
    /// Controls player character movement using WASD/Arrow keys.
    /// Handles walking, running, and rotation toward movement direction.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float acceleration = 8f;
        [SerializeField] private float deceleration = 10f;

        [Header("Bounds")]
        [SerializeField] private Vector2 moveLimitX = new Vector2(-20f, 20f);
        [SerializeField] private Vector2 moveLimitZ = new Vector2(-20f, 20f);

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

        private Vector3 moveDirection;
        private float currentSpeed;
        private bool isMoving;
        private bool isRunning;

        // Public properties for animation
        public bool IsMoving => isMoving;
        public bool IsRunning => isRunning;
        public float CurrentSpeed => currentSpeed;
        public float NormalizedSpeed => currentSpeed / runSpeed;
        public Vector3 MoveDirection => moveDirection;

        private void Update()
        {
            HandleMovementInput();
            HandleInteractionInput();
        }

        private void HandleMovementInput()
        {
            // Get input
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical = -1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;

            // Calculate movement direction relative to world (isometric-friendly)
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
            isRunning = Input.GetKey(runKey);

            if (moveDirection.magnitude > 0.1f)
            {
                isMoving = true;
                float targetSpeed = isRunning ? runSpeed : walkSpeed;
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

                // Move character
                Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
                Vector3 newPos = transform.position + movement;

                // Clamp to bounds
                newPos.x = Mathf.Clamp(newPos.x, moveLimitX.x, moveLimitX.y);
                newPos.z = Mathf.Clamp(newPos.z, moveLimitZ.x, moveLimitZ.y);

                transform.position = newPos;

                // Rotate toward movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                isMoving = false;
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(interactKey))
            {
                TryInteract();
            }
        }

        /// <summary>
        /// Try to interact with nearby objects (farm plots, animals, buildings)
        /// </summary>
        private void TryInteract()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);

            float closestDist = float.MaxValue;
            GameObject closestInteractable = null;

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                // Check for FarmPlot
                var plot = hit.GetComponent<Systems.FarmPlot>();
                if (plot != null)
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestInteractable = hit.gameObject;
                    }
                    continue;
                }

                // Check for animal
                var animal = hit.GetComponentInParent<Systems.AnimalIdleAnimation>();
                if (animal != null)
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestInteractable = animal.gameObject;
                    }
                }
            }

            if (closestInteractable != null)
            {
                GameEvents.TriggerPlayerInteract(closestInteractable);
            }
        }

        /// <summary>
        /// Teleport player to a specific world position
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
        }
    }
}
