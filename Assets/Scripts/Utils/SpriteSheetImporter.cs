using UnityEngine;
using System.Collections.Generic;

namespace FarmValley.Utils
{
    /// <summary>
    /// Utility to slice sprite sheets at runtime and map sprites to item IDs.
    /// The 16px sprite sheets from OpenGameArt (by Kelvin Shadewing, CC-BY-SA 4.0)
    /// contain item icons arranged in a grid. This class loads them and provides
    /// lookup by item ID for use in UI (inventory, shop, etc.).
    ///
    /// Usage:
    /// 1. Place materials1.png, materials2.png, materials3.png in Assets/Art/Sprites/
    /// 2. In Unity: Select each PNG → Inspector → Texture Type: Sprite (2D and UI)
    ///    → Sprite Mode: Multiple → Sprite Editor → Slice (Grid, 16x16) → Apply
    /// 3. Or use this runtime slicer for quick prototyping.
    /// </summary>
    public class SpriteSheetImporter : MonoBehaviour
    {
        [Header("Sprite Sheets")]
        [Tooltip("Assign materials1, materials2, materials3 textures here")]
        [SerializeField] private Texture2D[] spriteSheets;

        [Header("Settings")]
        [SerializeField] private int cellSize = 16;
        [SerializeField] private FilterMode filterMode = FilterMode.Point;

        private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        // Mapping of item IDs to sprite sheet index and cell position (col, row from top-left)
        // These correspond to the 16px items from the OpenGameArt sprite sheets
        private static readonly Dictionary<string, SpriteLocation> itemSpriteMap = new Dictionary<string, SpriteLocation>
        {
            // Materials1.png - gems, ores, food items
            { "wheat",      new SpriteLocation(0, 6, 0) },
            { "bread",      new SpriteLocation(0, 6, 1) },
            { "egg",        new SpriteLocation(0, 5, 4) },
            { "cheese",     new SpriteLocation(0, 5, 5) },
            { "apple",      new SpriteLocation(0, 5, 2) },
            { "pumpkin",    new SpriteLocation(0, 5, 6) },
            { "mushroom",   new SpriteLocation(0, 5, 7) },
            { "fish",       new SpriteLocation(0, 6, 2) },
            { "cake",       new SpriteLocation(0, 6, 4) },
            { "gem_red",    new SpriteLocation(0, 0, 0) },
            { "gem_blue",   new SpriteLocation(0, 0, 1) },
            { "gem_green",  new SpriteLocation(0, 0, 2) },
            { "gold_ore",   new SpriteLocation(0, 3, 0) },
            { "iron_ore",   new SpriteLocation(0, 3, 2) },
            { "wood",       new SpriteLocation(0, 4, 0) },
            { "stone",      new SpriteLocation(0, 4, 2) },
            { "coal",       new SpriteLocation(0, 4, 4) },
            { "coin",       new SpriteLocation(0, 2, 0) },
            { "potion",     new SpriteLocation(0, 6, 6) },

            // Materials2.png - tools, weapons, misc items
            { "axe",        new SpriteLocation(1, 1, 0) },
            { "pickaxe",    new SpriteLocation(1, 1, 1) },
            { "hoe",        new SpriteLocation(1, 1, 2) },
            { "sword",      new SpriteLocation(1, 0, 0) },
            { "shield",     new SpriteLocation(1, 0, 6) },
            { "hammer",     new SpriteLocation(1, 1, 4) },
            { "bucket",     new SpriteLocation(1, 2, 6) },
            { "meat",       new SpriteLocation(1, 5, 6) },

            // Materials3.png - more items
            { "corn",       new SpriteLocation(2, 1, 0) },
            { "carrot",     new SpriteLocation(2, 1, 2) },
            { "milk",       new SpriteLocation(2, 2, 0) },
        };

        private void Awake()
        {
            SliceAllSheets();
        }

        /// <summary>
        /// Slice all sprite sheets and cache the sprites
        /// </summary>
        public void SliceAllSheets()
        {
            spriteCache.Clear();

            if (spriteSheets == null || spriteSheets.Length == 0)
            {
                Debug.LogWarning("[SpriteSheetImporter] No sprite sheets assigned.");
                return;
            }

            foreach (var kvp in itemSpriteMap)
            {
                var loc = kvp.Value;
                if (loc.sheetIndex >= spriteSheets.Length) continue;
                if (spriteSheets[loc.sheetIndex] == null) continue;

                var tex = spriteSheets[loc.sheetIndex];
                Sprite sprite = ExtractSprite(tex, loc.col, loc.row, kvp.Key);
                if (sprite != null)
                {
                    spriteCache[kvp.Key] = sprite;
                }
            }

            Debug.Log($"[SpriteSheetImporter] Loaded {spriteCache.Count} item sprites");
        }

        private Sprite ExtractSprite(Texture2D texture, int col, int row, string name)
        {
            // Unity texture coordinates start from bottom-left, but our rows are from top
            int cols = texture.width / cellSize;
            int rows = texture.height / cellSize;

            if (col >= cols || row >= rows) return null;

            // Convert from top-left row to bottom-left y coordinate
            int pixelX = col * cellSize;
            int pixelY = texture.height - (row + 1) * cellSize;

            Rect rect = new Rect(pixelX, pixelY, cellSize, cellSize);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            return Sprite.Create(texture, rect, pivot, cellSize);
        }

        /// <summary>
        /// Get a sprite by item ID
        /// </summary>
        public Sprite GetSprite(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            // Try exact match
            if (spriteCache.TryGetValue(itemId, out Sprite sprite))
                return sprite;

            // Try lowercase
            string lower = itemId.ToLower();
            if (spriteCache.TryGetValue(lower, out sprite))
                return sprite;

            return null;
        }

        /// <summary>
        /// Check if a sprite exists for the given item ID
        /// </summary>
        public bool HasSprite(string itemId)
        {
            return GetSprite(itemId) != null;
        }

        /// <summary>
        /// Get all cached sprites
        /// </summary>
        public Dictionary<string, Sprite> GetAllSprites()
        {
            return new Dictionary<string, Sprite>(spriteCache);
        }
    }

    /// <summary>
    /// Identifies a sprite's location in a sprite sheet
    /// </summary>
    public struct SpriteLocation
    {
        public int sheetIndex;
        public int row;
        public int col;

        public SpriteLocation(int sheet, int row, int col)
        {
            this.sheetIndex = sheet;
            this.row = row;
            this.col = col;
        }
    }
}
