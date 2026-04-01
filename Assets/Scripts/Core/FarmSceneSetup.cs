using UnityEngine;
using UnityEngine.Rendering;

namespace FarmValley.Core
{
    /// <summary>
    /// Sets up the farming scene with proper lighting, camera, and environment.
    /// Attach this to a setup GameObject that runs on scene load.
    /// Useful for quickly setting up a working scene from scratch.
    /// </summary>
    public class FarmSceneSetup : MonoBehaviour
    {
        [Header("Setup Flags")]
        [SerializeField] private bool setupLighting = true;
        [SerializeField] private bool setupCamera = true;
        [SerializeField] private bool setupSkybox = true;

        private void Start()
        {
            if (setupLighting) SetupLighting();
            if (setupCamera) SetupCamera();
            if (setupSkybox) SetupSkybox();
        }

        private void SetupLighting()
        {
            // Check if directional light already exists
            var existingLight = FindFirstObjectByType<Light>();
            if (existingLight != null && existingLight.type == LightType.Directional)
                return;

            // Create sun light
            var sunObj = new GameObject("Sun (Directional Light)");
            var sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.88f, 0.70f); // Warm sunlight
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.7f;
            sun.shadowResolution = LightShadowResolution.Medium;
            sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ambient lighting
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.53f, 0.81f, 0.92f); // Sky blue
            RenderSettings.ambientEquatorColor = new Color(0.76f, 0.84f, 0.67f); // Warm green
            RenderSettings.ambientGroundColor = new Color(0.40f, 0.53f, 0.29f); // Dark green

            Debug.Log("[FarmSceneSetup] Lighting configured");
        }

        private void SetupCamera()
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            // Set to orthographic for isometric view
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f); // Sky blue

            // Position for isometric view (45-degree angle)
            float angle = 45f * Mathf.Deg2Rad;
            float distance = 20f;
            cam.transform.position = new Vector3(
                distance * Mathf.Cos(angle),
                distance * 0.8f,
                -distance * Mathf.Sin(angle)
            );
            cam.transform.LookAt(Vector3.zero);

            Debug.Log("[FarmSceneSetup] Camera configured for isometric view");
        }

        private void SetupSkybox()
        {
            // Use solid color sky (URP-friendly)
            RenderSettings.skybox = null;
            var cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f); // Light sky blue
            }

            // Fog for depth
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.53f, 0.81f, 0.92f);
            RenderSettings.fogDensity = 0.015f;

            Debug.Log("[FarmSceneSetup] Skybox and fog configured");
        }
    }
}
