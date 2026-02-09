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
        
        // Enhanced emitter properties
        public float SizeVariation { get; set; } // Random size variation (0-1)
        public bool EnableWobble { get; set; } // Enable wobble for spawned particles
        public float WobbleAmplitude { get; set; } = 0.3f;
        public float WobbleFrequency { get; set; } = 4.0f;
        public float SurfacePopY { get; set; } = float.MaxValue; // Y level where particles pop
        
        // Periodic burst emission
        public bool UseBurstMode { get; set; } // Emit in bursts
        public float BurstInterval { get; set; } = 3.0f; // Seconds between bursts
        public int BurstCount { get; set; } = 5; // Particles per burst
        private float _burstTimer;
        
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
                if (UseBurstMode)
                {
                    _burstTimer += deltaTime;
                    if (_burstTimer >= BurstInterval)
                    {
                        for (int i = 0; i < BurstCount; i++)
                        {
                            EmitParticle();
                        }
                        _burstTimer -= BurstInterval;
                    }
                }
                else
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
            
            // Apply size variation
            float size = ParticleSize;
            if (SizeVariation > 0)
            {
                float variation = 1.0f + ((float)_random.NextDouble() - 0.5f) * SizeVariation * 2;
                size *= Math.Max(0.1f, variation);
            }
            
            var particle = new Particle(spawnPos, velocity, ParticleColor, size, ParticleLifetime);
            
            // Apply wobble settings
            if (EnableWobble)
            {
                particle.HasWobble = true;
                particle.WobbleAmplitude = WobbleAmplitude;
                particle.WobbleFrequency = WobbleFrequency;
            }
            
            // Apply surface pop level
            particle.SurfacePopY = SurfacePopY;
            
            _particles.Add(particle);
        }
        
        public IReadOnlyList<Particle> GetParticles() => _particles.AsReadOnly();
        
        public void Clear()
        {
            _particles.Clear();
        }
    }
}
