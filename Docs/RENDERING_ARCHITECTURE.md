# Rendering Architecture - Timeless Tales

## Overview

Timeless Tales uses a **custom 3D rendering architecture** built directly on **MonoGame 3.8.4**, without using third-party 3D engines like GeonBit, Unity3D, or similar frameworks.

## Why No GeonBit?

**GeonBit** is a 3D engine built on top of MonoGame that provides scene management, 3D models, lighting, and other high-level features. However, Timeless Tales does **not** use GeonBit for the following reasons:

### Design Decisions

1. **Voxel-Specific Optimization**
   - Our voxel-based rendering has unique requirements (chunk meshing, face culling, greedy meshing potential)
   - Custom rendering provides more control over performance optimizations specific to block-based worlds
   - No overhead from features we don't need (skeletal animation rigs, complex material systems, etc.)

2. **Learning and Control**
   - Direct MonoGame usage provides deeper understanding of 3D graphics fundamentals
   - Full control over rendering pipeline and optimization strategies
   - Easier to debug and optimize for our specific use case

3. **Lightweight Dependencies**
   - Single dependency (MonoGame 3.8.4) keeps the project simple
   - Reduces potential compatibility issues and update dependencies
   - Smaller binary size and memory footprint

4. **Vintage Story Inspiration**
   - Vintage Story also uses a custom rendering engine tailored for voxel worlds
   - Following similar architectural patterns for similar gameplay

## Current Rendering Architecture

### Core Rendering Components

Our custom rendering system consists of several specialized renderers:

#### 1. WorldRenderer (`TimelessTales/Rendering/WorldRenderer.cs`)
**Purpose**: Renders the voxel-based terrain
- Chunk-based mesh generation (16x256x16 blocks per chunk)
- Face culling optimization (hidden faces not rendered)
- Vertex coloring for block types
- Depth rendering for proper occlusion
- Greedy meshing opportunities (future optimization)

**Key Features**:
- Efficient vertex buffer management
- Per-chunk mesh caching
- Dynamic chunk loading/unloading
- Support for transparent and opaque blocks

#### 2. WaterRenderer (`TimelessTales/Rendering/WaterRenderer.cs`)
**Purpose**: Specialized rendering for water blocks
- Separate rendering pass for transparency
- Alpha blending for see-through water
- Wave animation using vertex displacement
- Depth-based color variation (shallow vs deep)
- Face culling between adjacent water blocks

**Key Features**:
- Sum-of-sines wave function
- Time-based animation
- Dual water types (freshwater/saltwater)
- Proper render order (after opaque geometry)

#### 3. SkyboxRenderer (`TimelessTales/Rendering/SkyboxRenderer.cs`)
**Purpose**: Dynamic sky rendering with day/night cycle
- Procedurally generated skybox
- Sun and moon positioning based on time
- Atmospheric color transitions
- Star field rendering (500+ stars)
- Time-based ambient lighting

**Key Features**:
- Smooth color transitions (dawn, day, dusk, night)
- Celestial body movement
- Twinkling star effects
- Atmospheric scattering simulation

#### 4. PlayerRenderer (`TimelessTales/Rendering/PlayerRenderer.cs`)
**Purpose**: Voxel-based character rendering
- First-person arm rendering
- Body visibility when looking down
- Skeleton-based animation system
- Character customization rendering (skin, hair, clothing)

**Key Features**:
- Bone hierarchy for animation
- First-person perspective optimization
- Customizable appearance
- Animation blending (walk, swim, idle)

#### 5. UnderwaterEffectRenderer (`TimelessTales/Rendering/UnderwaterEffectRenderer.cs`)
**Purpose**: Post-processing for underwater visuals
- Blue tint overlay when submerged
- Depth-based color intensity
- Smooth transition effects

#### 6. ParticleRenderer (`TimelessTales/Particles/ParticleRenderer.cs`)
**Purpose**: Particle effect rendering
- Billboard particles (always face camera)
- Additive/alpha blending
- Batch rendering for performance
- Used for bubbles, effects, etc.

### Rendering Pipeline

```text
1. Clear Screen
   ↓
2. Setup Camera/View Matrix
   ↓
3. Render Skybox (background)
   ↓
4. Render Opaque Geometry (terrain chunks)
   ↓
5. Render Transparent Geometry (water)
   ↓
6. Render Particles
   ↓
7. Render Player (first-person view)
   ↓
8. Apply Post-Processing (underwater effects)
   ↓
9. Render UI/HUD
   ↓
10. Present Frame
```

