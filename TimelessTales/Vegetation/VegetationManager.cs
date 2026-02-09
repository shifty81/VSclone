using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TimelessTales.World;
using TimelessTales.Blocks;

namespace TimelessTales.Vegetation
{
    /// <summary>
    /// Manages all vegetation in the world including placement and growth
    /// </summary>
    public class VegetationManager
    {
        private readonly Dictionary<Vector3, Plant> _plants;
        private readonly WorldManager _worldManager;
        private readonly Random _random;
        
        // Vegetation placement parameters
        private const float GRASS_SPAWN_CHANCE = 0.15f;
        private const float TALL_GRASS_SPAWN_CHANCE = 0.06f;
        private const float SHRUB_SPAWN_CHANCE = 0.05f;
        private const float BERRY_SHRUB_SPAWN_CHANCE = 0.02f;
        private const float FLOWER_SPAWN_CHANCE = 0.03f;
        private const float UNDERWATER_VEGETATION_SPAWN_CHANCE = 0.12f;
        private const float KELP_SPAWN_CHANCE = 0.03f;
        private const float CORAL_SPAWN_CHANCE = 0.02f;
        private const int VEGETATION_CHECK_INTERVAL = 100; // Check every 100 blocks
        private const int SEA_LEVEL = 64;
        
        public VegetationManager(WorldManager worldManager)
        {
            _worldManager = worldManager;
            _plants = new Dictionary<Vector3, Plant>();
            _random = new Random();
        }
        
        // Throttle vegetation updates for performance
        private float _updateAccumulator;
        private const float VEGETATION_UPDATE_INTERVAL = 1.0f; // Only update growth every second
        
        /// <summary>
        /// Update all plants in the world (throttled for performance)
        /// </summary>
        public void Update(float deltaTime)
        {
            _updateAccumulator += deltaTime;
            if (_updateAccumulator < VEGETATION_UPDATE_INTERVAL)
                return;
            
            float elapsed = _updateAccumulator;
            _updateAccumulator = 0;
            
            // Update all plants with accumulated time
            foreach (var plant in _plants.Values)
            {
                if (plant.Update(elapsed))
                {
                    // Plant advanced to next stage - could trigger visual update
                    OnPlantStageChanged(plant);
                }
            }
        }
        
