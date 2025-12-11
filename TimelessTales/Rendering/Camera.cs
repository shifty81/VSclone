using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Represents the player's view into the 3D world
    /// </summary>
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector2 Rotation { get; set; } // X = pitch, Y = yaw
        
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }
        
        private readonly float _aspectRatio;
        private readonly float _nearPlane = 0.1f;
        private readonly float _farPlane = 500f;
        private readonly float _fieldOfView = MathHelper.PiOver4;

        public Camera(Viewport viewport)
        {
            _aspectRatio = viewport.AspectRatio;
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                _fieldOfView, _aspectRatio, _nearPlane, _farPlane);
            
            Position = Vector3.Zero;
            Rotation = Vector2.Zero;
        }

        public void Update()
        {
            // Create rotation matrix from pitch and yaw
            Matrix rotationMatrix = Matrix.CreateRotationX(Rotation.X) * 
                                   Matrix.CreateRotationY(Rotation.Y);
            
            // Calculate forward direction
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Vector3 target = Position + forward;
            
            // Create view matrix
            ViewMatrix = Matrix.CreateLookAt(Position, target, Vector3.Up);
        }

        public Vector3 GetForwardVector()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(Rotation.X) * 
                                   Matrix.CreateRotationY(Rotation.Y);
            return Vector3.Transform(Vector3.Forward, rotationMatrix);
        }

        public Vector3 GetRightVector()
        {
            Matrix rotationMatrix = Matrix.CreateRotationY(Rotation.Y);
            return Vector3.Transform(Vector3.Right, rotationMatrix);
        }
    }
}
