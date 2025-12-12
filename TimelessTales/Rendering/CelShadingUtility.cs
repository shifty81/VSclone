using Microsoft.Xna.Framework;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Utility class for cel shading (toon shading) effects
    /// </summary>
    public static class CelShadingUtility
    {
        /// <summary>
        /// Applies cel shading to a color by quantizing the RGB values into discrete bands
        /// </summary>
        /// <param name="color">The color to apply cel shading to</param>
        /// <param name="bands">Number of discrete color bands (typically 2-8)</param>
        /// <returns>Cel shaded color</returns>
        public static Color ApplyCelShading(Color color, int bands)
        {
            // Quantize each color channel to create cel shading effect
            int r = QuantizeColorChannel(color.R, bands);
            int g = QuantizeColorChannel(color.G, bands);
            int b = QuantizeColorChannel(color.B, bands);
            
            return new Color(r, g, b, color.A);
        }
        
        /// <summary>
        /// Quantizes a color channel (0-255) into discrete bands for cel shading
        /// </summary>
        /// <param name="value">Color channel value (0-255)</param>
        /// <param name="bands">Number of discrete bands</param>
        /// <returns>Quantized color channel value</returns>
        public static int QuantizeColorChannel(int value, int bands)
        {
            float normalized = value / 255.0f;
            float bandSize = 1.0f / bands;
            float bandIndex = MathF.Floor(normalized / bandSize);
            
            // Clamp bandIndex to prevent exceeding bounds when normalized == 1.0
            bandIndex = MathHelper.Clamp(bandIndex, 0, bands - 1);
            
            // Return the center of the band for smoother appearance
            float quantized = (bandIndex + 0.5f) * bandSize;
            return (int)MathHelper.Clamp(quantized * 255, 0, 255);
        }
        
        /// <summary>
        /// Quantizes a value (0-1) into N discrete bands for cel shading effect
        /// </summary>
        /// <param name="value">Value to quantize (0-1 range)</param>
        /// <param name="bands">Number of discrete bands</param>
        /// <returns>Quantized value in 0-1 range</returns>
        public static float QuantizeToNBands(float value, int bands)
        {
            // Clamp input to 0-1 range
            value = MathHelper.Clamp(value, 0, 1);
            
            // Quantize to discrete bands
            float bandSize = 1.0f / bands;
            float bandIndex = MathF.Floor(value / bandSize);
            
            // Clamp bandIndex to prevent exceeding bounds when value == 1.0
            bandIndex = MathHelper.Clamp(bandIndex, 0, bands - 1);
            
            // Return the center of the band for smoother appearance
            return (bandIndex + 0.5f) * bandSize;
        }
    }
}
