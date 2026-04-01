using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FarmValley.Core;
using FarmValley.Data;
using FarmValley.Systems;

namespace FarmValley.UI
{
    /// <summary>
    /// Handles the production building UI panel.
    /// Shows available recipes and production queue.
    /// </summary>
    public class ProductionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform buildingListContainer;
        [SerializeField] private Transform recipeContainer;
        [SerializeField] private Transform queueContainer;
        [SerializeField] private GameObject buildingCardPrefab;
        [SerializeField] private GameObject recipeCardPrefab;
        [SerializeField] private GameObject queueSlotPrefab;

        private string selectedBuildingId;

        public void Refresh()
        {
            ClearContainer(buildingListContainer);
            ClearContainer(recipeContainer);
            ClearContainer(queueContainer);

            var production = GameManager.Instance.Production;
            if (production == null) return;

            var buildings = production.GetAllBuildings();

            if (buildings.Count == 0)
            {
                CreateEmptyMessage(buildingListContainer, "No production buildings yet.\nBuy one from the shop!");
                return;
            }

            foreach (var building in buildings)
            {
                CreateBuildingCard(building);
            }

            // Auto-select first building
            if (buildings.Count > 0 && string.IsNullOrEmpty(selectedBuildingId))
            {
                SelectBuilding(buildings[0].instanceId);
            }
            else if (!string.IsNullOrEmpty(selectedBuildingId))
            {
                RefreshSelectedBuilding();
            }
        }

        private void CreateBuildingCard(BuildingInstance building)
        {
            if (buildingCardPrefab == null || buildingListContainer == null) return;

            var card = Instantiate(buildingCardPrefab, buildingListContainer);

            var texts = card.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "NameText")
                    text.text = building.buildingData.buildingName;
                else if (text.gameObject.name == "StatusText")
                {
                    int active = building.productionQueue.Count;
                    int max = building.buildingData.productionSlots;
                    text.text = $"{active}/{max} slots";
                }
            }

            var button = card.GetComponent<Button>();
            if (button == null) button = card.GetComponentInChildren<Button>();
            if (button != null)
            {
                string id = building.instanceId;
                button.onClick.AddListener(() => SelectBuilding(id));
            }
        }

        private void SelectBuilding(string buildingInstanceId)
        {
            selectedBuildingId = buildingInstanceId;
            RefreshSelectedBuilding();
        }

        private void RefreshSelectedBuilding()
        {
            ClearContainer(recipeContainer);
            ClearContainer(queueContainer);

            var production = GameManager.Instance.Production;
            var buildings = production.GetAllBuildings();
            var building = buildings.Find(b => b.instanceId == selectedBuildingId);

            if (string.IsNullOrEmpty(building.instanceId)) return;

            // Show available recipes
            foreach (var recipe in building.buildingData.availableRecipes)
            {
                if (recipe == null) continue;
                CreateRecipeCard(recipe, building);
            }

            // Show production queue
            for (int i = 0; i < building.productionQueue.Count; i++)
            {
                CreateQueueSlot(building, i);
            }
        }

        private void CreateRecipeCard(ProductionRecipeData recipe, BuildingInstance building)
        {
            if (recipeCardPrefab == null || recipeContainer == null) return;

            var card = Instantiate(recipeCardPrefab, recipeContainer);

            var texts = card.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "RecipeName" || text.gameObject.name == "NameText")
                    text.text = recipe.recipeName;
                else if (text.gameObject.name == "IngredientsText")
                {
                    string ingredients = "";
                    foreach (var ing in recipe.ingredients)
                    {
                        int have = GameManager.Instance.Inventory.GetItemCount(ing.itemId);
                        ingredients += $"{ing.itemName}: {have}/{ing.amount}  ";
                    }
                    text.text = ingredients.Trim();
                }
                else if (text.gameObject.name == "TimeText")
                    text.text = $"{Mathf.CeilToInt(recipe.productionTime)}s";
            }

            var button = card.GetComponentInChildren<Button>();
            if (button != null)
            {
                string bId = building.instanceId;
                button.interactable = recipe.CanCraft(id => GameManager.Instance.Inventory.GetItemCount(id));
                button.onClick.AddListener(() =>
                {
                    GameManager.Instance.Production.StartProduction(bId, recipe);
                    Refresh();
                });
            }
        }

        private void CreateQueueSlot(BuildingInstance building, int slotIndex)
        {
            if (queueSlotPrefab == null || queueContainer == null) return;

            var slot = Instantiate(queueSlotPrefab, queueContainer);
            var productionSlot = building.productionQueue[slotIndex];

            var texts = slot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "ProductName" || text.gameObject.name == "NameText")
                    text.text = productionSlot.recipe.recipeName;
                else if (text.gameObject.name == "StatusText")
                {
                    if (productionSlot.isComplete)
                        text.text = "Ready!";
                    else
                    {
                        float progress = GameManager.Instance.Production.GetSlotProgress(building.instanceId, slotIndex);
                        text.text = $"{Mathf.RoundToInt(progress * 100)}%";
                    }
                }
            }

            var progressBar = slot.GetComponentInChildren<Slider>();
            if (progressBar != null)
            {
                float progress = GameManager.Instance.Production.GetSlotProgress(building.instanceId, slotIndex);
                progressBar.value = progress;
            }

            if (productionSlot.isComplete)
            {
                var button = slot.GetComponentInChildren<Button>();
                if (button != null)
                {
                    string bId = building.instanceId;
                    int idx = slotIndex;
                    button.onClick.AddListener(() =>
                    {
                        GameManager.Instance.Production.CollectProduct(bId, idx);
                        Refresh();
                    });
                }
            }
        }

        private void CreateEmptyMessage(Transform container, string message)
        {
            if (container == null) return;
            var emptyObj = new GameObject("EmptyMessage");
            emptyObj.transform.SetParent(container, false);
            var text = emptyObj.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.4f, 0.3f, 0.25f);
            var rect = emptyObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 60);
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
