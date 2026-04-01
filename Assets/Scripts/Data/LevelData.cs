using UnityEngine;

namespace FarmValley.Data
{
    /// <summary>
    /// ScriptableObject defining level progression and unlocks.
    /// Create assets via: Create > FarmValley > Level Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevel", menuName = "FarmValley/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int level;
        public int xpRequired;

        [Header("Unlocks")]
        [Tooltip("Crop IDs unlocked at this level")]
        public string[] unlockedCrops;

        [Tooltip("Animal IDs unlocked at this level")]
        public string[] unlockedAnimals;

        [Tooltip("Building IDs unlocked at this level")]
        public string[] unlockedBuildings;

        [Tooltip("Recipe IDs unlocked at this level")]
        public string[] unlockedRecipes;

        [Header("Rewards")]
        public int coinReward = 0;
        [Tooltip("Additional farm plots unlocked")]
        public int additionalPlots = 0;
    }
}
