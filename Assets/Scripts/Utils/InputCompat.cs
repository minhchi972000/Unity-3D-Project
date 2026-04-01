using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FarmValley.Utils
{
    /// <summary>
    /// Input compatibility layer that works with both the legacy Input Manager
    /// and the new Input System package.
    /// 
    /// If your project uses "Input System Package (New)" in Player Settings,
    /// this class uses UnityEngine.InputSystem APIs.
    /// If it uses "Input Manager (Old)" or "Both", it uses UnityEngine.Input.
    /// </summary>
    public static class InputCompat
    {
        // ===== Keyboard =====

        public static bool GetKey(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            var k = KeyCodeToKey(key);
            return k != Key.None && keyboard[k].isPressed;
#else
            return Input.GetKey(key);
#endif
        }

        public static bool GetKeyDown(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            var k = KeyCodeToKey(key);
            return k != Key.None && keyboard[k].wasPressedThisFrame;
#else
            return Input.GetKeyDown(key);
#endif
        }

        // ===== Mouse =====

        public static bool GetMouseButton(int button)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var mouse = Mouse.current;
            if (mouse == null) return false;
            switch (button)
            {
                case 0: return mouse.leftButton.isPressed;
                case 1: return mouse.rightButton.isPressed;
                case 2: return mouse.middleButton.isPressed;
                default: return false;
            }
#else
            return Input.GetMouseButton(button);
#endif
        }

        public static bool GetMouseButtonDown(int button)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var mouse = Mouse.current;
            if (mouse == null) return false;
            switch (button)
            {
                case 0: return mouse.leftButton.wasPressedThisFrame;
                case 1: return mouse.rightButton.wasPressedThisFrame;
                case 2: return mouse.middleButton.wasPressedThisFrame;
                default: return false;
            }
#else
            return Input.GetMouseButtonDown(button);
#endif
        }

        public static Vector3 mousePosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                var mouse = Mouse.current;
                if (mouse == null) return Vector3.zero;
                Vector2 pos = mouse.position.ReadValue();
                return new Vector3(pos.x, pos.y, 0f);
#else
                return Input.mousePosition;
#endif
            }
        }

        public static float GetMouseAxisX()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var mouse = Mouse.current;
            if (mouse == null) return 0f;
            return mouse.delta.x.ReadValue() * 0.05f; // Scale to approximate legacy Input.GetAxis range
#else
            return Input.GetAxis("Mouse X");
#endif
        }

        public static float GetMouseAxisY()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var mouse = Mouse.current;
            if (mouse == null) return 0f;
            return mouse.delta.y.ReadValue() * 0.05f;
#else
            return Input.GetAxis("Mouse Y");
#endif
        }

        public static float GetScrollWheel()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var mouse = Mouse.current;
            if (mouse == null) return 0f;
            return mouse.scroll.y.ReadValue() / 120f * 0.1f; // Normalize to approximate legacy range
#else
            return Input.GetAxis("Mouse ScrollWheel");
#endif
        }

        // ===== Touch =====

        public static int touchCount
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
#else
                return Input.touchCount;
#endif
            }
        }

        public static Touch GetTouch(int index)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // Return a default Touch - for new Input System, use GetTouchPosition/GetTouchPhase instead
            return default;
#else
            return Input.GetTouch(index);
#endif
        }

        public static Vector2 GetTouchPosition(int index)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            if (index < touches.Count)
                return touches[index].screenPosition;
            return Vector2.zero;
#else
            return Input.GetTouch(index).position;
#endif
        }

        public static TouchPhase GetTouchPhase(int index)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
            if (index >= touches.Count) return TouchPhase.Canceled;
            var phase = touches[index].phase;
            switch (phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began: return TouchPhase.Began;
                case UnityEngine.InputSystem.TouchPhase.Moved: return TouchPhase.Moved;
                case UnityEngine.InputSystem.TouchPhase.Stationary: return TouchPhase.Stationary;
                case UnityEngine.InputSystem.TouchPhase.Ended: return TouchPhase.Ended;
                case UnityEngine.InputSystem.TouchPhase.Canceled: return TouchPhase.Canceled;
                default: return TouchPhase.Canceled;
            }
#else
            return Input.GetTouch(index).phase;
#endif
        }

        // ===== Pointer over UI =====

        public static bool IsPointerOverUI()
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null) return false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // For new Input System, check if pointer is over a UI element
            if (Mouse.current != null)
            {
                return eventSystem.IsPointerOverGameObject();
            }
            return false;
#else
            return eventSystem.IsPointerOverGameObject();
#endif
        }

        public static bool IsPointerOverUI(int fingerId)
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null) return false;
            return eventSystem.IsPointerOverGameObject(fingerId);
        }

        // ===== Key conversion helper =====

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private static Key KeyCodeToKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.W: return Key.W;
                case KeyCode.A: return Key.A;
                case KeyCode.S: return Key.S;
                case KeyCode.D: return Key.D;
                case KeyCode.E: return Key.E;
                case KeyCode.Q: return Key.Q;
                case KeyCode.R: return Key.R;
                case KeyCode.F: return Key.F;
                case KeyCode.Space: return Key.Space;
                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.LeftAlt: return Key.LeftAlt;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                default: return Key.None;
            }
        }
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Call this once on startup to enable enhanced touch support for the new Input System.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnableEnhancedTouch()
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        }
#endif
    }
}
