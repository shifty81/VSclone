using Microsoft.Xna.Framework;

namespace TimelessTales.Blocks
{
    /// <summary>
    /// Represents different types of blocks in the world
    /// </summary>
    public enum BlockType
    {
        Air = 0,
        Stone,
        Dirt,
        Grass,
        Sand,
        Gravel,
        Clay,
        // Rock types (geological layers)
        Granite,
        Limestone,
        Basalt,
        Sandstone,
        Slate,
        // Ores
        CopperOre,
        TinOre,
        IronOre,
        Coal,
        // Wood
        Wood,
        Leaves,
        // Crafted
        Planks,
        Cobblestone,
        // Trees
        OakLog,
        OakLeaves,
        PineLog,
        PineLeaves,
        BirchLog,
        BirchLeaves,
        // Water
        Water,
        Saltwater,
        // Clay types
        RedClay,
        BlueClay,
        FireClay,
        // Light sources
        Torch,
        Lantern,
        Stick
    }

    /// <summary>
    /// Defines properties and behavior of a block type
    /// </summary>
    public class BlockDefinition
    {
        public BlockType Type { get; set; }
        public string Name { get; set; }
        public float Hardness { get; set; } // Mining time multiplier
        public bool IsTransparent { get; set; }
        public bool IsSolid { get; set; }
        public bool AffectedByGravity { get; set; }
        public Color Color { get; set; } // Temporary color until textures are added
        public bool IsOre { get; set; }
        public int LightEmission { get; set; } // 0-15
        public int TextureIndex { get; set; } // Index in the texture atlas

        public BlockDefinition(BlockType type, string name, float hardness, Color color, 
                              bool isTransparent = false, bool affectedByGravity = false, bool isOre = false, int textureIndex = -1)
        {
            Type = type;
            Name = name;
            Hardness = hardness;
            Color = color;
            IsTransparent = isTransparent;
            IsSolid = !isTransparent;
            AffectedByGravity = affectedByGravity;
            IsOre = isOre;
            LightEmission = 0;
            TextureIndex = textureIndex >= 0 ? textureIndex : (int)type; // Default to enum value if not specified
        }
    }

    /// <summary>
    /// Registry of all block types and their properties
    /// </summary>
    public static class BlockRegistry
    {
        private static readonly Dictionary<BlockType, BlockDefinition> _blocks = new();

        static BlockRegistry()
        {
            RegisterDefaultBlocks();
        }

