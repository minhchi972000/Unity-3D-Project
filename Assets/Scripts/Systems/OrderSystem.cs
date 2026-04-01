using System.Collections.Generic;
using UnityEngine;
using FarmValley.Data;
using FarmValley.Events;
using FarmValley.Core;
using FarmValley.Utils;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages the order board system.
    /// Generates orders, tracks completion, and handles rewards.
    /// </summary>
    public class OrderSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem inventory;
        [SerializeField] private EconomySystem economy;

        private List<ActiveOrder> activeOrders = new List<ActiveOrder>();
        private float nextRefreshTime;

        public void Initialize()
        {
            nextRefreshTime = Time.time + 5f; // First refresh after 5 seconds
            Debug.Log("[OrderSystem] Initialized");
        }

        private void Update()
        {
            // Auto-refresh orders if below max
            if (Time.time >= nextRefreshTime && activeOrders.Count < Constants.MAX_ACTIVE_ORDERS)
            {
                GenerateNewOrder();
                nextRefreshTime = Time.time + Constants.ORDER_REFRESH_TIME;
            }
        }

        /// <summary>
        /// Generate a new random order based on player level
        /// </summary>
        public void GenerateNewOrder()
        {
            if (activeOrders.Count >= Constants.MAX_ACTIVE_ORDERS) return;

            var gameData = GameManager.Instance.Data;
            if (gameData.orders == null || gameData.orders.Length == 0) return;

            int playerLevel = economy.CurrentLevel;

            // Filter orders by level
            var eligible = new List<OrderData>();
            foreach (var order in gameData.orders)
            {
                if (order == null) continue;
                if (order.minLevel <= playerLevel &&
                    (order.maxLevel == 0 || order.maxLevel >= playerLevel))
                {
                    // Check we don't already have this order active
                    bool alreadyActive = activeOrders.Exists(a => a.orderData.orderId == order.orderId);
                    if (!alreadyActive)
                        eligible.Add(order);
                }
            }

            if (eligible.Count == 0) return;

            // Pick a random order
            var selected = eligible[Random.Range(0, eligible.Count)];
            string instanceId = $"order_{System.Guid.NewGuid().ToString().Substring(0, 8)}";

            activeOrders.Add(new ActiveOrder
            {
                instanceId = instanceId,
                orderData = selected,
                createdTime = Time.time,
            });

            GameEvents.TriggerOrderBoardRefreshed();
        }

        /// <summary>
        /// Attempt to fulfill an order
        /// </summary>
        public bool FulfillOrder(string orderInstanceId)
        {
            int index = activeOrders.FindIndex(o => o.instanceId == orderInstanceId);
            if (index < 0) return false;

            var order = activeOrders[index];

            // Check if player has all required items
            foreach (var req in order.orderData.requirements)
            {
                if (!inventory.HasItem(req.itemId, req.amount))
                {
                    GameEvents.TriggerNotification("You don't have all the required items!");
                    return false;
                }
            }

            // Consume items
            foreach (var req in order.orderData.requirements)
            {
                inventory.RemoveItem(req.itemId, req.amount);
            }

            // Give rewards
            economy.AddCoins(order.orderData.coinReward);
            economy.AddXP(order.orderData.xpReward);

            // Remove order
            activeOrders.RemoveAt(index);

            GameEvents.TriggerOrderCompleted(order.instanceId);
            GameEvents.TriggerNotification($"Order complete! +{order.orderData.coinReward} coins, +{order.orderData.xpReward} XP");

            return true;
        }

        /// <summary>
        /// Discard an order to make room for new ones
        /// </summary>
        public void DiscardOrder(string orderInstanceId)
        {
            activeOrders.RemoveAll(o => o.instanceId == orderInstanceId);
            GameEvents.TriggerOrderBoardRefreshed();
        }

        /// <summary>
        /// Check if an order can be fulfilled with current inventory
        /// </summary>
        public bool CanFulfillOrder(string orderInstanceId)
        {
            int index = activeOrders.FindIndex(o => o.instanceId == orderInstanceId);
            if (index < 0) return false;

            var order = activeOrders[index];
            foreach (var req in order.orderData.requirements)
            {
                if (!inventory.HasItem(req.itemId, req.amount))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get all active orders for UI
        /// </summary>
        public List<ActiveOrder> GetActiveOrders()
        {
            return new List<ActiveOrder>(activeOrders);
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public List<OrderSaveData> GetSaveData()
        {
            var data = new List<OrderSaveData>();
            foreach (var order in activeOrders)
            {
                data.Add(new OrderSaveData
                {
                    instanceId = order.instanceId,
                    orderId = order.orderData.orderId,
                    elapsedTime = Time.time - order.createdTime,
                });
            }
            return data;
        }

        /// <summary>
        /// Restore from save data
        /// </summary>
        public void RestoreOrders(List<OrderSaveData> saveData)
        {
            activeOrders.Clear();
            var gameData = GameManager.Instance.Data;

            foreach (var data in saveData)
            {
                OrderData orderData = null;
                foreach (var o in gameData.orders)
                {
                    if (o != null && o.orderId == data.orderId)
                    {
                        orderData = o;
                        break;
                    }
                }

                if (orderData != null)
                {
                    activeOrders.Add(new ActiveOrder
                    {
                        instanceId = data.instanceId,
                        orderData = orderData,
                        createdTime = Time.time - data.elapsedTime,
                    });
                }
            }

            GameEvents.TriggerOrderBoardRefreshed();
        }
    }

    public struct ActiveOrder
    {
        public string instanceId;
        public OrderData orderData;
        public float createdTime;
    }

    [System.Serializable]
    public struct OrderSaveData
    {
        public string instanceId;
        public string orderId;
        public float elapsedTime;
    }
}
