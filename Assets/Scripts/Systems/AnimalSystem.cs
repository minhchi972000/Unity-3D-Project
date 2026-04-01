using System.Collections.Generic;
using UnityEngine;
using FarmValley.Data;
using FarmValley.Events;
using FarmValley.Core;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages animals on the farm.
    /// Handles purchasing, placement, production cycles, and collection.
    /// </summary>
    public class AnimalSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EconomySystem economy;
        [SerializeField] private InventorySystem inventory;

        [Header("Animal Pen Positions")]
        [SerializeField] private Transform[] animalPenParents;

        private List<AnimalInstance> animals = new List<AnimalInstance>();
        private Dictionary<string, GameObject> animalVisuals = new Dictionary<string, GameObject>();

        public void Initialize()
        {
            Debug.Log("[AnimalSystem] Initialized");
        }

        /// <summary>
        /// Purchase and place a new animal
        /// </summary>
        public bool BuyAnimal(AnimalData animalData, Vector3 position)
        {
            if (animalData == null) return false;

            // Check cost
            if (!economy.CanAfford(animalData.buyCost))
            {
                GameEvents.TriggerNotification($"Not enough coins! Need {animalData.buyCost}");
                return false;
            }

            // Check level
            if (animalData.requiredLevel > economy.CurrentLevel)
            {
                GameEvents.TriggerNotification($"Requires level {animalData.requiredLevel}!");
                return false;
            }

            // Spend coins
            economy.SpendCoins(animalData.buyCost);

            // Create animal instance
            string instanceId = $"{animalData.animalId}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            var instance = new AnimalInstance
            {
                instanceId = instanceId,
                animalData = animalData,
                position = position,
                lastProduceTime = Time.time,
                hasProduct = false,
            };

            animals.Add(instance);

            // Spawn visual
            SpawnAnimalVisual(instance);

            GameEvents.TriggerNotification($"Bought a {animalData.animalName}!");
            return true;
        }

        /// <summary>
        /// Collect product from an animal
        /// </summary>
        public bool CollectProduct(string instanceId)
        {
            int index = animals.FindIndex(a => a.instanceId == instanceId);
            if (index < 0) return false;

            var animal = animals[index];
            if (!animal.hasProduct)
            {
                GameEvents.TriggerNotification("No product ready yet!");
                return false;
            }

            // Add to inventory
            inventory.AddItem(animal.animalData.produceItemId, animal.animalData.produceAmount);

            // Reset production timer
            animal.hasProduct = false;
            animal.lastProduceTime = Time.time;
            animals[index] = animal;

            // Update visual (remove product indicator)
            UpdateAnimalVisual(animal, false);

            GameEvents.TriggerAnimalProductCollected(animal.instanceId, animal.animalData.produceItemId);
            GameEvents.TriggerNotification($"Collected {animal.animalData.produceItemName}!");

            return true;
        }

        private void Update()
        {
            UpdateProduction();
        }

        private void UpdateProduction()
        {
            for (int i = 0; i < animals.Count; i++)
            {
                var animal = animals[i];
                if (animal.hasProduct) continue;

                float elapsed = Time.time - animal.lastProduceTime;
                if (elapsed >= animal.animalData.produceTime)
                {
                    animal.hasProduct = true;
                    animals[i] = animal;

                    // Show product indicator
                    UpdateAnimalVisual(animal, true);

                    GameEvents.TriggerAnimalProductReady(animal.instanceId);
                }
            }
        }

        private void SpawnAnimalVisual(AnimalInstance animal)
        {
            GameObject visual;

            if (animal.animalData.prefab != null)
            {
                visual = Instantiate(animal.animalData.prefab, animal.position, Quaternion.identity);
            }
            else
            {
                // Placeholder visual
                visual = CreatePlaceholderAnimal(animal);
            }

            visual.name = $"Animal_{animal.instanceId}";
            visual.transform.SetParent(transform);

            // Add idle animation component
            var idleAnim = visual.AddComponent<AnimalIdleAnimation>();
            idleAnim.animalType = animal.animalData.animalId;

            animalVisuals[animal.instanceId] = visual;
        }

        private GameObject CreatePlaceholderAnimal(AnimalInstance animal)
        {
            var parent = new GameObject($"Animal_{animal.animalData.animalId}");
            parent.transform.position = animal.position;

            // Body
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.transform.SetParent(parent.transform);
            body.transform.localPosition = new Vector3(0, 0.3f, 0);

            Color bodyColor;
            float bodyScale;

            switch (animal.animalData.animalId)
            {
                case "chicken":
                    bodyColor = Color.white;
                    bodyScale = 0.25f;
                    body.transform.localScale = new Vector3(bodyScale, bodyScale * 0.9f, bodyScale * 1.1f);
                    break;
                case "cow":
                    bodyColor = new Color(0.98f, 0.98f, 0.98f);
                    bodyScale = 0.45f;
                    body.transform.localScale = new Vector3(bodyScale * 1.3f, bodyScale, bodyScale * 0.8f);
                    body.transform.localPosition = new Vector3(0, 0.35f, 0);
                    break;
                case "pig":
                    bodyColor = new Color(0.97f, 0.73f, 0.82f);
                    bodyScale = 0.35f;
                    body.transform.localScale = new Vector3(bodyScale * 1.2f, bodyScale * 0.9f, bodyScale);
                    break;
                default:
                    bodyColor = Color.grey;
                    bodyScale = 0.3f;
                    body.transform.localScale = Vector3.one * bodyScale;
                    break;
            }

            var renderer = body.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = bodyColor;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            // Head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(parent.transform);
            head.transform.localScale = Vector3.one * bodyScale * 0.6f;
            head.transform.localPosition = new Vector3(bodyScale * 0.8f, 0.45f, 0);
            var headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                headRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                headRenderer.material.color = bodyColor;
            }

            // Product indicator (hidden by default)
            var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "ProductIndicator";
            indicator.transform.SetParent(parent.transform);
            indicator.transform.localScale = Vector3.one * 0.15f;
            indicator.transform.localPosition = new Vector3(0, 0.7f, 0);
            var indRenderer = indicator.GetComponent<Renderer>();
            if (indRenderer != null)
            {
                indRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                indRenderer.material.color = Color.yellow;
                indRenderer.material.SetFloat("_Smoothness", 0.9f);
            }
            indicator.SetActive(false);

            return parent;
        }

        private void UpdateAnimalVisual(AnimalInstance animal, bool showProduct)
        {
            if (animalVisuals.TryGetValue(animal.instanceId, out GameObject visual))
            {
                var indicator = visual.transform.Find("ProductIndicator");
                if (indicator != null)
                {
                    indicator.gameObject.SetActive(showProduct);
                }
            }
        }

        /// <summary>
        /// Get all animals for UI display
        /// </summary>
        public List<AnimalInstance> GetAllAnimals()
        {
            return new List<AnimalInstance>(animals);
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public List<AnimalSaveData> GetSaveData()
        {
            var data = new List<AnimalSaveData>();
            foreach (var animal in animals)
            {
                data.Add(new AnimalSaveData
                {
                    instanceId = animal.instanceId,
                    animalId = animal.animalData.animalId,
                    positionX = animal.position.x,
                    positionY = animal.position.y,
                    positionZ = animal.position.z,
                    elapsedProduceTime = Time.time - animal.lastProduceTime,
                    hasProduct = animal.hasProduct,
                });
            }
            return data;
        }

        /// <summary>
        /// Restore from save data
        /// </summary>
        public void RestoreAnimals(List<AnimalSaveData> saveData)
        {
            var gameData = GameManager.Instance.Data;

            foreach (var data in saveData)
            {
                var animalData = gameData.GetAnimal(data.animalId);
                if (animalData == null) continue;

                var instance = new AnimalInstance
                {
                    instanceId = data.instanceId,
                    animalData = animalData,
                    position = new Vector3(data.positionX, data.positionY, data.positionZ),
                    lastProduceTime = Time.time - data.elapsedProduceTime,
                    hasProduct = data.hasProduct,
                };

                animals.Add(instance);
                SpawnAnimalVisual(instance);

                if (data.hasProduct)
                    UpdateAnimalVisual(instance, true);
            }
        }
    }

    /// <summary>
    /// Runtime data for a placed animal
    /// </summary>
    public struct AnimalInstance
    {
        public string instanceId;
        public AnimalData animalData;
        public Vector3 position;
        public float lastProduceTime;
        public bool hasProduct;
    }

    [System.Serializable]
    public struct AnimalSaveData
    {
        public string instanceId;
        public string animalId;
        public float positionX, positionY, positionZ;
        public float elapsedProduceTime;
        public bool hasProduct;
    }

    /// <summary>
    /// Simple idle animation for animals (bobbing, rotating)
    /// </summary>
    public class AnimalIdleAnimation : MonoBehaviour
    {
        public string animalType;
        private float animTime;
        private Vector3 startPos;
        private float bobSpeed = 1.5f;
        private float bobAmount = 0.05f;
        private float wanderTimer;
        private float wanderInterval = 5f;
        private Quaternion targetRotation;
        private Vector3 wanderCenter;

        private void Start()
        {
            startPos = transform.position;
            wanderCenter = startPos;
            animTime = Random.Range(0f, Mathf.PI * 2f);
            targetRotation = transform.rotation;
            wanderTimer = Random.Range(0f, wanderInterval);
        }

        private void Update()
        {
            animTime += Time.deltaTime;

            // Gentle bobbing
            float bob = Mathf.Sin(animTime * bobSpeed) * bobAmount;
            transform.position = new Vector3(
                transform.position.x,
                startPos.y + bob,
                transform.position.z
            );

            // Occasional direction change
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                wanderTimer = wanderInterval + Random.Range(-1f, 1f);
                float randomAngle = Random.Range(0f, 360f);
                targetRotation = Quaternion.Euler(0, randomAngle, 0);

                // Small wander movement
                Vector3 wanderOffset = new Vector3(
                    Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f)
                );
                Vector3 newPos = wanderCenter + wanderOffset;
                startPos = new Vector3(newPos.x, startPos.y, newPos.z);
            }

            // Smooth rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        }
    }
}
