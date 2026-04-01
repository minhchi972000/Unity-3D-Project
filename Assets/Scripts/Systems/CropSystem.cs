using UnityEngine;
using FarmValley.Core;
using FarmValley.Data;
using FarmValley.Events;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages crop planting and harvesting logic.
    /// Works with FarmGridSystem and GrowthSystem.
    /// </summary>
    public class CropSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FarmGridSystem farmGrid;
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private EconomySystem economy;
        [SerializeField] private GrowthSystem growthSystem;

        [Header("Crop Visuals")]
        [SerializeField] private GameObject defaultSeedPrefab;
        [SerializeField] private GameObject defaultSproutPrefab;
        [SerializeField] private GameObject defaultGrowingPrefab;
        [SerializeField] private GameObject defaultMaturePrefab;

        // Track crop visuals per plot
        private System.Collections.Generic.Dictionary<Vector2Int, GameObject> cropVisuals =
            new System.Collections.Generic.Dictionary<Vector2Int, GameObject>();

        public void Initialize()
        {
            Debug.Log("[CropSystem] Initialized");
        }

        /// <summary>
        /// Attempt to plant a crop at the selected plot
        /// </summary>
        public bool PlantCrop(Vector2Int gridPos, CropData cropData)
        {
            if (cropData == null)
            {
                Debug.LogWarning("[CropSystem] No crop data provided");
                return false;
            }

            // Check if plot is available
            if (!farmGrid.IsPlotEmpty(gridPos))
            {
                GameEvents.TriggerNotification("This plot already has a crop!");
                return false;
            }

            // Check if player can afford seeds
            if (!economy.CanAfford(cropData.seedCost))
            {
                GameEvents.TriggerNotification($"Not enough coins! Need {cropData.seedCost}");
                return false;
            }

            // Check level requirement
            int currentLevel = economy.CurrentLevel;
            if (cropData.requiredLevel > currentLevel)
            {
                GameEvents.TriggerNotification($"Requires level {cropData.requiredLevel}!");
                return false;
            }

            // Spend coins
            economy.SpendCoins(cropData.seedCost);

            // Plant on grid
            float startTime = Time.time;
            farmGrid.SetCrop(gridPos, cropData.cropId, startTime);

            // Register with growth system
            growthSystem.RegisterCrop(gridPos, cropData, startTime);

            // Create visual
            SpawnCropVisual(gridPos, cropData, 0);

            // Fire event
            GameEvents.TriggerCropPlanted(gridPos, cropData.cropId);
            GameEvents.TriggerNotification($"Planted {cropData.cropName}!");

            return true;
        }

        /// <summary>
        /// Attempt to harvest a mature crop
        /// </summary>
        public bool HarvestCrop(Vector2Int gridPos)
        {
            var plotState = farmGrid.GetPlotState(gridPos);

            if (string.IsNullOrEmpty(plotState.cropId))
            {
                GameEvents.TriggerNotification("No crop to harvest here!");
                return false;
            }

            // Get crop data
            var cropData = GameManager.Instance.Data.GetCrop(plotState.cropId);
            if (cropData == null)
            {
                Debug.LogError($"[CropSystem] Crop data not found for: {plotState.cropId}");
                return false;
            }

            // Check if crop is mature
            if (!growthSystem.IsCropMature(gridPos))
            {
                GameEvents.TriggerNotification("Crop is not ready yet!");
                return false;
            }

            // Add to inventory
            inventory.AddItem(cropData.cropId, cropData.harvestYield);

            // Award XP
            economy.AddXP(cropData.xpReward);

            // Remove from growth tracking
            growthSystem.UnregisterCrop(gridPos);

            // Clear the plot
            farmGrid.ClearPlot(gridPos);

            // Remove visual
            RemoveCropVisual(gridPos);

            // Fire event
            GameEvents.TriggerCropHarvested(gridPos);
            GameEvents.TriggerNotification($"Harvested {cropData.harvestYield}x {cropData.cropName}!");

            return true;
        }

        /// <summary>
        /// Spawn or update the visual representation of a crop
        /// </summary>
        public void SpawnCropVisual(Vector2Int gridPos, CropData cropData, int stage)
        {
            RemoveCropVisual(gridPos);

            Vector3 worldPos = farmGrid.GridToWorld(gridPos);

            GameObject prefab = GetStagePrefab(cropData, stage);

            GameObject visual;
            if (prefab != null)
            {
                visual = Instantiate(prefab, worldPos, Quaternion.identity);
            }
            else
            {
                // Create placeholder visual based on stage
                visual = CreatePlaceholderCropVisual(worldPos, cropData, stage);
            }

            visual.name = $"Crop_{cropData.cropId}_{gridPos.x}_{gridPos.y}";
            visual.transform.SetParent(transform);

            cropVisuals[gridPos] = visual;
        }

        /// <summary>
        /// Update visual when growth stage changes
        /// </summary>
        public void UpdateCropVisual(Vector2Int gridPos, CropData cropData, int newStage)
        {
            SpawnCropVisual(gridPos, cropData, newStage);
            GameEvents.TriggerCropGrowthStageChanged(gridPos, newStage);
        }

        private void RemoveCropVisual(Vector2Int gridPos)
        {
            if (cropVisuals.TryGetValue(gridPos, out GameObject existing))
            {
                Destroy(existing);
                cropVisuals.Remove(gridPos);
            }
        }

        private GameObject GetStagePrefab(CropData cropData, int stage)
        {
            if (cropData.stagePrefabs != null && stage < cropData.stagePrefabs.Length)
                return cropData.stagePrefabs[stage];

            // Fall back to defaults
            switch (stage)
            {
                case 0: return defaultSeedPrefab;
                case 1: return defaultSproutPrefab;
                case 2: return defaultGrowingPrefab;
                case 3: return defaultMaturePrefab;
                default: return null;
            }
        }

        private GameObject CreatePlaceholderCropVisual(Vector3 position, CropData cropData, int stage)
        {
            GameObject visual = new GameObject($"CropVisual_{stage}");
            visual.transform.position = position;

            Color color;
            float scaleY;

            switch (stage)
            {
                case 0: // Seed
                    color = new Color(0.55f, 0.43f, 0.39f);
                    scaleY = 0.1f;
                    break;
                case 1: // Sprout
                    color = new Color(0.56f, 0.80f, 0.35f);
                    scaleY = 0.3f;
                    break;
                case 2: // Growing
                    color = new Color(0.40f, 0.73f, 0.42f);
                    scaleY = 0.6f;
                    break;
                case 3: // Mature
                    color = cropData.stageColors != null && cropData.stageColors.Length > 0
                        ? cropData.stageColors[cropData.stageColors.Length - 1]
                        : new Color(1f, 0.84f, 0.31f); // Golden
                    scaleY = 0.8f;
                    break;
                default:
                    color = Color.green;
                    scaleY = 0.5f;
                    break;
            }

            // Create a simple scaled cube as placeholder
            var meshObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            meshObj.transform.SetParent(visual.transform);
            meshObj.transform.localPosition = new Vector3(0, scaleY / 2f, 0);
            meshObj.transform.localScale = new Vector3(0.3f, scaleY, 0.3f);

            var renderer = meshObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = color;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            // Add bounce animation on spawn
            var anim = visual.AddComponent<CropSpawnAnimation>();
            anim.targetScale = visual.transform.localScale;

            return visual;
        }

        /// <summary>
        /// Check if a plot has a mature crop ready for harvest
        /// </summary>
        public bool IsReadyToHarvest(Vector2Int gridPos)
        {
            var plotState = farmGrid.GetPlotState(gridPos);
            if (string.IsNullOrEmpty(plotState.cropId)) return false;
            return growthSystem.IsCropMature(gridPos);
        }

        /// <summary>
        /// Get all crop visuals for cleanup
        /// </summary>
        public void ClearAllVisuals()
        {
            foreach (var kvp in cropVisuals)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value);
            }
            cropVisuals.Clear();
        }
    }

    /// <summary>
    /// Simple bounce animation when a crop is spawned
    /// </summary>
    public class CropSpawnAnimation : MonoBehaviour
    {
        public Vector3 targetScale = Vector3.one;
        private float animTime = 0f;
        private float duration = 0.4f;

        private void Start()
        {
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            animTime += Time.deltaTime;
            float t = Mathf.Clamp01(animTime / duration);

            // Elastic ease out
            float p = 0.3f;
            float s = p / 4f;
            float value = Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1f;

            transform.localScale = targetScale * value;

            if (t >= 1f)
            {
                transform.localScale = targetScale;
                Destroy(this);
            }
        }
    }
}
