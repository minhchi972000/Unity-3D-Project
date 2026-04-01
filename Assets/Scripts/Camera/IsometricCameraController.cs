using UnityEngine;
using FarmValley.Utils;

namespace FarmValley.Camera
{
    /// <summary>
    /// Isometric/3/4 top-down camera controller with pan and zoom.
    /// Supports both touch (mobile) and mouse input.
    /// </summary>
    public class IsometricCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float cameraAngle = 45f;
        [SerializeField] private float cameraHeight = 15f;
        [SerializeField] private float cameraDistance = 15f;

        [Header("Movement")]
        [SerializeField] private float panSpeed = 15f;
        [SerializeField] private float panSmoothing = 8f;
        [SerializeField] private Vector2 panLimitX = new Vector2(-10f, 10f);
        [SerializeField] private Vector2 panLimitZ = new Vector2(-10f, 10f);

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 8f;
        [SerializeField] private float maxZoom = 25f;
        [SerializeField] private float zoomSmoothing = 5f;

        [Header("Player Follow")]
        [SerializeField] private Transform playerTarget;
        [SerializeField] private bool followPlayer = true;
        [SerializeField] private float followSmoothing = 6f;

        private Vector3 targetPosition;
        private float targetZoom;
        private UnityEngine.Camera cam;

        // Touch input tracking
        private Vector2 lastTouchPos;
        private bool isDragging = false;
        private float lastPinchDistance;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            if (cam == null)
                cam = UnityEngine.Camera.main;
        }

        private void Start()
        {
            targetPosition = transform.position;
            targetZoom = cam.orthographic ? cam.orthographicSize : cameraDistance;
            SetupCameraAngle();
        }

        private void SetupCameraAngle()
        {
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(0, cameraHeight, -cameraDistance * Mathf.Cos(angleRad));
            transform.position = targetPosition + offset;
            transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
        }

        private void Update()
        {
            if (followPlayer && playerTarget != null)
            {
                // Follow player position
                Vector3 playerPos = new Vector3(playerTarget.position.x, 0f, playerTarget.position.z);
                targetPosition = Vector3.Lerp(targetPosition, playerPos, followSmoothing * Time.deltaTime);
            }

            HandleInput();
            SmoothFollow();
        }

        private void HandleInput()
        {
            // If following player, only handle zoom (not WASD pan)
            if (followPlayer && playerTarget != null)
            {
                HandleZoomOnlyInput();
                return;
            }

            // Mobile touch input
            if (InputCompat.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleZoomOnlyInput()
        {
            // Touch pinch zoom
            if (InputCompat.touchCount == 2)
            {
                Vector2 pos0 = InputCompat.GetTouchPosition(0);
                Vector2 pos1 = InputCompat.GetTouchPosition(1);
                float currentPinchDistance = Vector2.Distance(pos0, pos1);

                if (InputCompat.GetTouchPhase(1) == TouchPhase.Began)
                {
                    lastPinchDistance = currentPinchDistance;
                    return;
                }

                float pinchDelta = lastPinchDistance - currentPinchDistance;
                ZoomCamera(pinchDelta * 0.01f);
                lastPinchDistance = currentPinchDistance;
            }

            // Mouse scroll zoom
            float scroll = InputCompat.GetScrollWheel();
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ZoomCamera(-scroll * zoomSpeed * 5f);
            }
        }

        private void HandleTouchInput()
        {
            if (InputCompat.touchCount == 1)
            {
                Vector2 touchPos = InputCompat.GetTouchPosition(0);
                TouchPhase phase = InputCompat.GetTouchPhase(0);

                switch (phase)
                {
                    case TouchPhase.Began:
                        lastTouchPos = touchPos;
                        isDragging = true;
                        break;

                    case TouchPhase.Moved:
                        if (isDragging)
                        {
                            Vector2 delta = touchPos - lastTouchPos;
                            PanCamera(-delta.x, -delta.y);
                            lastTouchPos = touchPos;
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        isDragging = false;
                        break;
                }
            }
            else if (InputCompat.touchCount == 2)
            {
                // Pinch to zoom
                Vector2 pos0 = InputCompat.GetTouchPosition(0);
                Vector2 pos1 = InputCompat.GetTouchPosition(1);

                float currentPinchDistance = Vector2.Distance(pos0, pos1);

                if (InputCompat.GetTouchPhase(1) == TouchPhase.Began)
                {
                    lastPinchDistance = currentPinchDistance;
                    isDragging = false;
                    return;
                }

                float pinchDelta = lastPinchDistance - currentPinchDistance;
                ZoomCamera(pinchDelta * 0.01f);
                lastPinchDistance = currentPinchDistance;
            }
        }

        private void HandleMouseInput()
        {
            // Right-click drag to pan
            if (InputCompat.GetMouseButton(1) || (InputCompat.GetMouseButton(0) && InputCompat.GetKey(KeyCode.LeftAlt)))
            {
                float mouseX = InputCompat.GetMouseAxisX();
                float mouseY = InputCompat.GetMouseAxisY();
                PanCamera(-mouseX * 0.5f, -mouseY * 0.5f);
            }

            // Scroll wheel to zoom
            float scroll = InputCompat.GetScrollWheel();
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ZoomCamera(-scroll * zoomSpeed * 5f);
            }

            // WASD/Arrow keys for panning
            float h = 0f, v = 0f;
            if (InputCompat.GetKey(KeyCode.W) || InputCompat.GetKey(KeyCode.UpArrow)) v = 1f;
            if (InputCompat.GetKey(KeyCode.S) || InputCompat.GetKey(KeyCode.DownArrow)) v = -1f;
            if (InputCompat.GetKey(KeyCode.A) || InputCompat.GetKey(KeyCode.LeftArrow)) h = -1f;
            if (InputCompat.GetKey(KeyCode.D) || InputCompat.GetKey(KeyCode.RightArrow)) h = 1f;

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                PanCamera(h * 3f, v * 3f);
            }
        }

        private void PanCamera(float deltaX, float deltaZ)
        {
            // Convert screen-space pan to world-space movement (accounting for camera rotation)
            Vector3 right = transform.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up).normalized;

            Vector3 move = (right * deltaX + forward * deltaZ) * panSpeed * Time.deltaTime;
            targetPosition += move;

            // Clamp to bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, panLimitX.x, panLimitX.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, panLimitZ.x, panLimitZ.y);
        }

        private void ZoomCamera(float delta)
        {
            targetZoom += delta * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        private void SmoothFollow()
        {
            // Smooth position
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            float currentDistance = cam.orthographic ? cameraDistance : Mathf.Lerp(cameraDistance, targetZoom, 1f);
            Vector3 offset = new Vector3(0, cameraHeight, -currentDistance * Mathf.Cos(angleRad));
            Vector3 desiredPos = targetPosition + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * panSmoothing);

            // Smooth zoom
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothing);
            }
        }

        /// <summary>
        /// Snap camera to look at a specific world position
        /// </summary>
        public void LookAt(Vector3 worldPos)
        {
            targetPosition = new Vector3(worldPos.x, targetPosition.y, worldPos.z);
        }

        /// <summary>
        /// Set the player transform for camera to follow
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            playerTarget = target;
            followPlayer = target != null;
        }

        /// <summary>
        /// Toggle player follow mode
        /// </summary>
        public void SetFollowPlayer(bool follow)
        {
            followPlayer = follow;
        }
    }
}
