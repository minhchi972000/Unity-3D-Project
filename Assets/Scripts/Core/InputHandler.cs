using UnityEngine;
using FarmValley.Systems;
using FarmValley.Events;

namespace FarmValley.Core
{
    /// <summary>
    /// Handles player input (touch/mouse) and translates to game actions.
    /// Manages tool modes: plant, harvest, collect.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private FarmGridSystem farmGrid;
        [SerializeField] private CropSystem cropSystem;
        [SerializeField] private AnimalSystem animalSystem;

        [Header("Raycast")]
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float maxRayDistance = 100f;

        private ToolMode currentTool = ToolMode.None;
        private string selectedCropId;
        private bool isOverUI = false;

        public ToolMode CurrentTool => currentTool;

        public enum ToolMode
        {
            None,
            Plant,
            Harvest,
            Collect,
        }

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // Skip if over UI
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            // Handle touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    // Skip if over UI for touch
                    if (UnityEngine.EventSystems.EventSystem.current != null &&
                        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        return;

                    HandleTap(touch.position);
                }
            }
            // Handle mouse click
            else if (Input.GetMouseButtonDown(0))
            {
                HandleTap(Input.mousePosition);
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
            {
                // Check if we hit a farm plot
                var farmPlot = hit.collider.GetComponent<FarmPlot>();
                if (farmPlot != null)
                {
                    HandlePlotTap(farmPlot);
                    return;
                }

                // Check if we hit an animal (by checking parent hierarchy)
                var animalAnim = hit.collider.GetComponentInParent<AnimalIdleAnimation>();
                if (animalAnim != null)
                {
                    HandleAnimalTap(animalAnim.gameObject);
                    return;
                }
            }

            // Tapped on nothing - deselect
            farmGrid.DeselectCurrent();
        }

        private void HandlePlotTap(FarmPlot plot)
        {
            if (!plot.IsUnlocked)
            {
                GameEvents.TriggerNotification("This plot is locked!");
                return;
            }

            Vector2Int gridPos = plot.GridPosition;

            switch (currentTool)
            {
                case ToolMode.Plant:
                    if (!string.IsNullOrEmpty(selectedCropId))
                    {
                        var cropData = GameManager.Instance.Data.GetCrop(selectedCropId);
                        if (cropData != null)
                        {
                            cropSystem.PlantCrop(gridPos, cropData);
                        }
                    }
                    else
                    {
                        // Show crop selector
                        farmGrid.SelectPlot(gridPos);
                        GameEvents.TriggerPanelRequested("CropSelector");
                    }
                    break;

                case ToolMode.Harvest:
                    cropSystem.HarvestCrop(gridPos);
                    break;

                default:
                    // Default: select the plot and show context actions
                    farmGrid.SelectPlot(gridPos);

                    // Auto-detect action
                    if (cropSystem.IsReadyToHarvest(gridPos))
                    {
                        cropSystem.HarvestCrop(gridPos);
                    }
                    else if (farmGrid.IsPlotEmpty(gridPos))
                    {
                        GameEvents.TriggerPanelRequested("CropSelector");
                    }
                    else
                    {
                        // Show crop info / growth progress
                        GameEvents.TriggerPanelRequested("CropInfo");
                    }
                    break;
            }
        }

        private void HandleAnimalTap(GameObject animalObj)
        {
            // Find the animal instance by name
            string objName = animalObj.name;
            var animals = animalSystem.GetAllAnimals();

            foreach (var animal in animals)
            {
                if ($"Animal_{animal.instanceId}" == objName)
                {
                    if (animal.hasProduct)
                    {
                        animalSystem.CollectProduct(animal.instanceId);
                    }
                    else
                    {
                        float remaining = animal.animalData.produceTime - (Time.time - animal.lastProduceTime);
                        int seconds = Mathf.CeilToInt(remaining);
                        GameEvents.TriggerNotification($"{animal.animalData.animalName}: product ready in {seconds}s");
                    }
                    break;
                }
            }
        }

        // ===== Tool Mode Control (called by UI) =====

        public void SetToolMode(ToolMode mode)
        {
            currentTool = mode;
        }

        public void SetSelectedCrop(string cropId)
        {
            selectedCropId = cropId;
            currentTool = ToolMode.Plant;
        }

        public void ClearSelection()
        {
            currentTool = ToolMode.None;
            selectedCropId = null;
            farmGrid.DeselectCurrent();
        }
    }
}
