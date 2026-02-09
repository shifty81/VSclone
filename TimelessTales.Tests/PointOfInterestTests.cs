using TimelessTales.World;
using TimelessTales.Blocks;
using Xunit;

namespace TimelessTales.Tests
{
    public class PointOfInterestTests
    {
        [Fact]
        public void PoiGenerator_CreatesWithSeed()
        {
            // Arrange & Act
            var generator = new PointOfInterestGenerator(42);

            // Assert - no exceptions
            Assert.NotNull(generator);
        }

        [Fact]
        public void PoiGenerator_DeterministicGeneration()
        {
            // Arrange
            var gen1 = new PointOfInterestGenerator(12345);
            var gen2 = new PointOfInterestGenerator(12345);
            var worldGen = new WorldGenerator(12345);

            var chunk1 = new Chunk(10, 10);
            chunk1.Generate(worldGen);
            var chunk2 = new Chunk(10, 10);
            chunk2.Generate(worldGen);

            // Act
            gen1.GenerateForChunk(chunk1, worldGen);
            gen2.GenerateForChunk(chunk2, worldGen);

            // Assert - same seed produces same results
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                    {
                        Assert.Equal(chunk1.GetBlock(x, y, z), chunk2.GetBlock(x, y, z));
                    }
                }
            }
        }

        [Fact]
        public void PoiGenerator_DoesNotCorruptChunk()
        {
            // Arrange
            var generator = new PointOfInterestGenerator(42);
            var worldGen = new WorldGenerator(42);
            var chunk = new Chunk(5, 5);
            chunk.Generate(worldGen);

            // Act - should not throw
            generator.GenerateForChunk(chunk, worldGen);

            // Assert - chunk boundaries should be intact
            // Bottom should still be solid
            Assert.True(BlockRegistry.IsSolid(chunk.GetBlock(0, 0, 0)));
        }

        [Fact]
        public void PoiGenerator_GeneratesStructuresInSomeChunks()
        {
            // Arrange
            var generator = new PointOfInterestGenerator(42);
            var worldGen = new WorldGenerator(42);
            bool foundStructure = false;

            // Act - check many chunks to find at least one with a POI
            for (int cx = 0; cx < 50 && !foundStructure; cx++)
            {
                for (int cz = 0; cz < 50 && !foundStructure; cz++)
                {
                    var chunk = new Chunk(cx, cz);
                    chunk.Generate(worldGen);
                    
                    // Save state before POI generation
                    var beforeBlock = chunk.GetBlock(Chunk.CHUNK_SIZE / 2, 1, Chunk.CHUNK_SIZE / 2);
                    
                    generator.GenerateForChunk(chunk, worldGen);
                    
                    // Check if any blocks at mid-height were modified (POI typically place blocks above surface)
                    // Look for placed structure blocks (cobblestone, planks, lanterns)
                    for (int y = 60; y < 100; y++)
                    {
                        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                        {
                            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                            {
                                BlockType block = chunk.GetBlock(x, y, z);
                                if (block == BlockType.Cobblestone || block == BlockType.Planks || 
                                    block == BlockType.Lantern)
                                {
                                    foundStructure = true;
                                    break;
                                }
                            }
                            if (foundStructure) break;
                        }
                        if (foundStructure) break;
                    }
                }
            }

            // Assert - should find at least one structure in 2500 chunks
            Assert.True(foundStructure, "Should generate at least one POI in 2500 chunks");
        }

        [Fact]
        public void WorldManager_HasPoiGenerator()
        {
            // Arrange & Act
            var worldManager = new WorldManager(42);

            // Assert
            Assert.NotNull(worldManager.PoiGenerator);
        }

        [Fact]
        public void PointOfInterestType_HasExpectedValues()
        {
            // Assert - verify all POI types exist
            Assert.Equal(0, (int)PointOfInterestType.AncientRuins);
            Assert.Equal(1, (int)PointOfInterestType.AbandonedSettlement);
            Assert.Equal(2, (int)PointOfInterestType.CrystalCavern);
            Assert.Equal(3, (int)PointOfInterestType.NaturalArch);
            Assert.Equal(4, (int)PointOfInterestType.HotSpring);
            Assert.Equal(5, (int)PointOfInterestType.MeteorSite);
        }
    }
}
