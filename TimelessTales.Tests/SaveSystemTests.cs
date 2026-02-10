using TimelessTales.Blocks;
using TimelessTales.Core;
using TimelessTales.Entities;
using TimelessTales.World;

namespace TimelessTales.Tests
{
    public class SaveSystemTests : IDisposable
    {
        private readonly string _testDir;

        public SaveSystemTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "TimelessTalesTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        private string GetTestPath() => Path.Combine(_testDir, "test.sav");

        private WorldSaveData CreateTestSaveData()
        {
            return new WorldSaveData
            {
                Seed = 12345,
                SpawnX = 0.5f,
                SpawnY = 70f,
                SpawnZ = 0.5f,
                DayCount = 3,
                TimeOfDay = 0.5f,
                Player = new PlayerSaveData
                {
                    PositionX = 10f,
                    PositionY = 65f,
                    PositionZ = 20f,
                    RotationX = 0.1f,
                    RotationY = 1.5f,
                    Health = 80f,
                    Hunger = 60f,
                    Thirst = 45f,
                    SelectedBlock = BlockType.Dirt,
                    InventoryItems = new Dictionary<BlockType, int>
                    {
                        { BlockType.Stone, 10 },
                        { BlockType.Planks, 5 }
                    },
                    PouchMaterials = new Dictionary<MaterialType, float>
                    {
                        { MaterialType.StoneBits, 50f },
                        { MaterialType.WoodFibers, 25.5f }
                    }
                },
                Chunks = new List<ChunkSaveData>()
            };
        }

        [Fact]
        public void SaveExists_ReturnsFalse_WhenNoSaveFile()
        {
            string path = Path.Combine(_testDir, "nonexistent.sav");
            Assert.False(SaveSystem.SaveExists(path));
        }

        [Fact]
        public void SaveGame_WritesFileToPath()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();

            bool result = SaveSystem.SaveGame(data, path);

            Assert.True(result);
            Assert.True(File.Exists(path));
        }

        [Fact]
        public void LoadGame_ReadsDataCorrectly()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();
            SaveSystem.SaveGame(data, path);

            var loaded = SaveSystem.LoadGame(path);

