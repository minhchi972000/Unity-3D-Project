using System.Collections.Generic;
using UnityEngine;
using FarmValley.Data;
using FarmValley.Events;
using FarmValley.Core;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages progression unlocks based on player level.
    /// Listens for level-up events and unlocks new content.
    /// </summary>
    public class ProgressionSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FarmGridSystem farmGrid;
        [SerializeField] private EconomySystem economy;

        private HashSet<string> unlockedCrops = new HashSet<string>();
        private HashSet<string> unlockedAnimals = new HashSet<string>();
        private HashSet<string> unlockedBuildings = new HashSet<string>();
        private HashSet<string> unlockedRecipes = new HashSet<string>();

        public void Initialize()
        {
            // Subscribe to level up events
            GameEvents.OnLevelUp += OnLevelUp;

            // Process initial unlocks for starting level
            ProcessUnlocksUpToLevel(economy.CurrentLevel);

            Debug.Log("[ProgressionSystem] Initialized");
        }

        private void OnDestroy()
        {
            GameEvents.OnLevelUp -= OnLevelUp;
        }

        private void OnLevelUp(int newLevel)
        {
            ProcessLevelUnlocks(newLevel);
        }

        private void ProcessUnlocksUpToLevel(int level)
        {
            var gameData = GameManager.Instance.Data;
            if (gameData.levels == null) return;

            for (int i = 1; i <= level; i++)
            {
                var levelData = gameData.GetLevel(i);
                if (levelData != null)
                    ApplyUnlocks(levelData);
            }
        }

        private void ProcessLevelUnlocks(int level)
        {
            var gameData = GameManager.Instance.Data;
            var levelData = gameData.GetLevel(level);

            if (levelData == null) return;

            ApplyUnlocks(levelData);

            // Award level-up coin reward
            if (levelData.coinReward > 0)
            {
                economy.AddCoins(levelData.coinReward);
                GameEvents.TriggerNotification($"Level {level} reward: +{levelData.coinReward} coins!");
            }

            // Unlock additional farm plots
            if (levelData.additionalPlots > 0)
            {
                UnlockAdditionalPlots(levelData.additionalPlots);
            }
        }

        private void ApplyUnlocks(LevelData levelData)
        {
            if (levelData.unlockedCrops != null)
            {
                foreach (string cropId in levelData.unlockedCrops)
                    unlockedCrops.Add(cropId);
            }

            if (levelData.unlockedAnimals != null)
            {
                foreach (string animalId in levelData.unlockedAnimals)
                    unlockedAnimals.Add(animalId);
            }

            if (levelData.unlockedBuildings != null)
            {
                foreach (string buildingId in levelData.unlockedBuildings)
                    unlockedBuildings.Add(buildingId);
            }

            if (levelData.unlockedRecipes != null)
            {
                foreach (string recipeId in levelData.unlockedRecipes)
                    unlockedRecipes.Add(recipeId);
            }
        }

        private void UnlockAdditionalPlots(int count)
        {
            int unlocked = 0;
            // Expand outward from center
            for (int ring = 3; ring < farmGrid.GridWidth && unlocked < count; ring++)
            {
                for (int x = 0; x < farmGrid.GridWidth && unlocked < count; x++)
                {
                    for (int z = 0; z < farmGrid.GridHeight && unlocked < count; z++)
                    {
                        var pos = new Vector2Int(x, z);
                        var state = farmGrid.GetPlotState(pos);
                        if (!state.isUnlocked)
                        {
                            farmGrid.UnlockPlot(pos);
                            unlocked++;
                        }
                    }
                }
            }

            if (unlocked > 0)
            {
                GameEvents.TriggerNotification($"Unlocked {unlocked} new farm plots!");
            }
        }

        // ===== Query Methods =====

        public bool IsCropUnlocked(string cropId) => unlockedCrops.Contains(cropId);
        public bool IsAnimalUnlocked(string animalId) => unlockedAnimals.Contains(animalId);
        public bool IsBuildingUnlocked(string buildingId) => unlockedBuildings.Contains(buildingId);
        public bool IsRecipeUnlocked(string recipeId) => unlockedRecipes.Contains(recipeId);

        public HashSet<string> GetUnlockedCrops() => new HashSet<string>(unlockedCrops);
        public HashSet<string> GetUnlockedAnimals() => new HashSet<string>(unlockedAnimals);
        public HashSet<string> GetUnlockedBuildings() => new HashSet<string>(unlockedBuildings);
        public HashSet<string> GetUnlockedRecipes() => new HashSet<string>(unlockedRecipes);

        /// <summary>
        /// Get all unlocked crops as CropData references
        /// </summary>
        public List<CropData> GetAvailableCrops()
        {
            var result = new List<CropData>();
            var gameData = GameManager.Instance.Data;

            foreach (var crop in gameData.crops)
            {
                if (crop != null && (unlockedCrops.Contains(crop.cropId) || crop.requiredLevel <= economy.CurrentLevel))
                    result.Add(crop);
            }
            return result;
        }

        /// <summary>
        /// Get all unlocked animals as AnimalData references
        /// </summary>
        public List<AnimalData> GetAvailableAnimals()
        {
            var result = new List<AnimalData>();
            var gameData = GameManager.Instance.Data;

            foreach (var animal in gameData.animals)
            {
                if (animal != null && (unlockedAnimals.Contains(animal.animalId) || animal.requiredLevel <= economy.CurrentLevel))
                    result.Add(animal);
            }
            return result;
        }
    }
}
