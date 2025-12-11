# Developer Documentation - Timeless Tales

## Overview
Timeless Tales is a voxel-based survival sandbox game inspired by Vintage Story, built with C# and MonoGame. This document provides technical details for developers working on the project.

## Architecture

### Core Systems

#### 1. World System
The world is divided into chunks for efficient rendering and memory management.

**Chunk Structure:**
- Size: 16x256x16 blocks
- Y-axis: 0-255 (256 blocks high)
- Chunks load/unload based on player position
- Render distance: 8 chunks (configurable)

**WorldManager** (`World/WorldManager.cs`)
- Manages chunk loading/unloading
- Provides world-space to chunk-space coordinate conversion
- Handles block get/set operations across chunks

**WorldGenerator** (`World/WorldGenerator.cs`)
- Uses SimplexNoise for procedural generation
- Implements realistic geological layers:
  - Upper crust (Y: 46-64): Sedimentary rocks (Limestone, Sandstone)
  - Mid crust (Y: 26-45): Metamorphic rocks (Slate, Stone)
  - Lower crust (Y: 11-25): Igneous rocks (Granite, Basalt)
  - Deep layer (Y: 0-10): Hard igneous (Basalt)
- Ore distribution tied to specific rock types:
  - Copper: Limestone layers (Y: 30-60)
  - Tin: Granite layers (Y: 15-35)
  - Iron: Basalt layers (Y: 5-30)
  - Coal: Sandstone layers (Y: 40-70)

#### 2. Block System

**BlockRegistry** (`Blocks/BlockRegistry.cs`)
- Central registry for all block types
- Block properties:
  - `Hardness`: Mining time multiplier
  - `IsTransparent`: Affects face culling
  - `AffectedByGravity`: Sand/gravel physics
  - `Color`: Temporary color until textures added
  - `IsOre`: Marks ore blocks

**Adding New Blocks:**
```csharp
// In BlockType enum:
public enum BlockType
{
    // ...existing types...
    NewBlock
}

// In BlockRegistry.RegisterDefaultBlocks():
Register(new BlockDefinition(
    BlockType.NewBlock, 
    "New Block Name", 
    1.5f,                    // Hardness
    Color.Green,             // Color
    false,                   // IsTransparent
    false,                   // AffectedByGravity
    false                    // IsOre
));
```

#### 3. Rendering System

**WorldRenderer** (`Rendering/WorldRenderer.cs`)
- Builds optimized meshes for each chunk
- Implements face culling (hidden faces not rendered)
- Uses greedy meshing for performance
- Directional lighting:
  - Top faces: +20% brightness
  - Bottom faces: -30% brightness
  - Side faces: -10% brightness

**Camera** (`Rendering/Camera.cs`)
- First-person perspective camera
- View matrix updates based on player rotation
- Projection: 45° FOV, 0.1-500 units view distance

#### 4. Player System

**Player** (`Entities/Player.cs`)
- Movement: WASD keys, 4.5 blocks/second
- Sprint: 1.5x speed multiplier
- Jump: 8.0 units/second initial velocity
- Gravity: 20.0 units/second²
- Collision: Simple AABB (Axis-Aligned Bounding Box)

**Block Interaction:**
- Raycasting: Steps through 0.1-unit increments
- Reach distance: 5 blocks
- Break time: 1.0 seconds (modified by block hardness)
- Place validation: Checks player collision

**Inventory:**
- 40 slots total
- Dictionary-based storage: `Dictionary<BlockType, int>`
- Stack-based item management

#### 5. Input System

**InputManager** (`Core/InputManager.cs`)
- Tracks current and previous frame input states
- Provides methods:
  - `IsKeyDown(Keys key)`: Continuous press
  - `IsKeyPressed(Keys key)`: Single frame press
  - `IsLeftMouseDown()`: Continuous click
  - `IsLeftMousePressed()`: Single frame click

## Performance Considerations

### Chunk Meshing
- Chunks only rebuild when modified (`NeedsMeshRebuild` flag)
- Face culling eliminates hidden geometry
- Vertex data stored in arrays for fast GPU upload

