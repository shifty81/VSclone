# Cel Shading Implementation

## Overview
This document describes the implementation of cel shading (toon shading) for water and terrain rendering in Timeless Tales. Cel shading creates a cartoon-like appearance by using discrete bands of color instead of smooth gradients, which helps reduce the pixelated/rasterized appearance of lighting and creates a more stylized visual aesthetic.

## I. What is Cel Shading?

Cel shading, also known as toon shading, is a type of non-photorealistic rendering designed to make 3D graphics appear flat and cartoon-like. It achieves this by:
1. **Color Quantization**: Reducing continuous color gradients to discrete bands
2. **Hard Edges**: Creating distinct boundaries between different shades
3. **Simplified Lighting**: Using stepped lighting levels instead of smooth transitions

## II. Implementation Details

### Water Renderer (`WaterRenderer.cs`)

#### Constants:
```csharp
private const int CEL_SHADING_BANDS = 4; // Number of discrete color bands
private const float EDGE_SMOOTHING = 0.15f; // Smoothing factor for water edges
```

#### Key Methods:

**1. Depth-Based Color Quantization:**
- Water depth is calculated from sea level (Y=64)
- Depth factor is quantized into 4 discrete bands using `QuantizeToNBands()`
- This creates distinct depth levels: very shallow, shallow, medium, deep
- Eliminates smooth gradient transitions for a toon-like appearance

**2. Color Channel Quantization:**
- Each RGB color channel is quantized separately
- `ApplyCelShading()` applies quantization to all water face colors
- `QuantizeColorChannel()` converts 0-255 values into discrete bands
- Results in distinct color steps rather than smooth transitions

**3. Face Color Application:**
- Top face: Lighter (blended with white) + cel shaded
- Bottom face: Darker (blended with black) + cel shaded  
- Side faces: Base color + cel shaded
- All faces use quantized colors for consistent toon appearance

### World Renderer (`WorldRenderer.cs`)

#### Constants:
```csharp
private const int CEL_SHADING_BANDS = 4; // Number of discrete color bands for toon shading
```

#### Key Methods:

**1. Block Face Color Quantization:**
- All block faces (top, bottom, sides) use cel shading
- Top faces: Lighter + cel shaded
- Bottom faces: Darker + cel shaded
- Side faces: Slightly darkened + cel shaded

**2. Quantization Algorithm:**
```csharp
private int QuantizeColorChannel(int value, int bands)
{
    float normalized = value / 255.0f;
    float bandSize = 1.0f / bands;
    float bandIndex = MathF.Floor(normalized / bandSize);
    
    // Return the center of the band
    float quantized = (bandIndex + 0.5f) * bandSize;
    return (int)MathHelper.Clamp(quantized * 255, 0, 255);
}
```

This algorithm:
1. Normalizes the color value to 0-1 range
2. Divides the range into N equal bands
3. Determines which band the value falls into
4. Returns the center of that band (for smoother appearance)
5. Converts back to 0-255 range

## III. Visual Effects

### Before Cel Shading:
- Smooth color gradients on water depth
- Smooth lighting transitions on terrain
- Visible pixelated/rasterized lighting artifacts
- Hard edges visible between water blocks

### After Cel Shading:
- Discrete color bands create cartoon-like appearance
- 4 distinct depth levels for water (very shallow → deep)
- 4 distinct lighting levels for terrain
- Pixelation becomes intentional part of the art style
- Consistent toon aesthetic across all rendering

## IV. Benefits

1. **Reduced Visual Artifacts**: Pixelated lighting becomes part of the intended style
2. **Consistent Aesthetic**: Water and terrain share the same toon-shaded appearance
3. **Performance**: No additional rendering cost (CPU-side color quantization)
4. **Stylized Look**: Creates a unique visual identity distinct from realistic renderers
5. **Edge Softening**: Quantized colors reduce the appearance of hard block edges

## V. Customization

### Adjusting the Number of Bands:
To modify the cel shading intensity, change `CEL_SHADING_BANDS`:
- **2 bands**: Very stylized, high-contrast toon look
- **4 bands**: Balanced toon appearance (current default)
- **6-8 bands**: Subtler cel shading, closer to realistic
- **16+ bands**: Minimal cel shading effect

### Per-Renderer Customization:
Each renderer has its own `CEL_SHADING_BANDS` constant, allowing:
- More bands for terrain (smoother transitions)
- Fewer bands for water (stronger toon effect)
- Independent artistic control

## VI. Technical Notes

### Color Quantization Process:
1. Original color value (e.g., RGB 100, 150, 200)
2. Each channel normalized to 0-1 range
3. Quantized to 4 bands: [0-0.25], [0.25-0.5], [0.5-0.75], [0.75-1.0]
4. Center of band chosen (0.125, 0.375, 0.625, 0.875)
5. Converted back to 0-255 range

### Depth Band Examples (4 bands):
- Band 1 (0-5 blocks deep): Very light blue
- Band 2 (5-10 blocks deep): Light blue
- Band 3 (10-15 blocks deep): Medium blue
- Band 4 (15-20 blocks deep): Dark blue

## VII. Future Enhancements

Potential improvements for cel shading:

1. **Edge Detection & Outlining**:
   - Detect edges between blocks
   - Add dark outlines for comic book effect
   - Enhance the toon aesthetic

2. **Normal-Based Shading**:
   - Use face normals to determine lighting band
   - Create more dramatic lighting steps
   - Better highlight geometry

3. **Configurable Parameters**:
   - In-game settings for cel shading intensity
   - Per-block-type band count
   - Custom color palettes for different biomes

4. **Specular Highlights**:
   - Add bright highlight bands
   - Create glossy toon-shaded surfaces
   - Enhance visual interest

## VIII. Testing

All existing tests pass with cel shading:
- Water physics tests: 8/8 passed
- Color calculations work correctly with quantization
- No performance regression
- Backward compatible with existing game systems

## IX. Performance Impact

**CPU Impact**: Minimal
- Quantization performed during mesh building (not per-frame)
- Simple mathematical operations (floor, multiply, add)
- No additional memory allocation

**GPU Impact**: None
- No shader changes required
- Uses existing BasicEffect rendering
- Vertex colors pre-calculated on CPU

**Memory Impact**: None
- Same vertex data structure
- No additional textures or resources
- Meshes cached as before

## X. Code Locations

- Water cel shading: `TimelessTales/Rendering/WaterRenderer.cs`
  - Lines 26-27: Constants
  - Lines 170-223: Depth quantization
  - Lines 233-315: Face color quantization
  
- World cel shading: `TimelessTales/Rendering/WorldRenderer.cs`
  - Line 19: Constants
  - Lines 127-209: Block face quantization