### Vertex Formats

We use custom vertex formats optimized for our needs:

- **Standard Blocks**: `VertexPositionColor` (position + color)
- **Textured Blocks**: `VertexPositionColorTexture` (position + color + UV)
- **Particles**: `VertexPositionColorTexture` (billboard quads)

### Shader Approach

Currently using **BasicEffect** from MonoGame for simplicity:
- Built-in vertex color support
- Basic lighting (directional, ambient)
- Texture mapping capability
- Fog support (future)

**Future**: Custom HLSL shaders planned for:
- Advanced lighting (ambient occlusion)
- Normal mapping
- Specular highlights
- Caustics (water)
- Post-processing effects

## Performance Optimizations

### Current Optimizations
1. **Chunk-based Rendering**: Only render loaded chunks
2. **Face Culling**: Hidden block faces not added to mesh
3. **Water Face Culling**: Adjacent water blocks share faces
4. **Mesh Caching**: Chunks only rebuilt when modified
5. **Separate Render Passes**: Opaque vs transparent
6. **Billboard Particles**: Minimal geometry for effects

### Planned Optimizations
1. **Greedy Meshing**: Combine adjacent same-type faces
2. **Frustum Culling**: Don't render chunks outside view
3. **Occlusion Culling**: Skip chunks behind other chunks
4. **LOD System**: Lower detail for distant chunks
5. **Multithreaded Mesh Building**: Async chunk generation
6. **GPU Instancing**: For repeated geometry

## Comparison: Custom vs GeonBit

| Feature | Custom Rendering | GeonBit |
|---------|-----------------|---------|
| **Voxel Optimization** | ✅ Fully optimized | ⚠️ Generic 3D |
| **Learning Curve** | 📈 Steep (3D math) | 📉 Gentle (high-level) |
| **Performance** | ✅ Highly optimized | ⚠️ General purpose |
| **Control** | ✅ Complete control | ⚠️ Framework constraints |
| **Dependencies** | ✅ MonoGame only | ⚠️ MonoGame + GeonBit |
| **File Size** | ✅ Small | ⚠️ Larger |
| **Features** | 🎯 Exactly what we need | 📦 Many unused features |
| **Debug Ease** | ✅ Direct access | ⚠️ Framework abstraction |

## Technical Debt & Future Work

### Short-term
- [ ] Implement custom HLSL shaders
- [ ] Add texture atlas system
- [ ] Implement greedy meshing
- [ ] Add frustum culling

### Medium-term
- [ ] Ambient occlusion baking
- [ ] Dynamic shadows
- [ ] Normal mapping support
- [ ] Advanced water shaders (caustics, refraction)

### Long-term
- [ ] Full PBR (Physically Based Rendering) material system
- [ ] Deferred rendering pipeline
- [ ] Dynamic global illumination
- [ ] Advanced post-processing stack

## Conclusion

**Timeless Tales does not use GeonBit** and never has. The project uses a **custom voxel rendering architecture** built directly on MonoGame. This approach provides:

- ✅ Better performance for voxel-based worlds
- ✅ Complete control over rendering pipeline
- ✅ Simpler dependency management
- ✅ Deeper understanding of 3D graphics
- ✅ Tailored optimizations for our specific use case

While GeonBit is an excellent framework for general 3D games, our voxel-specific requirements make a custom rendering solution the better choice.

---

## References

- **MonoGame Documentation**: https://docs.monogame.net/
- **GeonBit Project**: https://github.com/RonenNess/GeonBit
- **Vintage Story**: https://www.vintagestory.at/ (inspiration)

## Related Documentation

- [WATER_SYSTEM.md](WATER_SYSTEM.md) - Water rendering details
- [SKYBOX_IMPLEMENTATION.md](SKYBOX_IMPLEMENTATION.md) - Sky rendering details
- [PARTICLE_SYSTEM.md](PARTICLE_SYSTEM.md) - Particle rendering
- [GDD.md](GDD.md) - Game Design Document
- [ROADMAP.md](../ROADMAP.md) - Development roadmap

---

**Last Updated**: 2025-12-13  
**Author**: Timeless Tales Development Team  
**Engine**: MonoGame 3.8.4 (Direct, no GeonBit)
