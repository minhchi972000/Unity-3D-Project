using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FarmValley.Events;
using FarmValley.Core;
using FarmValley.Systems;

namespace FarmValley.UI
{
    /// <summary>
    /// Central UI manager that coordinates all UI panels and the HUD.
    /// Attach to the main Canvas object.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD References")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Slider xpBar;
        [SerializeField] private TextMeshProUGUI xpText;

        [Header("Panels")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject cropSelectorPanel;
        [SerializeField] private GameObject orderBoardPanel;
        [SerializeField] private GameObject productionPanel;
        [SerializeField] private GameObject animalPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Sub-Managers")]
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private ShopUI shopUI;
        [SerializeField] private CropSelectorUI cropSelectorUI;
        [SerializeField] private OrderBoardUI orderBoardUI;
        [SerializeField] private ProductionUI productionUI;

        [Header("Notification")]
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationParent;

        [Header("Tool Buttons")]
        [SerializeField] private Button plantButton;
        [SerializeField] private Button harvestButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button orderButton;
        [SerializeField] private Button animalButton;

        [Header("References")]
        [SerializeField] private InputHandler inputHandler;

        private GameObject currentPanel;

        private void OnEnable()
        {
            // Subscribe to events
            GameEvents.OnCoinsChanged += UpdateCoinsDisplay;
            GameEvents.OnXPGained += OnXPChanged;
            GameEvents.OnLevelUp += UpdateLevelDisplay;
            GameEvents.OnNotification += ShowNotification;
            GameEvents.OnPanelRequested += OnPanelRequested;
            GameEvents.OnInventoryChanged += RefreshCurrentPanel;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinsChanged -= UpdateCoinsDisplay;
            GameEvents.OnXPGained -= OnXPChanged;
            GameEvents.OnLevelUp -= UpdateLevelDisplay;
            GameEvents.OnNotification -= ShowNotification;
            GameEvents.OnPanelRequested -= OnPanelRequested;
            GameEvents.OnInventoryChanged -= RefreshCurrentPanel;
        }

        private void Start()
        {
            SetupButtons();
            CloseAllPanels();
            RefreshHUD();
        }

        private void SetupButtons()
        {
            if (plantButton != null)
                plantButton.onClick.AddListener(() => OnToolSelected(InputHandler.ToolMode.Plant));
            if (harvestButton != null)
                harvestButton.onClick.AddListener(() => OnToolSelected(InputHandler.ToolMode.Harvest));
            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(() => TogglePanel(inventoryPanel));
            if (shopButton != null)
                shopButton.onClick.AddListener(() => TogglePanel(shopPanel));
            if (orderButton != null)
                orderButton.onClick.AddListener(() => TogglePanel(orderBoardPanel));
            if (animalButton != null)
                animalButton.onClick.AddListener(() => TogglePanel(animalPanel));
        }

        // ===== HUD Updates =====

        public void RefreshHUD()
        {
            var economy = GameManager.Instance.Economy;
            if (economy == null) return;

            UpdateCoinsDisplay(economy.Coins);
            UpdateLevelDisplay(economy.CurrentLevel);
            UpdateXPBar();
        }

        private void UpdateCoinsDisplay(int coins)
        {
            if (coinsText != null)
                coinsText.text = FormatNumber(coins);
        }

        private void UpdateLevelDisplay(int level)
        {
            if (levelText != null)
                levelText.text = $"Lv. {level}";
            UpdateXPBar();
        }

        private void OnXPChanged(int amount)
        {
            UpdateXPBar();
        }

        private void UpdateXPBar()
        {
            var economy = GameManager.Instance.Economy;
            if (economy == null) return;

            if (xpBar != null)
                xpBar.value = economy.XPProgress;

            if (xpText != null)
                xpText.text = $"{economy.XP}/{economy.XPToNextLevel}";
        }

        // ===== Panel Management =====

        private void OnToolSelected(InputHandler.ToolMode mode)
        {
            if (inputHandler != null)
            {
                if (inputHandler.CurrentTool == mode)
                {
                    inputHandler.ClearSelection();
                }
                else
                {
                    inputHandler.SetToolMode(mode);
                }
            }
            CloseAllPanels();
        }

        private void OnPanelRequested(string panelName)
        {
            switch (panelName)
            {
                case "CropSelector":
                    OpenPanel(cropSelectorPanel);
                    if (cropSelectorUI != null) cropSelectorUI.Refresh();
                    break;
                case "Inventory":
                    OpenPanel(inventoryPanel);
                    if (inventoryUI != null) inventoryUI.Refresh();
                    break;
                case "Shop":
                    OpenPanel(shopPanel);
                    if (shopUI != null) shopUI.Refresh();
                    break;
                case "Orders":
                    OpenPanel(orderBoardPanel);
                    if (orderBoardUI != null) orderBoardUI.Refresh();
                    break;
                case "Production":
                    OpenPanel(productionPanel);
                    if (productionUI != null) productionUI.Refresh();
                    break;
            }
        }

        public void TogglePanel(GameObject panel)
        {
            if (panel == null) return;

            if (currentPanel == panel && panel.activeSelf)
            {
                CloseAllPanels();
            }
            else
            {
                OpenPanel(panel);
            }
        }

        public void OpenPanel(GameObject panel)
        {
            if (panel == null) return;
            CloseAllPanels();
            panel.SetActive(true);
            currentPanel = panel;

            // Animate panel open
            var animator = panel.GetComponent<Animator>();
            if (animator != null)
                animator.SetTrigger("Open");
        }

        public void CloseAllPanels()
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (cropSelectorPanel != null) cropSelectorPanel.SetActive(false);
            if (orderBoardPanel != null) orderBoardPanel.SetActive(false);
            if (productionPanel != null) productionPanel.SetActive(false);
            if (animalPanel != null) animalPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            currentPanel = null;
        }

        private void RefreshCurrentPanel()
        {
            if (currentPanel == inventoryPanel && inventoryUI != null)
                inventoryUI.Refresh();
            else if (currentPanel == shopPanel && shopUI != null)
                shopUI.Refresh();
            else if (currentPanel == orderBoardPanel && orderBoardUI != null)
                orderBoardUI.Refresh();
        }

        // ===== Notifications =====

        public void ShowNotification(string message)
        {
            if (notificationPrefab == null || notificationParent == null) return;

            var notifObj = Instantiate(notificationPrefab, notificationParent);
            var notifText = notifObj.GetComponentInChildren<TextMeshProUGUI>();
            if (notifText != null)
                notifText.text = message;

            // Auto-destroy after duration
            Destroy(notifObj, 3f);
        }

        // ===== Utility =====

        private string FormatNumber(int number)
        {
            if (number >= 1000000) return (number / 1000000f).ToString("F1") + "M";
            if (number >= 1000) return (number / 1000f).ToString("F1") + "K";
            return number.ToString();
        }
    }
}
