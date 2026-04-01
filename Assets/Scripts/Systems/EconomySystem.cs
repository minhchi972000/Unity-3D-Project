using UnityEngine;
using FarmValley.Events;
using FarmValley.Utils;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages player currency (coins), XP, and level progression.
    /// </summary>
    public class EconomySystem : MonoBehaviour
    {
        private int coins;
        private int xp;
        private int level;
        private int xpToNextLevel;

        public int Coins => coins;
        public int XP => xp;
        public int CurrentLevel => level;
        public int XPToNextLevel => xpToNextLevel;
        public float XPProgress => xpToNextLevel > 0 ? (float)xp / xpToNextLevel : 1f;

        public void Initialize(int startingCoins, int startingLevel)
        {
            coins = startingCoins;
            level = startingLevel;
            xp = 0;
            xpToNextLevel = Constants.XPForLevel(level);

            GameEvents.TriggerCoinsChanged(coins);
            Debug.Log($"[EconomySystem] Initialized: {coins} coins, Level {level}");
        }

        /// <summary>
        /// Add coins to the player's wallet
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            coins += amount;
            GameEvents.TriggerCoinsChanged(coins);
        }

        /// <summary>
        /// Spend coins if affordable
        /// </summary>
        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (coins < amount) return false;

            coins -= amount;
            GameEvents.TriggerCoinsChanged(coins);
            return true;
        }

        /// <summary>
        /// Check if player has enough coins
        /// </summary>
        public bool CanAfford(int amount)
        {
            return coins >= amount;
        }

        /// <summary>
        /// Add experience points and check for level up
        /// </summary>
        public void AddXP(int amount)
        {
            if (amount <= 0) return;

            xp += amount;
            GameEvents.TriggerXPGained(amount);

            // Check for level up
            while (xp >= xpToNextLevel)
            {
                xp -= xpToNextLevel;
                level++;
                xpToNextLevel = Constants.XPForLevel(level);

                GameEvents.TriggerLevelUp(level);
                GameEvents.TriggerNotification($"Level Up! You are now level {level}!");

                Debug.Log($"[EconomySystem] Level Up! Level {level}");
            }
        }

        /// <summary>
        /// Sell an item for coins
        /// </summary>
        public void SellItem(string itemName, int price, int quantity = 1)
        {
            int total = price * quantity;
            AddCoins(total);
            GameEvents.TriggerNotification($"Sold {quantity}x {itemName} for {total} coins!");
        }

        /// <summary>
        /// Restore economy state from save data
        /// </summary>
        public void RestoreState(int savedCoins, int savedXP, int savedLevel)
        {
            coins = savedCoins;
            xp = savedXP;
            level = savedLevel;
            xpToNextLevel = Constants.XPForLevel(level);

            GameEvents.TriggerCoinsChanged(coins);
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public EconomySaveData GetSaveData()
        {
            return new EconomySaveData
            {
                coins = coins,
                xp = xp,
                level = level,
            };
        }
    }

    [System.Serializable]
    public struct EconomySaveData
    {
        public int coins;
        public int xp;
        public int level;
    }
}
