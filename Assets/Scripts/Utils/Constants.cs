namespace FarmValley.Utils
{
    /// <summary>
    /// Game-wide constants and configuration values.
    /// </summary>
    public static class Constants
    {
        // Grid
        public const int GRID_WIDTH = 8;
        public const int GRID_HEIGHT = 8;
        public const float CELL_SIZE = 1.5f;

        // Economy
        public const int STARTING_COINS = 100;
        public const int STARTING_LEVEL = 1;
        public const int XP_PER_HARVEST = 10;
        public const int XP_PER_ORDER = 25;
        public const int XP_PER_PRODUCTION = 15;

        // Progression
        public static int XPForLevel(int level) => 50 + (level * 30);

        // Save
        public const string SAVE_KEY = "FarmValley_SaveData";

        // Layers
        public const string FARM_PLOT_TAG = "FarmPlot";
        public const string ANIMAL_PEN_TAG = "AnimalPen";
        public const string BUILDING_TAG = "Building";

        // UI Animation
        public const float UI_FADE_DURATION = 0.25f;
        public const float UI_SLIDE_DURATION = 0.3f;
        public const float NOTIFICATION_DURATION = 2.5f;

        // Crop Growth Stages
        public const int GROWTH_STAGE_SEED = 0;
        public const int GROWTH_STAGE_SPROUT = 1;
        public const int GROWTH_STAGE_GROWING = 2;
        public const int GROWTH_STAGE_MATURE = 3;

        // Order Board
        public const int MAX_ACTIVE_ORDERS = 4;
        public const float ORDER_REFRESH_TIME = 120f; // seconds
    }
}
