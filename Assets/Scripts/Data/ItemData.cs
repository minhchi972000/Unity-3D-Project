using UnityEngine;

namespace FarmValley.Data
{
    /// <summary>
    /// ScriptableObject defining a generic item for the inventory.
    /// Create assets via: Create > FarmValley > Item Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "FarmValley/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;

        [Header("Economy")]
        public int sellPrice = 10;
        public int buyCost = 0;

        [Header("Properties")]
        public ItemCategory category;
        public int maxStack = 99;

        [Header("Unlock")]
        public int requiredLevel = 1;
    }

    public enum ItemCategory
    {
        Seed,
        Crop,
        AnimalProduct,
        CraftedGood,
        Tool,
        Special
    }
}
