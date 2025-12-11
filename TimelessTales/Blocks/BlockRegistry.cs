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
        Cobblestone
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

        public BlockDefinition(BlockType type, string name, float hardness, Color color, 
                              bool isTransparent = false, bool affectedByGravity = false, bool isOre = false)
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
            Register(new BlockDefinition(BlockType.Air, "Air", 0, Color.Transparent, true, false));
            Register(new BlockDefinition(BlockType.Stone, "Stone", 1.5f, Color.Gray));
            Register(new BlockDefinition(BlockType.Dirt, "Dirt", 0.5f, new Color(139, 69, 19)));
            Register(new BlockDefinition(BlockType.Grass, "Grass", 0.6f, Color.Green));
            Register(new BlockDefinition(BlockType.Sand, "Sand", 0.5f, Color.SandyBrown, false, true));
            Register(new BlockDefinition(BlockType.Gravel, "Gravel", 0.6f, Color.DarkGray, false, true));
            Register(new BlockDefinition(BlockType.Clay, "Clay", 0.6f, new Color(178, 140, 110)));
            
            // Geological rock layers
            Register(new BlockDefinition(BlockType.Granite, "Granite", 2.0f, new Color(100, 100, 100)));
            Register(new BlockDefinition(BlockType.Limestone, "Limestone", 1.2f, new Color(200, 200, 180)));
            Register(new BlockDefinition(BlockType.Basalt, "Basalt", 1.8f, new Color(60, 60, 70)));
            Register(new BlockDefinition(BlockType.Sandstone, "Sandstone", 1.0f, new Color(210, 180, 140)));
            Register(new BlockDefinition(BlockType.Slate, "Slate", 1.5f, new Color(70, 80, 90)));
            
            // Ores
            Register(new BlockDefinition(BlockType.CopperOre, "Copper Ore", 2.5f, new Color(184, 115, 51), false, false, true));
            Register(new BlockDefinition(BlockType.TinOre, "Tin Ore", 2.5f, new Color(150, 150, 150), false, false, true));
            Register(new BlockDefinition(BlockType.IronOre, "Iron Ore", 3.0f, new Color(139, 90, 90), false, false, true));
            Register(new BlockDefinition(BlockType.Coal, "Coal", 2.0f, new Color(30, 30, 30), false, false, true));
            
            // Wood
            Register(new BlockDefinition(BlockType.Wood, "Wood", 1.0f, new Color(139, 90, 43)));
            Register(new BlockDefinition(BlockType.Leaves, "Leaves", 0.2f, Color.DarkGreen, true));
            
            // Crafted
            Register(new BlockDefinition(BlockType.Planks, "Planks", 1.0f, new Color(160, 110, 60)));
            Register(new BlockDefinition(BlockType.Cobblestone, "Cobblestone", 1.5f, new Color(120, 120, 120)));
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
