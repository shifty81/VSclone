using Microsoft.Xna.Framework;

namespace TimelessTales.Entities
{
    /// <summary>
    /// Represents different types of crafting materials that can be obtained from breaking blocks
    /// </summary>
    public enum MaterialType
    {
        // Basic materials
        StoneBits,
        DirtBits,
        SandBits,
        GravelBits,
        ClayBits,
        
        // Wood materials
        WoodFibers,
        BarkFragments,
        
        // Rock materials
        GraniteChunks,
        LimestoneChunks,
        BasaltChunks,
        SandstoneChunks,
        SlateChunks,
        
        // Ore materials
        CopperNuggets,
        TinNuggets,
        IronNuggets,
        CoalFragments,
        
        // Organic materials
        PlantFibers,
        Leaves,
        GrassClippings
    }
    
    /// <summary>
    /// Material pouch that holds crafting materials in a cloud-like inventory
    /// Materials are automatically stored when breaking blocks
    /// </summary>
    public class MaterialPouch
    {
        private readonly Dictionary<MaterialType, float> _materials;
        private const float MAX_CAPACITY = 10000f; // Total capacity for all materials
        private float _currentWeight;
        
        public MaterialPouch()
        {
            _materials = new Dictionary<MaterialType, float>();
        }
        
        /// <summary>
        /// Adds material bits to the pouch
        /// </summary>
        /// <param name="type">Type of material</param>
        /// <param name="amount">Amount to add (in material units)</param>
        /// <returns>True if material was added, false if pouch is full</returns>
        public bool AddMaterial(MaterialType type, float amount)
        {
            if (_currentWeight + amount > MAX_CAPACITY)
            {
                // Pouch is full, can't add more
                return false;
            }
            
            if (_materials.ContainsKey(type))
                _materials[type] += amount;
            else
                _materials[type] = amount;
            
            _currentWeight += amount;
            return true;
        }
        
        /// <summary>
        /// Removes material from the pouch (for crafting)
        /// </summary>
        /// <param name="type">Type of material</param>
        /// <param name="amount">Amount to remove</param>
        /// <returns>True if material was removed, false if insufficient amount</returns>
        public bool RemoveMaterial(MaterialType type, float amount)
        {
            if (!_materials.ContainsKey(type) || _materials[type] < amount)
                return false;
            
            _materials[type] -= amount;
            _currentWeight -= amount;
            
            if (_materials[type] <= 0)
                _materials.Remove(type);
            
            return true;
        }
        
        /// <summary>
        /// Gets the amount of a specific material in the pouch
        /// </summary>
        public float GetMaterialAmount(MaterialType type)
        {
            return _materials.TryGetValue(type, out float amount) ? amount : 0f;
        }
        
        /// <summary>
        /// Gets all materials in the pouch
        /// </summary>
        public Dictionary<MaterialType, float> GetAllMaterials()
        {
            return new Dictionary<MaterialType, float>(_materials);
        }
        
        /// <summary>
        /// Gets the current total weight of materials
        /// </summary>
        public float GetCurrentWeight() => _currentWeight;
        
        /// <summary>
        /// Gets the maximum capacity of the pouch
        /// </summary>
        public float GetMaxCapacity() => MAX_CAPACITY;
        
        /// <summary>
        /// Gets the remaining capacity
        /// </summary>
        public float GetRemainingCapacity() => MAX_CAPACITY - _currentWeight;
        
        /// <summary>
        /// Gets the fill percentage (0-1)
        /// </summary>
        public float GetFillPercentage() => _currentWeight / MAX_CAPACITY;
    }
    
    /// <summary>
    /// Maps block types to the materials they drop when broken
    /// </summary>
    public static class MaterialDropTable
    {
        private static readonly Dictionary<Blocks.BlockType, (MaterialType material, float baseAmount)> _dropTable;
        
        static MaterialDropTable()
        {
            _dropTable = new Dictionary<Blocks.BlockType, (MaterialType, float)>();
            InitializeDropTable();
        }
        
