# ManicDigger Migration Consideration

## Request

The project owner has expressed interest in using ManicDigger as the game engine, following Vintage Story's approach of building on a ManicDigger fork.

## Current Situation

### Existing Codebase Status
- **11,412 lines of C# code** already implemented
- **45+ source files** across multiple systems
- **Alpha 0.1 release** with working features
- **~6 months of development** invested

### Implemented Systems
1. **Rendering System** (9 files)
   - WorldRenderer, WaterRenderer, SkyboxRenderer
   - PlayerRenderer, ParticleRenderer
   - Custom shader management
   - Chunk mesh optimization

2. **World System** (4 files)
   - Procedural world generation
   - Chunk management
   - Tree generation
   - Block registry

3. **Player System** (6 files)
   - First-person movement and physics
   - Swimming mechanics with animations
   - Character customization
   - Skeleton and bone system

4. **UI System** (9 files)
   - Tabbed menu (Character, Inventory, Crafting, Map)
   - Title screen, pause menu, settings
   - Debug overlay
   - HUD and minimap

5. **Supporting Systems**
   - Audio manager
   - Particle system
   - Time manager with day/night cycle
   - Logger with file output
   - Input manager

### Test Coverage
- **173 unit tests** across 9 test files
- Tests for water physics, collision, inventory, time management

## ManicDigger Overview

