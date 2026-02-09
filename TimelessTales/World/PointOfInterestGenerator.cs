using TimelessTales.Blocks;
using TimelessTales.Utils;
using System;

namespace TimelessTales.World
{
    /// <summary>
    /// Types of points of interest that can be generated
    /// </summary>
    public enum PointOfInterestType
    {
        AncientRuins,
        AbandonedSettlement,
        CrystalCavern,
        NaturalArch,
        HotSpring,
        MeteorSite
    }

    /// <summary>
    /// Generates procedural points of interest in the world
    /// such as ruins, settlements, crystal caverns, and natural formations
    /// </summary>
    public class PointOfInterestGenerator
    {
        private readonly int _seed;
        private readonly SimplexNoise _poiNoise;
        private readonly Random _random;

        // POI generation parameters
        private const float POI_SCALE = 0.003f; // Large scale for spacing POIs apart
        private const float POI_THRESHOLD = 0.75f; // High threshold = rare POIs
        private const int SEA_LEVEL = 64;
        private const int MIN_RUINS_SIZE = 3;
        private const int MAX_RUINS_SIZE = 8;
        private const int MIN_SETTLEMENT_SIZE = 4;
        private const int MAX_SETTLEMENT_SIZE = 10;

        public PointOfInterestGenerator(int seed)
        {
            _seed = seed;
            _poiNoise = new SimplexNoise(seed + 100);
            _random = new Random(seed + 101);
        }

        /// <summary>
        /// Check and generate POIs for a chunk after terrain generation
        /// </summary>
        public void GenerateForChunk(Chunk chunk, WorldGenerator worldGenerator)
        {
            int worldX = chunk.ChunkX * Chunk.CHUNK_SIZE;
            int worldZ = chunk.ChunkZ * Chunk.CHUNK_SIZE;

            // Sample POI noise at chunk center
            int centerX = worldX + Chunk.CHUNK_SIZE / 2;
            int centerZ = worldZ + Chunk.CHUNK_SIZE / 2;

            float poiValue = _poiNoise.Evaluate(centerX * POI_SCALE, centerZ * POI_SCALE);

            if (poiValue < POI_THRESHOLD)
                return;

            // Find surface height at center
            int surfaceY = FindSurfaceHeight(chunk, Chunk.CHUNK_SIZE / 2, Chunk.CHUNK_SIZE / 2);
            if (surfaceY <= 0)
                return;

            // Determine POI type based on noise value and location
            PointOfInterestType poiType = DeterminePoiType(centerX, centerZ, surfaceY, poiValue);

            // Use deterministic random for this chunk
            Random chunkRandom = new Random(centerX * 31 + centerZ * 17 + _seed);

            switch (poiType)
            {
                case PointOfInterestType.AncientRuins:
                    if (surfaceY > SEA_LEVEL)
                        GenerateAncientRuins(chunk, chunkRandom, surfaceY);
                    break;
                case PointOfInterestType.AbandonedSettlement:
                    if (surfaceY > SEA_LEVEL)
                        GenerateAbandonedSettlement(chunk, chunkRandom, surfaceY);
                    break;
                case PointOfInterestType.CrystalCavern:
                    GenerateCrystalCavern(chunk, chunkRandom, surfaceY);
                    break;
                case PointOfInterestType.NaturalArch:
                    if (surfaceY > SEA_LEVEL)
                        GenerateNaturalArch(chunk, chunkRandom, surfaceY);
                    break;
                case PointOfInterestType.HotSpring:
                    if (surfaceY > SEA_LEVEL)
                        GenerateHotSpring(chunk, chunkRandom, surfaceY);
                    break;
                case PointOfInterestType.MeteorSite:
                    if (surfaceY > SEA_LEVEL)
                        GenerateMeteorSite(chunk, chunkRandom, surfaceY);
                    break;
            }
        }

        /// <summary>
        /// Determine which type of POI to generate based on location
        /// </summary>
        private PointOfInterestType DeterminePoiType(int worldX, int worldZ, int surfaceY, float noiseValue)
        {
            // Use position-based hash for deterministic POI type selection
            int hash = Math.Abs((worldX * 73856093) ^ (worldZ * 19349663)) % 6;
            return (PointOfInterestType)hash;
        }

        /// <summary>
        /// Find the surface height at a local position in the chunk
        /// </summary>
        private int FindSurfaceHeight(Chunk chunk, int localX, int localZ)
        {
            for (int y = Chunk.CHUNK_HEIGHT - 1; y >= 0; y--)
            {
                if (BlockRegistry.IsSolid(chunk.GetBlock(localX, y, localZ)))
                    return y;
            }
            return -1;
        }

