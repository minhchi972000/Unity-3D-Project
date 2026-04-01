using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FarmValley.Core;
using FarmValley.Systems;

namespace FarmValley.UI
{
    /// <summary>
    /// Handles the order board UI panel.
    /// Displays active orders with requirements and rewards.
    /// </summary>
    public class OrderBoardUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform orderContainer;
        [SerializeField] private GameObject orderCardPrefab;

        public void Refresh()
        {
            ClearOrders();

            var orderSystem = GameManager.Instance.Orders;
            if (orderSystem == null) return;

            var orders = orderSystem.GetActiveOrders();

            if (orders.Count == 0)
            {
                CreateEmptyMessage();
                return;
            }

            foreach (var order in orders)
            {
                CreateOrderCard(order);
            }
        }

        private void CreateOrderCard(ActiveOrder order)
        {
            if (orderCardPrefab == null || orderContainer == null) return;

            var card = Instantiate(orderCardPrefab, orderContainer);

            var texts = card.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "OrderDescription" || text.gameObject.name == "DescriptionText")
                    text.text = order.orderData.orderDescription;
                else if (text.gameObject.name == "RewardText")
                    text.text = $"+{order.orderData.coinReward}c  +{order.orderData.xpReward}XP";
                else if (text.gameObject.name == "RequirementsText")
                {
                    string reqText = "";
                    foreach (var req in order.orderData.requirements)
                    {
                        int have = GameManager.Instance.Inventory.GetItemCount(req.itemId);
                        string color = have >= req.amount ? "green" : "red";
                        reqText += $"<color={color}>{req.itemName}: {have}/{req.amount}</color>\n";
                    }
                    text.text = reqText.TrimEnd('\n');
                }
            }

            // Fulfill button
            var buttons = card.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.gameObject.name == "FulfillButton" || btn.gameObject.name == "CompleteButton")
                {
                    string instanceId = order.instanceId;
                    bool canFulfill = GameManager.Instance.Orders.CanFulfillOrder(instanceId);
                    btn.interactable = canFulfill;
                    btn.onClick.AddListener(() =>
                    {
                        GameManager.Instance.Orders.FulfillOrder(instanceId);
                        Refresh();
                    });
                }
                else if (btn.gameObject.name == "DiscardButton" || btn.gameObject.name == "DeleteButton")
                {
                    string instanceId = order.instanceId;
                    btn.onClick.AddListener(() =>
                    {
                        GameManager.Instance.Orders.DiscardOrder(instanceId);
                        Refresh();
                    });
                }
            }

            // Customer icon
            var images = card.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject.name == "CustomerIcon" && order.orderData.customerIcon != null)
                {
                    img.sprite = order.orderData.customerIcon;
                    break;
                }
            }
        }

        private void CreateEmptyMessage()
        {
            if (orderContainer == null) return;

            var emptyObj = new GameObject("EmptyMessage");
            emptyObj.transform.SetParent(orderContainer, false);

            var text = emptyObj.AddComponent<TextMeshProUGUI>();
            text.text = "No orders right now!\nCheck back soon...";
            text.fontSize = 16;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.4f, 0.3f, 0.25f);

            var rect = emptyObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 80);
        }

        private void ClearOrders()
        {
            if (orderContainer == null) return;
            foreach (Transform child in orderContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
