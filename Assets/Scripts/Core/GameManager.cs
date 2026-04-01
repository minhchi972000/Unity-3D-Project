using UnityEngine;
using FarmValley.Utils;
using FarmValley.Systems;
using FarmValley.Events;

namespace FarmValley.Core
{
    /// <summary>
    /// Central game manager that initializes and coordinates all systems.
    /// Attach this to a persistent GameObject in the scene.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("System References")]
        [SerializeField] private FarmGridSystem farmGridSystem;
        [SerializeField] private CropSystem cropSystem;
        [SerializeField] private GrowthSystem growthSystem;
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private EconomySystem economySystem;
        [SerializeField] private AnimalSystem animalSystem;
        [SerializeField] private ProductionSystem productionSystem;
        [SerializeField] private OrderSystem orderSystem;
        [SerializeField] private SaveSystem saveSystem;
        [SerializeField] private ProgressionSystem progressionSystem;

        [Header("Data")]
        [SerializeField] private GameDataContainer gameData;

        public FarmGridSystem FarmGrid => farmGridSystem;
        public CropSystem Crops => cropSystem;
        public GrowthSystem Growth => growthSystem;
        public InventorySystem Inventory => inventorySystem;
        public EconomySystem Economy => economySystem;
        public AnimalSystem Animals => animalSystem;
        public ProductionSystem Production => productionSystem;
        public OrderSystem Orders => orderSystem;
        public SaveSystem Save => saveSystem;
        public ProgressionSystem Progression => progressionSystem;
        public GameDataContainer Data => gameData;

        private bool isInitialized = false;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            if (isInitialized) return;

            Debug.Log("[GameManager] Initializing Farm Valley systems...");

            // Initialize economy first (other systems depend on it)
            if (economySystem != null)
                economySystem.Initialize(Constants.STARTING_COINS, Constants.STARTING_LEVEL);

            // Initialize inventory
            if (inventorySystem != null)
                inventorySystem.Initialize();

            // Initialize farm grid
            if (farmGridSystem != null)
                farmGridSystem.Initialize(Constants.GRID_WIDTH, Constants.GRID_HEIGHT, Constants.CELL_SIZE);

            // Initialize crop system
            if (cropSystem != null)
                cropSystem.Initialize();

            // Initialize growth system
            if (growthSystem != null)
                growthSystem.Initialize();

            // Initialize animal system
            if (animalSystem != null)
                animalSystem.Initialize();

            // Initialize production system
            if (productionSystem != null)
                productionSystem.Initialize();

            // Initialize order system
            if (orderSystem != null)
                orderSystem.Initialize();

            // Initialize progression system
            if (progressionSystem != null)
                progressionSystem.Initialize();

            // Try to load save data
            if (saveSystem != null)
                saveSystem.Initialize();

            isInitialized = true;
            Debug.Log("[GameManager] All systems initialized successfully!");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && saveSystem != null)
            {
                saveSystem.SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            if (saveSystem != null)
            {
                saveSystem.SaveGame();
            }
        }
    }
}
