using UnityEngine;

namespace FarmValley.Data
{
    /// <summary>
    /// ScriptableObject defining an animal type.
    /// Create assets via: Create > FarmValley > Animal Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnimal", menuName = "FarmValley/Animal Data")]
    public class AnimalData : ScriptableObject
    {
        [Header("Identity")]
        public string animalId;
        public string animalName;
        public Sprite icon;

        [Header("Economy")]
        public int buyCost = 50;

        [Header("Production")]
        public string produceItemId;
        public string produceItemName;
        public Sprite produceIcon;
        public int produceAmount = 1;
        public int produceSellPrice = 8;
        [Tooltip("Time in seconds between each production cycle")]
        public float produceTime = 25f;

        [Header("Visuals")]
        public GameObject prefab;
        public RuntimeAnimatorController animatorController;

        [Header("Feeding")]
        [Tooltip("Item required to feed this animal (leave empty if no feeding required)")]
        public string feedItemId;
        public int feedAmount = 1;

        [Header("Unlock")]
        public int requiredLevel = 1;
    }
}
