using UnityEngine;
using System;

namespace FarmValley.Data
{
    /// <summary>
    /// ScriptableObject defining an order template for the order board.
    /// Create assets via: Create > FarmValley > Order Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewOrder", menuName = "FarmValley/Order Data")]
    public class OrderData : ScriptableObject
    {
        [Header("Identity")]
        public string orderId;
        public string orderDescription;
        public Sprite customerIcon;

        [Header("Requirements")]
        public OrderRequirement[] requirements;

        [Header("Rewards")]
        public int coinReward = 50;
        public int xpReward = 25;

        [Header("Difficulty")]
        [Tooltip("Minimum player level for this order to appear")]
        public int minLevel = 1;
        [Tooltip("Maximum player level (0 = no max)")]
        public int maxLevel = 0;
    }

    [Serializable]
    public struct OrderRequirement
    {
        public string itemId;
        public string itemName;
        public Sprite icon;
        public int amount;
    }
}
