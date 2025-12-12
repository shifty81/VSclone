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
        private const float SHRUB_SPAWN_CHANCE = 0.05f;
        private const int VEGETATION_CHECK_INTERVAL = 100; // Check every 100 blocks
        
        public VegetationManager(WorldManager worldManager)
        {
            _worldManager = worldManager;
            _plants = new Dictionary<Vector3, Plant>();
            _random = new Random();
        }
        
        /// <summary>
        /// Update all plants in the world
        /// </summary>
        public void Update(float deltaTime)
        {
            // Update all plants directly from dictionary values (no copy needed)
            foreach (var plant in _plants.Values)
            {
                if (plant.Update(deltaTime))
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
                        
                        // Only place on grass blocks with air above
                        if (groundBlock == BlockType.Grass && aboveBlock == BlockType.Air)
                        {
                            TryPlaceVegetation(worldX + x, topY + 1, worldZ + z, groundBlock);
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
        /// Attempt to place vegetation at a position
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
                if (roll < SHRUB_SPAWN_CHANCE)
                {
                    // Place shrub
                    PlacePlant(position, VegetationType.Shrub);
                }
                else if (roll < SHRUB_SPAWN_CHANCE + GRASS_SPAWN_CHANCE)
                {
                    // Place grass
                    PlacePlant(position, VegetationType.Grass);
                }
            }
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
