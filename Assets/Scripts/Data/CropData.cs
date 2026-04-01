using UnityEngine;

namespace FarmValley.Data
{
    /// <summary>
    /// ScriptableObject defining a crop type.
    /// Create assets via: Create > FarmValley > Crop Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewCrop", menuName = "FarmValley/Crop Data")]
    public class CropData : ScriptableObject
    {
        [Header("Identity")]
        public string cropId;
        public string cropName;
        public Sprite icon;

        [Header("Economy")]
        public int seedCost = 5;
        public int sellPrice = 10;
        public int harvestYield = 2;
        public int xpReward = 10;

        [Header("Growth")]
        [Tooltip("Total growth time in seconds")]
        public float growthTime = 30f;
        [Tooltip("Number of visual growth stages (typically 4: seed, sprout, growing, mature)")]
        public int growthStages = 4;

        [Header("Visuals")]
        [Tooltip("Prefab for each growth stage (index 0 = seed, last = mature)")]
        public GameObject[] stagePrefabs;
        [Tooltip("Colors for each growth stage")]
        public Color[] stageColors;

        [Header("Unlock")]
        public int requiredLevel = 1;

        /// <summary>
        /// Get the time threshold for a given stage index
        /// </summary>
        public float GetStageThreshold(int stageIndex)
        {
            if (growthStages <= 1) return 0f;
            return (float)stageIndex / (growthStages - 1) * growthTime;
        }

        /// <summary>
        /// Get the current stage based on elapsed growth time
        /// </summary>
        public int GetStageFromTime(float elapsed)
        {
            if (elapsed >= growthTime) return growthStages - 1;
            float progress = elapsed / growthTime;
            return Mathf.FloorToInt(progress * (growthStages - 1));
        }

        /// <summary>
        /// Check if the crop is fully grown
        /// </summary>
        public bool IsFullyGrown(float elapsed)
        {
            return elapsed >= growthTime;
        }
    }
}
