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
        private VertexPositionColor[] _vertexBuffer;
        private const int MAX_PARTICLES = 1000; // Maximum particles to render at once
        
        public ParticleRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                TextureEnabled = false
            };
            
            // Pre-allocate vertex buffer (6 vertices per particle quad)
            _vertexBuffer = new VertexPositionColor[MAX_PARTICLES * 6];
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
            
            int vertexIndex = 0;
            
            // Collect all particles from all emitters into pre-allocated buffer
            foreach (var emitter in emitters)
            {
                foreach (var particle in emitter.GetParticles())
                {
                    // Check buffer capacity
                    if (vertexIndex + 6 > _vertexBuffer.Length)
                        break; // Buffer full, stop adding particles
                    
                    AddParticleBillboard(camera, particle, ref vertexIndex);
                }
                
                if (vertexIndex + 6 > _vertexBuffer.Length)
                    break; // Buffer full
            }
            
            // Draw all particles in one batch if we have any
            if (vertexIndex > 0)
            {
                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawUserPrimitives(
                        PrimitiveType.TriangleList,
                        _vertexBuffer,
                        0,
                        vertexIndex / 3
                    );
                }
            }
            
            // Restore default render states
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }
        
        private void AddParticleBillboard(Camera camera, Particle particle, ref int vertexIndex)
        {
            // Billboard: quad that always faces the camera
            // Calculate right and up vectors from camera's forward direction
            Vector3 cameraForward = camera.ViewMatrix.Forward;
            Vector3 worldUp = Vector3.Up;
            
            // Right vector perpendicular to camera forward and world up
            Vector3 right = Vector3.Cross(worldUp, cameraForward);
            right.Normalize();
            
            // Up vector perpendicular to camera forward and right
            Vector3 up = Vector3.Cross(cameraForward, right);
            up.Normalize();
            
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
            _vertexBuffer[vertexIndex++] = new VertexPositionColor(bottomLeft, color);
            _vertexBuffer[vertexIndex++] = new VertexPositionColor(topLeft, color);
            _vertexBuffer[vertexIndex++] = new VertexPositionColor(topRight, color);
            
            // Second triangle
            _vertexBuffer[vertexIndex++] = new VertexPositionColor(topRight, color);
            _vertexBuffer[vertexIndex++] = new VertexPositionColor(bottomRight, color);
            _vertexBuffer[vertexIndex++] = new VertexPositionColor(bottomLeft, color);
        }
    }
}
