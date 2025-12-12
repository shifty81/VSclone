# Timeless Tales - Development Roadmap

This roadmap tracks the progress of all features and systems in the Timeless Tales game. It is updated with each pull request to reflect the current state of development.

**Last Updated:** 2025-12-12  
**Current Version:** Alpha 0.1

---

## Legend
- ✅ **Completed** - Feature fully implemented and tested
- 🚧 **In Progress** - Currently being developed
- 📋 **Planned** - Scheduled for future development
- 🔬 **Research** - Investigating technical approach
- ⏸️ **On Hold** - Temporarily paused

---

## 1. Water Rendering, Effects, and Swimming Animation

### 1.1 Real-Time Water Rendering
- ✅ Water mesh generation with transparency (alpha blending)
- ✅ Depth-based color variation (shallow vs deep water)
- ✅ Wave animation using sum of sines in vertex displacement
- ✅ Dual water types (freshwater and saltwater)
- ✅ Proper render order (after opaque blocks)
- ✅ Face culling optimization between water blocks
- 📋 Advanced shader effects:
  - 📋 Normal mapping for detailed wave normals
  - 📋 Caustics rendering on underwater surfaces
  - 📋 Refraction of objects viewed through water
  - 📋 Underwater fog/visibility reduction
  - 📋 Specular highlights and reflections

### 1.2 Swimming Effects and Animation
- ✅ Swimming animation system (treading water)
- ✅ Forward swimming animation with arm strokes
- ✅ Animation controller with state transitions
- ✅ Character skeleton system for voxel character
- 📋 Particle system for bubbles:
  - 📋 Bubble particle emitter
  - 📋 Upward floating physics for bubbles
  - 📋 Semi-transparent bubble rendering
  - 📋 Periodic emission from character's mouth underwater
  - 📋 Bubble pop animation when reaching surface

### 1.3 Character Water Interaction
- ✅ Buoyancy physics system (upward force when submerged)
- ✅ Water drag and resistance
- ✅ Reduced gravity in water (30% of normal)
- ✅ Speed reduction based on submersion depth
- ✅ Submersion depth calculation (0.0 to 1.0)
- ✅ Swimming controls (Space to swim up, Ctrl to dive)
- 📋 Advanced water physics:
  - 📋 Water currents and flow simulation
  - 📋 Different buoyancy for different items/blocks
  - 📋 Splash particles when entering/exiting water
  - 📋 Swimming stamina/energy system
  - 📋 Oxygen/breath meter for diving
  - 📋 Drowning mechanics
  - 📋 Swimming skill progression

---

## 2. Character Built from Voxels (Low Poly)

### 2.1 Voxel Character Model
- ✅ Low-poly voxel aesthetic maintained
- ✅ Character skeleton system with bones
- ✅ First-person arms visible
- ✅ Body visible when looking down
- 📋 Character customization:
  - 📋 Different voxel character models
  - 📋 Clothing/armor as voxel overlays
  - 📋 Character color/texture variations
- 📋 Support for importing voxel models:
  - 📋 MagicaVoxel (.vox) format support
  - 📋 Qubicle (.qb) format support
  - 📋 Custom voxel model loader

### 2.2 Animation System
- ✅ Walking/running animations
- ✅ Swimming animations (treading, forward swim)
- ✅ Idle animations
- ✅ Animation blending and transitions
- 📋 Additional animations:
  - 📋 Jumping/landing animations
  - 📋 Crouching animation
  - 📋 Tool use animations (mining, chopping)
  - 📋 Combat animations (if combat added)
  - 📋 Emote animations

---

## 3. World Generation to Terraformable Low Poly Terrain

### 3.1 Procedural World Generation
- ✅ Chunk-based infinite world (16x256x16 blocks)
- ✅ Simplex noise for terrain height generation
- ✅ Multi-octave noise for terrain detail
- ✅ Seed-based deterministic generation
- ✅ Realistic geological layers:
  - ✅ Sedimentary rocks (Limestone, Sandstone)
  - ✅ Metamorphic rocks (Slate)
  - ✅ Igneous rocks (Granite, Basalt)
- ✅ Multiple biomes (Tundra, Boreal, Temperate, Desert, Tropical)
- ✅ Cave system generation
- ✅ Ore distribution (Copper, Tin, Iron, Coal)
- ✅ Water bodies (lakes, oceans at sea level Y=64)
- 📋 Advanced world features:
  - 📋 River generation with flow
  - 📋 Underground aquifers
  - 📋 Volcanic formations
  - 📋 Glacier/ice cap generation
  - 📋 Beach and coastal transitions

