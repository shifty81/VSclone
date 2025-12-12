using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TimelessTales.Rendering;

namespace TimelessTales.Particles
{
    /// <summary>
    /// Renders particles with billboarding (always face camera)
    /// </summary>
    public class ParticleRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _effect;
        
        public ParticleRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                TextureEnabled = false
            };
        }
        
        public void Draw(Camera camera, IEnumerable<ParticleEmitter> emitters)
        {
            // Update camera matrices
            _effect.View = camera.ViewMatrix;
            _effect.Projection = camera.ProjectionMatrix;
            _effect.World = Matrix.Identity;
            
            // Set render states for transparent particles
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead; // Don't write to depth buffer
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            
            var vertices = new List<VertexPositionColor>();
            
            // Collect all particles from all emitters
            foreach (var emitter in emitters)
            {
                foreach (var particle in emitter.GetParticles())
                {
                    AddParticleBillboard(vertices, particle, camera);
                }
            }
            
            // Draw all particles in one batch
            if (vertices.Count > 0)
            {
                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawUserPrimitives(
                        PrimitiveType.TriangleList,
                        vertices.ToArray(),
                        0,
                        vertices.Count / 3
                    );
                }
            }
            
            // Restore default render states
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
        
        private void AddParticleBillboard(List<VertexPositionColor> vertices, Particle particle, Camera camera)
        {
            // Billboard: quad that always faces the camera
            Vector3 up = camera.ViewMatrix.Up;
            Vector3 right = camera.ViewMatrix.Right;
            
            float halfSize = particle.Size / 2;
            
            // Calculate color with alpha fade
            Color color = particle.Color;
            color.A = (byte)(particle.Alpha * 255);
            
            // Create quad vertices (two triangles)
            Vector3 topLeft = particle.Position + (up * halfSize) - (right * halfSize);
            Vector3 topRight = particle.Position + (up * halfSize) + (right * halfSize);
            Vector3 bottomLeft = particle.Position - (up * halfSize) - (right * halfSize);
            Vector3 bottomRight = particle.Position - (up * halfSize) + (right * halfSize);
            
            // First triangle
            vertices.Add(new VertexPositionColor(bottomLeft, color));
            vertices.Add(new VertexPositionColor(topLeft, color));
            vertices.Add(new VertexPositionColor(topRight, color));
            
            // Second triangle
            vertices.Add(new VertexPositionColor(topRight, color));
            vertices.Add(new VertexPositionColor(bottomRight, color));
            vertices.Add(new VertexPositionColor(bottomLeft, color));
        }
    }
}
