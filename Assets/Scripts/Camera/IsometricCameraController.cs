using UnityEngine;

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
            HandleInput();
            SmoothFollow();
        }

        private void HandleInput()
        {
            // Mobile touch input
            if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        lastTouchPos = touch.position;
                        isDragging = true;
                        break;

                    case TouchPhase.Moved:
                        if (isDragging)
                        {
                            Vector2 delta = touch.position - lastTouchPos;
                            PanCamera(-delta.x, -delta.y);
                            lastTouchPos = touch.position;
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        isDragging = false;
                        break;
                }
            }
            else if (Input.touchCount == 2)
            {
                // Pinch to zoom
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);

                if (touch1.phase == TouchPhase.Began)
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
            if (Input.GetMouseButton(1) || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt)))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                PanCamera(-mouseX * 0.5f, -mouseY * 0.5f);
            }

            // Scroll wheel to zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ZoomCamera(-scroll * zoomSpeed * 5f);
            }

            // WASD/Arrow keys for panning
            float h = 0f, v = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

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
    }
}
