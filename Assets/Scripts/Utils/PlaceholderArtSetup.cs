using UnityEngine;

namespace FarmValley.Utils
{
    /// <summary>
    /// Utility script to generate placeholder art assets at runtime.
    /// Attach to a GameObject in the scene and call SetupPlaceholderEnvironment() 
    /// to create the farm environment with simple primitives.
    /// 
    /// This allows the prototype to work without any custom 3D assets.
    /// Replace these with proper low-poly models when art is ready.
    /// </summary>
    public class PlaceholderArtSetup : MonoBehaviour
    {
        [Header("Environment Settings")]
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private Material groundMaterial;
        [SerializeField] private Material pathMaterial;

        private void Start()
        {
            if (createOnStart)
            {
                SetupPlaceholderEnvironment();
            }
        }

        public void SetupPlaceholderEnvironment()
        {
            CreateGround();
            CreateFences();
            CreateBarn();
            CreateTrees();
            CreateDecorations();
        }

        private void CreateGround()
        {
            // Main ground
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5f, 1f, 5f);

            var renderer = ground.GetComponent<Renderer>();
            if (groundMaterial != null)
            {
                renderer.material = groundMaterial;
            }
            else
            {
                renderer.material = CreateMaterial(new Color(0.54f, 0.76f, 0.29f)); // Grass green
            }
            renderer.receiveShadows = true;
            ground.isStatic = true;

            // Path from barn to farm
            var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = "Path";
            path.transform.position = new Vector3(-5f, 0.02f, 0f);
            path.transform.localScale = new Vector3(6f, 0.05f, 1.5f);
            var pathRenderer = path.GetComponent<Renderer>();
            if (pathMaterial != null)
                pathRenderer.material = pathMaterial;
            else
                pathRenderer.material = CreateMaterial(new Color(0.84f, 0.80f, 0.78f));
            path.isStatic = true;
        }

        private void CreateFences()
        {
            float gridExtent = Constants.GRID_WIDTH * Constants.CELL_SIZE / 2f + 1f;

            // Fence posts around the farm
            CreateFenceLine(
                new Vector3(-gridExtent, 0, -gridExtent),
                new Vector3(gridExtent, 0, -gridExtent), 8
            );
            CreateFenceLine(
                new Vector3(-gridExtent, 0, gridExtent),
                new Vector3(gridExtent, 0, gridExtent), 8
            );
            CreateFenceLine(
                new Vector3(-gridExtent, 0, -gridExtent),
                new Vector3(-gridExtent, 0, gridExtent), 8
            );
            CreateFenceLine(
                new Vector3(gridExtent, 0, -gridExtent),
                new Vector3(gridExtent, 0, gridExtent), 8
            );
        }

        private void CreateFenceLine(Vector3 start, Vector3 end, int postCount)
        {
            var fenceParent = new GameObject("FenceLine");
            fenceParent.transform.SetParent(transform);
            fenceParent.isStatic = true;

            Material woodMat = CreateMaterial(new Color(0.63f, 0.53f, 0.44f));

            for (int i = 0; i <= postCount; i++)
            {
                float t = (float)i / postCount;
                Vector3 pos = Vector3.Lerp(start, end, t);

                // Post
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.transform.SetParent(fenceParent.transform);
                post.transform.position = pos + Vector3.up * 0.3f;
                post.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
                post.GetComponent<Renderer>().material = woodMat;
                post.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                post.isStatic = true;
            }

            // Horizontal rails
            Vector3 midPoint = (start + end) / 2f;
            float length = Vector3.Distance(start, end);
            Vector3 direction = (end - start).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);

