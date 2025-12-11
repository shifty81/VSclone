using TimelessTales.Blocks;
using TimelessTales.Utils;

namespace TimelessTales.World
{
    /// <summary>
    /// Generates world terrain with realistic geology and biomes
    /// Inspired by Vintage Story's geological realism
    /// </summary>
    public class WorldGenerator
    {
        private readonly int _seed;
        private readonly SimplexNoise _terrainNoise;
        private readonly SimplexNoise _stoneLayerNoise;
        private readonly SimplexNoise _oreNoise;
        private readonly SimplexNoise _caveNoise;
        private readonly SimplexNoise _biomeNoise;

        // Terrain generation parameters
        private const int SEA_LEVEL = 64;
        private const int TERRAIN_HEIGHT_VARIATION = 40;
        private const float TERRAIN_SCALE = 0.01f;
        private const float STONE_LAYER_SCALE = 0.05f;

        public WorldGenerator(int seed)
        {
            _seed = seed;
            _terrainNoise = new SimplexNoise(seed);
            _stoneLayerNoise = new SimplexNoise(seed + 1);
            _oreNoise = new SimplexNoise(seed + 2);
            _caveNoise = new SimplexNoise(seed + 3);
            _biomeNoise = new SimplexNoise(seed + 4);
        }

        public void GenerateChunk(Chunk chunk)
        {
            int worldX = chunk.ChunkX * Chunk.CHUNK_SIZE;
            int worldZ = chunk.ChunkZ * Chunk.CHUNK_SIZE;

            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    int wx = worldX + x;
                    int wz = worldZ + z;

                    // Get terrain height
                    int surfaceHeight = GetTerrainHeight(wx, wz);
                    
                    // Get biome
                    BiomeType biome = GetBiome(wx, wz);

                    // Generate vertical column
                    for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                    {
                        BlockType block = GenerateBlock(wx, y, wz, surfaceHeight, biome);
                        chunk.SetBlock(x, y, z, block);
                    }
                }
            }
        }

        private int GetTerrainHeight(int worldX, int worldZ)
        {
            // Multi-octave noise for varied terrain
            float height = 0;
            float amplitude = 1.0f;
            float frequency = TERRAIN_SCALE;
            
            // Base terrain
            height += _terrainNoise.Evaluate(worldX * frequency, worldZ * frequency) * amplitude * TERRAIN_HEIGHT_VARIATION;
            
            // Add detail
            frequency *= 2;
            amplitude *= 0.5f;
            height += _terrainNoise.Evaluate(worldX * frequency, worldZ * frequency) * amplitude * TERRAIN_HEIGHT_VARIATION;
            
            return SEA_LEVEL + (int)height;
        }

        private BiomeType GetBiome(int worldX, int worldZ)
        {
            float biomeValue = _biomeNoise.Evaluate(worldX * 0.003f, worldZ * 0.003f);
            
            if (biomeValue < -0.3f) return BiomeType.Tundra;
            if (biomeValue < -0.1f) return BiomeType.Boreal;
            if (biomeValue < 0.2f) return BiomeType.Temperate;
            if (biomeValue < 0.5f) return BiomeType.Desert;
            return BiomeType.Tropical;
        }

        private BlockType GenerateBlock(int worldX, int y, int worldZ, int surfaceHeight, BiomeType biome)
        {
            // Air above surface
            if (y > surfaceHeight)
                return BlockType.Air;

            // Generate caves
            if (y < surfaceHeight - 3 && y > 5)
            {
                float caveValue = _caveNoise.Evaluate(worldX * 0.05f, y * 0.05f, worldZ * 0.05f);
                if (caveValue > 0.6f)
                    return BlockType.Air;
            }

            // Surface layer
            if (y == surfaceHeight)
            {
                return GetSurfaceBlock(biome);
            }

            // Subsurface layers (0-4 blocks deep)
            if (y > surfaceHeight - 4)
            {
                return BlockType.Dirt;
            }

            // Geological rock layers - realistic strata
            return GetRockLayer(y, worldX, worldZ);
        }

        private BlockType GetSurfaceBlock(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Tundra => BlockType.Gravel,
                BiomeType.Boreal => BlockType.Grass,
                BiomeType.Temperate => BlockType.Grass,
                BiomeType.Desert => BlockType.Sand,
                BiomeType.Tropical => BlockType.Grass,
                _ => BlockType.Grass
            };
        }

        private BlockType GetRockLayer(int y, int worldX, int worldZ)
        {
            // Realistic geological layers with ore deposits
            // Deeper layers have different rock types
            
            float layerNoise = _stoneLayerNoise.Evaluate(worldX * STONE_LAYER_SCALE, 
                                                          y * STONE_LAYER_SCALE * 2, 
                                                          worldZ * STONE_LAYER_SCALE);

            // Determine base rock type by depth
            BlockType baseRock;
            if (y > 45)
            {
                // Upper crust - sedimentary rocks
                baseRock = layerNoise > 0 ? BlockType.Limestone : BlockType.Sandstone;
            }
            else if (y > 25)
            {
                // Mid crust - metamorphic
                baseRock = layerNoise > 0 ? BlockType.Slate : BlockType.Stone;
            }
            else if (y > 10)
            {
                // Lower crust - igneous
                baseRock = layerNoise > 0.3f ? BlockType.Granite : BlockType.Basalt;
            }
            else
            {
                // Deep layer - hard igneous
                baseRock = BlockType.Basalt;
            }

            // Add ore deposits based on rock type and depth
            float oreValue = _oreNoise.Evaluate(worldX * 0.1f, y * 0.1f, worldZ * 0.1f);
            
            // Copper ore in sedimentary layers (y: 30-60)
            if (y > 30 && y < 60 && baseRock == BlockType.Limestone && oreValue > 0.85f)
                return BlockType.CopperOre;
            
            // Tin ore in granite (y: 15-35)
            if (y > 15 && y < 35 && baseRock == BlockType.Granite && oreValue > 0.88f)
                return BlockType.TinOre;
            
            // Iron ore deep in basalt (y: 5-30)
            if (y > 5 && y < 30 && baseRock == BlockType.Basalt && oreValue > 0.87f)
                return BlockType.IronOre;
            
            // Coal in sedimentary layers (y: 40-70)
            if (y > 40 && y < 70 && baseRock == BlockType.Sandstone && oreValue > 0.83f)
                return BlockType.Coal;

            return baseRock;
        }
    }

    public enum BiomeType
    {
        Tundra,
        Boreal,
        Temperate,
        Desert,
        Tropical
    }
}