        private static void InitializeDropTable()
        {
            // Basic terrain blocks
            _dropTable[Blocks.BlockType.Stone] = (MaterialType.StoneBits, 10f);
            _dropTable[Blocks.BlockType.Dirt] = (MaterialType.DirtBits, 8f);
            _dropTable[Blocks.BlockType.Grass] = (MaterialType.GrassClippings, 6f);
            _dropTable[Blocks.BlockType.Sand] = (MaterialType.SandBits, 8f);
            _dropTable[Blocks.BlockType.Gravel] = (MaterialType.GravelBits, 8f);
            _dropTable[Blocks.BlockType.Clay] = (MaterialType.ClayBits, 10f);
            _dropTable[Blocks.BlockType.RedClay] = (MaterialType.ClayBits, 10f);
            _dropTable[Blocks.BlockType.BlueClay] = (MaterialType.ClayBits, 10f);
            _dropTable[Blocks.BlockType.FireClay] = (MaterialType.ClayBits, 11f); // Slightly more valuable
            
            // Geological rocks
            _dropTable[Blocks.BlockType.Granite] = (MaterialType.GraniteChunks, 12f);
            _dropTable[Blocks.BlockType.Limestone] = (MaterialType.LimestoneChunks, 10f);
            _dropTable[Blocks.BlockType.Basalt] = (MaterialType.BasaltChunks, 12f);
            _dropTable[Blocks.BlockType.Sandstone] = (MaterialType.SandstoneChunks, 8f);
            _dropTable[Blocks.BlockType.Slate] = (MaterialType.SlateChunks, 11f);
            
            // Ores
            _dropTable[Blocks.BlockType.CopperOre] = (MaterialType.CopperNuggets, 15f);
            _dropTable[Blocks.BlockType.TinOre] = (MaterialType.TinNuggets, 15f);
            _dropTable[Blocks.BlockType.IronOre] = (MaterialType.IronNuggets, 18f);
            _dropTable[Blocks.BlockType.Coal] = (MaterialType.CoalFragments, 12f);
            
            // Wood
            _dropTable[Blocks.BlockType.Wood] = (MaterialType.WoodFibers, 10f);
            _dropTable[Blocks.BlockType.OakLog] = (MaterialType.WoodFibers, 12f);
            _dropTable[Blocks.BlockType.PineLog] = (MaterialType.WoodFibers, 12f);
            _dropTable[Blocks.BlockType.BirchLog] = (MaterialType.WoodFibers, 12f);
            
            // Leaves
            _dropTable[Blocks.BlockType.Leaves] = (MaterialType.Leaves, 3f);
            _dropTable[Blocks.BlockType.OakLeaves] = (MaterialType.Leaves, 3f);
            _dropTable[Blocks.BlockType.PineLeaves] = (MaterialType.Leaves, 3f);
            _dropTable[Blocks.BlockType.BirchLeaves] = (MaterialType.Leaves, 3f);
            
            // Crafted blocks
            _dropTable[Blocks.BlockType.Planks] = (MaterialType.WoodFibers, 8f);
            _dropTable[Blocks.BlockType.Cobblestone] = (MaterialType.StoneBits, 9f);
        }
        
        /// <summary>
        /// Gets the material drop for a given block type
        /// </summary>
        /// <param name="blockType">The block type being broken</param>
        /// <returns>Tuple of (MaterialType, amount) or null if block doesn't drop materials</returns>
        public static (MaterialType material, float amount)? GetDrop(Blocks.BlockType blockType)
        {
            if (_dropTable.TryGetValue(blockType, out var drop))
                return drop;
            return null;
        }
        
        /// <summary>
        /// Calculates the actual drop amount based on block hardness and tool
        /// </summary>
        /// <param name="baseAmount">Base material amount</param>
        /// <param name="hardness">Block hardness multiplier</param>
        /// <returns>Actual amount to drop</returns>
        public static float CalculateDropAmount(float baseAmount, float hardness)
        {
            // Harder blocks give more materials
            return baseAmount * (1.0f + hardness * 0.2f);
        }
    }
}
