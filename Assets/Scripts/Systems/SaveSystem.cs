using System.Collections.Generic;
using UnityEngine;
using FarmValley.Events;
using FarmValley.Core;
using FarmValley.Utils;

namespace FarmValley.Systems
{
    /// <summary>
    /// Handles persistent save/load using PlayerPrefs (JSON serialization).
    /// Saves: economy, inventory, farm grid, growing crops, animals, buildings, orders.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EconomySystem economy;
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private FarmGridSystem farmGrid;
        [SerializeField] private GrowthSystem growthSystem;
        [SerializeField] private AnimalSystem animalSystem;
        [SerializeField] private ProductionSystem productionSystem;
        [SerializeField] private OrderSystem orderSystem;

        [Header("Settings")]
        [SerializeField] private float autoSaveInterval = 60f;

        private float lastAutoSaveTime;

        public void Initialize()
        {
            lastAutoSaveTime = Time.time;

            // Try to load existing save
            if (HasSaveData())
            {
                LoadGame();
            }

            Debug.Log("[SaveSystem] Initialized");
        }

        private void Update()
        {
            // Auto-save periodically
            if (Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                SaveGame();
                lastAutoSaveTime = Time.time;
            }
        }

        /// <summary>
        /// Save all game state to persistent storage
        /// </summary>
        public void SaveGame()
        {
            var saveData = new GameSaveData();

            // Economy
            if (economy != null)
                saveData.economy = economy.GetSaveData();

            // Inventory
            if (inventory != null)
                saveData.inventory = inventory.GetSaveData();

            // Farm grid states
            if (farmGrid != null)
                saveData.plotStates = farmGrid.GetAllPlotStates();

            // Growing crops
            if (growthSystem != null)
            {
                saveData.growingCrops = new List<GrowingCropSaveEntry>();
                var growingCrops = growthSystem.GetAllGrowingCrops();
                foreach (var kvp in growingCrops)
                {
                    float elapsed = Time.time - kvp.Value.startTime;
                    saveData.growingCrops.Add(new GrowingCropSaveEntry
                    {
                        gridX = kvp.Key.x,
                        gridY = kvp.Key.y,
                        cropData = new GrowingCropSaveData
                        {
                            cropId = kvp.Value.cropData.cropId,
                            elapsedTime = elapsed,
                        }
                    });
                }
            }

            // Animals
            if (animalSystem != null)
                saveData.animals = animalSystem.GetSaveData();

            // Buildings
            if (productionSystem != null)
                saveData.buildings = productionSystem.GetSaveData();

            // Orders
            if (orderSystem != null)
                saveData.orders = orderSystem.GetSaveData();

            // Timestamp
            saveData.lastSaveTime = System.DateTime.UtcNow.ToString("o");

            // Serialize and store
            string json = JsonUtility.ToJson(saveData, true);
            PlayerPrefs.SetString(Constants.SAVE_KEY, json);
            PlayerPrefs.Save();

            GameEvents.TriggerGameSaved();
            Debug.Log("[SaveSystem] Game saved successfully");
        }

        /// <summary>
        /// Load game state from persistent storage
        /// </summary>
        public void LoadGame()
        {
            if (!HasSaveData())
            {
                Debug.Log("[SaveSystem] No save data found");
                return;
            }

            string json = PlayerPrefs.GetString(Constants.SAVE_KEY);
            var saveData = JsonUtility.FromJson<GameSaveData>(json);

            if (saveData == null)
            {
                Debug.LogWarning("[SaveSystem] Failed to parse save data");
                return;
            }

            // Restore economy
            if (economy != null)
                economy.RestoreState(saveData.economy.coins, saveData.economy.xp, saveData.economy.level);

            // Restore inventory
            if (inventory != null && saveData.inventory != null)
                inventory.RestoreItems(saveData.inventory);

            // Restore farm grid
            if (farmGrid != null && saveData.plotStates != null)
                farmGrid.RestorePlotStates(saveData.plotStates);

            // Restore growing crops
            if (growthSystem != null && saveData.growingCrops != null)
            {
                var cropDict = new Dictionary<Vector2Int, GrowingCropSaveData>();
                foreach (var entry in saveData.growingCrops)
                {
                    cropDict[new Vector2Int(entry.gridX, entry.gridY)] = entry.cropData;
                }
                growthSystem.RestoreGrowingCrops(cropDict);
            }

            // Restore animals
            if (animalSystem != null && saveData.animals != null)
                animalSystem.RestoreAnimals(saveData.animals);

            // Restore orders
            if (orderSystem != null && saveData.orders != null)
                orderSystem.RestoreOrders(saveData.orders);

            GameEvents.TriggerGameLoaded();
            Debug.Log("[SaveSystem] Game loaded successfully");
        }

        /// <summary>
        /// Check if save data exists
        /// </summary>
        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(Constants.SAVE_KEY);
        }

        /// <summary>
        /// Delete all save data
        /// </summary>
        public void DeleteSaveData()
        {
            PlayerPrefs.DeleteKey(Constants.SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[SaveSystem] Save data deleted");
        }
    }

    /// <summary>
    /// Root save data container
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        public string lastSaveTime;
        public EconomySaveData economy;
        public Dictionary<string, int> inventory;
        public List<PlotState> plotStates;
        public List<GrowingCropSaveEntry> growingCrops;
        public List<AnimalSaveData> animals;
        public List<BuildingSaveData> buildings;
        public List<OrderSaveData> orders;
    }

    [System.Serializable]
    public struct GrowingCropSaveEntry
    {
        public int gridX;
        public int gridY;
        public GrowingCropSaveData cropData;
    }
}
