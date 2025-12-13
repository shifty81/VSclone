# Timeless Tales - Vintage Story Clone

A Vintage Story inspired voxel-based survival sandbox game built with C# and MonoGame for Windows.

## Current Features (Alpha 0.1)

### ✅ Implemented
- **World Generation**
  - Procedural chunk-based terrain (16x256x16 blocks per chunk)
  - Realistic geological layers (sedimentary, metamorphic, igneous rock strata)
  - Multiple biomes (Tundra, Boreal, Temperate, Desert, Tropical)
  - Cave systems
  - Non-uniform ore distribution (Copper, Tin, Iron, Coal)

- **Player Character**
  - First-person 3D movement (WASD)
  - Sprint (Shift)
  - Jump with physics (Space)
  - Mouse look camera
  - Collision detection
  - **Character Customization**:
    - Customizable name (default: "Wanderer")
    - Human-like appearance system with skin tone, hair color/style, eye color
    - Body scaling and gender presentation options
    - Clothing colors (shirt, pants)

- **Block System**
  - 20+ block types including:
    - Natural blocks: Stone, Dirt, Grass, Sand, Gravel, Clay
    - Geological rocks: Granite, Limestone, Basalt, Sandstone, Slate
    - Ores: Copper Ore, Tin Ore, Iron Ore, Coal
    - Wood blocks: Wood, Leaves, Planks
  - Each block has properties (hardness, transparency, gravity)

- **Block Interaction**
  - **Left Click**: Break blocks (adds to inventory)
  - **Right Click**: Place blocks (from inventory)
  - Block breaking progress indicator
  - Raycasting for block selection
  - Reach distance: 5 blocks

- **Inventory & Equipment System**
  - 40-slot inventory
  - Item stacking
  - Starting items (Stone, Dirt, Planks, Cobblestone, Wood, Grass, Sand, Gravel, Clay)
  - Hotbar selection (1-9 keys)
  - Visual hotbar display with selected slot highlight
  - Block breaking progress bar
  - **Equipment System**:
    - Equipment slots: Head, Chest, Legs, Feet, Hands, Back, Main Hand, Off Hand
    - Visual equipment tracking (future: equipped items will show on character)
  - **Tabbed Menu UI** (Press C):
    - **Character Tab**: View stats, appearance, and equipment slots
    - **Inventory Tab**: Organized grid view of all items
    - **Crafting Tab**: Crafting system framework (knapping, pottery, metallurgy planned)
    - **Map Tab**: Quick access to world position and map info

- **Rendering**
  - 3D voxel rendering with face culling
  - Chunk-based mesh optimization
  - Depth rendering
  - Basic lighting (directional shading on block faces)
  - Crosshair UI
  - **Dynamic skybox with day/night cycle**
  - **Sun and moon that transit across the sky**
  - **Starry night sky with twinkling stars** (500+ stars)
  - **Atmospheric color transitions** (dawn, day, dusk, night)
  - **Ambient lighting based on time of day**

## Controls

| Key | Action |
|-----|--------|
| **W/A/S/D** | Move forward/left/backward/right |
| **Space** | Jump (can jump while moving) |
| **Left Shift** | Sprint |
| **Mouse** | Free look camera (smooth mouse look) |
| **Left Click** | Break block (hold to see progress) |
| **Right Click** | Place block |
| **1-9** | Select block type from hotbar |
| **I** | Toggle inventory (simple view) |
| **C** | Toggle character sheet / tabbed menu (character, inventory, crafting, map) |
| **M** | Toggle world map |
| **F3** | Toggle debug overlay (FPS, position, chunk info) |
| **F11** | Toggle fullscreen |
| **P** | Pause |
| **Escape** | Exit game / Close menus |

## User Interface

### HUD Elements
- **Crosshair**: Center screen targeting reticle
- **Hotbar**: Bottom of screen showing 9 quick-access block slots
- **Block Breaking Progress**: Progress bar displayed when breaking blocks