            for (int h = 0; h < 2; h++)
            {
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.transform.SetParent(fenceParent.transform);
                rail.transform.position = midPoint + Vector3.up * (0.2f + h * 0.25f);
                rail.transform.rotation = rotation;
                rail.transform.localScale = new Vector3(0.04f, 0.04f, length);
                rail.GetComponent<Renderer>().material = woodMat;
                rail.isStatic = true;
            }
        }

        private void CreateBarn()
        {
            var barnParent = new GameObject("Barn");
            barnParent.transform.position = new Vector3(-10f, 0f, 0f);
            barnParent.transform.SetParent(transform);

            Material barnMat = CreateMaterial(new Color(0.90f, 0.22f, 0.21f)); // Red
            Material roofMat = CreateMaterial(new Color(0.36f, 0.25f, 0.22f)); // Dark brown
            Material doorMat = CreateMaterial(new Color(0.47f, 0.33f, 0.28f)); // Wood
            Material windowMat = CreateMaterial(new Color(1f, 0.98f, 0.77f)); // Light yellow

            // Body
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(barnParent.transform);
            body.transform.localPosition = new Vector3(0, 1.25f, 0);
            body.transform.localScale = new Vector3(3f, 2.5f, 4f);
            body.GetComponent<Renderer>().material = barnMat;
            body.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            // Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(barnParent.transform);
            roof.transform.localPosition = new Vector3(0, 2.75f, 0);
            roof.transform.localScale = new Vector3(3.5f, 0.3f, 4.5f);
            roof.GetComponent<Renderer>().material = roofMat;
            roof.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            // Door
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.transform.SetParent(barnParent.transform);
            door.transform.localPosition = new Vector3(0, 0.9f, 2.05f);
            door.transform.localScale = new Vector3(1f, 1.8f, 0.1f);
            door.GetComponent<Renderer>().material = doorMat;

            // Windows
            foreach (float dx in new[] { -0.8f, 0.8f })
            {
                var win = GameObject.CreatePrimitive(PrimitiveType.Cube);
                win.transform.SetParent(barnParent.transform);
                win.transform.localPosition = new Vector3(dx, 1.8f, 2.05f);
                win.transform.localScale = new Vector3(0.5f, 0.5f, 0.1f);
                win.GetComponent<Renderer>().material = windowMat;
            }

            barnParent.isStatic = true;
        }

        private void CreateTrees()
        {
            Vector3[] treePositions = new Vector3[]
            {
                new Vector3(-12, 0, -8), new Vector3(-10, 0, 5), new Vector3(-14, 0, 0),
                new Vector3(14, 0, -6), new Vector3(13, 0, 8), new Vector3(-8, 0, -12),
                new Vector3(5, 0, -13), new Vector3(10, 0, 12), new Vector3(-12, 0, 10),
            };

            Material trunkMat = CreateMaterial(new Color(0.47f, 0.33f, 0.28f));

            foreach (var pos in treePositions)
            {
                var tree = new GameObject("Tree");
                tree.transform.position = pos;
                tree.transform.SetParent(transform);

                float scale = 0.8f + Random.Range(0f, 0.6f);
                tree.transform.localScale = Vector3.one * scale;

                // Trunk
                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.transform.SetParent(tree.transform);
                trunk.transform.localPosition = new Vector3(0, 0.75f, 0);
                trunk.transform.localScale = new Vector3(0.2f, 0.75f, 0.2f);
                trunk.GetComponent<Renderer>().material = trunkMat;
                trunk.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                // Foliage layers
                Color[] foliageColors = {
                    new Color(0.30f, 0.69f, 0.31f),
                    new Color(0.40f, 0.73f, 0.42f),
                    new Color(0.51f, 0.78f, 0.52f)
                };
                float[] heights = { 1.8f, 2.4f, 2.9f };
                float[] sizes = { 1.2f, 1.0f, 0.7f };

                for (int i = 0; i < 3; i++)
                {
                    var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    foliage.transform.SetParent(tree.transform);
                    foliage.transform.localPosition = new Vector3(0, heights[i], 0);
                    foliage.transform.localScale = Vector3.one * sizes[i];
                    foliage.GetComponent<Renderer>().material = CreateMaterial(foliageColors[i]);
                    foliage.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }

                tree.isStatic = true;
            }
        }

        private void CreateDecorations()
        {
            // Flowers
            Color[] flowerColors = {
                new Color(0.91f, 0.12f, 0.39f),
                new Color(1f, 0.92f, 0.23f),
                new Color(1f, 0.60f, 0f),
                new Color(0.61f, 0.15f, 0.69f),
                new Color(0.01f, 0.66f, 0.96f)
            };

            for (int i = 0; i < 15; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-20f, 20f),
                    0.15f,
                    Random.Range(-20f, 20f)
                );

                // Skip if too close to farm area
                if (Mathf.Abs(pos.x) < 8f && Mathf.Abs(pos.z) < 8f) continue;

                var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flower.name = "Flower";
                flower.transform.position = pos;
                flower.transform.localScale = Vector3.one * 0.15f;
                flower.transform.SetParent(transform);
                flower.GetComponent<Renderer>().material =
                    CreateMaterial(flowerColors[Random.Range(0, flowerColors.Length)]);
                flower.isStatic = true;
            }

            // Rocks
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-18f, 18f),
                    0.15f,
                    Random.Range(-18f, 18f)
                );

                if (Mathf.Abs(pos.x) < 9f && Mathf.Abs(pos.z) < 9f) continue;

                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "Rock";
                rock.transform.position = pos;
                float rockScale = 0.3f + Random.Range(0f, 0.3f);
                rock.transform.localScale = new Vector3(rockScale, rockScale * 0.6f, rockScale);
                rock.transform.SetParent(transform);
                rock.GetComponent<Renderer>().material = CreateMaterial(new Color(0.62f, 0.62f, 0.62f));
                rock.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                rock.isStatic = true;
            }
        }

        private Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            var mat = new Material(shader);
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.3f);
            return mat;
        }
    }
}
