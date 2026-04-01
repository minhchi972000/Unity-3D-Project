using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FarmValley.Core;
using FarmValley.Data;
using FarmValley.Events;

namespace FarmValley.UI
{
    /// <summary>
    /// Handles the shop panel UI.
    /// Displays seeds, animals, and buildings for purchase.
    /// Also handles selling items.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform buyContainer;
        [SerializeField] private Transform sellContainer;
        [SerializeField] private GameObject shopItemPrefab;

        [Header("Tab Buttons")]
        [SerializeField] private Button buyTabButton;
        [SerializeField] private Button sellTabButton;
        [SerializeField] private GameObject buyPanel;
        [SerializeField] private GameObject sellPanel;

        private bool showingBuyTab = true;

        private void Start()
        {
            if (buyTabButton != null)
                buyTabButton.onClick.AddListener(() => ShowTab(true));
            if (sellTabButton != null)
                sellTabButton.onClick.AddListener(() => ShowTab(false));
        }

        public void Refresh()
        {
            RefreshBuyTab();
            RefreshSellTab();
            ShowTab(showingBuyTab);
        }

        private void ShowTab(bool isBuyTab)
        {
            showingBuyTab = isBuyTab;
            if (buyPanel != null) buyPanel.SetActive(isBuyTab);
            if (sellPanel != null) sellPanel.SetActive(!isBuyTab);
        }

        private void RefreshBuyTab()
        {
            ClearContainer(buyContainer);

            var gameData = GameManager.Instance.Data;
            var economy = GameManager.Instance.Economy;

            // Seeds
            if (gameData.crops != null)
            {
                foreach (var crop in gameData.crops)
                {
                    if (crop == null) continue;
                    if (crop.requiredLevel > economy.CurrentLevel) continue;

                    CreateBuyItem(
                        crop.cropName + " Seed",
                        crop.seedCost,
                        crop.icon,
                        () => BuySeed(crop)
                    );
                }
            }

            // Animals
            if (gameData.animals != null)
            {
                foreach (var animal in gameData.animals)
                {
                    if (animal == null) continue;
                    if (animal.requiredLevel > economy.CurrentLevel) continue;

                    CreateBuyItem(
                        animal.animalName,
                        animal.buyCost,
                        animal.icon,
                        () => BuyAnimal(animal)
                    );
                }
            }

            // Buildings
            if (gameData.buildings != null)
            {
                foreach (var building in gameData.buildings)
                {
                    if (building == null) continue;
                    if (building.requiredLevel > economy.CurrentLevel) continue;

                    CreateBuyItem(
                        building.buildingName,
                        building.buyCost,
                        building.icon,
                        () => BuyBuilding(building)
                    );
                }
            }
        }

        private void RefreshSellTab()
        {
            ClearContainer(sellContainer);

            var inventory = GameManager.Instance.Inventory;
            var gameData = GameManager.Instance.Data;
            var items = inventory.GetAllItems();

            foreach (var kvp in items)
            {
                if (kvp.Value <= 0) continue;

                var itemData = gameData.GetItem(kvp.Key);
                if (itemData == null || itemData.sellPrice <= 0) continue;

                string itemId = kvp.Key;
                CreateSellItem(
                    itemData.itemName,
                    itemData.sellPrice,
                    kvp.Value,
                    itemData.icon,
                    () => SellItem(itemId, itemData.itemName, itemData.sellPrice)
                );
            }
        }

        private void CreateBuyItem(string name, int cost, Sprite icon, System.Action onBuy)
        {
            if (shopItemPrefab == null || buyContainer == null) return;

            var item = Instantiate(shopItemPrefab, buyContainer);

            var texts = item.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "ItemName" || text.gameObject.name == "NameText")
                    text.text = name;
                else if (text.gameObject.name == "PriceText")
                    text.text = $"{cost} coins";
            }

            var images = item.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if ((img.gameObject.name == "Icon" || img.gameObject.name == "ItemIcon") && icon != null)
                {
                    img.sprite = icon;
                    break;
                }
            }

            var button = item.GetComponentInChildren<Button>();
            if (button != null)
                button.onClick.AddListener(() => onBuy?.Invoke());
        }

        private void CreateSellItem(string name, int price, int count, Sprite icon, System.Action onSell)
        {
            if (shopItemPrefab == null || sellContainer == null) return;

            var item = Instantiate(shopItemPrefab, sellContainer);

            var texts = item.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "ItemName" || text.gameObject.name == "NameText")
                    text.text = $"{name} (x{count})";
                else if (text.gameObject.name == "PriceText")
                    text.text = $"Sell: {price}c";
            }

            var images = item.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if ((img.gameObject.name == "Icon" || img.gameObject.name == "ItemIcon") && icon != null)
                {
                    img.sprite = icon;
                    break;
                }
            }

            var button = item.GetComponentInChildren<Button>();
            if (button != null)
                button.onClick.AddListener(() =>
                {
                    onSell?.Invoke();
                    Refresh();
                });
        }

        private void BuySeed(CropData crop)
        {
            // Set the input handler to plant mode with this crop
            var inputHandler = FindFirstObjectByType<InputHandler>();
            if (inputHandler != null)
            {
                inputHandler.SetSelectedCrop(crop.cropId);
                GameEvents.TriggerNotification($"Selected {crop.cropName} - tap a plot to plant!");
            }

            // Close shop
            gameObject.SetActive(false);
        }

        private void BuyAnimal(AnimalData animal)
        {
            // Place at a default position (could be improved with placement mode)
            Vector3 position = new Vector3(
                Random.Range(8f, 12f),
                0f,
                Random.Range(-4f, 4f)
            );
            GameManager.Instance.Animals.BuyAnimal(animal, position);
            Refresh();
        }

        private void BuyBuilding(ProductionBuildingData building)
        {
            Vector3 position = new Vector3(
                Random.Range(-12f, -8f),
                0f,
                Random.Range(-4f, 4f)
            );
            GameManager.Instance.Production.PlaceBuilding(building, position);
            Refresh();
        }

        private void SellItem(string itemId, string itemName, int price)
        {
            var inventory = GameManager.Instance.Inventory;
            var economy = GameManager.Instance.Economy;

            if (inventory.RemoveItem(itemId, 1))
            {
                economy.SellItem(itemName, price, 1);
            }
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
