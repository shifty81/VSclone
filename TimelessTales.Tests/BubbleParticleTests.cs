using Microsoft.Xna.Framework;
using TimelessTales.Particles;
using Xunit;

namespace TimelessTales.Tests
{
    public class BubbleParticleTests
    {
        [Fact]
        public void Particle_WobbleMotionChangesPosition()
        {
            // Arrange
            var particle = new Particle(Vector3.Zero, new Vector3(0, 0.5f, 0), Color.White, 0.1f, 5.0f)
            {
                HasWobble = true,
                WobbleAmplitude = 0.3f,
                WobbleFrequency = 4.0f
            };

            var initialPos = particle.Position;

            // Act - update multiple frames to accumulate wobble
            for (int i = 0; i < 10; i++)
            {
                particle.Update(0.1f);
            }

            // Assert - X position should have changed from wobble, Y from velocity
            Assert.NotEqual(0, particle.Position.X);
            Assert.True(particle.Position.Y > 0); // Moving upward
        }

        [Fact]
        public void Particle_PopsAtSurfaceLevel()
        {
            // Arrange
            var particle = new Particle(new Vector3(0, 63, 0), new Vector3(0, 2, 0), Color.White, 0.1f, 5.0f)
            {
                SurfacePopY = 64.0f
            };

            // Act - update until particle passes surface
            bool alive = particle.Update(1.0f);

            // Assert - particle should have popped at surface
            Assert.False(alive);
        }

        [Fact]
        public void Particle_AgeTracksCorrectly()
        {
            // Arrange
            var particle = new Particle(Vector3.Zero, Vector3.UnitY, Color.White, 0.1f, 3.0f);

            // Act
            particle.Update(1.0f);

            // Assert
            Assert.Equal(1.0f, particle.Age, 0.01f);
            Assert.Equal(2.0f, particle.Life, 0.01f);
        }

        [Fact]
        public void ParticleEmitter_SizeVariationCreatesVariedParticles()
        {
            // Arrange
            var emitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 100.0f,
                ParticleSize = 0.1f,
                SizeVariation = 0.5f,
                IsActive = true
            };

            // Act
            emitter.Update(0.1f); // Emit ~10 particles

            // Assert - particles should have different sizes
            var particles = emitter.GetParticles();
            Assert.True(particles.Count > 0);

            bool hasVariation = false;
            float firstSize = particles[0].Size;
            foreach (var p in particles)
            {
                if (System.Math.Abs(p.Size - firstSize) > 0.001f)
                {
                    hasVariation = true;
                    break;
                }
            }
            Assert.True(hasVariation, "Particles should have size variation");
        }

        [Fact]
        public void ParticleEmitter_WobbleEnabledOnParticles()
        {
            // Arrange
            var emitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 10.0f,
                EnableWobble = true,
                WobbleAmplitude = 0.5f,
                WobbleFrequency = 3.0f,
                IsActive = true
            };

            // Act
            emitter.Update(0.5f);

            // Assert
            var particles = emitter.GetParticles();
            Assert.True(particles.Count > 0);
            Assert.True(particles[0].HasWobble);
            Assert.Equal(0.5f, particles[0].WobbleAmplitude, 0.01f);
            Assert.Equal(3.0f, particles[0].WobbleFrequency, 0.01f);
        }

        [Fact]
        public void ParticleEmitter_BurstModeEmitsInBursts()
        {
            // Arrange
            var emitter = new ParticleEmitter(Vector3.Zero)
            {
                UseBurstMode = true,
                BurstInterval = 1.0f,
                BurstCount = 5,
                ParticleLifetime = 10.0f, // Long lifetime so they don't die
                IsActive = true
            };

            // Act - update for less than burst interval
            emitter.Update(0.5f);
            int countBefore = emitter.GetParticles().Count;

            // Update past burst interval
            emitter.Update(0.6f); // total: 1.1s
            int countAfter = emitter.GetParticles().Count;

            // Assert
            Assert.Equal(0, countBefore); // No particles before first burst
            Assert.Equal(5, countAfter); // Exactly BurstCount particles after burst
        }

        [Fact]
        public void ParticleEmitter_SurfacePopYPropagatedToParticles()
        {
            // Arrange
            var emitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 10.0f,
                SurfacePopY = 64.0f,
                IsActive = true
            };

            // Act
            emitter.Update(0.5f);

            // Assert
            var particles = emitter.GetParticles();
            Assert.True(particles.Count > 0);
            Assert.Equal(64.0f, particles[0].SurfacePopY, 0.01f);
        }

        [Fact]
        public void BubbleEmitter_EnhancedConfiguration()
        {
            // Test that an enhanced bubble emitter has the correct settings
            var bubbleEmitter = new ParticleEmitter(Vector3.Zero)
            {
                EmissionRate = 3.0f,
                ParticleLifetime = 2.5f,
                ParticleSize = 0.08f,
                ParticleColor = new Color(200, 220, 255, 180),
                VelocityBase = new Vector3(0, 0.5f, 0),
                VelocityVariation = new Vector3(0.1f, 0.2f, 0.1f),
                EnableWobble = true,
                WobbleAmplitude = 0.3f,
                WobbleFrequency = 4.0f,
                SizeVariation = 0.4f,
                SurfacePopY = 64.0f,
                UseBurstMode = true,
                BurstInterval = 3.0f,
                BurstCount = 5
            };

            // Assert all configuration
            Assert.True(bubbleEmitter.EnableWobble);
            Assert.Equal(0.3f, bubbleEmitter.WobbleAmplitude);
            Assert.Equal(4.0f, bubbleEmitter.WobbleFrequency);
            Assert.Equal(0.4f, bubbleEmitter.SizeVariation);
            Assert.Equal(64.0f, bubbleEmitter.SurfacePopY);
            Assert.True(bubbleEmitter.UseBurstMode);
            Assert.Equal(3.0f, bubbleEmitter.BurstInterval);
            Assert.Equal(5, bubbleEmitter.BurstCount);
        }
    }
}
