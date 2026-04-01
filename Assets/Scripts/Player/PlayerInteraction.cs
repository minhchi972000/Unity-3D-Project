using UnityEngine;
using FarmValley.Core;
using FarmValley.Systems;
using FarmValley.Events;

namespace FarmValley.Player
{
    /// <summary>
    /// Handles player interaction with the game world.
    /// Listens for interaction events from PlayerController and
    /// delegates to the appropriate system (farming, animals, buildings).
    /// Also shows an interaction prompt when near interactable objects.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;

        [Header("Interaction")]
        [SerializeField] private float interactionCheckRadius = 2.5f;
        [SerializeField] private float interactionCheckInterval = 0.2f;

        private GameObject nearestInteractable;
        private float lastCheckTime;

        private void OnEnable()
        {
            GameEvents.OnPlayerInteract += HandleInteraction;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerInteract -= HandleInteraction;
        }

        private void Update()
        {
            // Periodically check for nearby interactables
            if (Time.time - lastCheckTime >= interactionCheckInterval)
            {
                CheckNearbyInteractables();
                lastCheckTime = Time.time;
            }
        }

        private void CheckNearbyInteractables()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactionCheckRadius);
            float closestDist = float.MaxValue;
            nearestInteractable = null;

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                bool isInteractable = false;

                // Check for farm plot
                if (hit.GetComponent<FarmPlot>() != null)
                    isInteractable = true;

                // Check for animal
                if (hit.GetComponentInParent<AnimalIdleAnimation>() != null)
                    isInteractable = true;

                if (isInteractable)
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        nearestInteractable = hit.gameObject;
                    }
                }
            }
        }

        private void HandleInteraction(GameObject target)
        {
            if (target == null) return;

            // Try farm plot interaction
            var plot = target.GetComponent<FarmPlot>();
            if (plot != null)
            {
                HandlePlotInteraction(plot);
                return;
            }

            // Try animal interaction
            var animal = target.GetComponentInParent<AnimalIdleAnimation>();
            if (animal != null)
            {
                HandleAnimalInteraction(animal.gameObject);
                return;
            }
        }

        private void HandlePlotInteraction(FarmPlot plot)
        {
            if (!plot.IsUnlocked)
            {
                GameEvents.TriggerNotification("This plot is locked!");
                return;
            }

            Vector2Int gridPos = plot.GridPosition;
            var cropSystem = GameManager.Instance.Crops;
            var farmGrid = GameManager.Instance.FarmGrid;

            // Auto-detect action based on plot state
            if (cropSystem.IsReadyToHarvest(gridPos))
            {
                cropSystem.HarvestCrop(gridPos);
            }
            else if (farmGrid.IsPlotEmpty(gridPos))
            {
                farmGrid.SelectPlot(gridPos);
                GameEvents.TriggerPanelRequested("CropSelector");
            }
            else
            {
                GameEvents.TriggerNotification("Crop is still growing...");
            }
        }

        private void HandleAnimalInteraction(GameObject animalObj)
        {
            var animalSystem = GameManager.Instance.Animals;
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

        /// <summary>
        /// Get the nearest interactable object (for UI prompt display)
        /// </summary>
        public GameObject GetNearestInteractable()
        {
            return nearestInteractable;
        }

        /// <summary>
        /// Check if player is near any interactable object
        /// </summary>
        public bool HasNearbyInteractable()
        {
            return nearestInteractable != null;
        }
    }
}
