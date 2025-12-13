# Game Design Document: Timeless Tales

## Executive Summary
**Title**: Timeless Tales  
**Genre**: Open-world Sandbox Survival & Creative  
**Platform**: PC (Windows)  
**Engine**: Custom C# Engine (MonoGame 3.8.4 with custom 3D rendering, no GeonBit)  
**Target Audience**: Fans of complex survival simulations, creative builders, geology enthusiasts  
**Monetization**: Single purchase, no microtransactions

> **Note**: This project uses a custom voxel rendering architecture built directly on MonoGame, without third-party 3D engines like GeonBit. See [RENDERING_ARCHITECTURE.md](RENDERING_ARCHITECTURE.md) for technical details.  

## 1. Core Gameplay Loop

### Moment-to-Moment Gameplay
Players spawn in a procedurally generated world with nothing but their hands. The immediate goals are:
1. **Gather Resources** - Break blocks (stone, wood, plants) with left-click
2. **Craft Tools** - Use collected materials to create basic tools (knapping flint)
3. **Build Shelter** - Place blocks with right-click to construct protective structures
4. **Survive** - Manage hunger, thirst, temperature, and health
5. **Progress** - Advance through technological ages (Stone → Copper → Bronze → Iron)

### Long-term Progression
- Explore diverse biomes and geological formations
- Master complex crafting chains (pottery, metallurgy, agriculture)
- Build elaborate bases and automated systems
- Uncover environmental lore and mysteries

## 2. Key Mechanics

### 2.1 World Generation
**Realistic Geology System**:
- **Rock Layers**: Multiple strata (igneous, sedimentary, metamorphic)
  - Granite, Limestone, Slate, Sandstone, Basalt
  - Each layer contains specific ore deposits
- **Soil Types**: Topsoil composition affects farming
  - Clay-rich, Sandy, Loamy, Peaty
- **Climate Zones**: Temperature and rainfall gradients
  - Tropical, Temperate, Boreal, Tundra, Desert
- **Elevation**: Mountains, valleys, plains, caves
- **Water Systems**: Rivers, lakes, underground aquifers

**Technical Implementation**:
- Chunk-based infinite world (16x256x16 blocks per chunk)
- Multi-octave Perlin/Simplex noise for terrain
- Seed-based deterministic generation

### 2.2 Survival Elements
**Hunger System**:
- Depletes over time, affects health regeneration
- Food sources: Foraged berries, hunted meat, farmed crops
- Nutrition types: Protein, Grain, Vegetable, Fruit, Dairy

**Thirst System**:
- Faster depletion than hunger
- Water sources: Rivers, lakes, wells, rain collection
- Water quality affects health

**Temperature Management**:
- Body temperature affected by:
  - Ambient climate and season
  - Clothing (furs, leather, linen)
  - Proximity to heat sources (fires, forges)
- Hypothermia/Hyperthermia status effects

**Seasonal Changes**:
- Four seasons with different day lengths
- Crop growth tied to seasons
- Animal migration patterns
- Temperature fluctuations

### 2.3 Crafting System
**Multi-Step Crafting Philosophy**:
Crafting is intentionally complex, requiring planning and multiple intermediate steps.

**Example: Creating an Axe**
1. **Knapping**: Break flint by right-clicking with a rock
   - Creates random flake patterns
   - Must achieve axe head shape
2. **Handle Crafting**: Strip bark from sticks
3. **Assembly**: Combine axe head + handle + plant fiber binding
4. **Tool Stats**: Durability varies based on materials

**Progression Tiers**:
- **Stone Age**: Flint knapping, basic wood tools
- **Copper Age**: Native copper collection, cold hammering
- **Bronze Age**: Ore smelting in bloomery, alloying
- **Iron Age**: Advanced metallurgy, steel creation

**Specialized Systems**:
- **Pottery**: Clay forming, drying, kiln firing
- **Metalworking**: Ore crushing, smelting, casting, forging
- **Textiles**: Fiber processing, spinning, weaving, sewing

