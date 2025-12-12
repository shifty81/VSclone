# Vegetation Growth System

## Overview
The vegetation system provides realistic plant growth with three distinct growth stages, allowing for dynamic vegetation that evolves over time in the Timeless Tales world.

## Architecture

### Core Components

#### 1. VegetationTypes Enum
**Location**: `TimelessTales/Vegetation/VegetationTypes.cs`

**Growth Stages**:
- `Seedling` (Stage 0): Just planted or newly sprouted
- `Growing` (Stage 1): Actively growing phase
- `Mature` (Stage 2): Fully grown and harvestable

**Vegetation Types**:
- `Grass`: Short grass patches
- `TallGrass`: Taller grass variants
- `Shrub`: Generic shrubs
- `BerryShrub`: Berry-producing bushes
- `Flowers`: Decorative flowering plants
- `Wheat`: Farmable grain crop
- `Carrot`: Root vegetable crop
- `Flax`: Fiber crop for textiles

#### 2. Plant Class
**Location**: `TimelessTales/Vegetation/Plant.cs`

Represents an individual plant instance with:
- **Position**: 3D world coordinates
- **Type**: One of the VegetationType enum values
- **Stage**: Current growth stage
- **GrowthProgress**: 0.0 to 1.0 progress within current stage
- **TimeToNextStage**: Countdown timer in seconds

#### 3. VegetationManager Class
**Location**: `TimelessTales/Vegetation/VegetationManager.cs`

Manages all vegetation in the world:
- Plant placement during world generation
- Growth updates each frame
- Plant lifecycle management
- Query interface for rendering

## Growth Mechanics

### Growth Timings

Default growth times (configurable per plant type in future):

```csharp
Seedling → Growing: 5 minutes (300 seconds)
Growing → Mature: 10 minutes (600 seconds)
Total growth time: 15 minutes
```

### Growth Progression

Plants progress through stages automatically:

1. **Seedling Stage**:
   - Size: 30-50% of full size
   - Color: Light green tint
   - Not harvestable
   - Duration: 5 minutes

2. **Growing Stage**:
   - Size: 50-80% of full size
   - Color: Medium green
   - Not harvestable (or minimal yield)
   - Duration: 10 minutes

3. **Mature Stage**:
   - Size: 80-100% of full size
   - Color: Full green
   - Fully harvestable
   - Duration: Indefinite (stays mature)

### Visual Representation

Plants change visually as they grow:

```csharp
// Size multiplier based on stage and progress
float size = plant.GetSizeMultiplier();
// Seedling: 0.3 to 0.5
// Growing:  0.5 to 0.8
// Mature:   0.8 to 1.0

// Color tint based on stage
Color color = plant.GetColorTint();
// Seedling: Light green → Medium green
// Growing:  Medium green → Full green
// Mature:   Full green
```

## World Generation Integration

### Automatic Vegetation Placement

When a chunk is generated, the `VegetationManager` populates it with plants:

```csharp
public void PopulateChunk(Chunk chunk)
{
    // For each block position in the chunk:
    // 1. Find the top solid block
    // 2. Check if it's suitable for vegetation (grass block)
    // 3. Randomly place vegetation based on spawn chances
}
```

**Spawn Probabilities**:
- Grass: 15% chance per suitable block
- Shrubs: 5% chance per suitable block

### Placement Rules

Vegetation is placed when:
- Ground block is `BlockType.Grass`
- Block above ground is `BlockType.Air`
- No existing vegetation at that position
- Random chance succeeds

## Usage

### Initialization

```csharp
// In WorldManager or main game class
VegetationManager vegetationManager = new VegetationManager(worldManager);

// After chunk generation:
vegetationManager.PopulateChunk(newChunk);
```

### Update Loop

```csharp
// In game Update():
float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
vegetationManager.Update(deltaTime);
```

### Manual Plant Placement

```csharp
// Plant a seed at a specific location
Vector3 position = new Vector3(10, 64, 10);
vegetationManager.PlacePlant(position, VegetationType.Wheat, GrowthStage.Seedling);

// Plant a mature shrub (for debugging/testing)
vegetationManager.PlacePlant(position, VegetationType.Shrub, GrowthStage.Mature);
```

### Querying Plants

```csharp
// Get a specific plant
Plant? plant = vegetationManager.GetPlant(new Vector3(10, 64, 10));

if (plant != null)
{
    Console.WriteLine($"Plant: {plant.Type}, Stage: {plant.Stage}");
}

// Get all plants for rendering
foreach (Plant plant in vegetationManager.GetAllPlants())
{
    RenderPlant(plant);
}
```

### Harvesting/Removing

```csharp
// When player breaks a plant block
Vector3 plantPosition = new Vector3(x, y, z);
bool removed = vegetationManager.RemovePlant(plantPosition);

if (removed)
{
    // Add items to player inventory based on plant type and stage
    GiveHarvestRewards(plant);
}
```

## Rendering Integration

### Rendering Plants

Plants should be rendered as small voxel models or billboards:

```csharp
public void RenderVegetation(VegetationManager vegManager, Camera camera)
{
    foreach (Plant plant in vegManager.GetAllPlants())
    {
        // Get visual properties
        float size = plant.GetSizeMultiplier();
        Color tint = plant.GetColorTint();
        
        // Render based on plant type
        switch (plant.Type)
        {
            case VegetationType.Grass:
                RenderGrass(plant.Position, size, tint);
                break;
            case VegetationType.Shrub:
                RenderShrub(plant.Position, size, tint);
                break;
            // ... other types
        }
    }
}
```

