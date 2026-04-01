using UnityEngine;

namespace FarmValley.Player
{
    /// <summary>
    /// Creates a placeholder 3D character model using Unity primitives.
    /// Generates a cute, cartoon-style farmer character with:
    /// - Body, head, arms, legs, hat, and overalls
    /// All built from basic shapes with colorful materials.
    /// Replace with a proper character model when art is ready.
    /// </summary>
    public class PlayerVisualSetup : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool createOnStart = true;

        private Transform visualRoot;

        private void Start()
        {
            if (createOnStart)
            {
                CreatePlayerVisual();
            }
        }

        public void CreatePlayerVisual()
        {
            // Create root for visual parts
            var root = new GameObject("PlayerVisual");
            root.transform.SetParent(transform);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            visualRoot = root.transform;

            // Colors
            Color skinColor = new Color(1f, 0.87f, 0.73f);       // Peachy skin
            Color shirtColor = new Color(0.20f, 0.60f, 0.86f);   // Blue shirt
            Color overallsColor = new Color(0.24f, 0.47f, 0.20f); // Green overalls
            Color pantsColor = new Color(0.24f, 0.47f, 0.20f);   // Green pants
            Color bootColor = new Color(0.40f, 0.26f, 0.13f);    // Brown boots
            Color hatColor = new Color(0.91f, 0.76f, 0.23f);     // Yellow straw hat
            Color hairColor = new Color(0.55f, 0.27f, 0.07f);    // Brown hair

            // ===== Body (torso) =====
            var body = CreatePart("Body", PrimitiveType.Capsule, root.transform,
                new Vector3(0, 0.85f, 0), new Vector3(0.45f, 0.4f, 0.35f), shirtColor);

            // ===== Overalls bib =====
            var bib = CreatePart("OverallsBib", PrimitiveType.Cube, root.transform,
                new Vector3(0, 0.75f, 0.12f), new Vector3(0.3f, 0.35f, 0.05f), overallsColor);

            // ===== Head =====
            var head = CreatePart("Head", PrimitiveType.Sphere, root.transform,
                new Vector3(0, 1.4f, 0), new Vector3(0.42f, 0.42f, 0.4f), skinColor);

            // ===== Eyes =====
            var leftEye = CreatePart("LeftEye", PrimitiveType.Sphere, root.transform,
                new Vector3(-0.08f, 1.45f, 0.17f), new Vector3(0.07f, 0.08f, 0.05f), Color.white);
            var leftPupil = CreatePart("LeftPupil", PrimitiveType.Sphere, root.transform,
                new Vector3(-0.08f, 1.45f, 0.20f), new Vector3(0.04f, 0.05f, 0.03f), new Color(0.15f, 0.15f, 0.15f));

            var rightEye = CreatePart("RightEye", PrimitiveType.Sphere, root.transform,
                new Vector3(0.08f, 1.45f, 0.17f), new Vector3(0.07f, 0.08f, 0.05f), Color.white);
            var rightPupil = CreatePart("RightPupil", PrimitiveType.Sphere, root.transform,
                new Vector3(0.08f, 1.45f, 0.20f), new Vector3(0.04f, 0.05f, 0.03f), new Color(0.15f, 0.15f, 0.15f));

            // ===== Mouth (smile) =====
            var mouth = CreatePart("Mouth", PrimitiveType.Cube, root.transform,
                new Vector3(0, 1.35f, 0.19f), new Vector3(0.1f, 0.02f, 0.02f), new Color(0.85f, 0.35f, 0.35f));

            // ===== Hat =====
            // Hat brim
            var hatBrim = CreatePart("HatBrim", PrimitiveType.Cylinder, root.transform,
                new Vector3(0, 1.58f, 0), new Vector3(0.55f, 0.03f, 0.55f), hatColor);

            // Hat top
            var hatTop = CreatePart("HatTop", PrimitiveType.Cylinder, root.transform,
                new Vector3(0, 1.68f, 0), new Vector3(0.35f, 0.1f, 0.35f), hatColor);

            // ===== Arms =====
            var leftArm = CreatePart("LeftArm", PrimitiveType.Capsule, root.transform,
                new Vector3(-0.32f, 0.85f, 0), new Vector3(0.14f, 0.25f, 0.14f), skinColor);

            var rightArm = CreatePart("RightArm", PrimitiveType.Capsule, root.transform,
                new Vector3(0.32f, 0.85f, 0), new Vector3(0.14f, 0.25f, 0.14f), skinColor);

            // ===== Legs =====
            var leftLeg = CreatePart("LeftLeg", PrimitiveType.Capsule, root.transform,
                new Vector3(-0.1f, 0.35f, 0), new Vector3(0.16f, 0.25f, 0.16f), pantsColor);

            var rightLeg = CreatePart("RightLeg", PrimitiveType.Capsule, root.transform,
                new Vector3(0.1f, 0.35f, 0), new Vector3(0.16f, 0.25f, 0.16f), pantsColor);

            // ===== Boots =====
            var leftBoot = CreatePart("LeftBoot", PrimitiveType.Cube, root.transform,
                new Vector3(-0.1f, 0.1f, 0.03f), new Vector3(0.14f, 0.15f, 0.2f), bootColor);

            var rightBoot = CreatePart("RightBoot", PrimitiveType.Cube, root.transform,
                new Vector3(0.1f, 0.1f, 0.03f), new Vector3(0.14f, 0.15f, 0.2f), bootColor);

            // Add shadow blob underneath
            var shadow = CreatePart("Shadow", PrimitiveType.Cylinder, root.transform,
                new Vector3(0, 0.01f, 0), new Vector3(0.5f, 0.005f, 0.5f),
                new Color(0, 0, 0, 0.3f));
        }

        private GameObject CreatePart(string name, PrimitiveType type, Transform parent,
            Vector3 localPos, Vector3 localScale, Color color)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent);
            part.transform.localPosition = localPos;
            part.transform.localScale = localScale;

            // Disable collider on visual parts (player has its own collider)
            var col = part.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            // Apply material
            var renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");

                var mat = new Material(shader);
                mat.color = color;
                mat.SetFloat("_Smoothness", 0.2f);
                renderer.material = mat;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            return part;
        }

        public Transform GetVisualRoot()
        {
            return visualRoot;
        }
    }
}
