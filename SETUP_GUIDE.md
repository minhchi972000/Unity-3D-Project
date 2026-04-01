# Farm Valley - 3D Farming Simulation Game

A 3D farming simulation game built in Unity with C#, inspired by casual mobile farm games. Features bright cartoon visuals, cute animals, satisfying harvesting, and simple production loops.

## Tech Stack

- **Engine**: Unity 2022.3+ (LTS recommended)
- **Language**: C#
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Target Platform**: Android first, scalable to iOS
- **Architecture**: Clean, modular, event-driven

---

## Project Architecture

```
Assets/
├── Scripts/
│   ├── Core/                    # Central managers and setup
│   │   ├── GameManager.cs       # Singleton orchestrator for all systems
│   │   ├── GameDataContainer.cs # ScriptableObject container for all game data
│   │   ├── InputHandler.cs      # Touch/mouse input → game actions
│   │   └── FarmSceneSetup.cs    # Auto-configures scene lighting/camera
│   │
│   ├── Systems/                 # Independent game systems (decoupled)
│   │   ├── FarmGridSystem.cs    # Grid-based farm land management
│   │   ├── CropSystem.cs        # Planting and harvesting logic
│   │   ├── GrowthSystem.cs      # Time-based crop growth tracking
│   │   ├── InventorySystem.cs   # Item storage and management
│   │   ├── EconomySystem.cs     # Coins, XP, and level progression
│   │   ├── AnimalSystem.cs      # Animal placement and production cycles
│   │   ├── ProductionSystem.cs  # Building crafting queues
│   │   ├── OrderSystem.cs       # Order board generation and fulfillment
│   │   ├── ProgressionSystem.cs # Level-based content unlocks
│   │   └── SaveSystem.cs        # Persistent save/load via PlayerPrefs
│   │
│   ├── Data/                    # ScriptableObject definitions
│   │   ├── CropData.cs          # Crop type definition
│   │   ├── AnimalData.cs        # Animal type definition
│   │   ├── ItemData.cs          # Generic item definition
│   │   ├── ProductionRecipeData.cs    # Crafting recipe
│   │   ├── ProductionBuildingData.cs  # Production building
│   │   ├── OrderData.cs         # Order template
│   │   └── LevelData.cs         # Level progression/unlocks
│   │
│   ├── UI/                      # UI panel controllers
│   │   ├── UIManager.cs         # Central UI coordinator + HUD
│   │   ├── InventoryUI.cs       # Inventory panel
│   │   ├── ShopUI.cs            # Buy/sell shop panel
│   │   ├── CropSelectorUI.cs    # Crop selection popup
│   │   ├── OrderBoardUI.cs      # Order board panel
│   │   └── ProductionUI.cs      # Production building panel
│   │
│   ├── Camera/
│   │   └── IsometricCameraController.cs  # Pan, zoom, isometric view
│   │
│   ├── Player/                  # Player character system
│   │   ├── PlayerController.cs  # WASD/arrow key movement + interaction
│   │   ├── PlayerVisualSetup.cs # Placeholder 3D character from primitives
│   │   ├── PlayerAnimator.cs    # Procedural idle/walk/run animations
│   │   └── PlayerInteraction.cs # E-key interaction with plots/animals
│   │
│   ├── Events/
│   │   └── GameEvents.cs        # Static event bus for system communication
│   │
│   └── Utils/
│       ├── Singleton.cs         # Generic singleton MonoBehaviour base
│       ├── Constants.cs         # Game-wide constants
│       ├── PlaceholderArtSetup.cs  # Runtime placeholder art generator
│       └── SpriteSheetImporter.cs  # OpenGameArt sprite sheet loader
│
├── ScriptableObjects/           # Data assets (create in editor)
│   ├── Crops/                   # Wheat, Corn, Carrot data
│   ├── Animals/                 # Chicken, Cow data
│   ├── Production/              # Feed Mill, Bakery recipes
│   ├── Orders/                  # Order templates
│   └── Progression/             # Level unlock data
│
├── Prefabs/                     # Reusable prefabs
│   ├── Crops/                   # Crop stage visuals
│   ├── Animals/                 # Animal models
│   ├── Buildings/               # Production buildings
│   ├── UI/                      # UI element prefabs
│   └── Farm/                    # Farm plot, fences, etc.
│
├── Materials/                   # Shared materials
│   ├── Crops/
│   ├── Animals/
│   ├── Buildings/
│   ├── Terrain/
│   └── UI/
│
├── Scenes/
│   └── FarmScene.unity          # Main game scene
│
└── Resources/                   # Runtime-loaded assets
```

