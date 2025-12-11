using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.Core;
using System;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Renders the skybox with day/night cycle, sun, moon, and stars
    /// </summary>
    public class SkyboxRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _effect;
        private readonly Random _random;
        
        // Star field
        private VertexPositionColor[] _stars = Array.Empty<VertexPositionColor>();
        private const int STAR_COUNT = 500;
        
        public SkyboxRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _random = new Random(12345);
            
            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
            
            GenerateStars();
        }
        
        private void GenerateStars()
        {
            _stars = new VertexPositionColor[STAR_COUNT];
            
            for (int i = 0; i < STAR_COUNT; i++)
            {
                // Generate random point on sphere
                float theta = (float)(_random.NextDouble() * Math.PI * 2); // Azimuth
                float phi = (float)(Math.Acos(2 * _random.NextDouble() - 1)); // Inclination
                
                float radius = 100f; // Distance from center
                
                Vector3 position = new Vector3(
                    radius * MathF.Sin(phi) * MathF.Cos(theta),
                    radius * MathF.Sin(phi) * MathF.Sin(theta),
                    radius * MathF.Cos(phi)
                );
                
                // Random star brightness
                float brightness = 0.5f + (float)_random.NextDouble() * 0.5f;
                Color color = Color.White * brightness;
                
                _stars[i] = new VertexPositionColor(position, color);
            }
        }
        
        public void Draw(Camera camera, TimeManager timeManager)
        {
            _effect.View = camera.ViewMatrix;
            _effect.Projection = camera.ProjectionMatrix;
            _effect.World = Matrix.Identity;
            
            // Disable depth writing for skybox (it's infinitely far away)
            var oldDepthState = _graphicsDevice.DepthStencilState;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            
            // Draw sky dome
            DrawSkyDome(camera, timeManager);
            
            // Draw stars (only visible at night)
            if (timeManager.IsNighttime)
            {
                DrawStars(camera, timeManager);
            }
            
            // Draw sun
            DrawSun(camera, timeManager);
            
            // Draw moon
            DrawMoon(camera, timeManager);
            
            // Restore depth state
            _graphicsDevice.DepthStencilState = oldDepthState;
        }
        
        private void DrawSkyDome(Camera camera, TimeManager timeManager)
        {
            // Create sky dome vertices
            var vertices = new VertexPositionColor[6]; // Two triangles for each cardinal direction
            
            Color skyColor = timeManager.GetSkyColor();
            Color horizonColor = timeManager.GetHorizonColor();
            
            float size = 150f;
            
            // Top hemisphere (sky color at top, horizon at bottom)
            // We'll draw multiple quads to create a dome-like appearance
            
            // North
            DrawSkyQuad(Vector3.Backward * size, Vector3.Right, Vector3.Up, size, skyColor, horizonColor);
            // South
            DrawSkyQuad(Vector3.Forward * size, Vector3.Left, Vector3.Up, size, skyColor, horizonColor);
            // East
            DrawSkyQuad(Vector3.Right * size, Vector3.Forward, Vector3.Up, size, skyColor, horizonColor);
            // West
            DrawSkyQuad(Vector3.Left * size, Vector3.Backward, Vector3.Up, size, skyColor, horizonColor);
            // Top
            DrawSkyQuad(Vector3.Up * size, Vector3.Right, Vector3.Forward, size, skyColor, skyColor);
        }
        
        private void DrawSkyQuad(Vector3 center, Vector3 right, Vector3 up, float size, Color topColor, Color bottomColor)
        {
            Vector3 halfRight = right * size;
            Vector3 halfUp = up * size;
            
            var vertices = new VertexPositionColor[6];
            
            // Determine if this is a side quad or top quad
            bool isSideQuad = up.Y > 0.5f;
            
            // First triangle
            vertices[0] = new VertexPositionColor(center - halfRight - halfUp, isSideQuad ? bottomColor : topColor);
            vertices[1] = new VertexPositionColor(center - halfRight + halfUp, topColor);
            vertices[2] = new VertexPositionColor(center + halfRight + halfUp, topColor);
            
            // Second triangle
            vertices[3] = new VertexPositionColor(center + halfRight + halfUp, topColor);
            vertices[4] = new VertexPositionColor(center + halfRight - halfUp, isSideQuad ? bottomColor : topColor);
            vertices[5] = new VertexPositionColor(center - halfRight - halfUp, isSideQuad ? bottomColor : topColor);
            
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    2
                );
            }
        }
        
        private void DrawStars(Camera camera, TimeManager timeManager)
        {
            // Calculate star visibility based on time of day
            float starAlpha = 0f;
            float time = timeManager.TimeOfDay;
            
            if (time < 0.2f)
            {
                starAlpha = 1.0f;
            }
            else if (time < 0.3f)
            {
                // Fade out during dawn
                starAlpha = 1.0f - ((time - 0.2f) / 0.1f);
            }
            else if (time > 0.8f)
            {
                starAlpha = 1.0f;
            }
            else if (time > 0.7f)
            {
                // Fade in during dusk
                starAlpha = (time - 0.7f) / 0.1f;
            }
            
            if (starAlpha <= 0) return;
            
            // Apply alpha to stars
            var visibleStars = new VertexPositionColor[_stars.Length];
            for (int i = 0; i < _stars.Length; i++)
            {
                Color starColor = _stars[i].Color * starAlpha;
                visibleStars[i] = new VertexPositionColor(_stars[i].Position, starColor);
            }
            
            // Rotate stars slightly with time to create twinkling effect
            float twinkleRotation = (float)Math.Sin(timeManager.TimeOfDay * 100) * 0.1f;
            _effect.World = Matrix.CreateRotationY(twinkleRotation);
            
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(
                    PrimitiveType.PointList,
                    visibleStars,
                    0,
                    _stars.Length
                );
            }
            
            _effect.World = Matrix.Identity;
        }
        
        private void DrawSun(Camera camera, TimeManager timeManager)
        {
            float sunAngle = timeManager.GetSunAngle();
            float sunSize = 8f;
            
            // Calculate sun position on the sky dome
            Vector3 sunPosition = new Vector3(
                MathF.Cos(sunAngle) * 100f,
                MathF.Sin(sunAngle) * 100f,
                0f
            );
            
            // Only draw if sun is above horizon
            if (sunPosition.Y > -10f)
            {
                Color sunColor = Color.Yellow;
                
                // During sunrise/sunset, make sun more orange/red
                float time = timeManager.TimeOfDay;
                if ((time >= 0.2f && time < 0.3f) || (time >= 0.7f && time < 0.8f))
                {
                    sunColor = Color.Orange;
                }
                
                DrawCelestialBody(sunPosition, sunSize, sunColor);
            }
        }
        
        private void DrawMoon(Camera camera, TimeManager timeManager)
        {
            float moonAngle = timeManager.GetMoonAngle();
            float moonSize = 6f;
            
            // Calculate moon position on the sky dome
            Vector3 moonPosition = new Vector3(
                MathF.Cos(moonAngle) * 100f,
                MathF.Sin(moonAngle) * 100f,
                0f
            );
            
            // Only draw if moon is above horizon
            if (moonPosition.Y > -10f)
            {
                Color moonColor = new Color(220, 220, 240);
                DrawCelestialBody(moonPosition, moonSize, moonColor);
            }
        }
        
        private void DrawCelestialBody(Vector3 position, float size, Color color)
        {
            // Draw as a simple billboard quad
            Vector3 right = Vector3.Right * size;
            Vector3 up = Vector3.Up * size;
            
            var vertices = new VertexPositionColor[6];
            
            // First triangle
            vertices[0] = new VertexPositionColor(position - right - up, color);
            vertices[1] = new VertexPositionColor(position - right + up, color);
            vertices[2] = new VertexPositionColor(position + right + up, color);
            
            // Second triangle
            vertices[3] = new VertexPositionColor(position + right + up, color);
            vertices[4] = new VertexPositionColor(position + right - up, color);
            vertices[5] = new VertexPositionColor(position - right - up, color);
            
            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    2
                );
            }
        }
    }
}
