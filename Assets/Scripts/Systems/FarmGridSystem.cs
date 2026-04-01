using System.Collections.Generic;
using UnityEngine;
using FarmValley.Events;
using FarmValley.Utils;

namespace FarmValley.Systems
{
    /// <summary>
    /// Manages the grid-based farm land.
    /// Handles plot creation, selection, and state tracking.
    /// </summary>
    public class FarmGridSystem : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private float cellSize = 1.5f;

        [Header("Prefabs")]
        [SerializeField] private GameObject farmPlotPrefab;
        [SerializeField] private GameObject lockedPlotPrefab;

        [Header("Materials")]
        [SerializeField] private Material soilMaterial;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material lockedMaterial;

        private PlotState[,] grid;
        private GameObject[,] plotObjects;
        private Vector2Int? selectedPlot = null;
        private Transform gridParent;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellSize => cellSize;

        public void Initialize(int width, int height, float size)
        {
            gridWidth = width;
            gridHeight = height;
            cellSize = size;

            grid = new PlotState[gridWidth, gridHeight];
            plotObjects = new GameObject[gridWidth, gridHeight];

            gridParent = new GameObject("FarmGrid").transform;
            gridParent.SetParent(transform);

            CreateGrid();

            Debug.Log($"[FarmGrid] Initialized {gridWidth}x{gridHeight} grid");
        }