        private static void RegisterDefaultBlocks()
        {
            // Texture indices match the order in TextureAtlas generation
            Register(new BlockDefinition(BlockType.Air, "Air", 0, Color.Transparent, true, false, false, -1));
            Register(new BlockDefinition(BlockType.Stone, "Stone", 1.5f, Color.Gray, false, false, false, 0));
            Register(new BlockDefinition(BlockType.Dirt, "Dirt", 0.5f, new Color(139, 69, 19), false, false, false, 1));
            Register(new BlockDefinition(BlockType.Grass, "Grass", 0.6f, Color.Green, false, false, false, 2));
            Register(new BlockDefinition(BlockType.Sand, "Sand", 0.5f, Color.SandyBrown, false, true, false, 3));
            Register(new BlockDefinition(BlockType.Gravel, "Gravel", 0.6f, Color.DarkGray, false, true, false, 4));
            Register(new BlockDefinition(BlockType.Clay, "Clay", 0.6f, new Color(178, 140, 110), false, false, false, 5));
            
            // Geological rock layers
            Register(new BlockDefinition(BlockType.Granite, "Granite", 2.0f, new Color(100, 100, 100), false, false, false, 6));
            Register(new BlockDefinition(BlockType.Limestone, "Limestone", 1.2f, new Color(200, 200, 180), false, false, false, 7));
            Register(new BlockDefinition(BlockType.Basalt, "Basalt", 1.8f, new Color(60, 60, 70), false, false, false, 8));
            Register(new BlockDefinition(BlockType.Sandstone, "Sandstone", 1.0f, new Color(210, 180, 140), false, false, false, 9));
            Register(new BlockDefinition(BlockType.Slate, "Slate", 1.5f, new Color(70, 80, 90), false, false, false, 10));
            
            // Ores
            Register(new BlockDefinition(BlockType.CopperOre, "Copper Ore", 2.5f, new Color(184, 115, 51), false, false, true, 11));
            Register(new BlockDefinition(BlockType.TinOre, "Tin Ore", 2.5f, new Color(150, 150, 150), false, false, true, 12));
            Register(new BlockDefinition(BlockType.IronOre, "Iron Ore", 3.0f, new Color(139, 90, 90), false, false, true, 13));
            Register(new BlockDefinition(BlockType.Coal, "Coal", 2.0f, new Color(30, 30, 30), false, false, true, 14));
            
            // Wood
            Register(new BlockDefinition(BlockType.Wood, "Wood", 1.0f, new Color(139, 90, 43), false, false, false, 15));
            Register(new BlockDefinition(BlockType.Leaves, "Leaves", 0.2f, Color.DarkGreen, true, false, false, 16));
            
            // Tree types
            Register(new BlockDefinition(BlockType.OakLog, "Oak Log", 1.2f, new Color(101, 67, 33), false, false, false, 17));
            Register(new BlockDefinition(BlockType.OakLeaves, "Oak Leaves", 0.2f, new Color(34, 139, 34), true, false, false, 18));
            Register(new BlockDefinition(BlockType.PineLog, "Pine Log", 1.1f, new Color(85, 53, 24), false, false, false, 19));
            Register(new BlockDefinition(BlockType.PineLeaves, "Pine Leaves", 0.2f, new Color(28, 95, 28), true, false, false, 20));
            Register(new BlockDefinition(BlockType.BirchLog, "Birch Log", 1.0f, new Color(220, 220, 200), false, false, false, 21));
            Register(new BlockDefinition(BlockType.BirchLeaves, "Birch Leaves", 0.2f, new Color(50, 150, 50), true, false, false, 22));
            
            // Crafted
            Register(new BlockDefinition(BlockType.Planks, "Planks", 1.0f, new Color(160, 110, 60), false, false, false, 23));
            Register(new BlockDefinition(BlockType.Cobblestone, "Cobblestone", 1.5f, new Color(120, 120, 120), false, false, false, 24));
            
            // Water
            Register(new BlockDefinition(BlockType.Water, "Water", 0.0f, new Color(30, 80, 200, 180), true, false, false, 25));
            Register(new BlockDefinition(BlockType.Saltwater, "Saltwater", 0.0f, new Color(20, 60, 160, 180), true, false, false, 26));
            
            // Clay types - different compositions for pottery and construction
            Register(new BlockDefinition(BlockType.RedClay, "Red Clay", 0.6f, new Color(165, 85, 70), false, false, false, 27));
            Register(new BlockDefinition(BlockType.BlueClay, "Blue Clay", 0.6f, new Color(100, 120, 140), false, false, false, 28));
            Register(new BlockDefinition(BlockType.FireClay, "Fire Clay", 0.7f, new Color(200, 180, 150), false, false, false, 29));
            
            // Light sources for illumination
            var torchDef = new BlockDefinition(BlockType.Torch, "Torch", 0.1f, new Color(255, 200, 100), true, false, false, 30);
            torchDef.LightEmission = 14; // Bright light source
            Register(torchDef);
            
            var lanternDef = new BlockDefinition(BlockType.Lantern, "Lantern", 0.3f, new Color(255, 230, 150), true, false, false, 31);
            lanternDef.LightEmission = 15; // Maximum light level
            Register(lanternDef);
            
            // Stick - crafting material and drops from trees
            Register(new BlockDefinition(BlockType.Stick, "Stick", 0.1f, new Color(120, 80, 40), false, false, false, 32));
        }

        public static void Register(BlockDefinition block)
        {
            _blocks[block.Type] = block;
        }

        public static BlockDefinition Get(BlockType type)
        {
            return _blocks.TryGetValue(type, out var block) ? block : _blocks[BlockType.Air];
        }

        public static bool IsSolid(BlockType type)
        {
            return Get(type).IsSolid;
        }

        public static bool IsTransparent(BlockType type)
        {
            return Get(type).IsTransparent;
        }
    }
}
