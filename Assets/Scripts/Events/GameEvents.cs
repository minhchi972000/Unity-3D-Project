using System;
using UnityEngine;

namespace FarmValley.Events
{
    /// <summary>
    /// Central event bus for decoupled system communication.
    /// Systems subscribe to events they care about without direct references.
    /// </summary>
    public static class GameEvents
    {
        // ===== Economy Events =====
        public static event Action<int> OnCoinsChanged;
        public static event Action<int> OnXPGained;
        public static event Action<int> OnLevelUp;

        public static void TriggerCoinsChanged(int newAmount) => OnCoinsChanged?.Invoke(newAmount);
        public static void TriggerXPGained(int amount) => OnXPGained?.Invoke(amount);
        public static void TriggerLevelUp(int newLevel) => OnLevelUp?.Invoke(newLevel);

        // ===== Farm Grid Events =====
        public static event Action<Vector2Int> OnPlotSelected;
        public static event Action<Vector2Int> OnPlotCleared;

        public static void TriggerPlotSelected(Vector2Int gridPos) => OnPlotSelected?.Invoke(gridPos);
        public static void TriggerPlotCleared(Vector2Int gridPos) => OnPlotCleared?.Invoke(gridPos);

        // ===== Crop Events =====
        public static event Action<Vector2Int, string> OnCropPlanted;
        public static event Action<Vector2Int> OnCropHarvested;
        public static event Action<Vector2Int, int> OnCropGrowthStageChanged;

        public static void TriggerCropPlanted(Vector2Int gridPos, string cropId) => OnCropPlanted?.Invoke(gridPos, cropId);
        public static void TriggerCropHarvested(Vector2Int gridPos) => OnCropHarvested?.Invoke(gridPos);
        public static void TriggerCropGrowthStageChanged(Vector2Int gridPos, int stage) => OnCropGrowthStageChanged?.Invoke(gridPos, stage);

        // ===== Inventory Events =====
        public static event Action<string, int> OnItemAdded;
        public static event Action<string, int> OnItemRemoved;
        public static event Action OnInventoryChanged;

        public static void TriggerItemAdded(string itemId, int amount) => OnItemAdded?.Invoke(itemId, amount);
        public static void TriggerItemRemoved(string itemId, int amount) => OnItemRemoved?.Invoke(itemId, amount);
        public static void TriggerInventoryChanged() => OnInventoryChanged?.Invoke();

        // ===== Animal Events =====
        public static event Action<string> OnAnimalProductReady;
        public static event Action<string, string> OnAnimalProductCollected;

        public static void TriggerAnimalProductReady(string animalId) => OnAnimalProductReady?.Invoke(animalId);
        public static void TriggerAnimalProductCollected(string animalId, string productId) => OnAnimalProductCollected?.Invoke(animalId, productId);

        // ===== Production Events =====
        public static event Action<string> OnProductionStarted;
        public static event Action<string, string> OnProductionCompleted;

        public static void TriggerProductionStarted(string buildingId) => OnProductionStarted?.Invoke(buildingId);
        public static void TriggerProductionCompleted(string buildingId, string productId) => OnProductionCompleted?.Invoke(buildingId, productId);

        // ===== Order Events =====
        public static event Action<string> OnOrderCompleted;
        public static event Action OnOrderBoardRefreshed;

        public static void TriggerOrderCompleted(string orderId) => OnOrderCompleted?.Invoke(orderId);
        public static void TriggerOrderBoardRefreshed() => OnOrderBoardRefreshed?.Invoke();

        // ===== UI Events =====
        public static event Action<string> OnNotification;
        public static event Action<string> OnPanelRequested;

        public static void TriggerNotification(string message) => OnNotification?.Invoke(message);
        public static void TriggerPanelRequested(string panelName) => OnPanelRequested?.Invoke(panelName);

        // ===== Player Events =====
        public static event Action<GameObject> OnPlayerInteract;

        public static void TriggerPlayerInteract(GameObject target) => OnPlayerInteract?.Invoke(target);

        // ===== Game State Events =====
        public static event Action OnGameSaved;
        public static event Action OnGameLoaded;

        public static void TriggerGameSaved() => OnGameSaved?.Invoke();
        public static void TriggerGameLoaded() => OnGameLoaded?.Invoke();
    }
}
