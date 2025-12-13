# Frequently Asked Questions (FAQ)

## General Questions

### What is Timeless Tales?
Timeless Tales is an open-world voxel-based survival sandbox game inspired by Vintage Story. It emphasizes realistic geology, complex crafting systems, and immersive gameplay.

### What platform does it run on?
Currently, Timeless Tales is designed for **Windows** (64-bit). The use of MonoGame means cross-platform support is possible in the future (Linux, macOS).

### Is this a commercial project?
This is an **educational project** created for learning game development concepts. See the LICENSE file for details.

---

## Technical Questions

### What happened to the GeonBit implementation?

**Short Answer**: Timeless Tales **never used GeonBit** and does not plan to. The project uses a custom 3D rendering architecture built directly on MonoGame.

**Detailed Answer**: 
GeonBit is a 3D engine framework built on top of MonoGame that provides scene management, lighting, and other high-level 3D features. However, Timeless Tales uses a **custom voxel rendering system** instead because:

1. **Performance**: Custom rendering is optimized specifically for voxel worlds (chunk meshing, face culling, greedy meshing)
2. **Control**: Direct access to rendering pipeline allows for tailored optimizations
3. **Simplicity**: Only one dependency (MonoGame) keeps the project lightweight
4. **Learning**: Building from scratch provides deeper understanding of 3D graphics
5. **Vintage Story Approach**: Follows similar architecture to the game that inspired this project

See **[RENDERING_ARCHITECTURE.md](RENDERING_ARCHITECTURE.md)** for a comprehensive technical explanation.

### Can we use ManicDigger as a starting point?

**Short Answer**: Not without starting over completely. Timeless Tales is already built with 11,412 lines of MonoGame code. Switching to ManicDigger would require discarding everything and starting a new project from scratch.

**Key Facts**:
- **Current Status**: Alpha 0.1 with ~6 months of development (11,412 lines of code)
- **Migration Impact**: 100% code rewrite required - completely different APIs
- **Time to Parity**: 6-12 months to reimplement current features on ManicDigger
- **Recommendation**: Continue with MonoGame unless you have compelling reasons to restart