---

## Scene Hierarchy

```
FarmScene
├── --- MANAGERS ---
│   ├── GameManager              [GameManager.cs]
│   │   ├── FarmGridSystem       [FarmGridSystem.cs]
│   │   ├── CropSystem           [CropSystem.cs]
│   │   ├── GrowthSystem         [GrowthSystem.cs]
│   │   ├── InventorySystem      [InventorySystem.cs]
│   │   ├── EconomySystem        [EconomySystem.cs]
│   │   ├── AnimalSystem         [AnimalSystem.cs]
│   │   ├── ProductionSystem     [ProductionSystem.cs]
│   │   ├── OrderSystem          [OrderSystem.cs]
│   │   ├── SaveSystem           [SaveSystem.cs]
│   │   └── ProgressionSystem    [ProgressionSystem.cs]
│   │
│   ├── InputHandler             [InputHandler.cs]
│   └── SceneSetup               [FarmSceneSetup.cs, PlaceholderArtSetup.cs]
│
├── --- PLAYER ---
│   └── Player                   [PlayerController.cs, PlayerVisualSetup.cs,
│                                 PlayerAnimator.cs, PlayerInteraction.cs]
│
├── --- CAMERA ---
│   └── Main Camera              [Camera, IsometricCameraController.cs]
│                                 (follows Player automatically)
│
├── --- ENVIRONMENT ---
│   ├── Ground                   (auto-generated or placed)
│   ├── Barn
│   ├── Trees
│   ├── Fences
│   └── Decorations
│
├── --- FARM ---
│   └── FarmGrid                 (auto-generated by FarmGridSystem)
│       ├── Plot_0_0
│       ├── Plot_0_1
│       └── ...
│
├── --- LIGHTING ---
│   ├── Directional Light (Sun)
│   └── (URP Volume if needed)
│
└── --- UI ---
    └── Canvas                   [UIManager.cs]
        ├── HUD
        │   ├── CoinDisplay
        │   ├── LevelDisplay
        │   └── XPBar
        ├── BottomToolbar
        │   ├── PlantButton
        │   ├── HarvestButton
        │   ├── InventoryButton
        │   ├── ShopButton
        │   ├── OrderButton
        │   └── AnimalButton
        ├── Panels
        │   ├── InventoryPanel   [InventoryUI.cs]
        │   ├── ShopPanel        [ShopUI.cs]
        │   ├── CropSelectorPanel [CropSelectorUI.cs]
        │   ├── OrderBoardPanel  [OrderBoardUI.cs]
        │   ├── ProductionPanel  [ProductionUI.cs]
        │   └── AnimalPanel
        ├── NotificationArea
        └── EventSystem
```

---

## Step-by-Step Setup Instructions

### Phase 1: Basic Setup

1. **Create a new Unity project** (or use this existing one) with URP.

2. **Import TextMeshPro**: When prompted, click "Import TMP Essentials".

3. **Create the Scene**:
   - Open or create `Assets/Scenes/FarmScene.unity`
   - Delete the default "SampleScene" if desired

4. **Create the GameManager**:
   - Create an empty GameObject named `GameManager`
   - Add the `GameManager.cs` script
   - Create child GameObjects for each system:
     - `FarmGridSystem`, `CropSystem`, `GrowthSystem`, `InventorySystem`
     - `EconomySystem`, `AnimalSystem`, `ProductionSystem`
     - `OrderSystem`, `SaveSystem`, `ProgressionSystem`
   - Add the corresponding script to each child
   - Wire up the references in the GameManager inspector

5. **Create ScriptableObject Data**:
   - **GameDataContainer**: Right-click in `ScriptableObjects/` → Create → FarmValley → Game Data Container
   - **Crops**: Create → FarmValley → Crop Data (create wheat, corn, carrot)
   - **Animals**: Create → FarmValley → Animal Data (create chicken, cow)
   - **Items**: Create → FarmValley → Item Data (create items for all resources)
   - **Buildings**: Create → FarmValley → Production Building (feed_mill, bakery)
   - **Recipes**: Create → FarmValley → Production Recipe (bread, popcorn, etc.)
   - **Orders**: Create → FarmValley → Order Data (various sell orders)
   - **Levels**: Create → FarmValley → Level Data (level 1-10 progression)
   - Assign all created assets to the GameDataContainer

