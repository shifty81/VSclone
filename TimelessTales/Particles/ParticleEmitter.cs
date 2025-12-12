using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TimelessTales.Particles
{
    /// <summary>
    /// Manages emission and rendering of particles
    /// </summary>
    public class ParticleEmitter
    {
        private readonly List<Particle> _particles;
        private readonly Random _random;
        private float _emissionTimer;
        
        public Vector3 Position { get; set; }
        public bool IsActive { get; set; }
        public float EmissionRate { get; set; } // Particles per second
        public float ParticleLifetime { get; set; }
        public float ParticleSize { get; set; }
        public Color ParticleColor { get; set; }
        public Vector3 VelocityBase { get; set; }
        public Vector3 VelocityVariation { get; set; }
        
        public ParticleEmitter(Vector3 position)
        {
            Position = position;
            _particles = new List<Particle>();
            _random = new Random();
            IsActive = true;
            EmissionRate = 5.0f; // 5 particles per second by default
            ParticleLifetime = 2.0f;
            ParticleSize = 0.1f;
            ParticleColor = Color.White;
            VelocityBase = new Vector3(0, 1, 0); // Default upward movement
            VelocityVariation = new Vector3(0.2f, 0.5f, 0.2f);
        }
        
        public void Update(float deltaTime)
        {
            // Update existing particles
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                if (!_particles[i].Update(deltaTime))
                {
                    _particles.RemoveAt(i);
                }
            }
            
            // Emit new particles
            if (IsActive)
            {
                _emissionTimer += deltaTime;
                float emissionInterval = 1.0f / EmissionRate;
                
                while (_emissionTimer >= emissionInterval)
                {
                    EmitParticle();
                    _emissionTimer -= emissionInterval;
                }
            }
        }
        
        private void EmitParticle()
        {
            // Random velocity variation
            Vector3 velocity = VelocityBase + new Vector3(
                ((float)_random.NextDouble() - 0.5f) * VelocityVariation.X * 2,
                ((float)_random.NextDouble() - 0.5f) * VelocityVariation.Y * 2,
                ((float)_random.NextDouble() - 0.5f) * VelocityVariation.Z * 2
            );
            
            // Small random position offset
            Vector3 spawnPos = Position + new Vector3(
                ((float)_random.NextDouble() - 0.5f) * 0.2f,
                0,
                ((float)_random.NextDouble() - 0.5f) * 0.2f
            );
            
            var particle = new Particle(spawnPos, velocity, ParticleColor, ParticleSize, ParticleLifetime);
            _particles.Add(particle);
        }
        
        public IReadOnlyList<Particle> GetParticles() => _particles.AsReadOnly();
        
        public void Clear()
        {
            _particles.Clear();
        }
    }
}
