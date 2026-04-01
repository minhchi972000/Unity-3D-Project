using UnityEngine;
using System;

namespace FarmValley.Data
{
    /// <summary>
    /// ScriptableObject defining a production recipe for buildings.
    /// Create assets via: Create > FarmValley > Production Recipe
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "FarmValley/Production Recipe")]
    public class ProductionRecipeData : ScriptableObject
    {
        [Header("Identity")]
        public string recipeId;
        public string recipeName;
        public Sprite icon;

        [Header("Inputs")]
        public RecipeIngredient[] ingredients;

        [Header("Output")]
        public string outputItemId;
        public string outputItemName;
        public int outputAmount = 1;
        public int sellPrice = 30;

        [Header("Production")]
        [Tooltip("Time in seconds to produce")]
        public float productionTime = 30f;
        public int xpReward = 15;

        [Header("Unlock")]
        public int requiredLevel = 1;

        /// <summary>
        /// Check if the player has all required ingredients
        /// </summary>
        public bool CanCraft(Func<string, int> getItemCount)
        {
            foreach (var ingredient in ingredients)
            {
                if (getItemCount(ingredient.itemId) < ingredient.amount)
                    return false;
            }
            return true;
        }
    }

    [Serializable]
    public struct RecipeIngredient
    {
        public string itemId;
        public string itemName;
        public Sprite icon;
        public int amount;
    }
}