### Minimap (Upper Right)
- **Location**: Top-right corner of screen
- **Size**: 150x150 pixel overhead view
- **Features**:
  - Terrain height visualization (green lowlands to gray highlands)
  - Red player position indicator at center
  - **Cardinal direction markers**: White bars at North/East, Gray bars at South/West
  - **Yellow direction arrow**: Shows player's current facing direction
  - Player coordinates displayed as colored bars below minimap:
    - Red bars = X coordinate
    - Green bars = Y coordinate (height)
    - Blue bars = Z coordinate
  - **Compass direction indicator**: Visual display showing current cardinal direction
    - North: Two tall cyan bars
    - East: Three medium yellow bars
    - South: One wide orange bar
    - West: Two wide magenta bars stacked

### Clock Gauge (Below Minimap and Compass)
- **Display**: Time of day progression gauge
- **Features**:
  - Yellow gauge fill during daytime
  - Dark blue gauge fill during nighttime
  - Moving sun/moon indicator shows current time position
  - Day count displayed as white bars (up to 10 days)
  - Visual representation of 10-minute day/night cycle

### World Map (Press M)
- **Display**: Full-screen overhead map view
- **Features**:
  - Large terrain area display (200 block radius)
  - Height-based terrain coloring
  - Red player position marker at center
  - Press M again to close

### First-Person View
- **Arms**: Visible in first-person view
- **Body Visibility**: Look down to see your character
  - Torso visible when looking down ~17+ degrees
  - Legs visible when looking down ~46+ degrees
  - Blue shirt and dark blue pants rendering

## Building & Running

### Prerequisites
- .NET 8.0 SDK or later
- Windows 10/11 (64-bit)
- DirectX 11 compatible GPU
- **Visual Studio 2022** (recommended for debugging and development)

### Opening in Visual Studio
1. Open `TimelessTales.sln` in Visual Studio 2022
2. Set `TimelessTales` as the startup project (right-click → Set as Startup Project)
3. Press F5 to build and run with debugging
4. Or press Ctrl+F5 to run without debugging

### Build Instructions (Command Line)
```bash
# Using the solution file
dotnet build TimelessTales.sln

# Or build just the game project
cd TimelessTales
dotnet build
dotnet run
```

### Build for Release
```bash
# Using the solution file (recommended)
dotnet build TimelessTales.sln -c Release

# Or build just the game project
cd TimelessTales
dotnet build -c Release
```

The executable will be in `bin/Release/net8.0/`

### Running Tests
```bash
# Run all tests
dotnet test TimelessTales.sln

# Run specific test class
dotnet test --filter "FullyQualifiedName~LoggerTests"
```

## Error Logging and Debugging

The game includes a comprehensive logging system to help diagnose crashes and errors.

### Log Files
- **Location**: `TimelessTales/bin/Debug/net8.0/logs/` (or `bin/Release/net8.0/logs/`)
- **Format**: `timeless_tales_YYYY-MM-DD_HH-mm-ss.log`
- **Content**: Timestamped entries with severity levels (INFO, WARNING, ERROR, FATAL)

### Log Levels
- **INFO**: General application flow (startup, initialization, shutdown)
- **WARNING**: Non-critical issues that don't prevent operation
- **ERROR**: Recoverable errors in the update/draw loop
- **FATAL**: Critical errors that cause the application to crash

### Debugging Tips
1. **Visual Studio Debugging**: Open `TimelessTales.sln` and use breakpoints
2. **Check Log Files**: After a crash, review the latest log file for error details
3. **Console Output**: All logs are also written to the console window
4. **Stack Traces**: Exception logs include full stack traces for debugging

### Common Issues
If the game crashes on launch:
1. Check the latest log file in the `logs/` directory
2. Look for FATAL or ERROR entries
3. Verify all prerequisites are installed (.NET 8.0, DirectX 11)
4. Try running from Visual Studio with debugging enabled (F5)