### 3.2 Terraformable Terrain
- ✅ Real-time terrain modification (break/place blocks)
- ✅ Voxel data stored in 3D structure (chunks)
- ✅ Chunk mesh rebuilding on modification
- ✅ Neighboring chunk updates
- ✅ Player inventory system for collected blocks
- ✅ Block placement validation
- 📋 Advanced terraforming:
  - 📋 Multi-block tool operations (area clear, fill)
  - 📋 Structural integrity system
  - 📋 Erosion simulation
  - 📋 Landslide physics for unsupported blocks

### 3.3 Points of Interest and Geographical Oddities
- 📋 Procedural structure generation:
  - 📋 Ancient ruins
  - 📋 Abandoned settlements
  - 📋 Cave systems with unique features
  - 📋 Natural arches and formations
  - 📋 Meteor impact sites
  - 📋 Hot springs and geysers
  - 📋 Crystal caverns
- 📋 Lore placement system:
  - 📋 Environmental storytelling elements
  - 📋 Ancient cave paintings
  - 📋 Mysterious artifacts
  - 📋 Buried treasure

### 3.4 Shrubbery, Grass, and Vegetation
- ✅ Tree generation (TreeGenerator class)
- ✅ Basic foliage blocks (Leaves, Wood)
- 📋 Grass system:
  - 📋 Grass block variants (short, medium, tall)
  - 📋 Grass placement on suitable terrain
  - 📋 Wind animation for grass
  - 📋 Seasonal grass color changes
- 📋 Shrubbery system:
  - 📋 Bush/shrub voxel models
  - 📋 Three growth stages (seedling, growing, mature)
  - 📋 Growth progression over time
  - 📋 Different shrub types per biome
  - 📋 Berry-producing shrubs
  - 📋 Harvestable resources from mature shrubs
- 📋 Advanced vegetation:
  - 📋 Flowers and decorative plants
  - 📋 Vines and climbing plants
  - 📋 Mushrooms and fungi
  - 📋 Crops and farmable plants
  - 📋 Plant growth simulation

---

## 4. Sound Changes Underwater

### 4.1 Audio System Foundation
- 📋 Basic audio system setup:
  - 📋 SoundEffect class integration
  - 📋 Audio manager for sound playback
  - 📋 3D spatial audio positioning
  - 📋 Volume and pitch controls

### 4.2 Underwater Audio Effects
- 📋 Low-pass filter implementation:
  - 📋 Frequency attenuation when underwater
  - 📋 Muffled sound effect simulation
  - 📋 Real-time audio manipulation
  - 📋 Smooth transition when entering/exiting water
- 📋 Underwater ambient sounds:
  - 📋 Underwater bubbles sound
  - 📋 Muffled movement sounds
  - 📋 Echo/reverb effects
  - 📋 Water entry/exit splash sounds

### 4.3 Environmental Audio
- 📋 Surface sounds:
  - 📋 Footsteps on different materials
  - 📋 Block breaking sounds
  - 📋 Block placement sounds
  - 📋 Tool usage sounds
- 📋 Ambient environment:
  - 📋 Wind sounds (varies by biome)
  - 📋 Water flow/wave sounds
  - 📋 Wildlife sounds
  - 📋 Cave ambience
  - 📋 Weather sounds (rain, thunder)
- 📋 Music system:
  - 📋 Dynamic music based on location
  - 📋 Day/night cycle music transitions
  - 📋 Underwater music themes
  - 📋 Combat music (if applicable)

---

## 5. Core Game Systems

### 5.1 World Systems
- ✅ WorldManager - Chunk loading/unloading
- ✅ WorldGenerator - Terrain generation
- ✅ Chunk system (16x256x16 blocks)
- ✅ Block registry system
- ✅ 20+ block types with properties
- ✅ Block hardness and transparency
- ✅ Gravity-affected blocks (sand, gravel)
- 📋 Save/load world system
- 📋 World corruption recovery
- 📋 World backup system

### 5.2 Player Systems
- ✅ First-person 3D movement (WASD)
- ✅ Sprint system (Shift)
- ✅ Jump physics (Space)
- ✅ Mouse look camera with smooth rotation
- ✅ Collision detection with terrain
- ✅ Player height 1.8 blocks
- ✅ Reach distance 5 blocks
- 📋 Temporal stability system
- 📋 Temperature management
- 📋 Hunger and nutrition mechanics
- 📋 Thirst system
- 📋 Health system
- 📋 Status effects

