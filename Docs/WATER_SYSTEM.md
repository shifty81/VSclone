# Water System Implementation

## Overview
This document describes the implementation of translucent water rendering, underwater effects, realistic buoyancy physics, and underwater vegetation in the Timeless Tales game.

## I. Visual Rendering

### WaterRenderer Class
The `WaterRenderer` is a specialized renderer that handles all water block rendering separately from solid blocks.

#### Key Features:
1. **Transparency**: Uses `BlendState.AlphaBlend` for proper alpha blending
2. **Depth-based Color**: Water appears lighter in shallow areas and darker in deep areas
3. **Wave Animation**: Animated surface using dual sine waves
4. **Render Order**: Water is rendered after opaque blocks to ensure correct transparency
5. **Seamless Edges**: Improved face overlap (0.01f) eliminates visible seams between water blocks

#### Technical Details:
- Water blocks are skipped in `WorldRenderer` and handled exclusively by `WaterRenderer`
- Color calculation: `brightness = 1.0 - (depthFactor * 0.5)` where depth factor is clamped to 0-1
- Wave offset calculated using: `sin(x * 0.3 + time) * 0.05 + sin(z * 0.4 + time * 1.2) * 0.05`
- Two water types supported:
  - **Water** (fresh water): RGB(30, 100, 200) - lighter blue/green
  - **Saltwater** (ocean): RGB(20, 60, 160) - deeper blue
- **Face Overlap**: All water faces extended by 0.01f to eliminate gaps and create seamless water body

### UnderwaterEffectRenderer Class
New renderer that applies immersive underwater visual effects when the player is submerged.

#### Key Features:
1. **Blue-Green Tint**: Applies atmospheric tinting RGB(20, 80, 120) when underwater
2. **Depth-Based Intensity**: Tint opacity increases with submersion depth
3. **Smooth Transitions**: Gradual effect changes as player enters/exits water
4. **Performance Optimized**: Fullscreen quad overlay with minimal GPU overhead

#### Effect Intensity:
- **Shallow Water** (0-30% submerged): Very light tint (0-15% opacity)
- **Medium Depth** (30-70% submerged): Increasing tint (15-40% opacity)
- **Deep Water** (70-100% submerged): Full tint (40% opacity)

### Rendering Pipeline:
```
1. Clear screen with sky color
2. Draw skybox
3. Draw opaque world blocks (WorldRenderer)
4. Draw translucent water (WaterRenderer)
5. Draw player arms/body
6. Draw underwater effects overlay (UnderwaterEffectRenderer) ← New
7. Draw 2D UI overlay
```

## II. Physics Simulation

### Buoyancy System
The buoyancy system creates realistic water physics that keep the player floating at the surface.

#### Constants:
- `BUOYANT_FORCE = 15.0f` - Upward force applied when underwater
- `WATER_GRAVITY_MULTIPLIER = 0.3f` - Gravity is reduced to 30% in water
- `WATER_DRAG = 0.9f` - Velocity is multiplied by 0.9 each frame in water
- `BUOYANCY_THRESHOLD = 0.5f` - Minimum submersion depth to apply buoyancy

#### Water Detection:
The player samples 4 points vertically (feet, waist, chest, head) to determine submersion:
- `_submersionDepth = waterSamples / totalSamples`
- Range: 0.0 (not in water) to 1.0 (fully submerged)

#### Physics Forces:
When in water, the following forces apply:

1. **Reduced Gravity**:
   ```csharp
   gravityMultiplier = _isInWater ? 0.3f : 1.0f;
   velocity.Y -= GRAVITY * gravityMultiplier * deltaTime;
   ```

2. **Buoyancy Force** (when submersion > 50%):
   ```csharp
   buoyancyForce = BUOYANT_FORCE * submersionDepth * deltaTime;
   velocity.Y += buoyancyForce;
   ```

3. **Water Drag**:
   ```csharp
   velocity = velocity * WATER_DRAG;
   ```

#### Movement in Water:
- **Speed Reduction**: `speed *= (0.5 + 0.5 * (1.0 - submersionDepth))`
  - Fully submerged: 50% speed
  - Half submerged: 75% speed
  - Not submerged: 100% speed

## III. Player Controls in Water

### Swimming Controls:
- **Space**: Swim upward (sets vertical velocity to 3.0)
- **Left Control**: Dive down (sets vertical velocity to -3.0)
- **WASD**: Move horizontally at reduced speed
- **Shift**: Sprint (further reduced in water)

### Behavior:
- **Idle in water**: Player naturally floats at surface (treading water animation)
- **Moving in water**: Swimming animation plays with arm strokes and leg kicks
- **Entering water**: Gravity and movement smoothly transition to water physics
- **Exiting water**: Normal gravity and movement resume

## IV. Animations

### Animation Types:
1. **TreadingWater** (idle in water):
   - Circular arm movements to stay afloat
   - Gentle alternating leg kicks
   - Subtle vertical bobbing motion
   - Animation speed: 1.5 cycles/second