### What is ManicDigger?
- Open-source voxel game engine (C#)
- Created by Marek Rosa (also creator of Space Engineers)
- Uses **OpenTK** for OpenGL rendering
- Designed for Minecraft-like block games
- Supports multiplayer out of the box

### Technical Stack
- **Language**: C#
- **Graphics**: OpenGL via OpenTK
- **Platform**: Windows, Linux, macOS
- **License**: Open source (specific license varies by component)

### Key Features
- Built-in chunk management
- Network multiplayer support
- Mod system
- Entity system
- Inventory system
- Block interaction

## Migration Implications

### What Would Need to Be Rewritten

#### Complete Rewrite Required (100%)
Since ManicDigger and MonoGame have completely different APIs and architectures, **everything** would need to be rewritten:

1. **Rendering System**
   - OpenTK API instead of MonoGame
   - Different shader system (GLSL vs MonoGame's Effect framework)
   - Different vertex buffer management
   - Different texture handling

2. **Game Loop**
   - ManicDigger's update/draw cycle
   - Different timing mechanisms
   - Different input handling

3. **World Management**
   - Adapt to ManicDigger's chunk system
   - Use ManicDigger's block types
   - Integrate with their world generator or replace it

4. **Player System**
   - Use ManicDigger's entity system
   - Reimplement physics using their framework
   - Adapt camera system

5. **UI System**
   - Completely different UI framework
   - Rewrite all menus and HUD
   - Different text rendering

6. **All Other Systems**
   - Audio
   - Particles  
   - Time management
   - Everything else

### What Could Be Preserved

**Game Design Only**:
- Block type definitions (conceptually)
- World generation algorithms (would need API translation)
- Game mechanics design
- Feature roadmap
- Documentation

**Code**: ~0% directly reusable due to different APIs

## Options Going Forward

### Option 1: Continue with MonoGame (Current)
**Pros**:
- Keep all existing work (11,412 lines of code)
- Continue current development trajectory
- Known architecture and patterns
- Working Alpha 0.1 already released
- Mature test coverage

**Cons**:
- Not following Vintage Story's exact technical approach
- Need to implement multiplayer from scratch (future)
- Need to build all engine features ourselves

**Effort**: Continue current development

---

### Option 2: Full Migration to ManicDigger
**Pros**:
- Follow Vintage Story's technical approach more closely
- Built-in multiplayer support
- Established voxel engine features
- Active(?) community and examples

**Cons**:
- **Discard all 11,412 lines of existing code**
- **Start completely from scratch**
- Learn entirely new codebase and API
- Different development paradigm
- Reimplement all features (world gen, rendering, physics, UI, etc.)
- Lose 6 months of development progress
- Need to verify ManicDigger's current maintenance status

**Effort**: 6-12 months to reach current feature parity

---

### Option 3: Hybrid Approach (Not Recommended)
Try to integrate ManicDigger into current MonoGame project.

**Reality**: This is not feasible. The engines are fundamentally incompatible and would not work together.

---

### Option 4: Fork ManicDigger (Like Vintage Story Did)
Start a new project by forking ManicDigger and building from there.

**Pros**:
- True Vintage Story approach
- Start with working voxel engine
- Modify engine to suit needs
- Built-in multiplayer

**Cons**:
- Abandon current Timeless Tales codebase entirely
- Need to understand ManicDigger's architecture first
- May inherit technical debt from ManicDigger
- Different from claimed "educational" custom engine approach
- Vintage Story spent years customizing their fork

**Effort**: 3-6 months to understand ManicDigger, then ongoing development

## Recommendations

### If Educational Goals Are Primary
**Stick with MonoGame** - You're learning more about game engine fundamentals by building from scratch. This provides deeper understanding of:
- 3D graphics programming
- Voxel rendering optimization
- Physics simulation
- Game architecture

### If Feature Velocity Is Primary
**Consider ManicDigger** - But understand this means:
1. Creating a **new repository** (shifty81/VSclone-ManicDigger)
2. Archiving or pausing current Timeless Tales
3. Starting fresh with ManicDigger fork
4. Spending months reaching current feature parity

### If Multiplayer Is Critical
**ManicDigger might be worth it** - Built-in networking is complex to implement from scratch. However:
- Multiplayer is marked "Future Consideration" on current roadmap
- Current MonoGame version is nowhere near needing multiplayer yet
- Could add networking library (Lidgren, LiteNetLib) to MonoGame later

## Decision Framework

Ask yourself:
1. **Why do you want ManicDigger?**
   - ☐ Built-in multiplayer
   - ☐ Follow Vintage Story's exact approach
   - ☐ Faster feature development
   - ☐ Community and examples
   - ☐ Other: ___________

2. **What is your primary goal?**
   - ☐ Learn game engine development (→ Stay with MonoGame)
   - ☐ Build Vintage Story clone quickly (→ Consider ManicDigger)
   - ☐ Educational project (→ Stay with MonoGame)
   - ☐ Commercial game (→ Reevaluate both options)

3. **Are you willing to:**
   - ☐ Discard 6 months of work
   - ☐ Restart from zero
   - ☐ Spend 6+ months reaching current features again
   - ☐ Learn completely new architecture

4. **Timeline expectations:**
   - With MonoGame: Continue adding features immediately
   - With ManicDigger: 6+ months to reach current state

## Next Steps

### If Staying with MonoGame
1. Continue development on current branch
2. Close this migration discussion
3. Update documentation to clarify architecture choice
4. Focus on roadmap features

### If Migrating to ManicDigger
1. **Research Phase** (1-2 weeks)
   - Download and run ManicDigger
   - Review source code and architecture
   - Check license compatibility
   - Verify current maintenance status
   - Test building and running

2. **Planning Phase** (1 week)
   - Create new repository
   - Document migration plan
   - Prioritize feature reimplementation order
   - Set realistic timeline

3. **Implementation Phase** (6+ months)
   - Fork ManicDigger
   - Reimplement world generation
   - Reimplement player systems
   - Reimplement UI
   - Reimplement all other features

4. **Archive Current Project**
   - Mark current repo as archived or paused
   - Document reasons for migration
   - Preserve for reference

## Important Considerations

### ManicDigger Status
**Research Required**: 
- Is ManicDigger still actively maintained?
- When was the last commit?
- Is the community active?
- Are there known issues or limitations?

### Vintage Story Context
Vintage Story didn't just use ManicDigger - they:
1. Forked it years ago
2. Heavily modified the engine
3. Added their own rendering pipeline
4. Built extensive custom systems on top
5. Have a professional development team

They likely chose ManicDigger because:
- They needed a starting point for multiplayer
- They had existing experience with it
- They had resources to customize heavily

### Legal/Licensing
- Verify ManicDigger's license
- Ensure compatibility with your project goals
- Check if forking is allowed and under what terms

## Conclusion

This is a **major architectural decision** that requires careful consideration. The current MonoGame implementation is working and advancing. Switching to ManicDigger would mean starting over completely.

**Recommendation**: Unless you have compelling reasons (like immediate multiplayer requirement), continue with MonoGame and leverage the 6 months of development already invested.

If you decide to migrate, this should be a **new project**, not a migration of the current codebase.

---

**Author**: Timeless Tales Development Team  
**Date**: 2025-12-13  
**Status**: Awaiting Decision  
**Current Project**: TimelessTales (MonoGame) - Alpha 0.1 - 11,412 LOC  
**Proposed Project**: New ManicDigger-based implementation - 0 LOC
