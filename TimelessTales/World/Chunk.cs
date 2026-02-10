using TimelessTales.Blocks;

namespace TimelessTales.World
{
    /// <summary>
    /// Represents a single chunk in the world (16x256x16 blocks)
    /// </summary>
    public class Chunk
    {
        public const int CHUNK_SIZE = 16;
        public const int CHUNK_HEIGHT = 256;
        
        public int ChunkX { get; private set; }
        public int ChunkZ { get; private set; }
        
        private readonly BlockType[,,] _blocks;
        public bool IsGenerated { get; private set; }
        public bool NeedsMeshRebuild { get; set; }

        public Chunk(int chunkX, int chunkZ)
        {
            ChunkX = chunkX;
            ChunkZ = chunkZ;
            _blocks = new BlockType[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
            IsGenerated = false;
            NeedsMeshRebuild = true;
        }

        public BlockType GetBlock(int x, int y, int z)
        {
            if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_HEIGHT || z < 0 || z >= CHUNK_SIZE)
                return BlockType.Air;
            
            return _blocks[x, y, z];
        }

        public void SetBlock(int x, int y, int z, BlockType blockType)
        {
            if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_HEIGHT || z < 0 || z >= CHUNK_SIZE)
                return;
            
            _blocks[x, y, z] = blockType;
            NeedsMeshRebuild = true;
        }
        
        /// <summary>
        /// Set a block without flagging mesh rebuild - use during bulk generation only
        /// </summary>
        public void SetBlockFast(int x, int y, int z, BlockType blockType)
        {
            if (x < 0 || x >= CHUNK_SIZE || y < 0 || y >= CHUNK_HEIGHT || z < 0 || z >= CHUNK_SIZE)
                return;
            
            _blocks[x, y, z] = blockType;
        }

        /// <summary>
        /// Creates a chunk with pre-loaded data (for save/load system)
        /// </summary>
        public Chunk(int chunkX, int chunkZ, bool isGenerated) : this(chunkX, chunkZ)
        {
            IsGenerated = isGenerated;
        }

        /// <summary>
        /// Gets the raw block at specified position for serialization.
        /// </summary>
        public BlockType GetBlockRaw(int x, int y, int z) => _blocks[x, y, z];

        public void Generate(WorldGenerator generator)
        {
            generator.GenerateChunk(this);
            IsGenerated = true;
            NeedsMeshRebuild = true;
        }
    }
}
