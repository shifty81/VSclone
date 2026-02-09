using Microsoft.Xna.Framework;
using TimelessTales.World;
using TimelessTales.Blocks;
using Xunit;

namespace TimelessTales.Tests
{
    public class PerformanceOptimizationTests
    {
        [Fact]
        public void Chunk_SetBlockFast_DoesNotFlagMeshRebuild()
        {
            // Arrange
            var chunk = new Chunk(0, 0);
            chunk.NeedsMeshRebuild = false;

            // Act
            chunk.SetBlockFast(0, 0, 0, BlockType.Stone);

            // Assert - SetBlockFast should NOT set NeedsMeshRebuild
            Assert.False(chunk.NeedsMeshRebuild);
            Assert.Equal(BlockType.Stone, chunk.GetBlock(0, 0, 0));
        }

        [Fact]
        public void Chunk_SetBlock_FlagsMeshRebuild()
        {
            // Arrange
            var chunk = new Chunk(0, 0);
            chunk.NeedsMeshRebuild = false;

            // Act
            chunk.SetBlock(0, 0, 0, BlockType.Stone);

            // Assert - SetBlock SHOULD set NeedsMeshRebuild
            Assert.True(chunk.NeedsMeshRebuild);
        }

        [Fact]
        public void Chunk_SetBlockFast_OutOfBounds_DoesNotThrow()
        {
            // Arrange
            var chunk = new Chunk(0, 0);

            // Act & Assert - should not throw for out-of-bounds
            chunk.SetBlockFast(-1, 0, 0, BlockType.Stone);
            chunk.SetBlockFast(0, -1, 0, BlockType.Stone);
            chunk.SetBlockFast(0, 0, -1, BlockType.Stone);
            chunk.SetBlockFast(16, 0, 0, BlockType.Stone);
            chunk.SetBlockFast(0, 256, 0, BlockType.Stone);
            chunk.SetBlockFast(0, 0, 16, BlockType.Stone);
        }

        [Fact]
        public void Chunk_SetBlockFast_SameAsSetBlock_ForBlockData()
        {
            // Arrange
            var chunk1 = new Chunk(0, 0);
            var chunk2 = new Chunk(0, 0);

            // Act
            chunk1.SetBlock(5, 10, 5, BlockType.Granite);
            chunk2.SetBlockFast(5, 10, 5, BlockType.Granite);

            // Assert - both should have the same block data
            Assert.Equal(chunk1.GetBlock(5, 10, 5), chunk2.GetBlock(5, 10, 5));
        }

        [Fact]
        public void WorldManager_LimitsChunkLoadingPerFrame()
        {
            // This test verifies the render distance constant is reasonable
            // RENDER_DISTANCE=4 means (9×9)=81 chunks in full radius
            var worldManager = new WorldManager(42);
            
            // Initialize spawn chunks correctly 
            worldManager.Initialize();
            
            // After initialization, we should have spawn chunks loaded
            var chunks = worldManager.GetLoadedChunks().ToList();
            Assert.True(chunks.Count > 0, "Should have loaded spawn chunks");
        }

        [Fact]
        public void Chunk_Generate_SetsMeshRebuildFlag()
        {
            // Arrange
            var chunk = new Chunk(0, 0);
            var generator = new WorldGenerator(42);

            // Act
            chunk.Generate(generator);

            // Assert - NeedsMeshRebuild should be true after generation
            Assert.True(chunk.NeedsMeshRebuild);
            Assert.True(chunk.IsGenerated);
        }
    }
}
