using UnityEngine;

namespace FarmValley.Data
{
    /// <summary>
    /// ScriptableObject defining a production building type.
    /// Create assets via: Create > FarmValley > Production Building
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuilding", menuName = "FarmValley/Production Building")]
    public class ProductionBuildingData : ScriptableObject
    {
        [Header("Identity")]
        public string buildingId;
        public string buildingName;
        public Sprite icon;

        [Header("Economy")]
        public int buyCost = 100;

        [Header("Production")]
        [Tooltip("Recipes this building can produce")]
        public ProductionRecipeData[] availableRecipes;

        [Header("Capacity")]
        [Tooltip("Number of items that can be produced simultaneously")]
        public int productionSlots = 1;

        [Header("Visuals")]
        public GameObject prefab;

        [Header("Unlock")]
        public int requiredLevel = 1;
    }
}