6. **Example Crop Data values**:
   | Crop   | Seed Cost | Sell Price | Growth Time | Yield | Required Level |
   |--------|-----------|------------|-------------|-------|----------------|
   | Wheat  | 5         | 10         | 30s         | 2     | 1              |
   | Corn   | 8         | 15         | 45s         | 2     | 1              |
   | Carrot | 3         | 7          | 20s         | 3     | 1              |

7. **Example Animal Data values**:
   | Animal  | Buy Cost | Produce     | Produce Time | Sell Price | Level |
   |---------|----------|-------------|--------------|------------|-------|
   | Chicken | 50       | Egg         | 25s          | 8          | 2     |
   | Cow     | 120      | Milk        | 40s          | 15         | 3     |

8. **Example Production Building values**:
   | Building  | Cost | Recipes                    | Level |
   |-----------|------|----------------------------|-------|
   | Feed Mill | 100  | Feed (wheat→feed)          | 2     |
   | Bakery    | 200  | Bread (wheat→bread), etc.  | 3     |

### Phase 2: Camera & Input

9. **Setup Camera**:
   - Select Main Camera
   - Set to Orthographic, size 10
   - Add `IsometricCameraController.cs`
   - Position: (14, 16, -14), Rotation: (45, -45, 0)

10. **Setup Input**:
    - Create empty `InputHandler` GameObject
    - Add `InputHandler.cs`
    - Wire up references to FarmGridSystem, CropSystem, AnimalSystem

### Phase 3: Environment

11. **Placeholder Art**:
    - Create empty `SceneSetup` GameObject
    - Add `FarmSceneSetup.cs` (auto-configures lighting and camera)
    - Add `PlaceholderArtSetup.cs` (generates ground, barn, trees, fences)
    - Both run automatically on Start

12. **Lighting**:
    - The FarmSceneSetup script auto-creates a warm directional light
    - For manual setup: Directional Light at rotation (50, -30, 0), warm color

### Phase 4: UI

13. **Create Canvas**:
    - Add Canvas (Screen Space - Overlay)
    - Add `UIManager.cs` to Canvas
    - Set Canvas Scaler: Scale With Screen Size, Reference Resolution 1080x1920
    - Add EventSystem

14. **HUD (top bar)**:
    - Create a horizontal layout at the top
    - Add TextMeshPro elements for: Coins, Level, XP Bar
    - Style: rounded backgrounds, large readable text, warm brown colors

15. **Bottom Toolbar**:
    - Create horizontal layout group at bottom center
    - Add 6 buttons: Plant, Harvest, Inventory, Shop, Orders, Animals
    - Style: 64px rounded buttons with icons, colorful gradients

16. **Panels**:
    - Create panel GameObjects for each UI panel (Inventory, Shop, etc.)
    - Add the corresponding UI scripts
    - Create item slot/card prefabs for dynamic content
    - Start with panels hidden (SetActive false)

17. **Notifications**:
    - Create a notification prefab with TextMeshPro text
    - Place notification parent at top-right of screen
    - Auto-destroy after 3 seconds

### Phase 5: Player Character

18. **Create Player**:
    - Create an empty GameObject named `Player`
    - Add scripts: `PlayerController.cs`, `PlayerVisualSetup.cs`, `PlayerAnimator.cs`, `PlayerInteraction.cs`
    - Position at farm center: (6, 0.5, 6)
    - Add a CapsuleCollider (height 1.8, radius 0.3) and Rigidbody (freeze rotation X/Y/Z)
    - PlayerVisualSetup auto-generates the farmer character model on Start
    - PlayerAnimator auto-finds visual parts on Start

19. **Wire Camera to Player**:
    - Select Main Camera → IsometricCameraController
    - Drag the Player GameObject into the `Player Target` field
    - Enable `Follow Player` checkbox (enabled by default)
    - Camera will now follow the player instead of WASD panning

20. **Player Controls**:
    - **WASD / Arrow Keys**: Move player around the farm
    - **Shift**: Hold to run (faster movement)
    - **E**: Interact with nearby plots and animals
    - Player stays within farm bounds automatically
    - Camera follows player with smooth lerp

### Phase 6: Sprite Assets (Optional)