### Optimization

For large numbers of plants:

1. **Frustum Culling**: Only render plants in view
2. **LOD (Level of Detail)**: Simpler models for distant plants
3. **Instanced Rendering**: Batch render identical plants
4. **Chunk-based Culling**: Only process plants in loaded chunks

## Future Enhancements

### Planned Features

#### 1. Environmental Factors
- **Seasons**: Growth rate varies by season
  - Spring: 150% growth rate
  - Summer: 100% growth rate
  - Autumn: 75% growth rate
  - Winter: 25% growth rate (or dormant)

- **Light Level**: Plants need sufficient light
  - Full sunlight: Normal growth
  - Partial shade: Slower growth
  - Dark areas: No growth or death

- **Water**: Proximity to water affects growth
  - Well-watered: Faster growth
  - Dry conditions: Slower growth or wilting

#### 2. Plant-Specific Behaviors

```csharp
// BerryShrub produces berries when mature
if (plant.Type == VegetationType.BerryShrub && plant.Stage == GrowthStage.Mature)
{
    // Every 5 minutes, berries can be harvested
    // After harvest, timer resets
}

// Wheat can be harvested once, then dies
if (plant.Type == VegetationType.Wheat && harvested)
{
    vegetationManager.RemovePlant(plant.Position);
}

// Grass spreads to nearby blocks
if (plant.Type == VegetationType.Grass && plant.Stage == GrowthStage.Mature)
{
    TrySpreadGrass(plant.Position);
}
```

#### 3. Advanced Growth System
- **Nutrient Depletion**: Soil quality decreases with farming
- **Crop Rotation**: Different crops restore different nutrients
- **Fertilizer**: Speed up growth or improve yield
- **Diseases**: Plants can become diseased and spread to neighbors
- **Pests**: Insects can damage plants

#### 4. Biome-Specific Vegetation
- **Tundra**: Hardy grass, low shrubs
- **Boreal**: Pine cones, moss, ferns
- **Temperate**: Varied flowers, deciduous plants
- **Desert**: Cacti, dry shrubs
- **Tropical**: Lush plants, vines, exotic flowers

## Performance Considerations

### Update Optimization

```csharp
// Instead of updating all plants every frame:
// 1. Update plants in chunks near the player
// 2. Use update intervals for distant plants
// 3. Skip updates for plants far from player

private float _updateTimer = 0;
private const float UPDATE_INTERVAL = 0.1f; // 10 updates per second

public void Update(float deltaTime)
{
    _updateTimer += deltaTime;
    
    if (_updateTimer >= UPDATE_INTERVAL)
    {
        // Update plants in loaded chunks
        UpdateNearbyPlants(_updateTimer);
        _updateTimer = 0;
    }
}
```

### Memory Management
- Store plants in spatial data structure (e.g., grid or octree)
- Unload plant data for unloaded chunks
- Save/load plant state with world data

## Testing

### Unit Tests

```csharp
[Test]
public void TestPlantGrowth()
{
    var plant = new Plant(Vector3.Zero, VegetationType.Grass);
    
    Assert.AreEqual(GrowthStage.Seedling, plant.Stage);
    
    // Update for 5 minutes (seedling -> growing)
    bool changed = plant.Update(300f);
    
    Assert.IsTrue(changed);
    Assert.AreEqual(GrowthStage.Growing, plant.Stage);
    
    // Update for 10 minutes (growing -> mature)
    changed = plant.Update(600f);
    
    Assert.IsTrue(changed);
    Assert.AreEqual(GrowthStage.Mature, plant.Stage);
    
    // Mature plants don't advance further
    changed = plant.Update(1000f);
    Assert.IsFalse(changed);
}

[Test]
public void TestSizeMultiplier()
{
    var plant = new Plant(Vector3.Zero, VegetationType.Shrub);
    
    // Seedling should be small
    float size = plant.GetSizeMultiplier();
    Assert.IsTrue(size >= 0.3f && size <= 0.5f);
    
    // Force to mature
    plant.SetStage(GrowthStage.Mature);
    size = plant.GetSizeMultiplier();
    Assert.IsTrue(size >= 0.8f && size <= 1.0f);
}

[Test]
public void TestVegetationPlacement()
{
    var vegManager = new VegetationManager(worldManager);
    var position = new Vector3(10, 64, 10);
    
    vegManager.PlacePlant(position, VegetationType.Grass);
    
    Plant? plant = vegManager.GetPlant(position);
    Assert.IsNotNull(plant);
    Assert.AreEqual(VegetationType.Grass, plant.Type);
}
```

## Data Persistence

### Save Format (Future)

Plants need to be saved with world data:

```json
{
  "position": { "x": 10, "y": 64, "z": 10 },
  "type": "Grass",
  "stage": "Growing",
  "timeToNextStage": 245.3
}
```

### Load Strategy
- Load plants when chunk is loaded
- Resume growth timers from saved state
- Recalculate growth for time elapsed since last save

---

**Version**: 1.0  
**Last Updated**: 2025-12-12  
**Author**: Timeless Tales Development Team
