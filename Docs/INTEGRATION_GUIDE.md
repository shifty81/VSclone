# System Integration Guide

This document provides step-by-step instructions for integrating the particle system, audio system, and vegetation system into the main game loop.

## Overview

The three new systems have been implemented and are ready for integration:

1. **Particle System** - For visual effects like bubbles, smoke, and sparks
2. **Audio System** - For sound effects and music with underwater filtering
3. **Vegetation System** - For plant growth with 3 stages

## Integration Steps

### 1. Particle System Integration

#### Step 1.1: Add Fields to TimelessTalesGame

Add these fields to `TimelessTales/Core/TimelessTalesGame.cs`:

```csharp
using TimelessTales.Particles;

// In TimelessTalesGame class:
private ParticleRenderer? _particleRenderer;
private List<ParticleEmitter> _particleEmitters;
private ParticleEmitter? _bubbleEmitter; // For underwater bubbles
```

#### Step 1.2: Initialize in StartNewGame()

In the `StartNewGame()` method, after initializing renderers:

```csharp
// Initialize particle system
Logger.Info("Initializing particle system...");
_particleRenderer = new ParticleRenderer(GraphicsDevice);
_particleEmitters = new List<ParticleEmitter>();
Logger.Info("Particle system initialized");
```

#### Step 1.3: Update Particles

In the `Update()` method, inside the playing state and not paused block:

```csharp
// Update particles
float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
foreach (var emitter in _particleEmitters)
{
    emitter.Update(deltaTime);
}

// Update bubble emitter position to player's mouth
if (_player!.IsUnderwater)
{
    if (_bubbleEmitter == null)
    {
        // Create bubble emitter when player goes underwater
        _bubbleEmitter = new ParticleEmitter(_player.Position)
        {
            EmissionRate = 3.0f,
            ParticleLifetime = 2.5f,
            ParticleSize = 0.08f,
            ParticleColor = new Color(200, 220, 255, 180),
            VelocityBase = new Vector3(0, 0.5f, 0),
            VelocityVariation = new Vector3(0.1f, 0.2f, 0.1f)
        };
        _particleEmitters.Add(_bubbleEmitter);
    }
    else
    {
        // Update position to player's mouth (head position)
        _bubbleEmitter.Position = _player.Position + new Vector3(0, 1.7f, 0);
    }
}
else
{
    // Remove bubble emitter when player exits water
    if (_bubbleEmitter != null)
    {
        _particleEmitters.Remove(_bubbleEmitter);
        _bubbleEmitter = null;
    }
}
```

#### Step 1.4: Render Particles

In the `Draw()` method, after water rendering and before UI:

```csharp
// Draw particles (after water, before UI)
_particleRenderer!.Draw(_camera!, _particleEmitters);
```

### 2. Audio System Integration

#### Step 2.1: Add Fields to TimelessTalesGame

```csharp
using TimelessTales.Audio;

// In TimelessTalesGame class:
private AudioManager? _audioManager;
```

#### Step 2.2: Initialize in Initialize()

In the `Initialize()` method:

```csharp
// Initialize audio manager
_audioManager = new AudioManager();
Logger.Info("Audio manager initialized");
```

#### Step 2.3: Load Sounds (When Available)

In the `LoadContent()` method (when sound files are added):

```csharp
// Load sound effects (placeholder for when sound files are added)
// _audioManager.LoadSound("footstep", Content.Load<SoundEffect>("Sounds/footstep"));
// _audioManager.LoadSound("water_splash", Content.Load<SoundEffect>("Sounds/water_splash"));
// _audioManager.LoadSound("bubble", Content.Load<SoundEffect>("Sounds/bubble"));
```

#### Step 2.4: Update Audio State

In the `Update()` method, inside the playing state:

```csharp
// Update audio manager
_audioManager!.Update();

// Set underwater state
_audioManager.IsUnderwater = _player!.IsUnderwater;
```

#### Step 2.5: Dispose Audio

In the `Dispose()` method:

```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        _audioManager?.Dispose();
    }
    base.Dispose(disposing);
}
```

### 3. Vegetation System Integration

#### Step 3.1: Add Fields to TimelessTalesGame

```csharp
using TimelessTales.Vegetation;

// In TimelessTalesGame class:
private VegetationManager? _vegetationManager;
```

#### Step 3.2: Initialize in StartNewGame()

In the `StartNewGame()` method, after world manager initialization:

```csharp
// Initialize vegetation manager
Logger.Info("Initializing vegetation manager...");
_vegetationManager = new VegetationManager(_worldManager);
Logger.Info("Vegetation manager initialized");
```

#### Step 3.3: Populate Chunks

In `WorldManager.cs`, after a chunk is generated, populate it with vegetation:

```csharp
// In WorldManager class, add a callback or method:
public void SetVegetationManager(VegetationManager vegManager)
{
    _vegetationManager = vegManager;
}

// In the chunk generation/loading code:
private void OnChunkLoaded(Chunk chunk)
{
    _vegetationManager?.PopulateChunk(chunk);
}
```

Or in `TimelessTalesGame.cs` after world initialization:

```csharp
// Populate initial chunks with vegetation
foreach (var chunk in _worldManager.GetLoadedChunks())
{
    _vegetationManager.PopulateChunk(chunk);
}
```

#### Step 3.4: Update Vegetation

In the `Update()` method, inside the playing state and not paused block:

```csharp
// Update vegetation growth
_vegetationManager!.Update(deltaTime);
```

#### Step 3.5: Render Vegetation (Future)

Vegetation rendering requires visual models/sprites. For now, this is a placeholder:

```csharp
// In Draw() method (to be implemented when plant models are ready):
// RenderVegetation();

private void RenderVegetation()
{
    // Future implementation:
    // foreach (var plant in _vegetationManager.GetAllPlants())
    // {
    //     RenderPlantModel(plant);
    // }
}
```

## Complete Integration Example

Here's how the `Update()` method would look with all systems integrated:

```csharp
protected override void Update(GameTime gameTime)
{
    try
    {
        // Handle exit (existing code)
        // ... (exit handling code)

        if (_currentState == GameState.Playing)
        {
            // Update input (existing)
            _inputManager!.Update();
            
            // Toggle controls (existing)
            // ... (pause, inventory, map toggle code)
            
            if (!_isPaused && !_inventoryOpen && !_worldMapOpen)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Update time (existing)
                _timeManager!.Update(gameTime);
                
                // Update audio manager
                _audioManager!.Update();
                
                // Update player (existing)
                _player!.Update(gameTime, _inputManager, _worldManager!);
                
                // Update audio underwater state
                _audioManager.IsUnderwater = _player.IsUnderwater;
                
                // Update camera (existing)
                _camera!.Position = _player.Position + new Vector3(0, 1.62f, 0);
                _camera.Rotation = _player.Rotation;
                
                // Update world (existing)
                _worldManager!.Update(_player.Position);
                
                // Update water renderer (existing)
                _waterRenderer!.Update(gameTime);
                
                // Update vegetation growth
                _vegetationManager!.Update(deltaTime);
                
                // Update particles
                foreach (var emitter in _particleEmitters)
                {
                    emitter.Update(deltaTime);
                }
                
                // Manage bubble emitter
                if (_player.IsUnderwater)
                {
                    if (_bubbleEmitter == null)
                    {
                        _bubbleEmitter = new ParticleEmitter(_player.Position + new Vector3(0, 1.7f, 0))
                        {
                            EmissionRate = 3.0f,
                            ParticleLifetime = 2.5f,
                            ParticleSize = 0.08f,
                            ParticleColor = new Color(200, 220, 255, 180),
                            VelocityBase = new Vector3(0, 0.5f, 0),
                            VelocityVariation = new Vector3(0.1f, 0.2f, 0.1f)
                        };
                        _particleEmitters.Add(_bubbleEmitter);
                    }
                    else
                    {
                        _bubbleEmitter.Position = _player.Position + new Vector3(0, 1.7f, 0);
                    }
                }
                else
                {
                    if (_bubbleEmitter != null)
                    {
                        _particleEmitters.Remove(_bubbleEmitter);
                        _bubbleEmitter = null;
                    }
                }
            }
        }

        base.Update(gameTime);
    }
    catch (Exception ex)
    {
        Logger.Error("Error in game update loop", ex);
    }
}
```

## Testing the Integration

### Testing Particle System

1. **Build and run the game**
2. **Start a new game**
3. **Find water** (look for blue blocks at Y=64 sea level)
4. **Walk into water** until the player is submerged
5. **Observe bubbles** rising from the player's mouth

Expected behavior:
- Small blue bubbles appear periodically (3 per second)
- Bubbles float upward slowly
- Bubbles fade out after 2-3 seconds

### Testing Audio System

1. **Add a test sound file** to Content/Sounds/ (when sound files are available)
2. **Uncomment sound loading code** in LoadContent()
3. **Add test sound trigger** (e.g., on block break):
   ```csharp
   _audioManager.PlaySound("block_break", volume: 0.8f);
   ```
4. **Enter water** and verify sounds are muffled

Expected behavior:
- Sounds play normally on land
- Underwater, sounds are quieter and lower pitched
- Transition is immediate when crossing water boundary

### Testing Vegetation System

1. **Start a new game**
2. **Explore the world** looking at grass blocks
3. **Wait for growth** (accelerate time if needed for testing)
4. **Check vegetation data**:
   ```csharp
   int plantCount = _vegetationManager.GetAllPlants().Count();
   Logger.Info($"Total plants: {plantCount}");
   ```

Expected behavior:
- Grass blocks have 15% chance of grass/shrubs
- Plants grow from Seedling → Growing → Mature over 15 minutes
- Growth continues even when far from player

## Performance Considerations

### Particle System
- Limit total particles to ~1000 for performance
- Remove emitters when player is far away
- Use frustum culling for rendering

### Audio System
- Limit concurrent sounds to ~32
- Stop looping sounds when not needed
- Use 3D audio sparingly for performance

### Vegetation System
- Update vegetation in chunks near player only
- Use update intervals (e.g., 0.1s instead of every frame)
- Implement chunk-based vegetation culling

## Troubleshooting

### Particles Not Showing
- Check if `_particleRenderer` is initialized
- Verify particles are being emitted (`emitter.GetParticles().Count()`)
- Ensure particle render call is in correct order (after world, before UI)
- Check particle colors have alpha > 0

### Audio Not Playing
- Verify sound files are in Content project
- Check sound files are built as Content
- Ensure `AudioManager` is initialized
- Check volume levels are > 0

### Vegetation Not Growing
- Verify `VegetationManager.Update()` is being called
- Check `deltaTime` is not 0
- Ensure chunks are populated with vegetation
- Check plants exist: `GetAllPlants().Count()`

## Next Steps

1. **Create sound effect assets** for audio system
2. **Create plant models/sprites** for vegetation rendering
3. **Add more particle effects** (smoke, sparks, dust)
4. **Implement vegetation harvesting** (player interaction)
5. **Add more vegetation types** (flowers, crops, etc.)

---

**Version**: 1.0  
**Last Updated**: 2025-12-12  
**Author**: Timeless Tales Development Team