### 5.3 Block Interaction
- ✅ Left-click block breaking
- ✅ Right-click block placement
- ✅ Block breaking progress indicator
- ✅ Raycasting for block selection
- ✅ Block hardness affects break time
- 📋 Tool effectiveness system
- 📋 Block drop system (silk touch vs normal)
- 📋 Multi-block structures

### 5.4 Inventory System
- ✅ 40-slot inventory
- ✅ Item stacking (64 per slot)
- ✅ Hotbar selection (1-9 keys)
- ✅ Starting items provided
- ✅ Visual hotbar display
- ✅ Selected slot highlight
- 📋 Inventory UI screen (I key)
- 📋 Item tooltips and descriptions
- 📋 Crafting grid integration
- 📋 Equipment slots

---

## 6. Rendering and Graphics

### 6.1 Core Rendering
- ✅ 3D voxel rendering
- ✅ Face culling optimization
- ✅ Chunk-based mesh optimization
- ✅ Depth rendering
- ✅ Basic directional lighting on block faces
- ✅ Transparent block rendering (water)
- 📋 Block textures (currently using colors)
- 📋 Texture atlas system
- 📋 Ambient occlusion
- 📋 Dynamic shadows

### 6.2 Skybox and Atmosphere
- ✅ Dynamic skybox with day/night cycle
- ✅ Sun and moon transit across sky
- ✅ Starry night sky (500+ twinkling stars)
- ✅ Atmospheric color transitions (dawn, day, dusk, night)
- ✅ Ambient lighting based on time of day
- 📋 Clouds system
- 📋 Weather effects (rain, snow)
- 📋 Fog system
- 📋 Aurora borealis (in tundra biome)

### 6.3 User Interface
- ✅ Crosshair targeting reticle
- ✅ Hotbar with 9 slots
- ✅ Block breaking progress bar
- ✅ Minimap (150x150 overhead view)
- ✅ Minimap compass with cardinal directions
- ✅ Clock gauge showing time of day
- ✅ Player coordinates display
- ✅ World map (M key, 200 block radius)
- ✅ Title screen
- ✅ Character status display (health, hunger, thirst bars)
- ✅ Settings/options menu
- ✅ Controls help screen
- ✅ Pause menu (P key)
- ✅ Debug overlay (F3 key with FPS, position, chunk info)
- 📋 Inventory screen polish
- 📋 Crafting interface

---

## 7. Crafting and Progression

### 7.1 Crafting Systems
- 📋 Knapping system (flint tools)
- 📋 Pottery system (clay forming, drying, firing)
- 📋 Metallurgy system:
  - 📋 Ore crushing
  - 📋 Smelting in bloomery/furnace
  - 📋 Metal casting
  - 📋 Forging and smithing
  - 📋 Alloying (bronze, steel)
- 📋 Textile system:
  - 📋 Fiber processing
  - 📋 Spinning thread
  - 📋 Weaving cloth
  - 📋 Sewing garments
- 📋 Carpentry system
- 📋 Advanced crafting stations

### 7.2 Technology Ages
- 📋 Stone Age:
  - 📋 Flint knapping
  - 📋 Basic wooden tools
  - 📋 Stone tools
- 📋 Copper Age:
  - 📋 Native copper collection
  - 📋 Cold hammering
  - 📋 Simple copper tools
- 📋 Bronze Age:
  - 📋 Ore smelting
  - 📋 Tin and copper alloying
  - 📋 Bronze tools and weapons
- 📋 Iron Age:
  - 📋 Advanced metallurgy
  - 📋 Steel creation
  - 📋 Iron tools and machinery

### 7.3 Prospecting and Resources
- 📋 Prospecting pick system
- 📋 Ore vein discovery
- 📋 Resource surveys
- 📋 Mining operations
- 📋 Renewable resources management

---

## 8. Entities and AI

### 8.1 Animals
- 📋 Passive animals:
  - 📋 Deer (food source)
  - 📋 Rabbits (food source)
  - 📋 Chickens (food, eggs)
  - 📋 Sheep (wool)
  - 📋 Cattle (food, leather, milk)
- 📋 Neutral animals:
  - 📋 Wolves (territorial)
  - 📋 Bears (territorial)
- 📋 Animal AI behaviors:
  - 📋 Grazing and foraging
  - 📋 Fleeing from threats
  - 📋 Hunting (predators)
  - 📋 Breeding cycles
  - 📋 Migration patterns
  - 📋 Day/night behavior changes

### 8.2 Hostile Entities
- 📋 Drifters (Vintage Story inspired)
- 📋 Environmental hazards
- 📋 Boss creatures
- 📋 Combat system (if implemented)

---

## 9. Seasonal and Environmental Systems

