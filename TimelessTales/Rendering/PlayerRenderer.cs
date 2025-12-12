using System.Collections.Generic;
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
        
        // Body visibility thresholds (in radians)
        private const float BODY_VISIBILITY_THRESHOLD = 0.3f;  // ~17 degrees
        private const float LEG_VISIBILITY_THRESHOLD = 0.8f;   // ~46 degrees
        
        // Visibility fade-in ranges (in radians)
        private const float BODY_FADE_RANGE = 0.5f;  // How quickly body fades in
        private const float LEG_FADE_RANGE = 0.5f;   // How quickly legs fade in
        
        // Visibility cutoff percentages
        private const float BODY_MIN_VISIBILITY = 0.3f;  // Show body when visibility > 30%
        private const float LEG_MIN_VISIBILITY = 0.2f;   // Show legs when visibility > 20%

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
            // Use skeleton bones for rendering with proper transformations
            
            // Update camera matrices
            _effect.View = camera.ViewMatrix;
            _effect.Projection = camera.ProjectionMatrix;
            
            // Set render states
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            // Build arm vertices using skeleton data
            var vertices = BuildSkeletonVertices(camera, player);
            
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

        private VertexPositionColor[] BuildSkeletonVertices(Camera camera, Player player)
        {
            var vertices = new List<VertexPositionColor>();
            
            // Colors for different body parts
            Color armColor = new Color(210, 180, 140);
            Color torsoColor = new Color(60, 100, 180); // Blue shirt
            Color legColor = new Color(50, 50, 120); // Dark blue pants
            
            // Calculate view directions
            Vector3 cameraPos = camera.Position;
            Vector3 forward = camera.GetForwardVector();
            Vector3 right = camera.GetRightVector();
            Vector3 up = Vector3.Up;
            float pitch = camera.Rotation.X;
            
            // Get skeleton bones
            var skeleton = player.Skeleton;
            Bone? rightArmBone = skeleton.GetBone("right_arm");
            Bone? leftArmBone = skeleton.GetBone("left_arm");
            Bone? torsoBone = skeleton.GetBone("torso");
            Bone? rightLegBone = skeleton.GetBone("right_leg");
            Bone? leftLegBone = skeleton.GetBone("left_leg");
            
            // Arm dimensions
            float armLength = 0.5f;
            float armWidth = 0.12f;
            float armHeight = 0.12f;
            
            // Draw right arm with bone rotation
            if (rightArmBone != null)
            {
                Vector3 armBase = cameraPos + right * 0.35f - up * 0.25f + forward * 0.4f;
                Matrix armRotation = Matrix.CreateRotationX(rightArmBone.LocalRotation.X) *
                                   Matrix.CreateRotationY(rightArmBone.LocalRotation.Y) *
                                   Matrix.CreateRotationZ(rightArmBone.LocalRotation.Z);
                AddBoxWithRotation(vertices, armBase, armWidth, armHeight, armLength, armColor, forward, right, up, armRotation);
            }
            
            // Draw left arm with bone rotation
            if (leftArmBone != null)
            {
                Vector3 leftArmBase = cameraPos - right * 0.35f - up * 0.25f + forward * 0.4f;
                Matrix armRotation = Matrix.CreateRotationX(leftArmBone.LocalRotation.X) *
                                   Matrix.CreateRotationY(leftArmBone.LocalRotation.Y) *
                                   Matrix.CreateRotationZ(leftArmBone.LocalRotation.Z);
                AddBoxWithRotation(vertices, leftArmBase, armWidth, armHeight, armLength, armColor, forward, right, up, armRotation);
            }
            
            // Add body parts when looking down
            if (pitch > BODY_VISIBILITY_THRESHOLD)
            {
                float bodyVisibility = MathHelper.Clamp((pitch - BODY_VISIBILITY_THRESHOLD) / BODY_FADE_RANGE, 0f, 1f);
                
                // Torso with animation
                if (bodyVisibility > BODY_MIN_VISIBILITY && torsoBone != null)
                {
                    float torsoWidth = 0.4f;
                    float torsoDepth = 0.2f;
                    float torsoHeight = 0.6f;
                    Vector3 torsoBase = cameraPos - up * 0.8f + forward * 0.3f;
                    
                    Matrix torsoRotation = Matrix.CreateRotationX(torsoBone.LocalRotation.X) *
                                         Matrix.CreateRotationY(torsoBone.LocalRotation.Y) *
                                         Matrix.CreateRotationZ(torsoBone.LocalRotation.Z);
                    AddBoxWithRotation(vertices, torsoBase, torsoWidth, torsoDepth, torsoHeight, torsoColor, up, right, forward, torsoRotation);
                }
                
                // Legs with animation
                if (pitch > LEG_VISIBILITY_THRESHOLD)
                {
                    float legVisibility = MathHelper.Clamp((pitch - LEG_VISIBILITY_THRESHOLD) / LEG_FADE_RANGE, 0f, 1f);
                    
                    if (legVisibility > LEG_MIN_VISIBILITY)
                    {
                        float legWidth = 0.18f;
                        float legDepth = 0.18f;
                        float legHeight = 0.8f;
                        
                        // Right leg with animation
                        if (rightLegBone != null)
                        {
                            Vector3 rightLegBase = cameraPos + right * 0.09f - up * 1.4f + forward * 0.2f;
                            Matrix legRotation = Matrix.CreateRotationX(rightLegBone.LocalRotation.X) *
                                               Matrix.CreateRotationY(rightLegBone.LocalRotation.Y) *
                                               Matrix.CreateRotationZ(rightLegBone.LocalRotation.Z);
                            AddBoxWithRotation(vertices, rightLegBase, legWidth, legDepth, legHeight, legColor, up, right, forward, legRotation);
                        }
                        
                        // Left leg with animation
                        if (leftLegBone != null)
                        {
                            Vector3 leftLegBase = cameraPos - right * 0.09f - up * 1.4f + forward * 0.2f;
                            Matrix legRotation = Matrix.CreateRotationX(leftLegBone.LocalRotation.X) *
                                               Matrix.CreateRotationY(leftLegBone.LocalRotation.Y) *
                                               Matrix.CreateRotationZ(leftLegBone.LocalRotation.Z);
                            AddBoxWithRotation(vertices, leftLegBase, legWidth, legDepth, legHeight, legColor, up, right, forward, legRotation);
                        }
                    }
                }
            }
            
            return vertices.ToArray();
        }

        private void AddBox(List<VertexPositionColor> vertices, Vector3 basePos, 
                           float width, float height, float length, Color color,
                           Vector3 forward, Vector3 right, Vector3 up)
        {
            AddBoxWithRotation(vertices, basePos, width, height, length, color, forward, right, up, Matrix.Identity);
        }
        
        private void AddBoxWithRotation(List<VertexPositionColor> vertices, Vector3 basePos, 
                           float width, float height, float length, Color color,
                           Vector3 forward, Vector3 right, Vector3 up, Matrix rotation)
        {
            // Calculate box corners relative to base position
            // The box extends along the forward direction
            Vector3 halfWidth = right * (width / 2);
            Vector3 halfHeight = up * (height / 2);
            Vector3 fullLength = forward * length;
            
            // Define 8 corners of the box (before rotation)
            Vector3[] localCorners = new Vector3[8];
            localCorners[0] = -halfWidth - halfHeight;           // Back bottom left
            localCorners[1] = halfWidth - halfHeight;           // Back bottom right
            localCorners[2] = -halfWidth + halfHeight;           // Back top left
            localCorners[3] = halfWidth + halfHeight;           // Back top right
            localCorners[4] = -halfWidth - halfHeight + fullLength; // Front bottom left
            localCorners[5] = halfWidth - halfHeight + fullLength; // Front bottom right
            localCorners[6] = -halfWidth + halfHeight + fullLength; // Front top left
            localCorners[7] = halfWidth + halfHeight + fullLength; // Front top right
            
            // Apply rotation and translation
            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                corners[i] = Vector3.Transform(localCorners[i], rotation) + basePos;
            }
            
            // Add slightly darker colors for different faces
            Color topColor = Color.Lerp(color, Color.White, 0.2f);
            Color bottomColor = Color.Lerp(color, Color.Black, 0.2f);
            Color sideColor = Color.Lerp(color, Color.Black, 0.1f);
            
            // Front face
            AddQuad(vertices, corners[4], corners[5], corners[7], corners[6], color);
            
            // Back face
            AddQuad(vertices, corners[1], corners[0], corners[2], corners[3], color);
            
            // Top face
            AddQuad(vertices, corners[2], corners[3], corners[7], corners[6], topColor);
            
            // Bottom face
            AddQuad(vertices, corners[0], corners[1], corners[5], corners[4], bottomColor);
            
            // Right face
            AddQuad(vertices, corners[5], corners[1], corners[3], corners[7], sideColor);
            
            // Left face
            AddQuad(vertices, corners[0], corners[4], corners[6], corners[2], sideColor);
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