**What is ManicDigger?**
- Open-source voxel game engine (C#)
- Uses OpenTK for OpenGL rendering (not MonoGame)
- Vintage Story's original foundation (they heavily modified it)
- Built-in multiplayer support
- Different architecture from MonoGame

**Why Current Project Uses MonoGame**:
1. **Already Implemented**: 45+ files with working systems
2. **Learning Goals**: Building from scratch teaches engine fundamentals
3. **Lightweight**: Single dependency, clean architecture
4. **Modern**: .NET 8.0 with modern C# features
5. **Working**: Alpha 0.1 already released with many features

**If You Want ManicDigger**:
This would be a **new project**, not a migration:
1. Create new repository (e.g., VSclone-ManicDigger)
2. Fork or use ManicDigger as base
3. Reimplement all features from scratch
4. Archive or pause current Timeless Tales project

**Alternative: Adopt ManicDigger Techniques**:
You can implement ManicDigger's rendering techniques (shaders, textures, water effects) in the current MonoGame codebase without a full rewrite:
- Study their GLSL shaders, translate to HLSL for MonoGame
- Implement advanced water rendering (caustics, refraction, reflection)
- Add texture atlas system (foundation already exists)
- **Effort**: 7-11 weeks vs 6-12 months for full rewrite

See **[MANICDIGGER_TECHNIQUES.md](MANICDIGGER_TECHNIQUES.md)** for practical implementation guide, including:
- How to implement advanced shaders in MonoGame
- Sample water shader code (HLSL)
- Texture system improvements
- Step-by-step implementation plan
- Effort estimates and limitations

See **[MANICDIGGER_MIGRATION.md](MANICDIGGER_MIGRATION.md)** for comprehensive analysis of full migration, including:
- Detailed comparison of options
- What would need to be rewritten
- Timeline estimates
- Decision framework
- Recommendations based on your goals

### Why MonoGame instead of Unity or Unreal?

**MonoGame** was chosen because:
- ✅ **Lightweight**: No heavy IDE or massive engine overhead
- ✅ **Learning**: Teaches 3D graphics fundamentals from the ground up
- ✅ **Control**: Complete control over every aspect of rendering
- ✅ **Open Source**: Free and open-source framework
- ✅ **C# Native**: Clean C# API without engine-specific abstractions
- ✅ **Cross-platform**: Potential for Linux/Mac support

**Unity/Unreal** are excellent engines but:
- ⚠️ Heavy for a voxel game
- ⚠️ More abstraction layers
- ⚠️ Licensing considerations
- ⚠️ Less educational value for low-level graphics

### Does Timeless Tales support modding?

Not yet, but modding support is on the **long-term roadmap**. Planned features include:
- Custom block types
- Custom items
- Custom entities
- Scripting support
- Mod loader

See [ROADMAP.md](../ROADMAP.md) Section 13 for details.

### Is multiplayer planned?

Multiplayer is listed as **future consideration** on the roadmap (Section 12). The focus is currently on completing single-player features first. A client-server architecture would need to be designed before multiplayer could be implemented.

---

## Gameplay Questions

### How do I get started?

1. Gather resources by breaking blocks (left-click)
2. Start with stone and wood
3. Craft basic tools (planned feature)
4. Build shelter
5. Progress through technological ages

See the Controls section in [README.md](../README.md) for keybindings.

### What's the goal of the game?

Timeless Tales is a **sandbox survival game** with no fixed end goal. Players set their own objectives:
- Survive and thrive
- Build elaborate structures
- Progress through technology ages (Stone → Copper → Bronze → Iron)
- Explore diverse biomes
- Master complex crafting systems
- Discover points of interest

### How does the crafting system work?

The crafting system is inspired by Vintage Story's realistic approach:
- **Knapping**: Break flint to create stone tools
- **Pottery**: Form clay, dry it, fire it in kilns
- **Metallurgy**: Mine ore, crush it, smelt it, forge tools
- **Textiles**: Process fibers, spin thread, weave cloth

Many of these systems are still in development. See [ROADMAP.md](../ROADMAP.md) Section 7 for progress.

### Can I play this now?

Yes! The game is in **Alpha 0.1** and playable. Current features include:
- Procedural world generation
- First-person movement and physics
- Block breaking and placement
- Basic inventory system
- Day/night cycle
- Swimming and water physics
- Character customization

See [README.md](../README.md) for build instructions.

---

## Development Questions

### Can I contribute?

Yes! Contributions are welcome. Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### How do I build the project?

```bash
# Clone the repository
git clone https://github.com/shifty81/VSclone.git
cd VSclone

# Build with .NET
dotnet build TimelessTales.sln

# Run the game
cd TimelessTales
dotnet run
```

See [README.md](../README.md) Building & Running section for more details.

### How do I report bugs?

1. Check if the issue already exists in the GitHub Issues
2. If not, create a new issue with:
   - Clear description of the bug
   - Steps to reproduce
   - Expected vs actual behavior
   - Log files from `TimelessTales/bin/Debug/net8.0/logs/`
   - Screenshot if applicable

### Where are the log files?

Log files are located in:
- **Debug**: `TimelessTales/bin/Debug/net8.0/logs/`
- **Release**: `TimelessTales/bin/Release/net8.0/logs/`

Log files are named: `timeless_tales_YYYY-MM-DD_HH-mm-ss.log`

---

## Performance Questions

### What are the system requirements?

**Minimum**:
- OS: Windows 10 (64-bit)
- CPU: Dual-core 2.0 GHz
- RAM: 4 GB
- GPU: DirectX 11 compatible
- Storage: 500 MB

**Recommended**:
- OS: Windows 11 (64-bit)
- CPU: Quad-core 3.0 GHz
- RAM: 8 GB
- GPU: Dedicated graphics card with 2GB VRAM
- Storage: 1 GB SSD

### The game runs slowly. What can I do?

Current optimizations include:
- Chunk-based rendering
- Face culling
- Mesh caching

Planned optimizations:
- Greedy meshing
- Frustum culling
- Occlusion culling
- LOD system
- Multithreaded chunk generation

See [ROADMAP.md](../ROADMAP.md) Section 11 for the optimization roadmap.

### How big is the world?

The world is **infinite** (limited only by storage):
- Chunk-based generation (16x256x16 blocks)
- Render distance: 8 chunks (configurable)
- Height: 256 blocks (Y: 0-255)
- Procedurally generated as you explore

---

## Comparison Questions

### How is this different from Minecraft?

**Similarities**:
- Voxel-based block world
- Creative and survival modes (planned)
- Crafting and building

**Differences**:
- ✅ Realistic geology (rock layers, ore distribution)
- ✅ Complex crafting (knapping, pottery, metallurgy)
- ✅ Technology progression (Stone → Iron ages)
- ✅ No magic or fantastical elements (realistic survival)
- ✅ More detailed resource processing

### How is this different from Vintage Story?

**Timeless Tales is inspired by Vintage Story** but differs in:
- ✅ Open source / educational project
- ✅ Custom MonoGame engine (not .NET custom engine)
- ✅ Simplified for learning purposes
- ⚠️ Much earlier in development
- ⚠️ Fewer features (currently)

This is a **learning project** that pays homage to Vintage Story's excellent design.

---

## Future Features

### When will [feature X] be added?

Check the [ROADMAP.md](../ROADMAP.md) for feature status:
- ✅ Completed
- 🚧 In Progress
- 📋 Planned
- 🔬 Research
- ⏸️ On Hold

Features are prioritized based on:
1. Core gameplay needs
2. Technical dependencies
3. Community feedback

### Can I request a feature?

Yes! Please:
1. Check if it's already on the roadmap
2. Open a GitHub Issue with the "Feature Request" label
3. Describe the feature clearly
4. Explain why it would enhance the game

---

## Legal Questions

### Can I use this code in my project?

See the [LICENSE](../LICENSE) file for legal details. This is an educational project - check licensing terms before reusing code.

### Is this affiliated with Vintage Story?

**No**. Timeless Tales is an **independent fan project** inspired by Vintage Story. It is not affiliated with, endorsed by, or connected to Anego Studios (creators of Vintage Story).

### Can I distribute modified versions?

Check the [LICENSE](../LICENSE) file for distribution terms.

---

## Support

### Where can I get help?

1. Read the documentation in [Docs/](.)
2. Check this FAQ
3. Review the [README.md](../README.md)
4. Open a GitHub Issue
5. Check existing GitHub Issues for similar problems

### The game crashes on startup. What should I do?

1. Check the latest log file in `logs/` directory
2. Look for FATAL or ERROR entries
3. Verify prerequisites:
   - .NET 8.0 SDK installed
   - DirectX 11 compatible GPU
   - Windows 10/11 64-bit
4. Try running from Visual Studio with debugging (F5)
5. Report the issue on GitHub with log file attached

---

## Additional Resources

- **[README.md](../README.md)** - Project overview and quick start
- **[ROADMAP.md](../ROADMAP.md)** - Development progress and plans
- **[GDD.md](GDD.md)** - Game Design Document
- **[RENDERING_ARCHITECTURE.md](RENDERING_ARCHITECTURE.md)** - Technical rendering details
- **[CONTRIBUTING.md](CONTRIBUTING.md)** - Contribution guidelines
- **[CHANGELOG.md](CHANGELOG.md)** - Version history

---

**Last Updated**: 2025-12-13  
**Version**: Alpha 0.1  
**Repository**: https://github.com/shifty81/VSclone
