using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using TimelessTales.World;
using TimelessTales.Blocks;
using TimelessTales.Core;
using Xunit;

namespace TimelessTales.Tests
{
    /// <summary>
    /// Tests for player collision detection to ensure player doesn't fall through world
    /// </summary>
    public class CollisionTests
    {
        [Fact]
        public void Player_ShouldNotFallThroughSolidBlocks()
        {
            // Arrange: Create a world with solid blocks
            var worldManager = new WorldManager(12345);
            worldManager.Initialize();
            
            // Find the spawn position (should be above solid ground)
            Vector3 spawnPosition = worldManager.GetSpawnPosition();
            
            // Create player at spawn position
            var player = new Player(spawnPosition);
            var inputManager = new InputManager();
            
            // Act: Simulate multiple frames of gravity pulling player down
            // Player should land on solid ground and not fall through
            for (int i = 0; i < 100; i++)
            {
                var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016)); // ~60 FPS
                player.Update(gameTime, inputManager, worldManager);
            }
            
            // Assert: Player should be resting on solid ground, not falling through
            // Check that player is on a solid block
            int blockX = (int)MathF.Floor(player.Position.X);
            int blockY = (int)MathF.Floor(player.Position.Y - 0.1f); // Check block just below player's feet
            int blockZ = (int)MathF.Floor(player.Position.Z);
            
            BlockType blockBelow = worldManager.GetBlock(blockX, blockY, blockZ);
            
            // The block below should be solid (not air)
            Assert.True(BlockRegistry.IsSolid(blockBelow), 
                $"Player fell through world! Position: {player.Position}, Block below: {blockBelow}");
        }
        
        [Fact]
        public void Player_ShouldLandOnTopOfBlock()
        {
            // Arrange: Create a simple test world
            var worldManager = new WorldManager(54321);
            worldManager.Initialize();
            
            // Find the actual terrain height at position (5, 5)
            int actualTerrainHeight = -1;
            for (int y = Chunk.CHUNK_HEIGHT - 1; y >= 0; y--)
            {
                BlockType block = worldManager.GetBlock(5, y, 5);
                if (BlockRegistry.IsSolid(block))
                {
                    actualTerrainHeight = y;
                    break;
                }
            }
            
            // Note: If terrain is below sea level (64), there will be water above it
            // and the player will float due to buoyancy instead of landing on terrain
            const int SEA_LEVEL = 64;
            
            // Create player above the terrain
            var player = new Player(new Vector3(5.5f, actualTerrainHeight + 10f, 5.5f));
            var inputManager = new InputManager();
            
            // Record initial Y position
            float initialY = player.Position.Y;
            
            // Act: Let player fall
            for (int i = 0; i < 200; i++)
            {
                var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));
                player.Update(gameTime, inputManager, worldManager);
            }
            
            // Assert: Player behavior depends on whether terrain is above or below sea level
            if (actualTerrainHeight >= SEA_LEVEL)
            {
                // Dry land - player should land on terrain
                float expectedY = actualTerrainHeight + 1f;
                Assert.True(player.Position.Y >= expectedY && player.Position.Y <= expectedY + 0.1f,
                    $"Player should be at Y={expectedY} (on top of terrain at height {actualTerrainHeight}), but is at Y={player.Position.Y}");
            }
            else
            {
                // Underwater terrain - player should float at or near sea level due to buoyancy
                // Allow a wider range since buoyancy creates bobbing motion
                Assert.True(player.Position.Y >= SEA_LEVEL - 2f && player.Position.Y <= SEA_LEVEL + 2f,
                    $"Player should be floating near sea level (Y={SEA_LEVEL}) due to water buoyancy, but is at Y={player.Position.Y}");
            }
            
            // Player should not be falling anymore (velocity should be zero or very small)
            Assert.True(player.Velocity.Y >= -0.5f && player.Velocity.Y <= 0.5f,
                $"Player should have stopped falling, but has velocity Y={player.Velocity.Y}");
            
            // Player should have fallen from initial position
            Assert.True(player.Position.Y < initialY,
                $"Player should have fallen from initial Y={initialY} to current Y={player.Position.Y}");
        }
    }
}