## Project Structure
```
TimelessTales/
├── Core/           # Core game systems
│   ├── TimelessTalesGame.cs  # Main game class
│   └── InputManager.cs       # Input handling
├── World/          # World generation and management
│   ├── WorldManager.cs       # Chunk loading/unloading
│   ├── WorldGenerator.cs     # Terrain generation
│   └── Chunk.cs              # Chunk data structure
├── Blocks/         # Block system
│   └── BlockRegistry.cs      # Block definitions
├── Entities/       # Game entities
│   └── Player.cs             # Player character & inventory
├── Rendering/      # Graphics rendering
│   ├── WorldRenderer.cs      # 3D world rendering
│   └── Camera.cs             # Camera system
├── UI/             # User interface
│   └── UIManager.cs          # HUD and UI
└── Utils/          # Utilities
    └── SimplexNoise.cs       # Noise generation
```

## Documentation

All project documentation has been organized in the **[Docs/](Docs/)** folder:

- **[ROADMAP.md](ROADMAP.md)** - Comprehensive development roadmap with feature tracking
- **[Docs/FAQ.md](Docs/FAQ.md)** - Frequently Asked Questions (GeonBit, technical choices, gameplay)
- **[Docs/GDD.md](Docs/GDD.md)** - Complete Game Design Document
- **[Docs/RENDERING_ARCHITECTURE.md](Docs/RENDERING_ARCHITECTURE.md)** - Why we use custom rendering (no GeonBit)
- **[Docs/QUICKSTART.md](Docs/QUICKSTART.md)** - Quick start guide for developers
- **[Docs/DEVELOPER.md](Docs/DEVELOPER.md)** - Developer documentation
- **[Docs/CONTRIBUTING.md](Docs/CONTRIBUTING.md)** - Contribution guidelines
- **[Docs/WATER_SYSTEM.md](Docs/WATER_SYSTEM.md)** - Water rendering and physics documentation
- **[Docs/SKYBOX_IMPLEMENTATION.md](Docs/SKYBOX_IMPLEMENTATION.md)** - Skybox system documentation
- **[Docs/CHANGELOG.md](Docs/CHANGELOG.md)** - Project changelog
- **[Docs/Screenshots/](Docs/Screenshots/)** - Progress screenshots for debugging

## Roadmap

See **[ROADMAP.md](ROADMAP.md)** for the complete development roadmap with detailed feature tracking.

### Recent Completions
- [x] Day/night cycle with sun/moon transit
- [x] Dynamic skybox with atmospheric color transitions
- [x] Water rendering with wave animation
- [x] Swimming physics with buoyancy
- [x] Basic lighting system (ambient lighting)
- [x] Improved inventory UI
- [x] Block breaking visual feedback
- [x] Documentation organization in Docs/ folder

### Next Steps
- [ ] Particle system for underwater bubbles
- [ ] Audio system with underwater sound filtering
- [ ] Vegetation growth stages (3 stages for grass/shrubs)
- [ ] Points of interest generation
- [ ] Temporal stability system
- [ ] Temperature and season system
- [ ] Hunger and nutrition mechanics
- [ ] Advanced crafting (knapping, pottery, metallurgy)
- [ ] Prospecting system for ore discovery
- [ ] Block textures (currently using colored blocks)
- [ ] Hostile entities (Drifters)
- [ ] Save/load system

## Technologies Used
- **C# 12** (.NET 8.0)
- **MonoGame 3.8.4** - Cross-platform game framework (Direct implementation, no GeonBit)
- **Custom 3D Rendering** - Purpose-built voxel rendering system
- **SimplexNoise** - Procedural terrain generation

### Why No GeonBit?
This project uses a **custom 3D rendering architecture** built directly on MonoGame, without using third-party 3D engines like GeonBit. This provides:
- Better performance for voxel-based worlds
- Complete control over rendering pipeline
- Simpler dependency management
- Tailored optimizations for block-based gameplay

See **[Docs/RENDERING_ARCHITECTURE.md](Docs/RENDERING_ARCHITECTURE.md)** for detailed technical explanation.

## Inspired By
This project is inspired by **Vintage Story** by Anego Studios, a remarkable survival sandbox game that emphasizes geological realism, complex crafting systems, and immersive gameplay.

## License
This is an educational project created for learning game development concepts.

## Development Notes
- World seed: 12345 (configurable in TimelessTalesGame.cs)
- Render distance: 8 chunks
- Chunk size: 16x256x16 blocks
- Player height: 1.8 blocks
- Reach distance: 5 blocks