### 2.4 Building System
**Block-Based Construction**:
- Fully destructible/placeable environment
- **Left-Click**: Break blocks (adds to inventory)
- **Right-Click**: Place blocks from inventory
- Block properties:
  - Hardness (mining speed)
  - Blast resistance
  - Flammability
  - Support requirements (some blocks need support)

**Structural Mechanics**:
- Gravity affects unsupported blocks (sand, gravel)
- Load-bearing calculations for large structures
- Different block types: Full blocks, slabs, stairs, fences

**Building Materials**:
- Natural: Stone, Wood, Clay, Adobe
- Processed: Bricks, Planks, Glass, Metal sheets
- Decorative: Colored blocks, patterns, furniture

### 2.5 Flora & Fauna
**Animals**:
- **Passive**: Deer, Rabbits, Chickens (food sources)
- **Neutral**: Wolves, Bears (territorial)
- **Hostile**: None initially (environmental challenges only)
- AI behaviors: Grazing, fleeing, hunting, breeding

**Plants**:
- **Wild**: Berries, Mushrooms, Herbs (foraging)
- **Farmable**: Wheat, Carrots, Flax (agriculture)
- **Trees**: Oak, Pine, Birch (renewable wood)
- Seasonal growth cycles

### 2.6 Technology Progression
**Age System**:
Players advance by discovering new materials and crafting techniques.

| Age | Key Technologies | Example Items |
|-----|------------------|---------------|
| Stone | Knapping, basic tools | Flint knife, wooden spear |
| Copper | Native copper, cold working | Copper axe, simple jewelry |
| Bronze | Smelting, alloying | Bronze sword, metal armor |
| Iron | Blast furnace, steel | Iron tools, advanced machinery |

## 3. Atmosphere & Lore

### Mood
- **Isolation**: Sparse signs of previous civilization
- **Ancient Mystery**: Ruins hint at a lost world
- **Geological Time**: Emphasis on Earth's deep history
- **Peaceful Solitude**: No forced combat, nature is the challenge

### Environmental Storytelling
- Abandoned structures with hints of former inhabitants
- Strange geological anomalies
- Ancient cave paintings
- Mysterious artifacts buried in rock layers

### Audio Design
- Ambient nature sounds (wind, water, wildlife)
- Subtle, melancholic musical themes
- Satisfying crafting sound effects (knapping, hammering)

## 4. Target Audience

**Primary Audience**:
- Ages 18-40
- Fans of: Minecraft, Terraria, Don't Starve, Wurm Online
- Players who enjoy:
  - Complex systems mastery
  - Creative building
  - Self-directed gameplay
  - Educational elements (geology, history)

**Appeal Factors**:
- Depth over immediate gratification
- Realistic simulation elements
- No hand-holding tutorials
- Modding support

## 5. Technical Specifications

**Minimum Requirements**:
- OS: Windows 10/11 (64-bit)
- Processor: Dual-core 2.5 GHz
- Memory: 4 GB RAM
- Graphics: DirectX 11 compatible GPU
- Storage: 2 GB available space

**Recommended Requirements**:
- OS: Windows 11 (64-bit)
- Processor: Quad-core 3.0 GHz
- Memory: 8 GB RAM
- Graphics: DirectX 11 GPU with 2GB VRAM
- Storage: 2 GB available space (SSD)

## 6. Monetization Strategy

- **Model**: One-time purchase ($24.99 USD)
- **No microtransactions**
- **Free content updates** for first year
- **Potential DLC**: Major expansions (new ages, mechanics)

## 7. Development Phases

### Phase 1 (Current): Core Systems
- World generation
- Player character and movement
- Block interaction (break/place)
- Basic rendering

### Phase 2: Survival Loop
- Hunger/thirst systems
- Temperature mechanics
- Day/night cycle
- Basic crafting

### Phase 3: Progression
- Full crafting tree
- Technology ages
- Flora and fauna

### Phase 4: Polish
- Sound design
- UI/UX refinement
- Performance optimization
- Modding API

## 8. Success Metrics

**Launch Goals**:
- 10,000 units sold in first month
- 85% positive Steam reviews
- Average playtime: 50+ hours

**Community Goals**:
- Active modding community
- Player-created content sharing
- Regular community events

---

*Document Version 1.0 - Created for Timeless Tales Development*