### Memory Management
- Chunks unload when beyond render distance + 2 chunks
- Vertex buffers reused between frames
- Dictionary-based chunk storage for O(1) lookup

### Optimization Opportunities
1. **Greedy Meshing**: Merge adjacent same-type faces
2. **Vertex Buffer Objects**: Use GPU-side buffers
3. **Frustum Culling**: Don't render chunks outside camera view
4. **Level of Detail**: Reduce mesh detail for distant chunks
5. **Multithreading**: Generate chunks on background threads

## Code Style

### Naming Conventions
- Classes: PascalCase (`WorldManager`)
- Methods: PascalCase (`GetBlock`)
- Private fields: _camelCase (`_chunks`)
- Constants: UPPER_SNAKE_CASE (`CHUNK_SIZE`)
- Parameters: camelCase (`worldX`)

### Organization
- One class per file
- Namespace matches folder structure
- Group related functionality in folders

## Testing the Game

### Running in Debug Mode
```bash
cd TimelessTales
dotnet run
```

### Running in Release Mode
```bash
cd TimelessTales
dotnet run -c Release
```

### Building Standalone Executable
```bash
cd TimelessTales
dotnet publish -c Release -r win-x64 --self-contained
```
Output: `bin/Release/net8.0/win-x64/publish/`

## Debugging Tips

### Common Issues

**Issue: Game doesn't start / Black screen**
- Check console output for exceptions
- Verify MonoGame dependencies are installed
- Ensure graphics drivers support DirectX 11

**Issue: Low FPS**
- Reduce render distance in `WorldManager.cs`
- Disable face culling temporarily to check if mesh building is slow
- Profile with Visual Studio Performance Profiler

**Issue: Blocks don't break/place**
- Check raycast logic in `Player.HandleBlockInteraction()`
- Verify inventory has items for placing
- Check collision detection isn't blocking placement

### Debug Shortcuts
Add these to `TimelessTalesGame.Update()`:
```csharp
// Toggle wireframe
if (input.IsKeyPressed(Keys.F1))
    _graphicsDevice.RasterizerState = RasterizerState.CullNone;

// Print player position
if (input.IsKeyPressed(Keys.F2))
    Console.WriteLine($"Player pos: {_player.Position}");
```

## Extending the Game

### Adding New Game Mechanics

1. **Temperature System**
   - Create `TemperatureManager.cs` in `World/`
   - Track player temperature in `Player.cs`
   - Update based on biome and time

2. **Crafting System**
   - Create `Crafting/` folder
   - Implement `Recipe.cs` and `CraftingManager.cs`
   - Add crafting UI in `UI/CraftingUI.cs`

3. **Mob System**
   - Create `Entities/Mob.cs` base class
   - Implement AI in `AI/` folder
   - Add to world update loop

### Adding Textures

1. Create `Content/Textures/` folder
2. Add texture files (.png)
3. Create `Content.mgcb` file:
```
#----------------------------- Global Properties ----------------------------#
/outputDir:bin/$(Platform)
/intermediateDir:obj/$(Platform)
/platform:DesktopGL
/config:
/profile:Reach
/compress:False

#---------------------------------- Content ---------------------------------#
#begin Textures/stone.png
/importer:TextureImporter
/processor:TextureProcessor
/build:Textures/stone.png
```
4. Load in `LoadContent()`:
```csharp
Texture2D stoneTexture = Content.Load<Texture2D>("Textures/stone");
```
5. Update `WorldRenderer` to use textures instead of colors

## Contributing

When adding new features:
1. Follow existing code style
2. Add XML documentation comments
3. Test thoroughly
4. Update README.md with new features
5. Update this documentation if architecture changes

## Resources

- [MonoGame Documentation](https://docs.monogame.net/)
- [Vintage Story Wiki](https://wiki.vintagestory.at/)
- [Voxel Engine Design](https://0fps.net/2012/06/30/meshing-in-a-minecraft-game/)
- [Perlin/Simplex Noise](https://adrianb.io/2014/08/09/perlinnoise.html)
