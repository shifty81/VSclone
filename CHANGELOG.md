# Changelog
All notable changes to Timeless Tales will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Block textures to replace colored blocks
- Save/load world functionality
- Day/night cycle with dynamic lighting
- Crafting system (knapping, pottery, metallurgy)
- Temperature and seasonal mechanics
- Hunger, thirst, and nutrition systems
- Prospecting system for ore discovery
- Hostile entities (Drifters)
- Temporal stability mechanic
- Sound effects and music
- Advanced UI (inventory screen, crafting interface)
- Chiseling for voxel-level editing

## [0.1.0] - 2025-12-11

### Added
- Initial release of Timeless Tales
- Procedural world generation with SimplexNoise
  - Chunk-based system (16x256x16 blocks)
  - 5 biomes: Tundra, Boreal, Temperate, Desert, Tropical
  - Realistic geological stratification (3 rock layers)
  - Cave systems
  - Non-random ore distribution
- Block system with 20+ block types
  - Natural blocks: Stone, Dirt, Grass, Sand, Gravel, Clay
  - Geological rocks: Granite, Limestone, Basalt, Sandstone, Slate
  - Ores: Copper, Tin, Iron, Coal
  - Wood blocks: Wood, Leaves, Planks
  - Properties: hardness, transparency, gravity physics
- Player character
  - First-person movement (WASD)
  - Sprint (Shift)
  - Jump with physics (Space)
  - Mouse-look camera with proper eye-height
  - Collision detection
- Block interaction
  - Left-click to break blocks
  - Right-click to place blocks
  - Raycasting for block selection
  - 5-block reach distance
  - Block breaking progress indicator
- Inventory system
  - 40-slot capacity
  - Item stacking
  - Hotbar selection (1-5 keys)
- 3D rendering
  - Voxel-based mesh generation
  - Face culling optimization
  - Directional lighting
  - 8-chunk render distance
- User interface
  - Crosshair for targeting
  - HUD framework
  - Pause functionality (P key)
- Input system
  - Keyboard state tracking
  - Mouse delta tracking
  - Press vs hold detection
- Documentation
  - Comprehensive README
  - Game Design Document (GDD)
  - Developer documentation
  - Quick start guide
  - Contributing guidelines
  - Implementation summary
- Build infrastructure
  - .NET 8.0 project structure
  - MonoGame 3.8.4 integration
  - Build script (build.sh)
  - .gitignore configuration

### Technical Details
- Built with C# 12 and .NET 8.0
- MonoGame framework for cross-platform support
- Zero security vulnerabilities (CodeQL verified)
- Clean modular architecture
- ~2,000 lines of code
- Extensive XML documentation

### Fixed
- Camera positioning now includes proper eye-height offset
- Boundary condition consistency in face culling
- Code duplication reduced with helper methods

### Known Issues
- No textures (colored blocks only)
- No save/load functionality
- No crafting system
- No survival mechanics
- No day/night cycle
- No sound/music
- Limited UI

## Release Notes

### Version 0.1.0 - "Foundation"

This is the foundational alpha release of Timeless Tales, establishing core game systems and architecture.

**What Works:**
- Complete voxel world generation with realistic geology
- Full player movement and physics
- Block mining and building mechanics
- Inventory management
- 3D rendering with optimization

**For Players:**
- Explore infinite procedurally generated worlds
- Build structures with various block types
- Discover ore deposits underground
- Experience different biomes

**For Developers:**
- Clean, extensible codebase
- Comprehensive documentation
- Ready for feature expansion
- Security-verified code

**Next Milestone:** Alpha 0.2 will focus on textures and basic crafting systems.

---

**Development Team:**
- Lead Developer: [Contributors]
- Inspired by: Vintage Story by Anego Studios
- Framework: MonoGame 3.8.4
- Language: C# 12 (.NET 8.0)

**Special Thanks:**
- MonoGame community
- Vintage Story community for inspiration
- All contributors and testers
