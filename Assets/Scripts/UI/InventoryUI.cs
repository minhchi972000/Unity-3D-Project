using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FarmValley.Core;
using FarmValley.Systems;

namespace FarmValley.UI
{
    /// <summary>
    /// Handles the inventory panel UI.
    /// Displays all items with icons and counts.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemSlotPrefab;

        public void Refresh()
        {
            ClearItems();

            var inventory = GameManager.Instance.Inventory;
            if (inventory == null) return;

            var items = inventory.GetAllItems();
            var gameData = GameManager.Instance.Data;

            foreach (var kvp in items)
            {
                if (kvp.Value <= 0) continue;

                var itemData = gameData.GetItem(kvp.Key);
                string displayName = itemData != null ? itemData.itemName : kvp.Key;
                Sprite icon = itemData != null ? itemData.icon : null;
                int sellPrice = itemData != null ? itemData.sellPrice : 0;

                CreateItemSlot(displayName, kvp.Value, icon, sellPrice);
            }
        }

        private void CreateItemSlot(string itemName, int count, Sprite icon, int sellPrice)
        {
            if (itemSlotPrefab == null || itemContainer == null) return;

            var slot = Instantiate(itemSlotPrefab, itemContainer);

            // Find and set text components
            var texts = slot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "ItemName" || text.gameObject.name == "NameText")
                    text.text = itemName;
                else if (text.gameObject.name == "ItemCount" || text.gameObject.name == "CountText")
                    text.text = $"x{count}";
                else if (text.gameObject.name == "PriceText")
                    text.text = sellPrice > 0 ? $"{sellPrice}c" : "";
            }

            // Find and set icon
            var images = slot.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject.name == "Icon" || img.gameObject.name == "ItemIcon")
                {
                    if (icon != null)
                        img.sprite = icon;
                    break;
                }
            }
        }

        private void ClearItems()
        {
            if (itemContainer == null) return;
            foreach (Transform child in itemContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
