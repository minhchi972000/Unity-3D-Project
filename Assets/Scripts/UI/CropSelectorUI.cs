using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FarmValley.Core;
using FarmValley.Data;

namespace FarmValley.UI
{
    /// <summary>
    /// Handles the crop selection popup.
    /// Shows available crops with cost and growth time info.
    /// </summary>
    public class CropSelectorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cropContainer;
        [SerializeField] private GameObject cropOptionPrefab;

        public void Refresh()
        {
            ClearOptions();

            var progression = GameManager.Instance.Progression;
            var availableCrops = progression.GetAvailableCrops();

            foreach (var crop in availableCrops)
            {
                CreateCropOption(crop);
            }
        }

        private void CreateCropOption(CropData crop)
        {
            if (cropOptionPrefab == null || cropContainer == null) return;

            var option = Instantiate(cropOptionPrefab, cropContainer);

            var texts = option.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "CropName" || text.gameObject.name == "NameText")
                    text.text = crop.cropName;
                else if (text.gameObject.name == "CostText")
                    text.text = $"{crop.seedCost}c";
                else if (text.gameObject.name == "TimeText")
                    text.text = FormatTime(crop.growthTime);
                else if (text.gameObject.name == "YieldText")
                    text.text = $"Yield: {crop.harvestYield}";
            }

            var images = option.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if ((img.gameObject.name == "Icon" || img.gameObject.name == "CropIcon") && crop.icon != null)
                {
                    img.sprite = crop.icon;
                    break;
                }
            }

            var button = option.GetComponent<Button>();
            if (button == null) button = option.GetComponentInChildren<Button>();
            if (button != null)
            {
                string cropId = crop.cropId;
                button.onClick.AddListener(() => OnCropSelected(cropId));
            }
        }

        private void OnCropSelected(string cropId)
        {
            var inputHandler = FindFirstObjectByType<InputHandler>();
            if (inputHandler != null)
            {
                inputHandler.SetSelectedCrop(cropId);
            }

            // Close this panel
            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
                uiManager.CloseAllPanels();

            var cropData = GameManager.Instance.Data.GetCrop(cropId);
            string name = cropData != null ? cropData.cropName : cropId;
            Events.GameEvents.TriggerNotification($"Selected {name} - tap a plot to plant!");
        }

        private string FormatTime(float seconds)
        {
            if (seconds < 60) return $"{Mathf.CeilToInt(seconds)}s";
            if (seconds < 3600) return $"{Mathf.CeilToInt(seconds / 60f)}m";
            return $"{(seconds / 3600f):F1}h";
        }

        private void ClearOptions()
        {
            if (cropContainer == null) return;
            foreach (Transform child in cropContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
