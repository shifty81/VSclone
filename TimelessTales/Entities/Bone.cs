using Microsoft.Xna.Framework;
using System;

namespace TimelessTales.Entities
{
    /// <summary>
    /// Represents a single bone in a skeleton hierarchy
    /// </summary>
    public class Bone
    {
        public string Name { get; set; }
        public Vector3 LocalPosition { get; set; }
        public Vector3 LocalRotation { get; set; } // Euler angles in radians
        public Vector3 LocalScale { get; set; }
        public Bone? Parent { get; set; }
        
        // Computed transform
        public Matrix LocalTransform { get; private set; }
        public Matrix WorldTransform { get; private set; }
        
        public Bone(string name, Vector3 localPosition)
        {
            Name = name;
            LocalPosition = localPosition;
            LocalRotation = Vector3.Zero;
            LocalScale = Vector3.One;
            Parent = null;
            UpdateTransforms();
        }
        
        public void SetRotation(Vector3 rotation)
        {
            LocalRotation = rotation;
            UpdateTransforms();
        }
        
        public void SetPosition(Vector3 position)
        {
            LocalPosition = position;
            UpdateTransforms();
        }
        
        public void UpdateTransforms()
        {
            // Build local transform matrix
            LocalTransform = Matrix.CreateScale(LocalScale) *
                           Matrix.CreateRotationX(LocalRotation.X) *
                           Matrix.CreateRotationY(LocalRotation.Y) *
                           Matrix.CreateRotationZ(LocalRotation.Z) *
                           Matrix.CreateTranslation(LocalPosition);
            
            // Build world transform (parent's world * this local)
            if (Parent != null)
            {
                WorldTransform = LocalTransform * Parent.WorldTransform;
            }
            else
            {
                WorldTransform = LocalTransform;
            }
        }
        
        public Vector3 GetWorldPosition()
        {
            return WorldTransform.Translation;
        }
    }
}
