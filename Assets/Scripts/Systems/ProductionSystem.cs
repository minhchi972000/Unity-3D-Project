using System.Collections.Generic;
using UnityEngine;
using FarmValley.Data;
using FarmValley.Events;
using FarmValley.Core;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages production buildings (feed mill, bakery, etc.)
    /// Handles crafting queue, timing, and output.
    /// </summary>
    public class ProductionSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private EconomySystem economy;

        private List<BuildingInstance> buildings = new List<BuildingInstance>();

        public void Initialize()
        {
            Debug.Log("[ProductionSystem] Initialized");
        }

        /// <summary>
        /// Place a new production building
        /// </summary>
        public bool PlaceBuilding(ProductionBuildingData buildingData, Vector3 position)
        {
            if (buildingData == null) return false;

            if (!economy.CanAfford(buildingData.buyCost))
            {
                GameEvents.TriggerNotification($"Not enough coins! Need {buildingData.buyCost}");
                return false;
            }

            if (buildingData.requiredLevel > economy.CurrentLevel)
            {
                GameEvents.TriggerNotification($"Requires level {buildingData.requiredLevel}!");
                return false;
            }

            economy.SpendCoins(buildingData.buyCost);

            string instanceId = $"{buildingData.buildingId}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            var instance = new BuildingInstance
            {
                instanceId = instanceId,
                buildingData = buildingData,
                position = position,
                productionQueue = new List<ProductionSlot>(),
            };

            buildings.Add(instance);
            SpawnBuildingVisual(instance);

            GameEvents.TriggerNotification($"Built {buildingData.buildingName}!");
            return true;
        }

        /// <summary>
        /// Start producing a recipe in a building
        /// </summary>
        public bool StartProduction(string buildingInstanceId, ProductionRecipeData recipe)
        {
            int index = buildings.FindIndex(b => b.instanceId == buildingInstanceId);
            if (index < 0) return false;

            var building = buildings[index];

            // Check capacity
            if (building.productionQueue.Count >= building.buildingData.productionSlots)
            {
                GameEvents.TriggerNotification("Production slots are full!");
                return false;
            }

            // Check if we have ingredients
            if (!recipe.CanCraft(itemId => inventory.GetItemCount(itemId)))
            {
                GameEvents.TriggerNotification("Not enough ingredients!");
                return false;
            }

            // Consume ingredients
            foreach (var ingredient in recipe.ingredients)
            {
                inventory.RemoveItem(ingredient.itemId, ingredient.amount);
            }

            // Add to production queue
            building.productionQueue.Add(new ProductionSlot
            {
                recipe = recipe,
                startTime = Time.time,
                isComplete = false,
            });

            buildings[index] = building;

            GameEvents.TriggerProductionStarted(building.instanceId);
            GameEvents.TriggerNotification($"Started producing {recipe.recipeName}!");
            return true;
        }

        /// <summary>
        /// Collect a completed product from a building
        /// </summary>
        public bool CollectProduct(string buildingInstanceId, int slotIndex)
        {
            int buildingIndex = buildings.FindIndex(b => b.instanceId == buildingInstanceId);
            if (buildingIndex < 0) return false;

            var building = buildings[buildingIndex];

            if (slotIndex < 0 || slotIndex >= building.productionQueue.Count)
                return false;

            var slot = building.productionQueue[slotIndex];
            if (!slot.isComplete) return false;

            // Add product to inventory
            inventory.AddItem(slot.recipe.outputItemId, slot.recipe.outputAmount);
            economy.AddXP(slot.recipe.xpReward);

            // Remove from queue
            building.productionQueue.RemoveAt(slotIndex);
            buildings[buildingIndex] = building;

            GameEvents.TriggerProductionCompleted(building.instanceId, slot.recipe.outputItemId);
            GameEvents.TriggerNotification($"Collected {slot.recipe.outputItemName}!");
            return true;
        }

        private void Update()
        {
            UpdateProduction();
        }

        private void UpdateProduction()
        {
            for (int b = 0; b < buildings.Count; b++)
            {
                var building = buildings[b];
                bool changed = false;

                for (int s = 0; s < building.productionQueue.Count; s++)
                {
                    var slot = building.productionQueue[s];
                    if (slot.isComplete) continue;

                    float elapsed = Time.time - slot.startTime;
                    if (elapsed >= slot.recipe.productionTime)
                    {
                        slot.isComplete = true;
                        building.productionQueue[s] = slot;
                        changed = true;

                        GameEvents.TriggerNotification($"{slot.recipe.recipeName} is ready!");
                    }
                }

                if (changed)
                    buildings[b] = building;
            }
        }

        private void SpawnBuildingVisual(BuildingInstance building)
        {
            GameObject visual;

            if (building.buildingData.prefab != null)
            {
                visual = Instantiate(building.buildingData.prefab, building.position, Quaternion.identity);
            }
            else
            {
                visual = CreatePlaceholderBuilding(building);
            }

            visual.name = $"Building_{building.instanceId}";
            visual.transform.SetParent(transform);
        }

        private GameObject CreatePlaceholderBuilding(BuildingInstance building)
        {
            var parent = new GameObject($"Building_{building.buildingData.buildingId}");
            parent.transform.position = building.position;

            // Base
            var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseObj.transform.SetParent(parent.transform);
            baseObj.transform.localPosition = new Vector3(0, 0.75f, 0);
            baseObj.transform.localScale = new Vector3(2f, 1.5f, 2f);
            var baseRenderer = baseObj.GetComponent<Renderer>();
            if (baseRenderer != null)
            {
                baseRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                baseRenderer.material.color = building.buildingData.buildingId == "feed_mill"
                    ? new Color(0.76f, 0.65f, 0.55f)
                    : new Color(0.90f, 0.72f, 0.53f);
                baseRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            // Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(parent.transform);
            roof.transform.localPosition = new Vector3(0, 1.75f, 0);
            roof.transform.localScale = new Vector3(2.4f, 0.3f, 2.4f);
            roof.transform.rotation = Quaternion.Euler(0, 45, 0);
            var roofRenderer = roof.GetComponent<Renderer>();
            if (roofRenderer != null)
            {
                roofRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                roofRenderer.material.color = new Color(0.36f, 0.25f, 0.22f);
                roofRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            return parent;
        }

        /// <summary>
        /// Get all buildings for UI
        /// </summary>
        public List<BuildingInstance> GetAllBuildings()
        {
            return new List<BuildingInstance>(buildings);
        }

        /// <summary>
        /// Get production progress for a slot (0-1)
        /// </summary>
        public float GetSlotProgress(string buildingInstanceId, int slotIndex)
        {
            int index = buildings.FindIndex(b => b.instanceId == buildingInstanceId);
            if (index < 0) return 0f;

            var building = buildings[index];
            if (slotIndex < 0 || slotIndex >= building.productionQueue.Count) return 0f;

            var slot = building.productionQueue[slotIndex];
            if (slot.isComplete) return 1f;

            float elapsed = Time.time - slot.startTime;
            return Mathf.Clamp01(elapsed / slot.recipe.productionTime);
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public List<BuildingSaveData> GetSaveData()
        {
            var data = new List<BuildingSaveData>();
            foreach (var building in buildings)
            {
                var slots = new List<ProductionSlotSaveData>();
                foreach (var slot in building.productionQueue)
                {
                    slots.Add(new ProductionSlotSaveData
                    {
                        recipeId = slot.recipe.recipeId,
                        elapsedTime = Time.time - slot.startTime,
                        isComplete = slot.isComplete,
                    });
                }

                data.Add(new BuildingSaveData
                {
                    instanceId = building.instanceId,
                    buildingId = building.buildingData.buildingId,
                    positionX = building.position.x,
                    positionY = building.position.y,
                    positionZ = building.position.z,
                    productionSlots = slots,
                });
            }
            return data;
        }
    }

    public struct BuildingInstance
    {
        public string instanceId;
        public ProductionBuildingData buildingData;
        public Vector3 position;
        public List<ProductionSlot> productionQueue;
    }

    public struct ProductionSlot
    {
        public ProductionRecipeData recipe;
        public float startTime;
        public bool isComplete;
    }

    [System.Serializable]
    public struct BuildingSaveData
    {
        public string instanceId;
        public string buildingId;
        public float positionX, positionY, positionZ;
        public List<ProductionSlotSaveData> productionSlots;
    }

    [System.Serializable]
    public struct ProductionSlotSaveData
    {
        public string recipeId;
        public float elapsedTime;
        public bool isComplete;
    }
}
