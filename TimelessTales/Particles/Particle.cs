using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TimelessTales.Particles
{
    /// <summary>
    /// Represents a single particle in the particle system
    /// </summary>
    public class Particle
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }
        public float Life { get; set; } // Remaining life in seconds
        public float MaxLife { get; set; } // Total lifetime
        public float Alpha => Life / MaxLife; // Fade based on remaining life
        
        public Particle(Vector3 position, Vector3 velocity, Color color, float size, float lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Size = size;
            Life = lifetime;
            MaxLife = lifetime;
        }
        
        /// <summary>
        /// Update particle position and life
        /// </summary>
        public bool Update(float deltaTime)
        {
            Position += Velocity * deltaTime;
            Life -= deltaTime;
            return Life > 0; // Return false when particle is dead
        }
    }
}
