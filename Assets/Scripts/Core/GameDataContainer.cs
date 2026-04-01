using UnityEngine;
using FarmValley.Data;

namespace FarmValley.Core
{
    /// <summary>
    /// Container for all game data ScriptableObjects.
    /// Assign in inspector for easy access.
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataContainer", menuName = "FarmValley/Game Data Container")]
    public class GameDataContainer : ScriptableObject
    {
        [Header("Crops")]
        public CropData[] crops;

        [Header("Animals")]
        public AnimalData[] animals;

        [Header("Production Buildings")]
        public ProductionBuildingData[] buildings;

        [Header("Recipes")]
        public ProductionRecipeData[] recipes;

        [Header("Orders")]
        public OrderData[] orders;

        [Header("Items")]
        public ItemData[] items;

        [Header("Progression")]
        public LevelData[] levels;

        /// <summary>
        /// Find a crop by its ID
        /// </summary>
        public CropData GetCrop(string cropId)
        {
            foreach (var crop in crops)
            {
                if (crop != null && crop.cropId == cropId)
                    return crop;
            }
            return null;
        }

        /// <summary>
        /// Find an animal by its ID
        /// </summary>
        public AnimalData GetAnimal(string animalId)
        {
            foreach (var animal in animals)
            {
                if (animal != null && animal.animalId == animalId)
                    return animal;
            }
            return null;
        }

        /// <summary>
        /// Find a building by its ID
        /// </summary>
        public ProductionBuildingData GetBuilding(string buildingId)
        {
            foreach (var building in buildings)
            {
                if (building != null && building.buildingId == buildingId)
                    return building;
            }
            return null;
        }

        /// <summary>
        /// Find a recipe by its ID
        /// </summary>
        public ProductionRecipeData GetRecipe(string recipeId)
        {
            foreach (var recipe in recipes)
            {
                if (recipe != null && recipe.recipeId == recipeId)
                    return recipe;
            }
            return null;
        }

        /// <summary>
        /// Find an item by its ID
        /// </summary>
        public ItemData GetItem(string itemId)
        {
            foreach (var item in items)
            {
                if (item != null && item.itemId == itemId)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Get level data for a specific level
        /// </summary>
        public LevelData GetLevel(int level)
        {
            foreach (var lvl in levels)
            {
                if (lvl != null && lvl.level == level)
                    return lvl;
            }
            return null;
        }
    }
}
