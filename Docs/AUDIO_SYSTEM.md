# Audio System Implementation

## Overview
The audio system manages all sound effects and music in Timeless Tales, with special support for underwater audio filtering and environmental effects.

## Architecture

### Core Component: AudioManager

**Location**: `TimelessTales/Audio/AudioManager.cs`

The `AudioManager` class is responsible for:
- Loading and storing sound effects
- Playing one-shot sounds
- Managing looping ambient sounds
- Applying underwater audio filtering
- Volume and pitch control

## Features

### 1. Basic Sound Playback

#### Loading Sounds
```csharp
// In game initialization:
AudioManager audioManager = new AudioManager();

// Load sound effects (when MonoGame Content is available)
SoundEffect footstepSound = Content.Load<SoundEffect>("Sounds/footstep");
audioManager.LoadSound("footstep", footstepSound);

SoundEffect waterSplash = Content.Load<SoundEffect>("Sounds/water_splash");
audioManager.LoadSound("water_splash", waterSplash);
```

#### Playing Sounds
```csharp
// Play a one-shot sound effect
audioManager.PlaySound("footstep", volume: 0.8f, pitch: 0.0f, pan: 0.0f);

// Play with variation for realism
audioManager.PlaySound("footstep", volume: 0.8f, pitch: Random(-0.1f, 0.1f));
```

### 2. Looping Ambient Sounds

```csharp
// Start ambient sound (e.g., underground cave ambience)
audioManager.PlayLoopingSound("cave_ambience", volume: 0.5f);

// Stop when leaving area
audioManager.StopLoopingSound("cave_ambience");

// Stop all looping sounds
audioManager.StopAllLoopingSounds();
```

### 3. Underwater Audio Effect

The audio system automatically applies a low-pass filter effect when the player is underwater:

```csharp
// In player update logic:
if (player.IsUnderwater)
{
    audioManager.IsUnderwater = true;
}
else
{
    audioManager.IsUnderwater = false;
}
```

**Underwater Effect Parameters**:
- **Volume Reduction**: 60% of normal volume (muffled sound)
- **Pitch Shift**: -0.3 (lower pitch simulates underwater acoustics)
- **Applies to**: All sounds (one-shot and looping)
- **Transition**: Immediate when entering/exiting water

### 4. Volume Control

```csharp
// Master volume (affects all audio)
audioManager.MasterVolume = 0.8f; // 80% volume

// Sound effect volume (multiplied with master)
audioManager.SoundEffectVolume = 1.0f; // 100%

// Effective volume = volume * SoundEffectVolume * MasterVolume
// If underwater: effectiveVolume *= 0.6
```

## Planned Sound Effects

### Environmental Sounds

| Sound | Category | Description | Trigger |
|-------|----------|-------------|---------|
| `footstep_grass` | Movement | Soft grass rustling | Walking on grass blocks |
| `footstep_stone` | Movement | Hard stone steps | Walking on stone blocks |
| `footstep_wood` | Movement | Wooden plank creaks | Walking on wood blocks |
| `footstep_sand` | Movement | Sand crunching | Walking on sand blocks |
| `water_splash` | Water | Splash sound | Entering water |
| `water_swim` | Water | Swimming strokes | Moving in water |
| `underwater_ambience` | Ambient | Muffled underwater sounds | While submerged |
| `bubble_pop` | Particle | Small bubble burst | Bubble reaching surface |

### Block Interaction Sounds

| Sound | Description | Trigger |
|-------|-------------|---------|
| `block_break_stone` | Stone breaking | Breaking stone blocks |
| `block_break_dirt` | Dirt/soil breaking | Breaking dirt/grass |
| `block_break_wood` | Wood breaking | Breaking wood blocks |
| `block_place` | Generic placement | Placing any block |
| `ore_break` | Ore-specific sound | Breaking ore blocks |

### Tool Sounds

| Sound | Description | Trigger |
|-------|-------------|---------|
| `pickaxe_swing` | Pickaxe swinging | Using pickaxe |
| `axe_chop` | Axe chopping wood | Using axe on trees |
| `shovel_dig` | Shovel digging | Using shovel |

### Ambient Environment

| Sound | Description | When |
|-------|-------------|------|
| `wind_gentle` | Light wind | Daytime, surface |
| `wind_strong` | Strong wind | Storms, high altitude |
| `cave_drips` | Water dripping | In caves |
| `night_crickets` | Cricket sounds | Nighttime |
| `ocean_waves` | Wave sounds | Near ocean |

## Implementation Details

### Underwater Audio Filtering

The underwater effect simulates how sound behaves underwater by:

1. **Reducing Volume**: High-frequency sounds are absorbed more underwater
2. **Lowering Pitch**: Sound travels differently in water, creating a deeper tone
3. **Smooth Transition**: Applied/removed immediately when crossing water boundary