        /// <summary>
        /// Generate ancient ruins - crumbling stone walls and pillars
        /// </summary>
        private void GenerateAncientRuins(Chunk chunk, Random rng, int surfaceY)
        {
            int size = rng.Next(MIN_RUINS_SIZE, MAX_RUINS_SIZE + 1);
            int startX = Math.Max(1, (Chunk.CHUNK_SIZE - size) / 2);
            int startZ = Math.Max(1, (Chunk.CHUNK_SIZE - size) / 2);

            // Generate ruined walls
            for (int x = startX; x < startX + size && x < Chunk.CHUNK_SIZE - 1; x++)
            {
                for (int z = startZ; z < startZ + size && z < Chunk.CHUNK_SIZE - 1; z++)
                {
                    bool isWall = (x == startX || x == startX + size - 1 ||
                                   z == startZ || z == startZ + size - 1);

                    if (isWall)
                    {
                        // Ruined wall - varying height with gaps
                        int wallHeight = rng.Next(1, 4);
                        if (rng.NextDouble() > 0.3) // 70% chance of wall section existing
                        {
                            for (int y = 1; y <= wallHeight; y++)
                            {
                                int placeY = surfaceY + y;
                                if (placeY < Chunk.CHUNK_HEIGHT)
                                {
                                    chunk.SetBlock(x, placeY, z, BlockType.Cobblestone);
                                }
                            }
                        }
                    }
                    else if (rng.NextDouble() < 0.1) // 10% chance of interior rubble
                    {
                        chunk.SetBlock(x, surfaceY + 1, z, BlockType.Cobblestone);
                    }
                }
            }

            // Add corner pillars (taller)
            AddPillar(chunk, startX, startZ, surfaceY, rng, 3, 5);
            AddPillar(chunk, startX + size - 1, startZ, surfaceY, rng, 3, 5);
            AddPillar(chunk, startX, startZ + size - 1, surfaceY, rng, 3, 5);
            AddPillar(chunk, startX + size - 1, startZ + size - 1, surfaceY, rng, 3, 5);
        }