21. **Import OpenGameArt Sprites**:
    - Sprite sheets are in `Assets/Art/Sprites/` (materials1-3.png)
    - See `Assets/Art/Sprites/ATTRIBUTION.md` for import instructions
    - Select each PNG → Inspector → Texture Type: Sprite (2D and UI)
    - Sprite Mode: Multiple → Sprite Editor → Slice (Grid, 16x16) → Apply
    - Filter Mode: Point (no filter) for pixel art crispness
    - Optionally add `SpriteSheetImporter.cs` to a GameObject and assign sheets

### Phase 7: Tags & Layers

22. **Configure Tags**:
    - Add tag: `FarmPlot`
    - Add tag: `AnimalPen`
    - Add tag: `Building`

23. **Configure Layers** (optional):
    - Layer 6: `FarmGrid`
    - Layer 7: `Animals`
    - Layer 8: `Buildings`

---

## Placeholder Art Strategy

The prototype uses **runtime-generated primitives** so no custom 3D assets are needed:

| Element      | Placeholder          | Replace With                |
|-------------|---------------------|-----------------------------|
| Farm plots   | Brown cubes          | Tilled soil mesh            |
| Crops (seed) | Small brown spheres  | Seed mound models           |
| Crops (grow) | Green cylinders      | Stage-specific crop models  |
| Crops (ripe) | Golden cylinders     | Full-grown crop models      |
| Chickens     | White spheres        | Low-poly chicken model      |
| Cows         | Large white boxes    | Low-poly cow model          |
| Barn         | Red box + brown roof | Detailed barn model         |
| Trees        | Stacked green spheres| Low-poly cartoon trees      |
| Fences       | Thin cylinders       | Wooden fence models         |
| Buildings    | Colored boxes        | Production building models  |

All placeholders are created in code via `PlaceholderArtSetup.cs` and the system's `CreatePlaceholder*` methods. When real art is ready, simply assign prefabs to the ScriptableObject `stagePrefabs` and `prefab` fields.

---

## Key Design Patterns

### Event-Driven Communication
Systems communicate via `GameEvents.cs` static events:
```csharp
// Subscribe
GameEvents.OnCropHarvested += HandleHarvest;

// Trigger
GameEvents.TriggerCropHarvested(gridPosition);
```

### ScriptableObject Data
All game data is defined as ScriptableObjects for designer-friendly editing:
```csharp
[CreateAssetMenu(menuName = "FarmValley/Crop Data")]
public class CropData : ScriptableObject { ... }
```

### Singleton Pattern
Core managers use a generic Singleton base:
```csharp
public class GameManager : Singleton<GameManager> { ... }
// Access: GameManager.Instance.Economy.AddCoins(50);
```

---

## Implementation Phases

### Phase 1 (Core) ✓
- Isometric camera with pan/zoom
- Grid-based farm with plot selection
- Crop planting, growth, and harvesting
- Inventory system
- Coin economy with XP/levels

### Phase 2 (Content) ✓
- Animal system (chickens, cows) with production
- Production buildings (feed mill, bakery)
- Crafting recipes
- Order board system
- Level progression with unlocks

### Phase 3 (Polish) ✓
- Save/load system (PlayerPrefs + JSON)
- Auto-save on pause/quit
- UI panels with dynamic content
- Notification system
- Placeholder art generator
- Mobile touch input support

### Phase 4 (Player Character) ✓
- Player character with WASD/arrow key movement
- Shift to run, E to interact
- Procedural animations (idle bob, walk/run limb swing)
- Placeholder 3D farmer model (primitives)
- Camera follows player
- Player interaction with farm plots and animals
- OpenGameArt sprite assets (CC-BY-SA 4.0) for item icons

---

## Extending the Game

### Add a New Crop
1. Create a new CropData asset: Create → FarmValley → Crop Data
2. Fill in: cropId, name, costs, growth time, yield
3. Optionally assign stage prefabs for custom visuals
4. Add to the GameDataContainer's crops array
5. Add corresponding ItemData for the harvested crop
6. Add to a LevelData's unlockedCrops if level-gated

### Add a New Animal
1. Create AnimalData asset with produce info
2. Add to GameDataContainer's animals array
3. Create corresponding ItemData for the produce

### Add a New Recipe
1. Create ProductionRecipeData with ingredients and output
2. Add to a ProductionBuildingData's availableRecipes
3. Create ItemData for the output product

### Add a New Order
1. Create OrderData with requirements and rewards
2. Add to GameDataContainer's orders array
3. Set min/max level range
