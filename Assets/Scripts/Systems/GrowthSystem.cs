using System.Collections.Generic;
using UnityEngine;
using FarmValley.Data;
using FarmValley.Core;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages time-based crop growth.
    /// Tracks growth progress and triggers visual updates.
    /// </summary>
    public class GrowthSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CropSystem cropSystem;

        private Dictionary<Vector2Int, GrowingCrop> growingCrops = new Dictionary<Vector2Int, GrowingCrop>();

        public void Initialize()
        {
            Debug.Log("[GrowthSystem] Initialized");
        }

        /// <summary>
        /// Register a newly planted crop for growth tracking
        /// </summary>
        public void RegisterCrop(Vector2Int gridPos, CropData cropData, float startTime)
        {
            growingCrops[gridPos] = new GrowingCrop
            {
                cropData = cropData,
                startTime = startTime,
                currentStage = 0,
            };
        }

        /// <summary>
        /// Remove a crop from growth tracking (after harvest)
        /// </summary>
        public void UnregisterCrop(Vector2Int gridPos)
        {
            growingCrops.Remove(gridPos);
        }

        /// <summary>
        /// Check if a crop is fully mature
        /// </summary>
        public bool IsCropMature(Vector2Int gridPos)
        {
            if (!growingCrops.TryGetValue(gridPos, out GrowingCrop crop))
                return false;

            float elapsed = Time.time - crop.startTime;
            return crop.cropData.IsFullyGrown(elapsed);
        }

        /// <summary>
        /// Get the growth progress (0.0 to 1.0)
        /// </summary>
        public float GetGrowthProgress(Vector2Int gridPos)
        {
            if (!growingCrops.TryGetValue(gridPos, out GrowingCrop crop))
                return 0f;

            float elapsed = Time.time - crop.startTime;
            return Mathf.Clamp01(elapsed / crop.cropData.growthTime);
        }

        /// <summary>
        /// Get remaining growth time in seconds
        /// </summary>
        public float GetRemainingTime(Vector2Int gridPos)
        {
            if (!growingCrops.TryGetValue(gridPos, out GrowingCrop crop))
                return 0f;

            float elapsed = Time.time - crop.startTime;
            return Mathf.Max(0f, crop.cropData.growthTime - elapsed);
        }

        private void Update()
        {
            UpdateGrowth();
        }

        private void UpdateGrowth()
        {
            // Use a temporary list to avoid modifying dict during iteration
            var positionsToUpdate = new List<Vector2Int>();

            foreach (var kvp in growingCrops)
            {
                var crop = kvp.Value;
                float elapsed = Time.time - crop.startTime;
                int newStage = crop.cropData.GetStageFromTime(elapsed);

                if (newStage != crop.currentStage)
                {
                    positionsToUpdate.Add(kvp.Key);
                }
            }

            foreach (var pos in positionsToUpdate)
            {
                if (growingCrops.TryGetValue(pos, out GrowingCrop crop))
                {
                    float elapsed = Time.time - crop.startTime;
                    int newStage = crop.cropData.GetStageFromTime(elapsed);
                    crop.currentStage = newStage;
                    growingCrops[pos] = crop;

                    // Update visual
                    if (cropSystem != null)
                    {
                        cropSystem.UpdateCropVisual(pos, crop.cropData, newStage);
                    }
                }
            }
        }

        /// <summary>
        /// Get all growing crop data for save system
        /// </summary>
        public Dictionary<Vector2Int, GrowingCrop> GetAllGrowingCrops()
        {
            return new Dictionary<Vector2Int, GrowingCrop>(growingCrops);
        }

        /// <summary>
        /// Restore growing crops from save data
        /// </summary>
        public void RestoreGrowingCrops(Dictionary<Vector2Int, GrowingCropSaveData> saveData)
        {
            growingCrops.Clear();
            var gameData = GameManager.Instance.Data;

            foreach (var kvp in saveData)
            {
                var cropData = gameData.GetCrop(kvp.Value.cropId);
                if (cropData != null)
                {
                    float adjustedStartTime = Time.time - kvp.Value.elapsedTime;
                    growingCrops[kvp.Key] = new GrowingCrop
                    {
                        cropData = cropData,
                        startTime = adjustedStartTime,
                        currentStage = cropData.GetStageFromTime(kvp.Value.elapsedTime),
                    };

                    // Spawn visual at correct stage
                    int stage = cropData.GetStageFromTime(kvp.Value.elapsedTime);
                    if (cropSystem != null)
                    {
                        cropSystem.SpawnCropVisual(kvp.Key, cropData, stage);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Runtime data for a growing crop
    /// </summary>
    public struct GrowingCrop
    {
        public CropData cropData;
        public float startTime;
        public int currentStage;
    }

    /// <summary>
    /// Serializable data for saving growing crops
    /// </summary>
    [System.Serializable]
    public struct GrowingCropSaveData
    {
        public string cropId;
        public float elapsedTime;
    }
}
