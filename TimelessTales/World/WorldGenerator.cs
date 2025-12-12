using TimelessTales.Blocks;
using TimelessTales.Utils;
using System;

namespace TimelessTales.World
{
    /// <summary>
    /// Generates world terrain with realistic geology and biomes
    /// Features: continents, islands, rivers, lakes, oceans, trees, erosion
    /// Inspired by Vintage Story's geological realism
    /// </summary>
    public class WorldGenerator
    {
        private readonly int _seed;
        private readonly SimplexNoise _terrainNoise;
        private readonly SimplexNoise _continentNoise;
        private readonly SimplexNoise _erosionNoise;
        private readonly SimplexNoise _moistureNoise;
        private readonly SimplexNoise _temperatureNoise;
        private readonly SimplexNoise _stoneLayerNoise;
        private readonly SimplexNoise _oreNoise;
        private readonly SimplexNoise _caveNoise;
        private readonly SimplexNoise _riverNoise;
        private readonly Random _random;

        // Terrain generation parameters (adjusted for realistic topography)
        private const int SEA_LEVEL = 64;
        private const int MAX_TERRAIN_HEIGHT = 30; // Reduced from 40 to make less extreme mountains
        private const float CONTINENT_SCALE = 0.0008f; // Large scale for continents
        private const float TERRAIN_SCALE = 0.008f; // Reduced for smoother terrain
        private const float DETAIL_SCALE = 0.02f;
        private const float EROSION_SCALE = 0.015f;
        private const float MOISTURE_SCALE = 0.005f;
        private const float TEMPERATURE_SCALE = 0.004f;
        private const float STONE_LAYER_SCALE = 0.05f;