        private void CreateGrid()
        {
            float offsetX = -(gridWidth * cellSize) / 2f + cellSize / 2f;
            float offsetZ = -(gridHeight * cellSize) / 2f + cellSize / 2f;

            for (int z = 0; z < gridHeight; z++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Vector3 worldPos = new Vector3(
                        offsetX + x * cellSize,
                        0f,
                        offsetZ + z * cellSize
                    );

                    // Create plot visual
                    GameObject plotObj;
                    bool isUnlocked = IsPlotUnlocked(x, z);

                    if (farmPlotPrefab != null)
                    {
                        plotObj = Instantiate(
                            isUnlocked ? farmPlotPrefab : (lockedPlotPrefab != null ? lockedPlotPrefab : farmPlotPrefab),
                            worldPos,
                            Quaternion.identity,
                            gridParent
                        );
                    }
                    else
                    {
                        // Placeholder: simple quad
                        plotObj = CreatePlaceholderPlot(worldPos, isUnlocked);
                    }

                    plotObj.name = $"Plot_{x}_{z}";
                    plotObj.tag = Constants.FARM_PLOT_TAG;

                    // Store grid data on the object
                    var plotComponent = plotObj.AddComponent<FarmPlot>();
                    plotComponent.GridPosition = new Vector2Int(x, z);
                    plotComponent.IsUnlocked = isUnlocked;

                    grid[x, z] = new PlotState
                    {
                        position = new Vector2Int(x, z),
                        isUnlocked = isUnlocked,
                        cropId = null,
                        growthStartTime = 0f,
                    };

                    plotObjects[x, z] = plotObj;
                }
            }
        }

        private GameObject CreatePlaceholderPlot(Vector3 position, bool isUnlocked)
        {
            GameObject plot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plot.transform.position = position;
            plot.transform.localScale = new Vector3(cellSize * 0.9f, 0.15f, cellSize * 0.9f);

            var renderer = plot.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (isUnlocked)
                {
                    if (soilMaterial != null)
                        renderer.material = soilMaterial;
                    else
                        renderer.material.color = new Color(0.55f, 0.43f, 0.39f); // Soil brown
                }
                else
                {
                    if (lockedMaterial != null)
                        renderer.material = lockedMaterial;
                    else
                        renderer.material.color = new Color(0.6f, 0.6f, 0.6f, 0.5f); // Grey locked
                }
            }

            return plot;
        }

        private bool IsPlotUnlocked(int x, int z)
        {
            // Start with a 4x4 area unlocked in the center
            int centerX = gridWidth / 2;
            int centerZ = gridHeight / 2;
            return Mathf.Abs(x - centerX) < 2 && Mathf.Abs(z - centerZ) < 2;
        }

        /// <summary>
        /// Try to select a plot at the given grid position
        /// </summary>
        public bool SelectPlot(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos)) return false;
            if (!grid[gridPos.x, gridPos.y].isUnlocked) return false;

            // Deselect previous
            if (selectedPlot.HasValue)
                DeselectPlot(selectedPlot.Value);

            selectedPlot = gridPos;
            HighlightPlot(gridPos, true);
            GameEvents.TriggerPlotSelected(gridPos);
            return true;
        }

        public void DeselectPlot(Vector2Int gridPos)
        {
            HighlightPlot(gridPos, false);
            if (selectedPlot.HasValue && selectedPlot.Value == gridPos)
                selectedPlot = null;
        }

        public void DeselectCurrent()
        {
            if (selectedPlot.HasValue)
                DeselectPlot(selectedPlot.Value);
        }

        private void HighlightPlot(Vector2Int gridPos, bool highlighted)
        {
            var plotObj = plotObjects[gridPos.x, gridPos.y];
            if (plotObj == null) return;

            var renderer = plotObj.GetComponent<Renderer>();
            if (renderer == null) return;

            if (highlighted && highlightMaterial != null)
            {
                renderer.material = highlightMaterial;
            }
            else if (soilMaterial != null)
            {
                renderer.material = soilMaterial;
            }
            else
            {
                renderer.material.color = highlighted
                    ? new Color(0.47f, 0.80f, 0.29f) // Green highlight
                    : new Color(0.55f, 0.43f, 0.39f); // Soil brown
            }
        }

        /// <summary>
        /// Get the current state of a plot
        /// </summary>
        public PlotState GetPlotState(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos)) return default;
            return grid[gridPos.x, gridPos.y];
        }

        /// <summary>
        /// Set the crop on a plot
        /// </summary>
        public void SetCrop(Vector2Int gridPos, string cropId, float startTime)
        {
            if (!IsValidPosition(gridPos)) return;
            grid[gridPos.x, gridPos.y].cropId = cropId;
            grid[gridPos.x, gridPos.y].growthStartTime = startTime;
        }

        /// <summary>
        /// Clear the crop from a plot
        /// </summary>
        public void ClearPlot(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos)) return;
            grid[gridPos.x, gridPos.y].cropId = null;
            grid[gridPos.x, gridPos.y].growthStartTime = 0f;
            GameEvents.TriggerPlotCleared(gridPos);
        }

        /// <summary>
        /// Unlock a plot for farming
        /// </summary>
        public void UnlockPlot(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos)) return;
            grid[gridPos.x, gridPos.y].isUnlocked = true;

            var plotObj = plotObjects[gridPos.x, gridPos.y];
            if (plotObj != null)
            {
                var renderer = plotObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (soilMaterial != null)
                        renderer.material = soilMaterial;
                    else
                        renderer.material.color = new Color(0.55f, 0.43f, 0.39f);
                }

                var farmPlot = plotObj.GetComponent<FarmPlot>();
                if (farmPlot != null)
                    farmPlot.IsUnlocked = true;
            }
        }

        /// <summary>
        /// Get the world position of a grid cell
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float offsetX = -(gridWidth * cellSize) / 2f + cellSize / 2f;
            float offsetZ = -(gridHeight * cellSize) / 2f + cellSize / 2f;
            return new Vector3(
                offsetX + gridPos.x * cellSize,
                0.2f,
                offsetZ + gridPos.y * cellSize
            );
        }

        /// <summary>
        /// Convert a world position to grid coordinates
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            float offsetX = -(gridWidth * cellSize) / 2f + cellSize / 2f;
            float offsetZ = -(gridHeight * cellSize) / 2f + cellSize / 2f;
            int x = Mathf.RoundToInt((worldPos.x - offsetX) / cellSize);
            int z = Mathf.RoundToInt((worldPos.z - offsetZ) / cellSize);
            return new Vector2Int(x, z);
        }

        /// <summary>
        /// Get the plot GameObject at given grid position
        /// </summary>
        public GameObject GetPlotObject(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos)) return null;
            return plotObjects[gridPos.x, gridPos.y];
        }

        public bool IsValidPosition(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < gridWidth &&
                   gridPos.y >= 0 && gridPos.y < gridHeight;
        }

        public bool IsPlotEmpty(Vector2Int gridPos)
        {
            if (!IsValidPosition(gridPos)) return false;
            return grid[gridPos.x, gridPos.y].isUnlocked &&
                   string.IsNullOrEmpty(grid[gridPos.x, gridPos.y].cropId);
        }

        public Vector2Int? GetSelectedPlot() => selectedPlot;

        /// <summary>
        /// Get all plots with crops for save/load
        /// </summary>
        public List<PlotState> GetAllPlotStates()
        {
            var states = new List<PlotState>();
            for (int z = 0; z < gridHeight; z++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    states.Add(grid[x, z]);
                }
            }
            return states;
        }

        /// <summary>
        /// Restore plot states from save data
        /// </summary>
        public void RestorePlotStates(List<PlotState> states)
        {
            foreach (var state in states)
            {
                if (IsValidPosition(state.position))
                {
                    grid[state.position.x, state.position.y] = state;
                    if (state.isUnlocked)
                        UnlockPlot(state.position);
                }
            }
        }
    }

    /// <summary>
    /// State of a single farm plot
    /// </summary>
    [System.Serializable]
    public struct PlotState
    {
        public Vector2Int position;
        public bool isUnlocked;
        public string cropId;
        public float growthStartTime;
    }

    /// <summary>
    /// Component attached to each farm plot object for raycasting identification
    /// </summary>
    public class FarmPlot : MonoBehaviour
    {
        public Vector2Int GridPosition { get; set; }
        public bool IsUnlocked { get; set; }
    }
}