        /// <summary>
        /// Place vegetation on a chunk after it's generated
        /// </summary>
        public void PopulateChunk(Chunk chunk)
        {
            int worldX = chunk.ChunkX * Chunk.CHUNK_SIZE;
            int worldZ = chunk.ChunkZ * Chunk.CHUNK_SIZE;
            
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    // Find the top solid block
                    int topY = FindTopBlock(chunk, x, z);
                    
                    if (topY > 0 && topY < Chunk.CHUNK_HEIGHT - 1)
                    {
                        BlockType groundBlock = chunk.GetBlock(x, topY, z);
                        BlockType aboveBlock = chunk.GetBlock(x, topY + 1, z);
                        
                        // Place land vegetation on grass blocks with air above
                        if (groundBlock == BlockType.Grass && aboveBlock == BlockType.Air)
                        {
                            TryPlaceVegetation(worldX + x, topY + 1, worldZ + z, groundBlock);
                        }
                        // Place underwater vegetation on solid blocks with water above
                        else if (IsWaterBlock(aboveBlock) && BlockRegistry.IsSolid(groundBlock) && topY < SEA_LEVEL)
                        {
                            TryPlaceUnderwaterVegetation(worldX + x, topY + 1, worldZ + z, groundBlock);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Find the topmost solid block at a position in a chunk
        /// </summary>
        private int FindTopBlock(Chunk chunk, int x, int z)
        {
            for (int y = Chunk.CHUNK_HEIGHT - 1; y >= 0; y--)
            {
                BlockType block = chunk.GetBlock(x, y, z);
                if (BlockRegistry.IsSolid(block))
                {
                    return y;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Get biome-specific vegetation density multiplier
        /// </summary>
        public static float GetBiomeDensityMultiplier(BiomeType biome, VegetationType vegType)
        {
            return biome switch
            {
                BiomeType.Tropical => vegType switch
                {
                    VegetationType.Grass => 1.5f,
                    VegetationType.TallGrass => 1.8f,
                    VegetationType.Shrub => 1.4f,
                    VegetationType.BerryShrub => 1.6f,
                    VegetationType.Flowers => 2.0f,
                    _ => 1.0f
                },
                BiomeType.Temperate => vegType switch
                {
                    VegetationType.Grass => 1.2f,
                    VegetationType.TallGrass => 1.0f,
                    VegetationType.Shrub => 1.2f,
                    VegetationType.BerryShrub => 1.3f,
                    VegetationType.Flowers => 1.5f,
                    _ => 1.0f
                },
                BiomeType.Boreal => vegType switch
                {
                    VegetationType.Grass => 0.8f,
                    VegetationType.TallGrass => 0.5f,
                    VegetationType.Shrub => 1.0f,
                    VegetationType.BerryShrub => 1.2f, // Berry bushes common in boreal
                    VegetationType.Flowers => 0.6f,
                    _ => 1.0f
                },
                BiomeType.Tundra => vegType switch
                {
                    VegetationType.Grass => 0.4f,
                    VegetationType.TallGrass => 0.1f,
                    VegetationType.Shrub => 0.3f,
                    VegetationType.BerryShrub => 0.4f,
                    VegetationType.Flowers => 0.2f,
                    _ => 1.0f
                },
                BiomeType.Desert => vegType switch
                {
                    VegetationType.Grass => 0.1f,
                    VegetationType.TallGrass => 0.05f,
                    VegetationType.Shrub => 0.2f,
                    VegetationType.BerryShrub => 0.05f,
                    VegetationType.Flowers => 0.1f,
                    _ => 1.0f
                },
                _ => 1.0f
            };
        }
        
        /// <summary>
        /// Attempt to place vegetation at a position with biome-aware density
        /// </summary>
        private void TryPlaceVegetation(int x, int y, int z, BlockType groundType)
        {
            Vector3 position = new Vector3(x, y, z);
            
            // Don't place if vegetation already exists
            if (_plants.ContainsKey(position))
            {
                return;
            }
            
            float roll = (float)_random.NextDouble();
            
            // Determine what to place based on ground type and random chance
            if (groundType == BlockType.Grass)
            {
                float cursor = 0f;
                
                // Berry shrub (rarest)
                cursor += BERRY_SHRUB_SPAWN_CHANCE;
                if (roll < cursor)
                {
                    PlacePlant(position, VegetationType.BerryShrub);
                    return;
                }
                
                // Shrub
                cursor += SHRUB_SPAWN_CHANCE;
                if (roll < cursor)
                {
                    PlacePlant(position, VegetationType.Shrub);
                    return;
                }
                
                // Flowers
                cursor += FLOWER_SPAWN_CHANCE;
                if (roll < cursor)
                {
                    PlacePlant(position, VegetationType.Flowers);
                    return;
                }
                
                // Tall grass
                cursor += TALL_GRASS_SPAWN_CHANCE;
                if (roll < cursor)
                {
                    PlacePlant(position, VegetationType.TallGrass);
                    return;
                }
                
                // Regular grass (most common)
                cursor += GRASS_SPAWN_CHANCE;
                if (roll < cursor)
                {
                    PlacePlant(position, VegetationType.Grass);
                    return;
                }
            }
        }
        
        /// <summary>
        /// Attempt to place underwater vegetation at a position
        /// </summary>
        private void TryPlaceUnderwaterVegetation(int x, int y, int z, BlockType groundType)
        {
            Vector3 position = new Vector3(x, y, z);
            
            // Don't place if vegetation already exists
            if (_plants.ContainsKey(position))
            {
                return;
            }
            
            float roll = (float)_random.NextDouble();
            
            // Calculate water depth (distance from sea level)
            int waterDepth = SEA_LEVEL - y;
            
            // Safety check: only place vegetation if underwater (positive depth)
            if (waterDepth <= 0)
            {
                return;
            }
            
            // Different vegetation based on depth
            if (waterDepth > 10) // Deep water
            {
                if (roll < KELP_SPAWN_CHANCE)
                {
                    // Place kelp in deep water
                    PlacePlant(position, VegetationType.Kelp, GrowthStage.Mature);
                }
                else if (roll < KELP_SPAWN_CHANCE + CORAL_SPAWN_CHANCE)
                {
                    // Place coral in deep water
                    PlacePlant(position, VegetationType.Coral, GrowthStage.Mature);
                }
            }
            else if (waterDepth > 3) // Medium depth
            {
                if (roll < UNDERWATER_VEGETATION_SPAWN_CHANCE)
                {
                    // Randomly choose between seaweed and sea grass
                    VegetationType plantType = roll < UNDERWATER_VEGETATION_SPAWN_CHANCE / 2 
                        ? VegetationType.Seaweed 
                        : VegetationType.SeaGrass;
                    PlacePlant(position, plantType, GrowthStage.Mature);
                }
            }
            else // Shallow water
            {
                if (roll < UNDERWATER_VEGETATION_SPAWN_CHANCE * 0.7f)
                {
                    // Mostly sea grass in shallow water
                    PlacePlant(position, VegetationType.SeaGrass, GrowthStage.Mature);
                }
            }
        }
        
        /// <summary>
        /// Check if a block type is water
        /// </summary>
        private bool IsWaterBlock(BlockType blockType)
        {
            return blockType == BlockType.Water || blockType == BlockType.Saltwater;
        }
        
        /// <summary>
        /// Place a plant at a specific position
        /// </summary>
        public void PlacePlant(Vector3 position, VegetationType type, GrowthStage initialStage = GrowthStage.Seedling)
        {
            var plant = new Plant(position, type, initialStage);
            _plants[position] = plant;
        }
        
        /// <summary>
        /// Remove a plant at a position (e.g., when player breaks it)
        /// </summary>
        public bool RemovePlant(Vector3 position)
        {
            return _plants.Remove(position);
        }
        
        /// <summary>
        /// Get a plant at a specific position
        /// </summary>
        public Plant? GetPlant(Vector3 position)
        {
            return _plants.TryGetValue(position, out var plant) ? plant : null;
        }
        
        /// <summary>
        /// Get all plants in the world
        /// </summary>
        public IEnumerable<Plant> GetAllPlants()
        {
            return _plants.Values;
        }
        
        /// <summary>
        /// Get count of plants by type
        /// </summary>
        public int GetPlantCount(VegetationType? type = null)
        {
            if (type == null)
                return _plants.Count;
            
            int count = 0;
            foreach (var plant in _plants.Values)
            {
                if (plant.Type == type.Value)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// Called when a plant advances to the next growth stage
        /// </summary>
        private void OnPlantStageChanged(Plant plant)
        {
            // Future: Trigger visual/audio effects
            // For now, just a placeholder for future implementation
        }
        
        /// <summary>
        /// Clear all vegetation data
        /// </summary>
        public void Clear()
        {
            _plants.Clear();
        }
    }
}
