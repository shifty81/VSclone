using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.Entities;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Renders underwater visual effects including fog, tinting, and lighting adjustments
    /// </summary>
    public class UnderwaterEffectRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _overlayEffect;
        private VertexPositionColor[] _screenQuad = null!;
        
        // Underwater visual parameters
        private const float MAX_UNDERWATER_TINT_ALPHA = 0.4f; // Maximum overlay opacity when fully submerged
        private const float SHALLOW_WATER_TINT_ALPHA = 0.15f; // Light tint near surface
        
        // Underwater fog color (blue-green tint)
        private readonly Color UNDERWATER_TINT = new Color(20, 80, 120);
        
        public UnderwaterEffectRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            
            // Initialize effect for screen overlay
            _overlayEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                TextureEnabled = false
            };
            
            // Create fullscreen quad vertices
            InitializeScreenQuad();
        }
        
        private void InitializeScreenQuad()
        {
            // Create a fullscreen quad in normalized device coordinates (-1 to 1)
            _screenQuad = new VertexPositionColor[6];
            
            Color tintColor = UNDERWATER_TINT;
            
            // First triangle (top-left to bottom-right)
            _screenQuad[0] = new VertexPositionColor(new Vector3(-1, -1, 0), tintColor);
            _screenQuad[1] = new VertexPositionColor(new Vector3(1, 1, 0), tintColor);
            _screenQuad[2] = new VertexPositionColor(new Vector3(-1, 1, 0), tintColor);
            
            // Second triangle (bottom-left to top-right)
            _screenQuad[3] = new VertexPositionColor(new Vector3(-1, -1, 0), tintColor);
            _screenQuad[4] = new VertexPositionColor(new Vector3(1, -1, 0), tintColor);
            _screenQuad[5] = new VertexPositionColor(new Vector3(1, 1, 0), tintColor);
        }
        
        public void Draw(Player player)
        {
            // Only render if player is in water
            if (!player.IsUnderwater)
                return;
            
            float submersionDepth = player.SubmersionDepth;
            
            // Calculate tint intensity based on submersion depth
            // Shallow water (0-0.3): very light tint
            // Medium depth (0.3-0.7): increasing tint
            // Deep water (0.7-1.0): full tint
            float tintAlpha;
            if (submersionDepth < 0.3f)
            {
                // Very light tint near surface
                tintAlpha = SHALLOW_WATER_TINT_ALPHA * (submersionDepth / 0.3f);
            }
            else
            {
                // Gradually increase tint as we go deeper
                float deepFactor = (submersionDepth - 0.3f) / 0.7f;
                tintAlpha = MathHelper.Lerp(SHALLOW_WATER_TINT_ALPHA, MAX_UNDERWATER_TINT_ALPHA, deepFactor);
            }
            
            // Update quad colors with calculated alpha
            // Clamp alpha to valid range [0, 1] to prevent overflow
            tintAlpha = MathHelper.Clamp(tintAlpha, 0.0f, 1.0f);
            int alphaValue = (int)MathHelper.Clamp(tintAlpha * 255, 0, 255);
            Color underwaterColor = new Color(UNDERWATER_TINT.R, UNDERWATER_TINT.G, UNDERWATER_TINT.B, alphaValue);
            for (int i = 0; i < _screenQuad.Length; i++)
            {
                _screenQuad[i].Color = underwaterColor;
            }
            
            // Setup rendering state for overlay
            var originalBlendState = _graphicsDevice.BlendState;
            var originalDepthStencilState = _graphicsDevice.DepthStencilState;
            var originalRasterizerState = _graphicsDevice.RasterizerState;
            
            // Use alpha blending to overlay the tint
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            // Don't write to or test depth buffer (overlay on top)
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            
            // Set up orthographic projection for screen-space rendering
            _overlayEffect.World = Matrix.Identity;
            _overlayEffect.View = Matrix.Identity;
            _overlayEffect.Projection = Matrix.Identity;
            
            // Draw the fullscreen quad
            foreach (var pass in _overlayEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    _screenQuad,
                    0,
                    2 // 2 triangles = 1 quad
                );
            }
            
            // Restore original render states
            _graphicsDevice.BlendState = originalBlendState;
            _graphicsDevice.DepthStencilState = originalDepthStencilState;
            _graphicsDevice.RasterizerState = originalRasterizerState;
        }
    }
}
