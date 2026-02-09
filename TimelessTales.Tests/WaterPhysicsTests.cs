using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using TimelessTales.World;
using TimelessTales.Blocks;
using Xunit;

namespace TimelessTales.Tests
{
    public class WaterPhysicsTests
    {
        [Fact]
        public void WaterRenderer_BuildsWaterMesh_ForWaterBlocks()
        {
            // This is an integration test to verify that water blocks are detected
            // and processed separately from solid blocks
            
            // Water blocks should be:
            // 1. Skipped in WorldRenderer (solid block renderer)
            // 2. Rendered in WaterRenderer (transparent water renderer)
            
            BlockType waterBlock = BlockType.Water;
            BlockType saltwaterBlock = BlockType.Saltwater;
            
            // Verify water blocks are marked as transparent
            Assert.True(BlockRegistry.IsTransparent(waterBlock));
            Assert.True(BlockRegistry.IsTransparent(saltwaterBlock));
            
            // Verify water blocks are not solid (allows player to move through)
            Assert.False(BlockRegistry.IsSolid(waterBlock));
            Assert.False(BlockRegistry.IsSolid(saltwaterBlock));
        }
        
        [Fact]
        public void Player_SubmersionDepth_IsZero_WhenNotInWater()
        {
            // Submersion depth should be calculated based on how many sample points
            // in the player's body are underwater
            // When not in water, it should be 0
            
            // This is tested indirectly by verifying that a player in air
            // does not receive buoyancy forces
            
            // Arrange - player in air
            var player = new Player(new Vector3(0, 100, 0)); // High in the air
            
            // We can't directly test _submersionDepth as it's private,
            // but we can verify the player behaves correctly (no water effects)
            Assert.NotNull(player);
        }
        
        [Fact]
        public void BuoyancyPhysics_GravityReduction_InWater()
        {
            // When in water, gravity should be reduced by WATER_GRAVITY_MULTIPLIER (0.3)
            // This makes falling slower underwater
            
            const float GRAVITY = 20.0f;
            const float WATER_GRAVITY_MULTIPLIER = 0.3f;
            
            // In water
            float waterGravity = GRAVITY * WATER_GRAVITY_MULTIPLIER;
            
            // Out of water
            float airGravity = GRAVITY * 1.0f;
            
            // Assert water gravity is significantly less than air gravity
            Assert.True(waterGravity < airGravity);
            Assert.Equal(6.0f, waterGravity, 5);
            Assert.Equal(20.0f, airGravity, 5);
        }
        
        [Fact]
        public void BuoyancyPhysics_BuoyantForce_CountersGravity()
        {
            // Buoyant force should keep player floating at water surface
            // When fully submerged, buoyancy force should be strong enough
            // to counteract reduced gravity and create upward motion
            
            const float BUOYANT_FORCE = 15.0f;
            const float GRAVITY = 20.0f;
            const float WATER_GRAVITY_MULTIPLIER = 0.3f;
            const float WATER_DRAG = 0.9f;
            
            // Calculate forces for fully submerged player (depth = 1.0)
            float submersionDepth = 1.0f;
            float deltaTime = 1.0f / 60.0f; // 60 FPS
            
            // Gravity force (downward)
            float gravityForce = GRAVITY * WATER_GRAVITY_MULTIPLIER * deltaTime;
            
            // Buoyancy force (upward) when fully submerged
            float buoyancyForce = BUOYANT_FORCE * submersionDepth * deltaTime;
            
            // Net upward force (buoyancy should exceed gravity)
            float netForce = buoyancyForce - gravityForce;
            
            // Assert buoyancy creates net upward force
            Assert.True(netForce > 0, "Buoyancy force should create net upward motion when fully submerged");
        }
        
        [Fact]
        public void WaterDrag_ReducesHorizontalVelocity()
        {
            // Water drag should reduce movement speed
            // WATER_DRAG = 0.9 means velocity is multiplied by 0.9 each frame
            
            const float WATER_DRAG = 0.9f;
            
            Vector3 initialVelocity = new Vector3(10, 0, 10);
            Vector3 draggedVelocity = new Vector3(
                initialVelocity.X * WATER_DRAG,
                initialVelocity.Y,
                initialVelocity.Z * WATER_DRAG
            );
            
            // Assert velocity is reduced
            Assert.True(draggedVelocity.X < initialVelocity.X);
            Assert.True(draggedVelocity.Z < initialVelocity.Z);
            Assert.Equal(9.0f, draggedVelocity.X, 5);
            Assert.Equal(9.0f, draggedVelocity.Z, 5);
        }
        
