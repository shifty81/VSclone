using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
        
        // Enhanced particle properties
        public bool HasWobble { get; set; } // Wobble motion (for bubbles)
        public float WobbleAmplitude { get; set; } // Horizontal wobble strength
        public float WobbleFrequency { get; set; } // Wobble speed
        public float SurfacePopY { get; set; } = float.MaxValue; // Y level at which particle pops
        public float Age => MaxLife - Life; // Time since spawn
        
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
            // Apply wobble motion (sinusoidal horizontal drift)
            if (HasWobble)
            {
                float wobbleOffset = MathF.Sin(Age * WobbleFrequency) * WobbleAmplitude * deltaTime;
                Position += new Vector3(wobbleOffset, 0, wobbleOffset * 0.7f);
            }
            
            Position += Velocity * deltaTime;
            Life -= deltaTime;
            
            // Pop at surface level
            if (Position.Y >= SurfacePopY)
            {
                Life = 0;
                return false;
            }
            
            return Life > 0; // Return false when particle is dead
        }
    }
}