2. **Swimming** (moving in water):
   - Alternating arm strokes (like freestyle swimming)
   - Flutter kick leg movements
   - Forward body tilt (~0.2 radians)
   - Animation speed: 2.5 cycles/second

## V. Testing

### Test Coverage:
The `WaterPhysicsTests` class includes 8 unit tests:

1. **Water Block Properties**: Verifies water blocks are transparent and non-solid
2. **Gravity Reduction**: Tests that gravity is 30% in water vs 100% in air
3. **Buoyancy Force**: Verifies buoyancy creates net upward force when fully submerged
4. **Water Drag**: Tests velocity reduction from drag
5. **Speed Reduction**: Validates speed formula at different submersion depths
6. **Depth-based Color**: Tests color darkening calculation
7. **Wave Animation**: Verifies wave offset changes over time
8. **Collision Tests**: Updated to handle water buoyancy scenarios

All 59 tests pass successfully (including vegetation tests).

## VI. Underwater Vegetation

### Vegetation Types
Four new underwater plant types have been added to enhance the underwater environment:

1. **Kelp**: Tall swaying seaweed found in deep water (10+ blocks deep)
   - Color: RGB(40, 100, 60) - Dark green
   - Spawn chance: 3% in deep water
   - Typical height: Multiple blocks tall

2. **Seaweed**: Medium-length aquatic plants found in medium depth water (3-10 blocks deep)
   - Color: RGB(60, 120, 80) - Medium green
   - Spawn chance: 12% in medium depth water
   - Typical height: 1-2 blocks

3. **Coral**: Colorful coral formations in deep water (10+ blocks deep)
   - Color: RGB(255, 100, 150) - Pink/red
   - Spawn chance: 2% in deep water
   - Provides visual variety on ocean floor

4. **Sea Grass**: Short underwater grass in shallow water (1-3 blocks deep)
   - Color: RGB(80, 140, 100) - Light green
   - Spawn chance: 8.4% in shallow water (70% of 12%)
   - Creates dense carpets in shallow areas

### Spawning System
Underwater vegetation is automatically placed during chunk generation:

```csharp
// Placement rules:
- Must be on solid block (sand, stone, etc.)
- Must have water block above
- Must be below sea level (Y < 64)
- Spawn chance varies by depth
```

### Depth-Based Distribution:
- **Deep Water (10+ blocks)**: Kelp and coral
- **Medium Depth (3-10 blocks)**: Seaweed and sea grass
- **Shallow Water (1-3 blocks)**: Primarily sea grass

### Integration with VegetationManager:
- Underwater plants tracked alongside land vegetation
- All underwater plants spawn at mature growth stage
- Compatible with existing growth system (though underwater plants don't currently grow)
- Can be harvested/removed like land plants

## VII. Performance Considerations

### Optimizations:
- Water meshes are built once per chunk and cached
- Only rebuild when chunk is modified (`NeedsMeshRebuild` flag)
- Face culling: Water faces between adjacent water blocks are not rendered
- Separate mesh for water allows independent updates from solid blocks
- Underwater effect overlay uses simple fullscreen quad
- Vegetation updates only occur near loaded chunks

### Rendering Cost:
- Water rendering adds minimal overhead due to:
  - Efficient mesh caching
  - Face culling between water blocks
  - Simple vertex shader (BasicEffect)
  - No expensive per-pixel effects
- Underwater effects add ~0.1ms per frame (negligible)
- Vegetation rendering integrated with existing plant system

## VIII. Future Enhancements

Potential improvements for future iterations:

1. **Advanced Visual Effects**:
   - Normal mapping for more detailed waves
   - Caustics on underwater surfaces
   - ~~Underwater fog/visibility reduction~~ ✅ Implemented
   - Refraction of objects viewed through water
   - Animated underwater plant swaying

2. **Physics Enhancements**:
   - Different buoyancy for different items
   - Water currents and flow simulation
   - Splash particles when entering/exiting water
   - Swimming stamina system

3. **Gameplay Features**:
   - Oxygen/breath meter for diving
   - Swimming skill progression
   - Underwater visibility based on water clarity
   - ~~Fish and underwater creatures~~ 🚧 Planned for future update
   - Harvestable underwater resources (kelp, coral)

4. **Underwater Vegetation Enhancements**:
   - Animated swaying motion for kelp and seaweed
   - Growth stages for underwater plants
   - Bioluminescent plants for deep water
   - Different coral colors and shapes

## IX. Technical Notes

### Block Registry:
Water blocks are marked as:
- `IsTransparent = true` - Allows seeing through them
- `IsSolid = false` - Allows player movement through them
- `Hardness = 0.0f` - Cannot be broken/mined

### World Generation:
Water is generated at sea level (Y=64):
- Blocks between surface height and sea level are filled with water
- Ocean biome uses Saltwater blocks
- Other biomes use regular Water blocks

### Coordinate System:
- **Y-axis**: Up (positive) / Down (negative)
- **Sea Level**: Y = 64
- **Submersion**: Measured from player's feet (Y) to head (Y + 1.8)
