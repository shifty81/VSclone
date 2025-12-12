# Particle System Implementation

## Overview
The particle system provides a flexible framework for rendering visual effects such as bubbles, smoke, sparks, and other environmental effects in Timeless Tales.

## Architecture

### Core Components

#### 1. Particle Class
- **Location**: `TimelessTales/Particles/Particle.cs`
- **Purpose**: Represents a single particle instance
- **Properties**:
  - `Position` - 3D world position
  - `Velocity` - Movement vector (blocks per second)
  - `Color` - Particle color with alpha
  - `Size` - Particle size in blocks
  - `Life` - Remaining lifetime in seconds
  - `MaxLife` - Total lifetime for fade calculation
  - `Alpha` - Computed alpha based on life remaining (0.0 to 1.0)

#### 2. ParticleEmitter Class
- **Location**: `TimelessTales/Particles/ParticleEmitter.cs`
- **Purpose**: Manages emission and updates for a group of particles
- **Key Features**:
  - Configurable emission rate (particles per second)
  - Velocity base and variation for randomization
  - Automatic particle lifecycle management
  - Position-based spawning with random offset

**Configuration Parameters**:
```csharp
EmissionRate = 5.0f;        // Particles per second
ParticleLifetime = 2.0f;     // Seconds
ParticleSize = 0.1f;         // Block units
ParticleColor = Color.White;
VelocityBase = new Vector3(0, 1, 0);      // Base upward movement
VelocityVariation = new Vector3(0.2f, 0.5f, 0.2f); // Random variation
```

#### 3. ParticleRenderer Class
- **Location**: `TimelessTales/Particles/ParticleRenderer.cs`
- **Purpose**: Renders particles with billboarding technique
- **Rendering Technique**:
  - **Billboarding**: Particles always face the camera
  - **Alpha Blending**: Transparent particles with fade effect
  - **Depth Read Only**: Particles don't write to depth buffer
  - **Batched Rendering**: All particles rendered in a single draw call

## Underwater Bubble System

### Implementation Example

```csharp
// Create bubble emitter when player is underwater
var bubbleEmitter = new ParticleEmitter(playerMouthPosition)
{
    EmissionRate = 3.0f,              // 3 bubbles per second
    ParticleLifetime = 2.5f,           // Bubbles last 2.5 seconds
    ParticleSize = 0.08f,              // Small bubbles
    ParticleColor = new Color(200, 220, 255, 180), // Light blue, semi-transparent
    VelocityBase = new Vector3(0, 0.5f, 0),        // Float upward slowly
    VelocityVariation = new Vector3(0.1f, 0.2f, 0.1f) // Slight wobble
};
```

### Bubble Behavior

1. **Emission**: Bubbles emit periodically from the player's mouth when underwater
2. **Movement**: Float upward with slight horizontal wobble
3. **Lifetime**: Fade out after 2-3 seconds or when reaching water surface
4. **Appearance**: Small, semi-transparent, light blue spheres

## Usage in Game

### Integration Steps

1. **Create ParticleRenderer** in main game class:
```csharp
private ParticleRenderer _particleRenderer;
private List<ParticleEmitter> _emitters;

// In Initialize():
_particleRenderer = new ParticleRenderer(GraphicsDevice);
_emitters = new List<ParticleEmitter>();
```

2. **Create Emitters** as needed:
```csharp
// When player goes underwater:
var bubbles = new ParticleEmitter(player.HeadPosition);
bubbles.EmissionRate = 3.0f;
bubbles.ParticleColor = new Color(200, 220, 255, 180);
_emitters.Add(bubbles);
```

3. **Update** each frame:
```csharp
// In Update():
float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
foreach (var emitter in _emitters)
{
    emitter.Update(deltaTime);
}
```

4. **Render** after world, before UI:
```csharp
// In Draw():
_particleRenderer.Draw(camera, _emitters);
```

## Future Enhancements

### Planned Features
- **Particle Textures**: Support for sprite textures instead of solid colors
- **Physics Interactions**: Wind, water currents affecting particles
- **Collision Detection**: Particles bounce off surfaces
- **Particle Trails**: Motion blur or trail effects
- **Advanced Emitters**: Cone, sphere, and custom emission shapes
- **Particle Effects**:
  - Smoke from fires
  - Dust clouds from mining
  - Sparkles from ore veins
  - Rain and snow particles
  - Splash effects when entering water
  - Tool impact particles

### Performance Considerations

- **Particle Pooling**: Reuse particle objects instead of creating new ones
- **Culling**: Don't update/render particles outside view frustum
- **LOD**: Reduce particle count for distant emitters
- **Batch Limits**: Cap maximum particles per emitter (e.g., 1000)

## Testing

Particle system can be tested independently:

```csharp
[Test]
public void TestParticleLifetime()
{
    var particle = new Particle(Vector3.Zero, Vector3.UnitY, Color.White, 0.1f, 2.0f);
    
    // Update for 1 second
    bool alive = particle.Update(1.0f);
    Assert.IsTrue(alive);
    Assert.AreEqual(1.0f, particle.Life, 0.01f);
    
    // Update past lifetime
    alive = particle.Update(2.0f);
    Assert.IsFalse(alive);
}

[Test]
public void TestEmitterRate()
{
    var emitter = new ParticleEmitter(Vector3.Zero);
    emitter.EmissionRate = 10.0f; // 10 particles per second
    
    // Update for 0.5 seconds should create 5 particles
    emitter.Update(0.5f);
    Assert.AreEqual(5, emitter.GetParticles().Count);
}
```

## Technical Notes

### Coordinate System
- Particles use world coordinates (same as blocks)
- Position (0, 0, 0) is world origin
- Y-axis is up (positive = higher altitude)

### Rendering Order
1. Opaque world geometry
2. Transparent water
3. Particles (depth read only, alpha blended)
4. UI overlay

### Memory Management
- Dead particles are removed from emitter's list
- Emitters can be disabled (IsActive = false) to stop emission
- Call `emitter.Clear()` to immediately remove all particles

---

**Version**: 1.0  
**Last Updated**: 2025-12-12  
**Author**: Timeless Tales Development Team
