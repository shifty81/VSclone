using Microsoft.Xna.Framework;
using TimelessTales.Blocks;

namespace TimelessTales.World
{
    /// <summary>
    /// Manages the world, including chunk loading/unloading and block operations
    /// </summary>
    public class WorldManager
    {
        private readonly Dictionary<(int, int), Chunk> _chunks;
        private readonly WorldGenerator _generator;
        private readonly int _seed;
        
        private const int RENDER_DISTANCE = 8; // Chunks
        private Vector3 _spawnPosition;

        public WorldManager(int seed)
        {
            _seed = seed;
            _chunks = new Dictionary<(int, int), Chunk>();
            _generator = new WorldGenerator(seed);
            _spawnPosition = new Vector3(0, 70, 0);
        }

        public void Initialize()
        {
            // Pre-generate spawn chunks
            for (int x = -4; x <= 4; x++)
            {
                for (int z = -4; z <= 4; z++)
                {
                    GetOrCreateChunk(x, z);
                }
            }
            
            // Find suitable spawn position
            _spawnPosition = FindSpawnPosition();
        }

        private Vector3 FindSpawnPosition()
        {
            // Find a safe spawn position near (0, 0)
            for (int y = Chunk.CHUNK_HEIGHT - 1; y >= 0; y--)
            {
                BlockType block = GetBlock(0, y, 0);
                if (BlockRegistry.IsSolid(block))
                {
                    return new Vector3(0.5f, y + 2, 0.5f); // Spawn 2 blocks above solid ground
                }
            }
            return new Vector3(0, 70, 0);
        }

        public Vector3 GetSpawnPosition() => _spawnPosition;

        public void Update(Vector3 playerPosition)
        {
            // Load chunks around player
            int playerChunkX = (int)MathF.Floor(playerPosition.X / Chunk.CHUNK_SIZE);
            int playerChunkZ = (int)MathF.Floor(playerPosition.Z / Chunk.CHUNK_SIZE);

            // Load nearby chunks
            for (int x = playerChunkX - RENDER_DISTANCE; x <= playerChunkX + RENDER_DISTANCE; x++)
            {
                for (int z = playerChunkZ - RENDER_DISTANCE; z <= playerChunkZ + RENDER_DISTANCE; z++)
                {
                    GetOrCreateChunk(x, z);
                }
            }

            // Unload far chunks (simple implementation)
            var chunksToRemove = _chunks.Keys
                .Where(key => Math.Abs(key.Item1 - playerChunkX) > RENDER_DISTANCE + 2 ||
                             Math.Abs(key.Item2 - playerChunkZ) > RENDER_DISTANCE + 2)
                .ToList();

            foreach (var key in chunksToRemove)
            {
                _chunks.Remove(key);
            }
        }

        public Chunk GetOrCreateChunk(int chunkX, int chunkZ)
        {
            var key = (chunkX, chunkZ);
            if (!_chunks.TryGetValue(key, out var chunk))
            {
                chunk = new Chunk(chunkX, chunkZ);
                chunk.Generate(_generator);
                _chunks[key] = chunk;
            }
            return chunk;
        }

        private (int chunkX, int chunkZ, int localX, int localZ) WorldToChunkCoordinates(int worldX, int worldZ)
        {
            int chunkX = (int)MathF.Floor((float)worldX / Chunk.CHUNK_SIZE);
            int chunkZ = (int)MathF.Floor((float)worldZ / Chunk.CHUNK_SIZE);
            
            int localX = worldX - chunkX * Chunk.CHUNK_SIZE;
            int localZ = worldZ - chunkZ * Chunk.CHUNK_SIZE;
            
            if (localX < 0) { localX += Chunk.CHUNK_SIZE; chunkX--; }
            if (localZ < 0) { localZ += Chunk.CHUNK_SIZE; chunkZ--; }
            
            return (chunkX, chunkZ, localX, localZ);
        }

        public BlockType GetBlock(int worldX, int worldY, int worldZ)
        {
            if (worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
                return BlockType.Air;

            var coords = WorldToChunkCoordinates(worldX, worldZ);
            var chunk = GetOrCreateChunk(coords.chunkX, coords.chunkZ);
            return chunk.GetBlock(coords.localX, worldY, coords.localZ);
        }

        public void SetBlock(int worldX, int worldY, int worldZ, BlockType blockType)
        {
            if (worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
                return;

            var coords = WorldToChunkCoordinates(worldX, worldZ);
            var chunk = GetOrCreateChunk(coords.chunkX, coords.chunkZ);
            chunk.SetBlock(coords.localX, worldY, coords.localZ, blockType);
        }

        public IEnumerable<Chunk> GetLoadedChunks()
        {
            return _chunks.Values;
        }

        public bool IsBlockSolid(int worldX, int worldY, int worldZ)
        {
            return BlockRegistry.IsSolid(GetBlock(worldX, worldY, worldZ));
        }

        /// <summary>
        /// Gets the top surface block position and type at the given world coordinates.
        /// Returns the first non-air block from the top down.
        /// </summary>
        public (int y, BlockType blockType) GetTopSurfaceBlock(int worldX, int worldZ)
        {
            // Search from top down for first non-air block
            for (int y = Chunk.CHUNK_HEIGHT - 1; y >= 0; y--)
            {
                BlockType block = GetBlock(worldX, y, worldZ);
                if (block != BlockType.Air)
                {
                    return (y, block);
                }
            }
            return (-1, BlockType.Air);
        }
    }
}
