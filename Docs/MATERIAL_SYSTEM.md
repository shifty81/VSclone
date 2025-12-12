# Material Pouch System

## Overview
The Material Pouch system implements a terraformable poly-based crafting system where breaking blocks drops material bits instead of whole blocks. These material bits are stored in a belt-mounted pouch with cloud-like inventory management, providing a more realistic and flexible crafting system.

## I. Concept

### Traditional Block Inventory vs Material Pouch
**Before (Traditional):**
- Breaking stone → Get 1 stone block
- Inventory stores discrete block items
- Limited by slot count (40 slots)

**After (Material Pouch):**
- Breaking stone → Get 10-12 stone bits (depending on hardness)
- Materials stored as continuous quantities (not discrete items)
- Cloud-like inventory: single pouch holds all material types
- Limited by total weight (10,000 units) instead of slot count

### Benefits
1. **More Realistic**: Blocks break into bits/chunks/fragments
2. **Flexible Crafting**: Use exact amounts needed (e.g., 15.5 wood fibers)
3. **Space Efficient**: One pouch holds all material types
4. **Scalable**: Can hold many different materials without slot management
5. **Terraformable**: Different blocks yield different amounts based on properties

## II. Material Types

### 25+ Material Types Organized by Category:

#### Basic Terrain Materials
- `StoneBits` - From stone blocks
- `DirtBits` - From dirt
- `SandBits` - From sand
- `GravelBits` - From gravel
- `ClayBits` - From clay

#### Wood Materials
- `WoodFibers` - From logs and wood blocks
- `BarkFragments` - From tree bark (future)

#### Rock Materials (Geological)
- `GraniteChunks` - From granite
- `LimestoneChunks` - From limestone
- `BasaltChunks` - From basalt
- `SandstoneChunks` - From sandstone
- `SlateChunks` - From slate

#### Ore Materials
- `CopperNuggets` - From copper ore
- `TinNuggets` - From tin ore
- `IronNuggets` - From iron ore
- `CoalFragments` - From coal

#### Organic Materials
- `PlantFibers` - From plants
- `Leaves` - From leaf blocks
- `GrassClippings` - From grass

## III. Material Pouch Implementation

### Class: `MaterialPouch`

#### Properties:
```csharp
private Dictionary<MaterialType, float> _materials  // Cloud-like storage
private const float MAX_CAPACITY = 10000f          // Total weight limit
private float _currentWeight                        // Current total weight
```

#### Key Features:
1. **Cloud-Like Storage**: All materials in one unified container
2. **Continuous Quantities**: Materials stored as floating-point values
3. **Weight-Based**: Total capacity is weight, not slots
4. **Auto-Stacking**: Same material types automatically combine

#### Methods:

**AddMaterial(MaterialType, float amount)**
- Adds material bits to the pouch
- Returns false if pouch is full
- Automatically stacks with existing materials

**RemoveMaterial(MaterialType, float amount)**
- Removes material for crafting
- Returns false if insufficient amount
- Auto-removes material type when depleted

**GetMaterialAmount(MaterialType)**
- Query how much of a material you have

**GetFillPercentage()**
- Returns 0-1 representing pouch fullness
- Used for UI indicators

## IV. Material Drop System

### MaterialDropTable

Maps block types to their material drops:

```csharp
BlockType.Stone → (StoneBits, 10.0 units base)
BlockType.CopperOre → (CopperNuggets, 15.0 units base)
BlockType.Wood → (WoodFibers, 10.0 units base)
```

### Drop Amount Calculation

```csharp
actualAmount = baseAmount * (1.0 + hardness * 0.2)
```

**Example:**
- Granite (hardness 2.0): 12 chunks * (1 + 2.0 * 0.2) = 16.8 chunks
- Leaves (hardness 0.2): 3 leaves * (1 + 0.2 * 0.2) = 3.12 leaves

Harder blocks give more materials, making mining effort worthwhile!

## V. Integration with Player

### Player Class Updates:

```csharp
public MaterialPouch MaterialPouch { get; private set; }
```

### Block Breaking Modified:

**Old Flow:**
1. Break block
2. Add 1 block to inventory

**New Flow:**
1. Break block
2. Get material drop from MaterialDropTable
3. Calculate drop amount based on hardness
4. Add material bits to MaterialPouch
5. Log collection (with pouch fill percentage)

### Example Log Output:
```
Collected 16.8 GraniteChunks (Pouch: 23% full)
Collected 3.1 Leaves (Pouch: 23% full)
Material pouch is full! Cannot collect StoneBits
```

## VI. Usage Examples

### Example 1: Mining Stone
```csharp
// Player breaks granite (hardness 2.0)
// Base drop: 12 GraniteChunks
// Actual drop: 12 * (1 + 2.0 * 0.2) = 16.8 chunks

pouch.AddMaterial(MaterialType.GraniteChunks, 16.8f);
// Pouch now contains 16.8 GraniteChunks
```

### Example 2: Collecting Wood
```csharp
// Player breaks oak log (hardness 1.2)
// Base drop: 12 WoodFibers
// Actual drop: 12 * (1 + 1.2 * 0.2) = 14.88 fibers

pouch.AddMaterial(MaterialType.WoodFibers, 14.88f);
```

