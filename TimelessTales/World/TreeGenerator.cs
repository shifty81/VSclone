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
            // Oak tree: 5-7 blocks tall trunk with wide, rounded canopy
            int trunkHeight = 5 + random.Next(3);
            
            // Generate trunk with slight variations for more organic look
            for (int y = 0; y < trunkHeight; y++)
            {
                if (baseY + y < Chunk.CHUNK_HEIGHT)
                {
                    SetBlockSafe(chunk, x, baseY + y, z, BlockType.OakLog);
                    
                    // Add occasional trunk thickness at base for more rounded appearance
                    if (y < 2 && random.NextDouble() < 0.3)
                    {
                        int offset = random.Next(2) == 0 ? 1 : -1;
                        if (random.Next(2) == 0)
                            SetBlockSafe(chunk, x + offset, baseY + y, z, BlockType.OakLog);
                        else
                            SetBlockSafe(chunk, x, baseY + y, z + offset, BlockType.OakLog);
                    }
                }
            }
            
            // Generate canopy (more rounded, organic shape)
            int canopyY = baseY + trunkHeight - 1; // Start canopy slightly lower
            int canopyRadius = 2 + random.Next(2);
            
            for (int dy = -canopyRadius; dy <= canopyRadius + 1; dy++)
            {
                for (int dx = -canopyRadius; dx <= canopyRadius; dx++)
                {
                    for (int dz = -canopyRadius; dz <= canopyRadius; dz++)
                    {
                        // Create more organic spherical canopy with distance calculation
                        float distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
                        float adjustedRadius = canopyRadius + (float)(random.NextDouble() - 0.5);
                        
                        if (distance <= adjustedRadius)
                        {
                            // Don't replace trunk center
                            if (!(dx == 0 && dz == 0 && dy <= 1))
                            {
                                int leafY = canopyY + dy;
                                if (leafY >= 0 && leafY < Chunk.CHUNK_HEIGHT)
                                {
                                    // Vary leaf density - denser in center, sparser at edges
                                    float densityChance = 0.9f - (distance / adjustedRadius) * 0.3f;
                                    if (random.NextDouble() < densityChance)
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
            // Pine tree: 7-12 blocks tall with natural conical shape
            int trunkHeight = 7 + random.Next(6);
            
            // Generate trunk with slight base widening
            for (int y = 0; y < trunkHeight; y++)
            {
                if (baseY + y < Chunk.CHUNK_HEIGHT)
                {
                    SetBlockSafe(chunk, x, baseY + y, z, BlockType.PineLog);
                    
                    // Widen trunk at base for stability
                    if (y == 0 && random.NextDouble() < 0.4)
                    {
                        SetBlockSafe(chunk, x + 1, baseY + y, z, BlockType.PineLog);
                        SetBlockSafe(chunk, x - 1, baseY + y, z, BlockType.PineLog);
                        SetBlockSafe(chunk, x, baseY + y, z + 1, BlockType.PineLog);
                        SetBlockSafe(chunk, x, baseY + y, z - 1, BlockType.PineLog);
                    }
                }
            }
            
            // Generate natural conical canopy with layered appearance
            int canopyStart = baseY + 2;
            int canopyLevels = trunkHeight - 2;
            
            for (int level = 0; level < canopyLevels; level++)
            {
                int y = canopyStart + level;
                // Create stepped conical shape - larger steps at top for natural look
                float radiusFloat = (canopyLevels - level) * 0.4f + 1.0f;
                int radius = (int)MathF.Ceiling(radiusFloat);
                
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        // Create slightly irregular circular layers
                        float distance = MathF.Sqrt(dx * dx + dz * dz);
                        float variance = (float)(random.NextDouble() * 0.5);
                        
                        if (distance <= radius + variance)
                        {
                            // Don't replace trunk
                            if (!(dx == 0 && dz == 0))
                            {
                                if (y >= 0 && y < Chunk.CHUNK_HEIGHT)
                                {
                                    // Slightly sparse at edges for natural look
                                    if (distance < radius || random.NextDouble() < 0.7)
                                    {
                                        SetBlockSafe(chunk, x + dx, y, z + dz, BlockType.PineLeaves);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Add pointed top (1-2 blocks)
            if (baseY + trunkHeight < Chunk.CHUNK_HEIGHT)
            {
                SetBlockSafe(chunk, x, baseY + trunkHeight, z, BlockType.PineLeaves);
                if (random.NextDouble() < 0.5 && baseY + trunkHeight + 1 < Chunk.CHUNK_HEIGHT)
                {
                    SetBlockSafe(chunk, x, baseY + trunkHeight + 1, z, BlockType.PineLeaves);
                }
            }
        }

        private static void GenerateBirchTree(Chunk chunk, int x, int baseY, int z, Random random)
        {
            // Birch tree: 6-8 blocks tall, slender with narrow canopy
            int trunkHeight = 6 + random.Next(3);
            
            // Generate slender trunk (birch are typically narrow and tall)
            for (int y = 0; y < trunkHeight; y++)
            {
                if (baseY + y < Chunk.CHUNK_HEIGHT)
                    SetBlockSafe(chunk, x, baseY + y, z, BlockType.BirchLog);
            }
            
            // Generate canopy (narrower and taller than oak - characteristic birch shape)
            int canopyY = baseY + trunkHeight - 1;
            int canopyRadius = 2;
            
            // Birch has a more vertical, narrow crown
            for (int dy = -1; dy <= 3; dy++)
            {
                // Radius varies with height - narrow at bottom, widest in middle, narrow at top
                int layerRadius = canopyRadius;
                if (dy == -1 || dy == 3)
                    layerRadius = 1;
                else if (dy == 0 || dy == 2)
                    layerRadius = 2;
                else if (dy == 1)
                    layerRadius = 2; // Widest in middle
                
                for (int dx = -layerRadius; dx <= layerRadius; dx++)
                {
                    for (int dz = -layerRadius; dz <= layerRadius; dz++)
                    {
                        float distance = MathF.Sqrt(dx * dx + dz * dz);
                        
                        // Create slightly irregular rounded canopy
                        if (distance <= layerRadius)
                        {
                            // Don't replace trunk
                            if (!(dx == 0 && dz == 0 && dy <= 0))
                            {
                                int leafY = canopyY + dy;
                                if (leafY >= 0 && leafY < Chunk.CHUNK_HEIGHT)
                                {
                                    // Dense foliage with slight variation
                                    float densityChance = 0.9f - (distance / layerRadius) * 0.2f;
                                    if (random.NextDouble() < densityChance)
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
