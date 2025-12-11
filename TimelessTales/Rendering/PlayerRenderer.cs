using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.Entities;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Renders the player's arms in first-person view
    /// </summary>
    public class PlayerRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _effect;

        public PlayerRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            
            // Initialize basic effect for rendering
            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        public void Draw(Camera camera, Player player)
        {
            // Update camera matrices
            _effect.View = camera.ViewMatrix;
            _effect.Projection = camera.ProjectionMatrix;
            
            // Set render states
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            // Build arm vertices
            var vertices = BuildArmVertices(camera);
            
            if (vertices.Length > 0)
            {
                _effect.World = Matrix.Identity;
                
                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _graphicsDevice.DrawUserPrimitives(
                        PrimitiveType.TriangleList,
                        vertices,
                        0,
                        vertices.Length / 3
                    );
                }
            }
        }

        private VertexPositionColor[] BuildArmVertices(Camera camera)
        {
            var vertices = new List<VertexPositionColor>();
            
            // Arm color (tan/skin color)
            Color armColor = new Color(210, 180, 140);
            
            // Calculate arm positions relative to camera
            Vector3 cameraPos = camera.Position;
            Vector3 forward = camera.GetForwardVector();
            Vector3 right = camera.GetRightVector();
            Vector3 up = Vector3.Up;
            
            // Right arm dimensions (sized to be visible but not obtrusive)
            float armLength = 0.5f;
            float armWidth = 0.12f;
            float armHeight = 0.12f;
            
            // Position right arm in view
            // Offset from camera: right, down, and forward
            Vector3 armBase = cameraPos 
                + right * 0.35f      // Move to the right
                - up * 0.25f         // Move down
                + forward * 0.4f;    // Move forward
            
            // Create a simple rectangular arm
            AddBox(vertices, armBase, armWidth, armHeight, armLength, armColor, forward, right, up);
            
            // Left arm (optional, mirror of right arm)
            Vector3 leftArmBase = cameraPos 
                - right * 0.35f      // Move to the left
                - up * 0.25f         // Move down
                + forward * 0.4f;    // Move forward
            
            AddBox(vertices, leftArmBase, armWidth, armHeight, armLength, armColor, forward, right, up);
            
            return vertices.ToArray();
        }

        private void AddBox(List<VertexPositionColor> vertices, Vector3 basePos, 
                           float width, float height, float length, Color color,
                           Vector3 forward, Vector3 right, Vector3 up)
        {
            // Calculate box corners relative to base position
            // The box extends along the forward direction
            Vector3 halfWidth = right * (width / 2);
            Vector3 halfHeight = up * (height / 2);
            Vector3 fullLength = forward * length;
            
            // Define 8 corners of the box
            Vector3 c0 = basePos - halfWidth - halfHeight;           // Back bottom left
            Vector3 c1 = basePos + halfWidth - halfHeight;           // Back bottom right
            Vector3 c2 = basePos - halfWidth + halfHeight;           // Back top left
            Vector3 c3 = basePos + halfWidth + halfHeight;           // Back top right
            Vector3 c4 = basePos - halfWidth - halfHeight + fullLength; // Front bottom left
            Vector3 c5 = basePos + halfWidth - halfHeight + fullLength; // Front bottom right
            Vector3 c6 = basePos - halfWidth + halfHeight + fullLength; // Front top left
            Vector3 c7 = basePos + halfWidth + halfHeight + fullLength; // Front top right
            
            // Add slightly darker colors for different faces
            Color topColor = Color.Lerp(color, Color.White, 0.2f);
            Color bottomColor = Color.Lerp(color, Color.Black, 0.2f);
            Color sideColor = Color.Lerp(color, Color.Black, 0.1f);
            
            // Front face
            AddQuad(vertices, c4, c5, c7, c6, color);
            
            // Back face
            AddQuad(vertices, c1, c0, c2, c3, color);
            
            // Top face
            AddQuad(vertices, c2, c3, c7, c6, topColor);
            
            // Bottom face
            AddQuad(vertices, c0, c1, c5, c4, bottomColor);
            
            // Right face
            AddQuad(vertices, c5, c1, c3, c7, sideColor);
            
            // Left face
            AddQuad(vertices, c0, c4, c6, c2, sideColor);
        }

        private void AddQuad(List<VertexPositionColor> vertices,
                            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color color)
        {
            // First triangle
            vertices.Add(new VertexPositionColor(v1, color));
            vertices.Add(new VertexPositionColor(v2, color));
            vertices.Add(new VertexPositionColor(v3, color));
            
            // Second triangle
            vertices.Add(new VertexPositionColor(v3, color));
            vertices.Add(new VertexPositionColor(v4, color));
            vertices.Add(new VertexPositionColor(v1, color));
        }
    }
}