### Example 3: Crafting (Future)
```csharp
// Craft a plank requires 5.0 WoodFibers
if (pouch.GetMaterialAmount(MaterialType.WoodFibers) >= 5.0f)
{
    pouch.RemoveMaterial(MaterialType.WoodFibers, 5.0f);
    // Create plank
}
```

## VII. Technical Details

### Storage Format
- Materials stored as `Dictionary<MaterialType, float>`
- Efficient lookup: O(1) for any material type
- Memory efficient: Only stores materials you have

### Capacity Management
- Total capacity: 10,000 weight units
- Current weight tracked automatically
- Prevents overflow: `AddMaterial()` returns false if full
- Fill percentage: `currentWeight / maxCapacity`

### Material Weights
All materials have unit weight = 1.0:
- 10 StoneBits = 10 weight units
- 15.5 WoodFibers = 15.5 weight units

Future enhancement: Different material densities

## VIII. Future Enhancements

### Planned Features:
1. **Material Densities**: Different materials have different weights
   - Stone bits: 1.5 weight per unit (denser)
   - Wood fibers: 0.5 weight per unit (lighter)
   
2. **Pouch Upgrades**:
   - Small pouch: 5,000 capacity (starter)
   - Medium pouch: 10,000 capacity (current)
   - Large pouch: 20,000 capacity (craftable)

3. **Material Quality**:
   - Poor/Normal/High quality materials
   - Better quality from better tools
   - Quality affects crafting outcomes

4. **Visual Particles**:
   - Material bits fly toward player when collected
   - Different colors for different material types
   - Satisfying collection feedback

5. **UI Improvements**:
   - Material pouch interface (press B?)
   - Visual indicators on belt
   - Fill meter on HUD
   - Material tooltips

6. **Crafting Integration**:
   - Recipes use material bits instead of whole blocks
   - More flexible crafting ratios
   - Partial crafting (make 2.5 planks)

## IX. Testing

### Test Coverage (13 tests, all passing):

1. **Initialization**: Pouch starts empty
2. **Adding Materials**: Increases weight correctly
3. **Multiple Materials**: Tracks each type independently
4. **Stacking**: Same materials combine
5. **Removing Materials**: Decreases weight
6. **Insufficient Removal**: Returns false
7. **Capacity Limit**: Rejects when full
8. **Fill Percentage**: Calculates correctly
9. **Drop Table**: Returns correct materials
10. **Ore Drops**: Different ores drop different materials
11. **Hardness Scaling**: Drop amount increases with hardness
12. **No Drop for Air**: Returns null appropriately
13. **Get All Materials**: Returns complete inventory

## X. Block → Material Mapping

### Complete Drop Table:

| Block Type | Material Type | Base Amount |
|-----------|--------------|-------------|
| Stone | StoneBits | 10.0 |
| Dirt | DirtBits | 8.0 |
| Grass | GrassClippings | 6.0 |
| Sand | SandBits | 8.0 |
| Gravel | GravelBits | 8.0 |
| Clay | ClayBits | 10.0 |
| Granite | GraniteChunks | 12.0 |
| Limestone | LimestoneChunks | 10.0 |
| Basalt | BasaltChunks | 12.0 |
| Sandstone | SandstoneChunks | 8.0 |
| Slate | SlateChunks | 11.0 |
| Copper Ore | CopperNuggets | 15.0 |
| Tin Ore | TinNuggets | 15.0 |
| Iron Ore | IronNuggets | 18.0 |
| Coal | CoalFragments | 12.0 |
| Wood/Logs | WoodFibers | 10-12.0 |
| Leaves | Leaves | 3.0 |
| Planks | WoodFibers | 8.0 |
| Cobblestone | StoneBits | 9.0 |

*Actual amounts vary based on block hardness*

## XI. Code Locations

- Material system: `TimelessTales/Entities/MaterialPouch.cs`
- Player integration: `TimelessTales/Entities/Player.cs`
  - Line 58: MaterialPouch property
  - Line 72: Pouch initialization
  - Lines 460-479: Modified block breaking
- Tests: `TimelessTales.Tests/MaterialPouchTests.cs`

## XII. Performance Considerations

### Optimizations:
- Dictionary lookup: O(1) for material queries
- No per-frame operations
- Lazy initialization: Materials only added when collected
- Efficient memory: Only stores collected materials

### Memory Usage:
- Empty pouch: ~100 bytes (dictionary overhead)
- With 25 material types: ~800 bytes
- Negligible compared to chunk meshes

## XIII. Design Philosophy

The material pouch system transforms block-breaking from a simple item collection into a more nuanced resource gathering experience:

1. **Granular Resources**: Blocks become sources of raw materials
2. **Meaningful Differences**: Different blocks yield different materials and amounts
3. **Progressive Gathering**: Harder blocks reward more materials
4. **Unified Storage**: One cloud-like container for all crafting materials
5. **Crafting Flexibility**: Use exact amounts needed, no waste

This creates a more engaging and realistic survival experience aligned with the Vintage Story inspiration.