        public WorldGenerator(int seed)
        {
            _seed = seed;
            _random = new Random(seed);
            _terrainNoise = new SimplexNoise(seed);
            _continentNoise = new SimplexNoise(seed + 1);
            _erosionNoise = new SimplexNoise(seed + 2);
            _moistureNoise = new SimplexNoise(seed + 3);
            _temperatureNoise = new SimplexNoise(seed + 4);
            _stoneLayerNoise = new SimplexNoise(seed + 5);
            _oreNoise = new SimplexNoise(seed + 6);
            _caveNoise = new SimplexNoise(seed + 7);
            _riverNoise = new SimplexNoise(seed + 8);
            
            TreeGenerator.SetSeed(seed + 9);
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

                    // Get terrain parameters
                    int surfaceHeight = GetTerrainHeight(wx, wz);
                    float moisture = GetMoisture(wx, wz, surfaceHeight);
                    float temperature = GetTemperature(wx, wz, surfaceHeight);
                    BiomeType biome = GetBiome(moisture, temperature, surfaceHeight);

                    // Generate vertical column
                    for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                    {
                        BlockType block = GenerateBlock(wx, y, wz, surfaceHeight, biome);
                        chunk.SetBlock(x, y, z, block);
                    }
                    
                    // Generate trees
                    if (ShouldPlaceTree(wx, wz, surfaceHeight, biome))
                    {
                        TreeType treeType = TreeGenerator.GetTreeTypeForBiome(biome);
                        TreeGenerator.GenerateTree(chunk, x, surfaceHeight + 1, z, treeType);
                    }
                }
            }
        }

        private int GetTerrainHeight(int worldX, int worldZ)
        {
            // Continent-scale landmasses
            float continentValue = _continentNoise.Evaluate(worldX * CONTINENT_SCALE, worldZ * CONTINENT_SCALE);
            
            // Create continents and ocean basins
            // Values < -0.2 = deep ocean, -0.2 to 0 = shallow ocean/coast, > 0 = land
            float continentHeight = 0;
            if (continentValue < -0.2f)
            {
                // Deep ocean
                continentHeight = (continentValue + 0.2f) * 15; // Up to -15 below sea level
            }
            else if (continentValue < 0)
            {
                // Shallow ocean / continental shelf
                continentHeight = continentValue * 10; // 0 to -10
            }
            else
            {
                // Land - smooth transition from coast to highlands
                continentHeight = continentValue * 25; // 0 to 25 above sea level
            }
            
            // Base terrain with multiple octaves
            float terrainHeight = 0;
            float amplitude = 1.0f;
            float frequency = TERRAIN_SCALE;
            float totalAmplitude = 0;
            
            // Multiple octaves for varied terrain (fewer octaves = smoother terrain)
            for (int octave = 0; octave < 3; octave++)
            {
                terrainHeight += _terrainNoise.Evaluate(worldX * frequency, worldZ * frequency) * amplitude;
                totalAmplitude += amplitude;
                frequency *= 2.0f;
                amplitude *= 0.5f;
            }
            
            terrainHeight = (terrainHeight / totalAmplitude) * MAX_TERRAIN_HEIGHT;
            
            // Apply erosion simulation (lower in valleys, preserve peaks)
            float erosion = _erosionNoise.Evaluate(worldX * EROSION_SCALE, worldZ * EROSION_SCALE);
            float erosionFactor = Math.Clamp(erosion, -0.5f, 0.5f);
            terrainHeight *= (1.0f - erosionFactor * 0.3f); // Reduce height by up to 30% in eroded areas
            
            // River carving - create deep valleys for rivers
            float riverValue = _riverNoise.Evaluate(worldX * 0.01f, worldZ * 0.01f);
            if (Math.Abs(riverValue) < 0.05f && continentValue > 0)
            {
                // River valley
                float riverDepth = (0.05f - Math.Abs(riverValue)) / 0.05f;
                terrainHeight -= riverDepth * 12; // Carve river valley
            }
            
            // Combine continent shape with local terrain
            float finalHeight = continentHeight + terrainHeight;
            
            return SEA_LEVEL + (int)finalHeight;
        }

        private float GetMoisture(int worldX, int worldZ, int height)
        {
            float moisture = _moistureNoise.Evaluate(worldX * MOISTURE_SCALE, worldZ * MOISTURE_SCALE);
            
            // Proximity to water increases moisture
            if (height < SEA_LEVEL)
            {
                moisture += 0.5f;
            }
            else if (height < SEA_LEVEL + 5)
            {
                moisture += 0.3f; // Coastal areas are more moist
            }
            
            // Height reduces moisture (rain shadow effect)
            float heightFactor = Math.Clamp((height - SEA_LEVEL) / 30.0f, 0, 1);
            moisture -= heightFactor * 0.3f;
            
            return Math.Clamp(moisture, -1, 1);
        }

        private float GetTemperature(int worldX, int worldZ, int height)
        {
            float temperature = _temperatureNoise.Evaluate(worldX * TEMPERATURE_SCALE, worldZ * TEMPERATURE_SCALE);
            
            // Height reduces temperature
            float heightFactor = Math.Clamp((height - SEA_LEVEL) / 40.0f, 0, 1);
            temperature -= heightFactor * 0.6f;
            
            return Math.Clamp(temperature, -1, 1);
        }

        private BiomeType GetBiome(float moisture, float temperature, int height)
        {
            // Ocean biome
            if (height < SEA_LEVEL - 2)
            {
                return BiomeType.Ocean;
            }
            
            // Temperature-based biomes with moisture variation
            if (temperature < -0.4f)
            {
                return BiomeType.Tundra;
            }
            else if (temperature < -0.1f)
            {
                return BiomeType.Boreal;
            }
            else if (temperature < 0.3f)
            {
                // Temperate zone - moisture matters
                if (moisture < -0.2f)
                    return BiomeType.Desert;
                else
                    return BiomeType.Temperate;
            }
            else
            {
                // Warm zone
                if (moisture < -0.1f)
                    return BiomeType.Desert;
                else
                    return BiomeType.Tropical;
            }
        }

        private BlockType GenerateBlock(int worldX, int y, int worldZ, int surfaceHeight, BiomeType biome)
        {
            // Water at sea level and below
            if (y > surfaceHeight && y <= SEA_LEVEL)
            {
                return biome == BiomeType.Ocean ? BlockType.Saltwater : BlockType.Water;
            }
            
            // Air above surface/water
            if (y > surfaceHeight && y > SEA_LEVEL)
                return BlockType.Air;

            // Generate larger, more connected caves
            if (y < surfaceHeight - 3 && y > 5)
            {
                float caveValue = _caveNoise.Evaluate(worldX * 0.04f, y * 0.04f, worldZ * 0.04f);
                if (caveValue > 0.55f) // Lowered threshold for larger caves
                    return BlockType.Air;
            }

            // Surface layer
            if (y == surfaceHeight)
            {
                // Underwater surface
                if (surfaceHeight < SEA_LEVEL)
                {
                    return biome == BiomeType.Ocean ? BlockType.Sand : BlockType.Clay; // Ocean floor
                }
                return GetSurfaceBlock(biome);
            }

            // Subsurface layers (0-4 blocks deep)
            if (y > surfaceHeight - 4 && y < surfaceHeight)
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
                BiomeType.Ocean => BlockType.Sand,
                _ => BlockType.Grass
            };
        }

        private BlockType GetRockLayer(int y, int worldX, int worldZ)
        {
            // Realistic geological layers with ore deposits
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

        private bool ShouldPlaceTree(int worldX, int worldZ, int surfaceHeight, BiomeType biome)
        {
            // No trees in ocean or underwater
            if (surfaceHeight <= SEA_LEVEL || biome == BiomeType.Ocean)
                return false;
            
            // Tree density varies by biome
            float treeDensity = biome switch
            {
                BiomeType.Tropical => 0.15f,
                BiomeType.Temperate => 0.08f,
                BiomeType.Boreal => 0.12f,
                BiomeType.Tundra => 0.02f,
                BiomeType.Desert => 0.005f,
                _ => 0.0f
            };
            
            // Use world position for consistent tree placement
            Random treeRandom = new Random(worldX * 31 + worldZ * 17 + _seed);
            return treeRandom.NextDouble() < treeDensity;
        }
    }

    public enum BiomeType
    {
        Tundra,
        Boreal,
        Temperate,
        Desert,
        Tropical,
        Ocean
    }
}
