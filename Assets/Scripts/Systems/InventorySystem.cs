using System.Collections.Generic;
using UnityEngine;
using FarmValley.Events;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages the player's inventory of items.
    /// Tracks item counts by string ID for flexibility.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        private Dictionary<string, int> items = new Dictionary<string, int>();

        public void Initialize()
        {
            items.Clear();
            Debug.Log("[InventorySystem] Initialized");
        }

        /// <summary>
        /// Add items to inventory
        /// </summary>
        public void AddItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return;

            if (items.ContainsKey(itemId))
                items[itemId] += amount;
            else
                items[itemId] = amount;

            GameEvents.TriggerItemAdded(itemId, amount);
            GameEvents.TriggerInventoryChanged();
        }

        /// <summary>
        /// Remove items from inventory
        /// </summary>
        public bool RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;

            if (!items.ContainsKey(itemId) || items[itemId] < amount)
                return false;

            items[itemId] -= amount;
            if (items[itemId] <= 0)
                items.Remove(itemId);

            GameEvents.TriggerItemRemoved(itemId, amount);
            GameEvents.TriggerInventoryChanged();
            return true;
        }

        /// <summary>
        /// Check if player has enough of an item
        /// </summary>
        public bool HasItem(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            return items.ContainsKey(itemId) && items[itemId] >= amount;
        }

        /// <summary>
        /// Get the count of a specific item
        /// </summary>
        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;
            return items.ContainsKey(itemId) ? items[itemId] : 0;
        }

        /// <summary>
        /// Get all items in inventory (for UI display)
        /// </summary>
        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(items);
        }

        /// <summary>
        /// Clear the entire inventory
        /// </summary>
        public void ClearInventory()
        {
            items.Clear();
            GameEvents.TriggerInventoryChanged();
        }

        /// <summary>
        /// Restore inventory from save data
        /// </summary>
        public void RestoreItems(Dictionary<string, int> savedItems)
        {
            items = new Dictionary<string, int>(savedItems);
            GameEvents.TriggerInventoryChanged();
        }

        /// <summary>
        /// Get inventory data for saving
        /// </summary>
        public Dictionary<string, int> GetSaveData()
        {
            return new Dictionary<string, int>(items);
        }
    }
}
