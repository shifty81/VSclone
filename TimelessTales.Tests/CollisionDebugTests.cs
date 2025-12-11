using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using TimelessTales.World;
using TimelessTales.Blocks;
using TimelessTales.Core;
using Xunit;
using Xunit.Abstractions;

namespace TimelessTales.Tests
{
    /// <summary>
    /// Debug tests for player collision detection
    /// </summary>
    public class CollisionDebugTests
    {
        private readonly ITestOutputHelper _output;

        public CollisionDebugTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DebugCollision_PlayerFallingOntoBlock()
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
                    _output.WriteLine($"First solid block at Y={y}, Type={block}");
                    break;
                }
            }
            
            _output.WriteLine($"Terrain height at (5, 5): {actualTerrainHeight}");
            
            // Show blocks below terrain for context
            _output.WriteLine("\nBlocks below terrain:");
            for (int y = actualTerrainHeight; y >= Math.Max(0, actualTerrainHeight - 5); y--)
            {
                BlockType block = worldManager.GetBlock(5, y, 5);
                _output.WriteLine($"  Y={y}: {block} (Solid={BlockRegistry.IsSolid(block)})");
            }
            
            // Create player above the terrain
            var startY = actualTerrainHeight + 5f;
            var player = new Player(new Vector3(5.5f, startY, 5.5f));
            var inputManager = new InputManager();
            
            _output.WriteLine($"\nPlayer starting at Y={player.Position.Y}");
            
            // Act: Let player fall and record positions
            for (int i = 0; i < 50; i++)
            {
                var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));
                player.Update(gameTime, inputManager, worldManager);
                
                if (i % 10 == 0 || i < 5)
                {
                    _output.WriteLine($"Frame {i}: Y={player.Position.Y:F3}, VelY={player.Velocity.Y:F3}");
                }
                
                // Check if player went below terrain
                if (player.Position.Y < actualTerrainHeight)
                {
                    _output.WriteLine($"!!! Player fell below terrain at frame {i}! Y={player.Position.Y}");
                }
            }
            
            _output.WriteLine($"\nFinal position: Y={player.Position.Y}");
            _output.WriteLine($"Final velocity: Y={player.Velocity.Y}");
            _output.WriteLine($"Expected Y: {actualTerrainHeight + 1}");
            
            float expectedY = actualTerrainHeight + 1f;
            Assert.True(player.Position.Y >= expectedY - 0.1f,
                $"Player fell through terrain! Expected Y >= {expectedY}, got Y={player.Position.Y}");
        }
    }
}
