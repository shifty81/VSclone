using Microsoft.Xna.Framework;
using TimelessTales.Particles;
using Xunit;

namespace TimelessTales.Tests
{
    public class ParticleSystemTests
    {
        [Fact]
        public void Particle_UpdatesPositionBasedOnVelocity()
        {
            // Arrange
            var position = new Vector3(0, 0, 0);
            var velocity = new Vector3(1, 2, 3);
            var particle = new Particle(position, velocity, Color.White, 0.1f, 2.0f);
            
            // Act
            float deltaTime = 0.5f;
            bool alive = particle.Update(deltaTime);
            
            // Assert
            Assert.True(alive);
            Assert.Equal(0.5f, particle.Position.X, 0.01f);
            Assert.Equal(1.0f, particle.Position.Y, 0.01f);
            Assert.Equal(1.5f, particle.Position.Z, 0.01f);
            Assert.Equal(1.5f, particle.Life, 0.01f);
        }
        
        [Fact]
        public void Particle_DiesWhenLifetimeExpires()
        {
            // Arrange
            var particle = new Particle(Vector3.Zero, Vector3.UnitY, Color.White, 0.1f, 1.0f);
            
            // Act - update for longer than lifetime
            bool alive = particle.Update(1.5f);
            
            // Assert
            Assert.False(alive);
            Assert.True(particle.Life < 0);
        }
        
        [Fact]
        public void Particle_AlphaFadesOverLifetime()
        {
            // Arrange
            var particle = new Particle(Vector3.Zero, Vector3.UnitY, Color.White, 0.1f, 2.0f);
            
            // Initial alpha should be 1.0
            Assert.Equal(1.0f, particle.Alpha, 0.01f);
            
            // Act - update for half lifetime
            particle.Update(1.0f);
            
            // Assert - alpha should be 0.5
            Assert.Equal(0.5f, particle.Alpha, 0.01f);
        }
        
        [Fact]
        public void ParticleEmitter_EmitsCorrectNumberOfParticles()
        {
            // Arrange
            var emitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 10.0f, // 10 particles per second
                IsActive = true
            };
            
            // Act - update for 0.5 seconds (should emit 5 particles)
            emitter.Update(0.5f);
            
            // Assert
            Assert.Equal(5, emitter.GetParticles().Count);
        }
        
        [Fact]
        public void ParticleEmitter_DoesNotEmitWhenInactive()
        {
            // Arrange
            var emitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 10.0f,
                IsActive = false // Inactive
            };
            
            // Act
            emitter.Update(1.0f);
            
            // Assert
            Assert.Empty(emitter.GetParticles());
        }
        
        [Fact]
        public void ParticleEmitter_RemovesDeadParticles()
        {
            // Arrange
            var emitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 10.0f,
                ParticleLifetime = 0.5f, // Very short lifetime
                IsActive = true
            };
            
            // Act - emit particles
            emitter.Update(0.1f); // Create 1 particle
            Assert.Single(emitter.GetParticles());
            
            // Wait for particles to die
            emitter.IsActive = false; // Stop emitting
            emitter.Update(1.0f); // Update past particle lifetime
            
            // Assert - dead particles removed
            Assert.Empty(emitter.GetParticles());
        }
        
        [Fact]
        public void BubbleEmitter_HasCorrectConfiguration()
        {
            // Test that bubble emitter has appropriate settings
            var bubbleEmitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 3.0f,
                ParticleLifetime = 2.5f,
                ParticleSize = 0.08f,
                ParticleColor = new Color(200, 220, 255, 180),
                VelocityBase = new Vector3(0, 0.5f, 0),
                VelocityVariation = new Vector3(0.1f, 0.2f, 0.1f)
            };
            
            // Assert configuration
            Assert.Equal(3.0f, bubbleEmitter.EmissionRate);
            Assert.Equal(2.5f, bubbleEmitter.ParticleLifetime);
            Assert.Equal(0.08f, bubbleEmitter.ParticleSize);
            Assert.Equal(0.5f, bubbleEmitter.VelocityBase.Y); // Floats upward
        }
        
        [Fact]
        public void SplashEmitter_HasCorrectConfiguration()
        {
            // Test that splash emitter has appropriate settings for burst effect
            var splashEmitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 50.0f, // High rate for burst
                ParticleLifetime = 0.8f, // Short lifetime
                ParticleSize = 0.12f,
                ParticleColor = new Color(150, 190, 230, 200),
                VelocityBase = new Vector3(0, 2.0f, 0),
                VelocityVariation = new Vector3(1.5f, 1.0f, 1.5f)
            };
            
            // Assert configuration
            Assert.Equal(50.0f, splashEmitter.EmissionRate); // High for burst
            Assert.Equal(0.8f, splashEmitter.ParticleLifetime); // Short duration
            Assert.True(splashEmitter.ParticleSize > 0.08f); // Larger than bubbles
            Assert.Equal(2.0f, splashEmitter.VelocityBase.Y); // Faster upward motion
        }
    }
}