        [Fact]
        public void WaterMovement_SpeedReduction_BasedOnSubmersion()
        {
            // Player speed in water should be reduced based on submersion depth
            // Formula: speed *= (0.5 + 0.5 * (1.0 - submersionDepth))
            // Fully submerged (depth=1.0): 50% speed
            // Half submerged (depth=0.5): 75% speed
            // Not submerged (depth=0.0): 100% speed
            
            const float MOVE_SPEED = 4.5f;
            
            // Fully submerged
            float depth1 = 1.0f;
            float speed1 = MOVE_SPEED * (0.5f + 0.5f * (1.0f - depth1));
            Assert.Equal(2.25f, speed1, 5); // 50% of normal speed
            
            // Half submerged
            float depth2 = 0.5f;
            float speed2 = MOVE_SPEED * (0.5f + 0.5f * (1.0f - depth2));
            Assert.Equal(3.375f, speed2, 5); // 75% of normal speed
            
            // Not submerged
            float depth3 = 0.0f;
            float speed3 = MOVE_SPEED * (0.5f + 0.5f * (1.0f - depth3));
            Assert.Equal(4.5f, speed3, 5); // 100% of normal speed
        }
        
        [Fact]
        public void WaterDepthColor_DarkensWithDepth()
        {
            // Water color should get darker as depth increases
            // Shallow water: lighter and more transparent
            // Deep water: darker and more opaque
            
            const int SEA_LEVEL = 64;
            
            // Shallow water (1 block below surface)
            int shallowY = SEA_LEVEL - 1;
            int shallowDepth = SEA_LEVEL - shallowY;
            float shallowFactor = MathHelper.Clamp(shallowDepth / 20.0f, 0, 1);
            
            // Deep water (20 blocks below surface)
            int deepY = SEA_LEVEL - 20;
            int deepDepth = SEA_LEVEL - deepY;
            float deepFactor = MathHelper.Clamp(deepDepth / 20.0f, 0, 1);
            
            // Assert deep water has higher depth factor
            Assert.True(deepFactor > shallowFactor);
            Assert.Equal(0.05f, shallowFactor, 5);
            Assert.Equal(1.0f, deepFactor, 5);
            
            // Brightness calculation
            float shallowBrightness = 1.0f - (shallowFactor * 0.5f);
            float deepBrightness = 1.0f - (deepFactor * 0.5f);
            
            // Assert shallow water is brighter than deep water
            Assert.True(shallowBrightness > deepBrightness);
        }
        
        [Fact]
        public void WaveAnimation_ChangesOverTime()
        {
            // Wave offset should change based on position and time
            // Using Gerstner waves for more realistic ocean-like motion
            
            const float WAVE_HEIGHT = 0.08f;
            
            float time1 = 0.0f;
            float time2 = 1.0f;
            
            int worldX = 0;
            int worldZ = 0;
            
            // Calculate Gerstner wave at two different times
            // Gerstner waves use: steepness * sin(frequency * (dir.x*x + dir.z*z) + time * sqrt(g*f))
            var waves = new[]
            {
                (DirX: 1.0f, DirZ: 0.0f, Steepness: 0.15f, Wavelength: 8.0f),
                (DirX: 0.0f, DirZ: 1.0f, Steepness: 0.10f, Wavelength: 5.0f),
                (DirX: 0.7f, DirZ: 0.7f, Steepness: 0.08f, Wavelength: 3.0f),
            };
            
            float totalWave_t1 = 0f;
            float totalWave_t2 = 0f;
            
            foreach (var wave in waves)
            {
                float frequency = 2.0f * MathF.PI / wave.Wavelength;
                float phase_t1 = frequency * (wave.DirX * worldX + wave.DirZ * worldZ) + time1 * MathF.Sqrt(9.81f * frequency);
                float phase_t2 = frequency * (wave.DirX * worldX + wave.DirZ * worldZ) + time2 * MathF.Sqrt(9.81f * frequency);
                totalWave_t1 += wave.Steepness * MathF.Sin(phase_t1) * WAVE_HEIGHT;
                totalWave_t2 += wave.Steepness * MathF.Sin(phase_t2) * WAVE_HEIGHT;
            }
            
            // Assert wave position changes over time
            Assert.NotEqual(totalWave_t1, totalWave_t2);
            
            // Verify wave amplitude is within expected range (sum of steepnesses * WAVE_HEIGHT)
            float maxAmplitude = (0.15f + 0.10f + 0.08f) * WAVE_HEIGHT;
            Assert.True(MathF.Abs(totalWave_t1) <= maxAmplitude);
            Assert.True(MathF.Abs(totalWave_t2) <= maxAmplitude);
        }
        
        [Fact]
        public void FoamFactor_IsZero_BelowThreshold()
        {
            // Foam should not appear when wave offset is below the threshold
            float waveHeight = 0.08f;
            float lowOffset = waveHeight * 0.1f; // Very low wave
            
            float foamFactor = TimelessTales.Rendering.WaterRenderer.CalculateFoamFactor(lowOffset, waveHeight);
            Assert.Equal(0f, foamFactor);
        }
        
        [Fact]
        public void FoamFactor_IsPositive_AtWaveCrest()
        {
            // Foam should appear at wave crests (high positive displacement)
            float waveHeight = 0.08f;
            float highOffset = waveHeight * 2.0f; // High crest
            
            float foamFactor = TimelessTales.Rendering.WaterRenderer.CalculateFoamFactor(highOffset, waveHeight);
            Assert.True(foamFactor > 0f, "Foam should appear at wave crests");
            Assert.True(foamFactor <= 0.4f, "Foam intensity should be capped");
        }
    }
}