**Technical Implementation**:
```csharp
private void ApplyUnderwaterEffect()
{
    foreach (var instance in _loopingSounds.Values)
    {
        if (_isUnderwater)
        {
            instance.Volume *= UNDERWATER_VOLUME_MULTIPLIER; // 0.6
            instance.Pitch = UNDERWATER_PITCH_SHIFT;        // -0.3
        }
        else
        {
            instance.Volume = SoundEffectVolume * MasterVolume;
            instance.Pitch = 0.0f;
        }
    }
}
```

### 3D Spatial Audio (Future Enhancement)

Currently, the audio system supports basic pan control (-1 to +1 for left/right).

**Future 3D audio features**:
- Calculate pan based on sound source position relative to camera
- Distance-based volume attenuation
- Doppler effect for moving sound sources
- Environmental reverb (caves, open spaces)

## Usage Examples

### Complete Integration Example

```csharp
public class TimelessTalesGame : Game
{
    private AudioManager _audioManager;
    
    protected override void Initialize()
    {
        _audioManager = new AudioManager();
        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        // Load sound effects
        _audioManager.LoadSound("footstep", Content.Load<SoundEffect>("Sounds/footstep"));
        _audioManager.LoadSound("splash", Content.Load<SoundEffect>("Sounds/water_splash"));
        _audioManager.LoadSound("underwater", Content.Load<SoundEffect>("Sounds/underwater_ambience"));
    }
    
    protected override void Update(GameTime gameTime)
    {
        // Update audio system
        _audioManager.Update();
        
        // Set underwater state based on player
        _audioManager.IsUnderwater = player.IsUnderwater;
        
        // Play sounds based on player actions
        if (player.IsWalking)
        {
            PlayFootstepSound();
        }
        
        if (player.JustEnteredWater)
        {
            _audioManager.PlaySound("splash", volume: 1.0f);
            _audioManager.PlayLoopingSound("underwater", volume: 0.3f);
        }
        
        if (player.JustExitedWater)
        {
            _audioManager.StopLoopingSound("underwater");
        }
    }
    
    private void PlayFootstepSound()
    {
        // Vary pitch slightly for realism
        float pitch = (float)(random.NextDouble() * 0.2 - 0.1);
        _audioManager.PlaySound("footstep", volume: 0.7f, pitch: pitch);
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _audioManager?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

### Bubble Sound Effect

```csharp
// When bubble particle pops at water surface:
public void OnBubblePop(Vector3 bubblePosition)
{
    // Calculate pan based on position relative to camera
    float pan = CalculatePan(bubblePosition, camera.Position);
    
    // Play bubble pop with slight pitch variation
    float pitch = (float)(random.NextDouble() * 0.3 - 0.15);
    _audioManager.PlaySound("bubble_pop", volume: 0.5f, pitch: pitch, pan: pan);
}

private float CalculatePan(Vector3 soundPos, Vector3 listenerPos)
{
    Vector3 toSound = soundPos - listenerPos;
    Vector3 cameraRight = camera.ViewMatrix.Right;
    float pan = Vector3.Dot(toSound, cameraRight);
    return MathHelper.Clamp(pan, -1.0f, 1.0f);
}
```

## Future Enhancements

### Short-term
1. **Sound Effect Library**: Create/acquire all planned sound effects
2. **3D Spatial Audio**: Distance and direction-based sound
3. **Sound Categories**: Separate volume controls for music, SFX, ambient
4. **Audio Fade**: Smooth volume transitions

### Medium-term
1. **Music System**: Background music with dynamic transitions
2. **Environmental Reverb**: Cave echo, outdoor open sound
3. **Weather Sounds**: Rain, thunder, wind based on weather system
4. **Biome-specific Ambience**: Different sounds per biome type

### Long-term
1. **Real-time Audio DSP**: Advanced filtering beyond pitch/volume
2. **Adaptive Music**: Music changes based on gameplay events
3. **HRTF 3D Audio**: Head-related transfer function for better 3D positioning
4. **Voice/Dialog System**: Character voices and narration

## Performance Considerations

### Resource Management
- **Sound Effect Instances**: Limit concurrent sounds to prevent audio clipping
- **Looping Sounds**: Stop unused ambient sounds to save resources
- **Memory**: Unload unused sound effects in different game areas

### MonoGame Audio Limits
- **Concurrent Sounds**: MonoGame can handle many simultaneous sounds, but limit to ~32 for performance
- **Audio Formats**: Use .wav for short effects, .ogg for music/long ambience
- **Sample Rates**: 44.1kHz recommended for quality/performance balance

## Technical Notes

### MonoGame Audio Components
- **SoundEffect**: Loaded from .xnb content files
- **SoundEffectInstance**: For advanced control (looping, real-time changes)
- **DynamicSoundEffectInstance**: For procedural audio generation

### Thread Safety
- MonoGame audio calls should be made on the main game thread
- Audio updates occur in the main Update() loop

---

**Version**: 1.0  
**Last Updated**: 2025-12-12  
**Author**: Timeless Tales Development Team