        /// <summary>
        /// Generate abandoned settlement - small huts with partial roofs
        /// </summary>
        private void GenerateAbandonedSettlement(Chunk chunk, Random rng, int surfaceY)
        {
            int numHuts = rng.Next(1, 3);

            for (int h = 0; h < numHuts; h++)
            {
                int hutSize = rng.Next(MIN_SETTLEMENT_SIZE, MAX_SETTLEMENT_SIZE / 2 + 1);
                int hutX = rng.Next(1, Math.Max(2, Chunk.CHUNK_SIZE - hutSize - 1));
                int hutZ = rng.Next(1, Math.Max(2, Chunk.CHUNK_SIZE - hutSize - 1));

                // Ensure within bounds
                hutX = Math.Min(hutX, Chunk.CHUNK_SIZE - hutSize - 1);
                hutZ = Math.Min(hutZ, Chunk.CHUNK_SIZE - hutSize - 1);
                if (hutX < 0) hutX = 1;
                if (hutZ < 0) hutZ = 1;

                // Build hut walls
                for (int x = hutX; x < hutX + hutSize && x < Chunk.CHUNK_SIZE; x++)
                {
                    for (int z = hutZ; z < hutZ + hutSize && z < Chunk.CHUNK_SIZE; z++)
                    {
                        bool isWall = (x == hutX || x == hutX + hutSize - 1 ||
                                       z == hutZ || z == hutZ + hutSize - 1);

                        if (isWall)
                        {
                            // Door opening on one side
                            bool isDoor = (x == hutX + hutSize / 2 && z == hutZ) ||
                                         (z == hutZ + hutSize / 2 && x == hutX);

                            if (!isDoor)
                            {
                                for (int y = 1; y <= 3; y++)
                                {
                                    int placeY = surfaceY + y;
                                    if (placeY < Chunk.CHUNK_HEIGHT)
                                    {
                                        BlockType wallBlock = rng.NextDouble() > 0.3 ? BlockType.Planks : BlockType.Wood;
                                        chunk.SetBlock(x, placeY, z, wallBlock);
                                    }
                                }
                            }
                        }
                    }
                }

                // Partial roof (some planks missing)
                for (int x = hutX; x < hutX + hutSize && x < Chunk.CHUNK_SIZE; x++)
                {
                    for (int z = hutZ; z < hutZ + hutSize && z < Chunk.CHUNK_SIZE; z++)
                    {
                        if (rng.NextDouble() > 0.4) // 60% of roof intact
                        {
                            int roofY = surfaceY + 4;
                            if (roofY < Chunk.CHUNK_HEIGHT)
                            {
                                chunk.SetBlock(x, roofY, z, BlockType.Planks);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate crystal cavern - underground cavity with lanterns simulating crystals
        /// </summary>
        private void GenerateCrystalCavern(Chunk chunk, Random rng, int surfaceY)
        {
            int cavernY = Math.Max(10, surfaceY - rng.Next(15, 30));
            int radius = rng.Next(3, 6);
            int centerX = Chunk.CHUNK_SIZE / 2;
            int centerZ = Chunk.CHUNK_SIZE / 2;

            // Carve out cavern
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int z = centerZ - radius; z <= centerZ + radius; z++)
                {
                    for (int y = cavernY - radius / 2; y <= cavernY + radius; y++)
                    {
                        if (x < 0 || x >= Chunk.CHUNK_SIZE || z < 0 || z >= Chunk.CHUNK_SIZE)
                            continue;
                        if (y < 1 || y >= Chunk.CHUNK_HEIGHT)
                            continue;

                        float dx = x - centerX;
                        float dy = (y - cavernY) * 0.7f; // Stretch vertically
                        float dz = z - centerZ;
                        float dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

                        if (dist < radius)
                        {
                            chunk.SetBlock(x, y, z, BlockType.Air);
                        }
                    }
                }
            }

            // Place crystal clusters (lanterns as placeholders) on walls
            for (int i = 0; i < rng.Next(5, 12); i++)
            {
                int cx = centerX + rng.Next(-radius + 1, radius);
                int cy = cavernY + rng.Next(-radius / 2 + 1, radius);
                int cz = centerZ + rng.Next(-radius + 1, radius);

                if (cx >= 0 && cx < Chunk.CHUNK_SIZE && cy >= 1 && cy < Chunk.CHUNK_HEIGHT &&
                    cz >= 0 && cz < Chunk.CHUNK_SIZE)
                {
                    // Only place on empty spaces adjacent to solid blocks
                    if (chunk.GetBlock(cx, cy, cz) == BlockType.Air && HasAdjacentSolid(chunk, cx, cy, cz))
                    {
                        chunk.SetBlock(cx, cy, cz, BlockType.Lantern);
                    }
                }
            }
        }

        /// <summary>
        /// Generate natural arch formation
        /// </summary>
        private void GenerateNaturalArch(Chunk chunk, Random rng, int surfaceY)
        {
            int archHeight = rng.Next(5, 10);
            int archWidth = rng.Next(4, 8);
            int startX = (Chunk.CHUNK_SIZE - archWidth) / 2;
            int centerZ = Chunk.CHUNK_SIZE / 2;

            // Build two pillars
            for (int y = 1; y <= archHeight; y++)
            {
                int placeY = surfaceY + y;
                if (placeY >= Chunk.CHUNK_HEIGHT) break;

                // Left pillar
                for (int dz = -1; dz <= 1; dz++)
                {
                    int z = centerZ + dz;
                    if (z >= 0 && z < Chunk.CHUNK_SIZE && startX >= 0 && startX < Chunk.CHUNK_SIZE)
                    {
                        chunk.SetBlock(startX, placeY, z, BlockType.Stone);
                    }
                }

                // Right pillar
                int rightX = startX + archWidth - 1;
                for (int dz = -1; dz <= 1; dz++)
                {
                    int z = centerZ + dz;
                    if (z >= 0 && z < Chunk.CHUNK_SIZE && rightX >= 0 && rightX < Chunk.CHUNK_SIZE)
                    {
                        chunk.SetBlock(rightX, placeY, z, BlockType.Stone);
                    }
                }
            }

            // Build arch top connecting the pillars
            int archTopY = surfaceY + archHeight;
            if (archTopY < Chunk.CHUNK_HEIGHT)
            {
                for (int x = startX; x < startX + archWidth && x < Chunk.CHUNK_SIZE; x++)
                {
                    if (x < 0) continue;
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        int z = centerZ + dz;
                        if (z >= 0 && z < Chunk.CHUNK_SIZE)
                        {
                            chunk.SetBlock(x, archTopY, z, BlockType.Stone);
                            // Extra layer on top for thickness
                            if (archTopY + 1 < Chunk.CHUNK_HEIGHT)
                                chunk.SetBlock(x, archTopY + 1, z, BlockType.Stone);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate hot spring - small water pool surrounded by gravel
        /// </summary>
        private void GenerateHotSpring(Chunk chunk, Random rng, int surfaceY)
        {
            int radius = rng.Next(2, 4);
            int centerX = Chunk.CHUNK_SIZE / 2;
            int centerZ = Chunk.CHUNK_SIZE / 2;

            for (int x = centerX - radius - 1; x <= centerX + radius + 1; x++)
            {
                for (int z = centerZ - radius - 1; z <= centerZ + radius + 1; z++)
                {
                    if (x < 0 || x >= Chunk.CHUNK_SIZE || z < 0 || z >= Chunk.CHUNK_SIZE)
                        continue;

                    float dx = x - centerX;
                    float dz = z - centerZ;
                    float dist = MathF.Sqrt(dx * dx + dz * dz);

                    if (dist <= radius)
                    {
                        // Dig pool and fill with water
                        chunk.SetBlock(x, surfaceY, z, BlockType.Water);
                        chunk.SetBlock(x, surfaceY - 1, z, BlockType.Gravel);
                        // Place torch under water for steam effect
                        if (rng.NextDouble() < 0.15)
                        {
                            chunk.SetBlock(x, surfaceY - 1, z, BlockType.Torch);
                        }
                    }
                    else if (dist <= radius + 1)
                    {
                        // Gravel rim
                        chunk.SetBlock(x, surfaceY, z, BlockType.Gravel);
                    }
                }
            }
        }

        /// <summary>
        /// Generate meteor impact site - crater with rare ores
        /// </summary>
        private void GenerateMeteorSite(Chunk chunk, Random rng, int surfaceY)
        {
            int craterRadius = rng.Next(3, 6);
            int craterDepth = rng.Next(2, 4);
            int centerX = Chunk.CHUNK_SIZE / 2;
            int centerZ = Chunk.CHUNK_SIZE / 2;

            for (int x = centerX - craterRadius; x <= centerX + craterRadius; x++)
            {
                for (int z = centerZ - craterRadius; z <= centerZ + craterRadius; z++)
                {
                    if (x < 0 || x >= Chunk.CHUNK_SIZE || z < 0 || z >= Chunk.CHUNK_SIZE)
                        continue;

                    float dx = x - centerX;
                    float dz = z - centerZ;
                    float dist = MathF.Sqrt(dx * dx + dz * dz);

                    if (dist <= craterRadius)
                    {
                        // Crater bowl shape
                        float depthFactor = 1.0f - (dist / craterRadius);
                        int localDepth = (int)(craterDepth * depthFactor);

                        for (int d = 0; d < localDepth; d++)
                        {
                            int clearY = surfaceY - d;
                            if (clearY > 0)
                            {
                                chunk.SetBlock(x, clearY, z, BlockType.Air);
                            }
                        }

                        // Scorched ground at bottom
                        int bottomY = surfaceY - localDepth;
                        if (bottomY > 0)
                        {
                            chunk.SetBlock(x, bottomY, z, BlockType.Basalt);
                        }

                        // Rare ore deposits in crater center
                        if (dist < craterRadius * 0.3f && rng.NextDouble() < 0.3)
                        {
                            int oreY = surfaceY - localDepth - 1;
                            if (oreY > 0)
                            {
                                chunk.SetBlock(x, oreY, z, BlockType.IronOre);
                            }
                        }
                    }
                    else if (dist <= craterRadius + 1.5f)
                    {
                        // Raised rim
                        int rimHeight = rng.Next(1, 3);
                        for (int y = 1; y <= rimHeight; y++)
                        {
                            int rimY = surfaceY + y;
                            if (rimY < Chunk.CHUNK_HEIGHT)
                            {
                                chunk.SetBlock(x, rimY, z, BlockType.Stone);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a stone pillar at a position
        /// </summary>
        private void AddPillar(Chunk chunk, int x, int z, int surfaceY, Random rng, int minHeight, int maxHeight)
        {
            if (x < 0 || x >= Chunk.CHUNK_SIZE || z < 0 || z >= Chunk.CHUNK_SIZE)
                return;

            int height = rng.Next(minHeight, maxHeight + 1);
            for (int y = 1; y <= height; y++)
            {
                int placeY = surfaceY + y;
                if (placeY < Chunk.CHUNK_HEIGHT)
                {
                    chunk.SetBlock(x, placeY, z, BlockType.Cobblestone);
                }
            }
        }

        /// <summary>
        /// Check if a position has an adjacent solid block
        /// </summary>
        private bool HasAdjacentSolid(Chunk chunk, int x, int y, int z)
        {
            if (x > 0 && BlockRegistry.IsSolid(chunk.GetBlock(x - 1, y, z))) return true;
            if (x < Chunk.CHUNK_SIZE - 1 && BlockRegistry.IsSolid(chunk.GetBlock(x + 1, y, z))) return true;
            if (y > 0 && BlockRegistry.IsSolid(chunk.GetBlock(x, y - 1, z))) return true;
            if (y < Chunk.CHUNK_HEIGHT - 1 && BlockRegistry.IsSolid(chunk.GetBlock(x, y + 1, z))) return true;
            if (z > 0 && BlockRegistry.IsSolid(chunk.GetBlock(x, y, z - 1))) return true;
            if (z < Chunk.CHUNK_SIZE - 1 && BlockRegistry.IsSolid(chunk.GetBlock(x, y, z + 1))) return true;
            return false;
        }
    }
}