            Assert.NotNull(loaded);
            Assert.Equal(12345, loaded.Seed);
            Assert.Equal(0.5f, loaded.SpawnX);
            Assert.Equal(70f, loaded.SpawnY);
            Assert.Equal(3, loaded.DayCount);
            Assert.Equal(0.5f, loaded.TimeOfDay);
        }

        [Fact]
        public void RoundTrip_PlayerSaveData_PreservesAllFields()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();
            SaveSystem.SaveGame(data, path);

            var loaded = SaveSystem.LoadGame(path);

            Assert.NotNull(loaded);
            var p = loaded.Player;
            Assert.Equal(10f, p.PositionX);
            Assert.Equal(65f, p.PositionY);
            Assert.Equal(20f, p.PositionZ);
            Assert.Equal(0.1f, p.RotationX);
            Assert.Equal(1.5f, p.RotationY);
            Assert.Equal(80f, p.Health);
            Assert.Equal(60f, p.Hunger);
            Assert.Equal(45f, p.Thirst);
            Assert.Equal(BlockType.Dirt, p.SelectedBlock);
            Assert.Equal(10, p.InventoryItems[BlockType.Stone]);
            Assert.Equal(5, p.InventoryItems[BlockType.Planks]);
            Assert.Equal(50f, p.PouchMaterials[MaterialType.StoneBits]);
            Assert.Equal(25.5f, p.PouchMaterials[MaterialType.WoodFibers]);
        }

        [Fact]
        public void RoundTrip_ChunkData_PreservesBlocks()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();

            var chunkData = new ChunkSaveData
            {
                ChunkX = 1,
                ChunkZ = -2,
                Blocks = new BlockType[Chunk.CHUNK_SIZE, Chunk.CHUNK_HEIGHT, Chunk.CHUNK_SIZE]
            };
            // Set some specific blocks
            chunkData.Blocks[0, 0, 0] = BlockType.Stone;
            chunkData.Blocks[5, 64, 5] = BlockType.Grass;
            chunkData.Blocks[15, 255, 15] = BlockType.Water;
            data.Chunks.Add(chunkData);

            SaveSystem.SaveGame(data, path);
            var loaded = SaveSystem.LoadGame(path);

            Assert.NotNull(loaded);
            Assert.Single(loaded.Chunks);
            var c = loaded.Chunks[0];
            Assert.Equal(1, c.ChunkX);
            Assert.Equal(-2, c.ChunkZ);
            Assert.Equal(BlockType.Stone, c.Blocks[0, 0, 0]);
            Assert.Equal(BlockType.Grass, c.Blocks[5, 64, 5]);
            Assert.Equal(BlockType.Water, c.Blocks[15, 255, 15]);
            Assert.Equal(BlockType.Air, c.Blocks[8, 128, 8]);
        }

        [Fact]
        public void RLE_HandlesRunsOfSameBlock()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();

            // All air chunk - should compress extremely well
            var chunkData = new ChunkSaveData
            {
                ChunkX = 0,
                ChunkZ = 0,
                Blocks = new BlockType[Chunk.CHUNK_SIZE, Chunk.CHUNK_HEIGHT, Chunk.CHUNK_SIZE]
            };
            data.Chunks.Add(chunkData);

            SaveSystem.SaveGame(data, path);
            var loaded = SaveSystem.LoadGame(path);

            Assert.NotNull(loaded);
            Assert.Single(loaded.Chunks);
            // Verify all blocks are Air
            var blocks = loaded.Chunks[0].Blocks;
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                        Assert.Equal(BlockType.Air, blocks[x, y, z]);
        }

        [Fact]
        public void RLE_HandlesAlternatingBlocks()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();

            var chunkData = new ChunkSaveData
            {
                ChunkX = 0,
                ChunkZ = 0,
                Blocks = new BlockType[Chunk.CHUNK_SIZE, Chunk.CHUNK_HEIGHT, Chunk.CHUNK_SIZE]
            };
            // Create an alternating pattern in the z-direction for the first row
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                chunkData.Blocks[0, 0, z] = z % 2 == 0 ? BlockType.Stone : BlockType.Dirt;
            }
            data.Chunks.Add(chunkData);

            SaveSystem.SaveGame(data, path);
            var loaded = SaveSystem.LoadGame(path);

            Assert.NotNull(loaded);
            var blocks = loaded.Chunks[0].Blocks;
            for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
            {
                Assert.Equal(z % 2 == 0 ? BlockType.Stone : BlockType.Dirt, blocks[0, 0, z]);
            }
        }

        [Fact]
        public void LoadGame_ReturnsNull_ForMissingFile()
        {
            string path = Path.Combine(_testDir, "missing.sav");
            var result = SaveSystem.LoadGame(path);
            Assert.Null(result);
        }

        [Fact]
        public void LoadGame_ReturnsNull_ForCorruptData()
        {
            string path = GetTestPath();
            File.WriteAllBytes(path, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x02 });

            var result = SaveSystem.LoadGame(path);
            Assert.Null(result);
        }

        [Fact]
        public void SaveGame_WithEmptyInventory()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();
            data.Player.InventoryItems.Clear();
            data.Player.PouchMaterials.Clear();

            SaveSystem.SaveGame(data, path);
            var loaded = SaveSystem.LoadGame(path);

            Assert.NotNull(loaded);
            Assert.Empty(loaded.Player.InventoryItems);
            Assert.Empty(loaded.Player.PouchMaterials);
        }

        [Fact]
        public void SaveGame_WithEmptyWorld_NoChunks()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();
            data.Chunks.Clear();

            SaveSystem.SaveGame(data, path);
            var loaded = SaveSystem.LoadGame(path);

            Assert.NotNull(loaded);
            Assert.Empty(loaded.Chunks);
        }

        [Fact]
        public void SaveGame_WithMultipleChunks()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();

            for (int i = 0; i < 3; i++)
            {
                var chunkData = new ChunkSaveData
                {
                    ChunkX = i,
                    ChunkZ = -i,
                    Blocks = new BlockType[Chunk.CHUNK_SIZE, Chunk.CHUNK_HEIGHT, Chunk.CHUNK_SIZE]
                };
                chunkData.Blocks[0, 0, 0] = (BlockType)(i + 1);
                data.Chunks.Add(chunkData);
            }

            SaveSystem.SaveGame(data, path);
            var loaded = SaveSystem.LoadGame(path);

            Assert.NotNull(loaded);
            Assert.Equal(3, loaded.Chunks.Count);
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(i, loaded.Chunks[i].ChunkX);
                Assert.Equal(-i, loaded.Chunks[i].ChunkZ);
                Assert.Equal((BlockType)(i + 1), loaded.Chunks[i].Blocks[0, 0, 0]);
            }
        }

        [Fact]
        public void SaveExists_ReturnsTrue_AfterSave()
        {
            string path = GetTestPath();
            var data = CreateTestSaveData();
            SaveSystem.SaveGame(data, path);

            Assert.True(SaveSystem.SaveExists(path));
        }

        [Fact]
        public void ChunkGetBlockRaw_ReturnsCorrectBlock()
        {
            var chunk = new Chunk(0, 0);
            chunk.SetBlock(5, 10, 3, BlockType.Granite);

            Assert.Equal(BlockType.Granite, chunk.GetBlockRaw(5, 10, 3));
            Assert.Equal(BlockType.Air, chunk.GetBlockRaw(0, 0, 0));
        }

        [Fact]
        public void ChunkConstructor_WithIsGenerated_SetsFlag()
        {
            var chunk = new Chunk(1, 2, true);

            Assert.True(chunk.IsGenerated);
            Assert.Equal(1, chunk.ChunkX);
            Assert.Equal(2, chunk.ChunkZ);
        }

        [Fact]
        public void WorldManager_LoadChunk_AddsChunkToWorld()
        {
            var wm = new WorldManager(42);
            var chunk = new Chunk(5, 5, true);
            chunk.SetBlock(0, 0, 0, BlockType.IronOre);

            wm.LoadChunk(chunk);

            var loaded = wm.GetChunkIfLoaded(5, 5);
            Assert.NotNull(loaded);
            Assert.Equal(BlockType.IronOre, loaded.GetBlock(0, 0, 0));
        }

        [Fact]
        public void WorldManager_Seed_ReturnsCorrectSeed()
        {
            var wm = new WorldManager(99999);
            Assert.Equal(99999, wm.Seed);
        }
    }
}
