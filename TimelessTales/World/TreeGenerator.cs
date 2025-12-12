using TimelessTales.Blocks;
using System;

namespace TimelessTales.World
{
    /// <summary>
    /// Tree type for generation
    /// </summary>
    public enum TreeType
    {
        Oak,
        Pine,
        Birch
    }

    /// <summary>
    /// Generates trees in the world
    /// </summary>
    public static class TreeGenerator
    {
        private static int _seed = 0;

        public static void SetSeed(int seed)
        {
            _seed = seed;
        }

        /// <summary>
        /// Generate a tree at the specified position in the chunk
        /// </summary>
        public static void GenerateTree(Chunk chunk, int localX, int baseY, int localZ, TreeType treeType)
        {
            // Create a deterministic Random based on position and seed for thread safety
            Random random = new Random(localX * 73856093 ^ baseY * 19349663 ^ localZ * 83492791 ^ _seed);
            
            switch (treeType)
            {
                case TreeType.Oak:
                    GenerateOakTree(chunk, localX, baseY, localZ, random);
                    break;
                case TreeType.Pine:
                    GeneratePineTree(chunk, localX, baseY, localZ, random);
                    break;
                case TreeType.Birch:
                    GenerateBirchTree(chunk, localX, baseY, localZ, random);
                    break;
            }
        }

        private static void GenerateOakTree(Chunk chunk, int x, int baseY, int z, Random random)
        {
            // Oak tree: 4-6 blocks tall trunk with wide canopy
            int trunkHeight = 4 + random.Next(3);
            
            // Generate trunk
            for (int y = 0; y < trunkHeight; y++)
            {
                if (baseY + y < Chunk.CHUNK_HEIGHT)
                    SetBlockSafe(chunk, x, baseY + y, z, BlockType.OakLog);
            }
            
            // Generate canopy (spherical shape)
            int canopyY = baseY + trunkHeight;
            int canopyRadius = 2 + random.Next(2);
            
            for (int dy = -canopyRadius; dy <= canopyRadius + 1; dy++)
            {
                for (int dx = -canopyRadius; dx <= canopyRadius; dx++)
                {
                    for (int dz = -canopyRadius; dz <= canopyRadius; dz++)
                    {
                        // Create spherical canopy
                        if (dx * dx + dy * dy + dz * dz <= canopyRadius * canopyRadius)
                        {
                            // Don't replace trunk
                            if (!(dx == 0 && dz == 0 && dy <= 0))
                            {
                                int leafY = canopyY + dy;
                                if (leafY >= 0 && leafY < Chunk.CHUNK_HEIGHT)
                                {
                                    // 80% chance to place leaf (create some gaps)
                                    if (random.NextDouble() < 0.8)
                                    {
                                        SetBlockSafe(chunk, x + dx, leafY, z + dz, BlockType.OakLeaves);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void GeneratePineTree(Chunk chunk, int x, int baseY, int z, Random random)
        {
            // Pine tree: 6-10 blocks tall with conical shape
            int trunkHeight = 6 + random.Next(5);
            
            // Generate trunk
            for (int y = 0; y < trunkHeight; y++)
            {
                if (baseY + y < Chunk.CHUNK_HEIGHT)
                    SetBlockSafe(chunk, x, baseY + y, z, BlockType.PineLog);
            }
            
            // Generate conical canopy
            int canopyStart = baseY + 2;
            int canopyLevels = trunkHeight - 2;
            
            for (int level = 0; level < canopyLevels; level++)
            {
                int y = canopyStart + level;
                // Radius decreases as we go up (conical shape)
                int radius = Math.Max(1, canopyLevels / 2 - level / 2);
                
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        // Create circular layers
                        if (dx * dx + dz * dz <= radius * radius)
                        {
                            // Don't replace trunk
                            if (!(dx == 0 && dz == 0))
                            {
                                if (y >= 0 && y < Chunk.CHUNK_HEIGHT)
                                {
                                    SetBlockSafe(chunk, x + dx, y, z + dz, BlockType.PineLeaves);
                                }
                            }
                        }
                    }
                }
            }
            
            // Add a point at the top
            if (baseY + trunkHeight < Chunk.CHUNK_HEIGHT)
            {
                SetBlockSafe(chunk, x, baseY + trunkHeight, z, BlockType.PineLeaves);
            }
        }

        private static void GenerateBirchTree(Chunk chunk, int x, int baseY, int z, Random random)
        {
            // Birch tree: 5-7 blocks tall, similar to oak but slightly taller and narrower
            int trunkHeight = 5 + random.Next(3);
            
            // Generate trunk
            for (int y = 0; y < trunkHeight; y++)
            {
                if (baseY + y < Chunk.CHUNK_HEIGHT)
                    SetBlockSafe(chunk, x, baseY + y, z, BlockType.BirchLog);
            }
            
            // Generate canopy (slightly smaller than oak)
            int canopyY = baseY + trunkHeight - 1;
            int canopyRadius = 2;
            
            for (int dy = -1; dy <= 2; dy++)
            {
                for (int dx = -canopyRadius; dx <= canopyRadius; dx++)
                {
                    for (int dz = -canopyRadius; dz <= canopyRadius; dz++)
                    {
                        // Create rounded canopy
                        if (dx * dx + dz * dz + dy * dy <= canopyRadius * canopyRadius + 1)
                        {
                            // Don't replace trunk
                            if (!(dx == 0 && dz == 0 && dy <= 0))
                            {
                                int leafY = canopyY + dy;
                                if (leafY >= 0 && leafY < Chunk.CHUNK_HEIGHT)
                                {
                                    if (random.NextDouble() < 0.85)
                                    {
                                        SetBlockSafe(chunk, x + dx, leafY, z + dz, BlockType.BirchLeaves);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Safely set a block, checking bounds
        /// </summary>
        private static void SetBlockSafe(Chunk chunk, int x, int y, int z, BlockType blockType)
        {
            // Only set if within chunk bounds
            if (x >= 0 && x < Chunk.CHUNK_SIZE && 
                y >= 0 && y < Chunk.CHUNK_HEIGHT && 
                z >= 0 && z < Chunk.CHUNK_SIZE)
            {
                // Only replace air
                if (chunk.GetBlock(x, y, z) == BlockType.Air)
                {
                    chunk.SetBlock(x, y, z, blockType);
                }
            }
        }

        /// <summary>
        /// Get appropriate tree type for a biome
        /// </summary>
        public static TreeType GetTreeTypeForBiome(BiomeType biome)
        {
            // Use deterministic selection based on biome
            return biome switch
            {
                BiomeType.Tundra => TreeType.Pine,
                BiomeType.Boreal => TreeType.Pine,
                BiomeType.Temperate => TreeType.Oak, // Could vary with sub-biomes
                BiomeType.Desert => TreeType.Oak, // Rare trees in desert
                BiomeType.Tropical => TreeType.Oak,
                _ => TreeType.Oak
            };
        }
    }
}