### 9.1 Day/Night Cycle
- ✅ 10-minute day/night cycle
- ✅ Time of day tracking
- ✅ Day counter display
- ✅ Sun/moon position based on time
- ✅ Lighting changes throughout day
- 📋 Configurable day length

### 9.2 Seasons
- 📋 Four seasons with transitions
- 📋 Different day lengths per season
- 📋 Temperature variation by season
- 📋 Seasonal crop growth
- 📋 Animal migration by season
- 📋 Seasonal biome changes:
  - 📋 Snow in winter
  - 📋 Leaf color changes in autumn
  - 📋 Flower blooming in spring

### 9.3 Weather System
- 📋 Rain
- 📋 Snow (temperature dependent)
- 📋 Thunderstorms
- 📋 Fog
- 📋 Wind (affects particles, trees)
- 📋 Weather effects on gameplay (crop growth, temperature)

---

## 10. Debugging and Development Tools

### 10.1 Logging and Error Tracking
- ✅ Comprehensive logging system
- ✅ Log files with timestamps
- ✅ Severity levels (INFO, WARNING, ERROR, FATAL)
- ✅ Stack trace logging
- ✅ Console output mirroring

### 10.2 Debug Tools
- ✅ Screenshots folder for progress tracking
- ✅ Documentation organization (Docs/)
- ✅ Debug overlay (F3 style):
  - ✅ FPS counter
  - ✅ Chunk loading info
  - ✅ Player position (precise)
  - ✅ Player rotation (yaw/pitch)
  - ✅ Water submersion depth
  - 📋 Block looking at
  - 📋 Memory usage
  - 📋 Render statistics
- 📋 Collision visualization
- 📋 Chunk boundary visualization
- 📋 Performance profiling tools

### 10.3 Testing Infrastructure
- ✅ Unit test project
- ✅ WaterPhysicsTests (8 tests)
- ✅ PlayerMovementTests
- ✅ InventoryTests
- ✅ CollisionTests
- ✅ TimeManagerTests
- ✅ LoggerTests
- 📋 Integration tests
- 📋 Performance benchmarks
- 📋 Automated build pipeline

---

## 11. Performance and Optimization

### 11.1 Rendering Optimizations
- ✅ Chunk mesh caching
- ✅ Face culling between blocks
- ✅ Water mesh separated from solid blocks
- 📋 Level of Detail (LOD) system for distant chunks
- 📋 Frustum culling
- 📋 Occlusion culling
- 📋 Batch rendering optimizations

### 11.2 Memory Management
- 📋 Chunk unloading for distant areas
- 📋 Asset streaming
- 📋 Texture compression
- 📋 Memory pooling for objects

### 11.3 Multithreading
- 📋 Async chunk generation
- 📋 Async mesh building
- 📋 Background saving
- 📋 Thread-safe world access

---

## 12. Multiplayer (Future Consideration)

- ⏸️ Client-server architecture
- ⏸️ Player synchronization
- ⏸️ Block modification synchronization
- ⏸️ Chat system
- ⏸️ Player permissions
- ⏸️ Server administration tools

---

## 13. Modding Support (Future Consideration)

- ⏸️ Mod API design
- ⏸️ Custom block types
- ⏸️ Custom items
- ⏸️ Custom entities
- ⏸️ Scripting support
- ⏸️ Mod loader
- ⏸️ Mod configuration files

---

## Priority for Next Development Cycle

### Immediate (Current PR)
1. ✅ Documentation organization (Docs/ folder)
2. ✅ ROADMAP.md creation
3. 🚧 Particle system foundation for bubbles
4. 🚧 Audio system foundation
5. 🚧 Vegetation growth stages system

### Short-term (Next 1-2 PRs)
1. Bubble particle effects underwater
2. Underwater audio filtering
3. Grass and shrubbery placement system
4. Points of interest generation

### Medium-term (Next 2-4 months)
1. Crafting system implementation
2. Survival mechanics (hunger, thirst, temperature)
3. Tool progression system
4. Block textures and texture atlas
5. Save/load system

### Long-term (4+ months)
1. Technology age progression
2. Advanced crafting (pottery, metallurgy)
3. Animals and fauna
4. Seasonal system
5. Weather system
6. Multiplayer consideration

---

## Notes

- This roadmap is a living document and will be updated with each pull request
- Features may be reprioritized based on user feedback and development insights
- All completed features (✅) have corresponding tests where applicable
- Documentation for each major system is maintained in the Docs/ folder

---

**Repository:** shifty81/VSclone  
**Project:** Timeless Tales - A Vintage Story inspired voxel survival game  
**Engine:** C# with MonoGame 3.8.4 on .NET 8.0
