using System.Collections.Generic;
using TimelessTales.Blocks;

namespace TimelessTales.Core
{
    public enum ToolType
    {
        None,
        Pickaxe,
        Axe,
        Shovel,
        Hoe
    }

    public enum ToolTier
    {
        Hand = 0,
        Wood = 1,
        Stone = 2,
        Copper = 3,
        Bronze = 4,
        Iron = 5
    }

    public class ToolDefinition
    {
        public string Name { get; }
        public ToolType Type { get; }
        public ToolTier Tier { get; }
        public float SpeedMultiplier { get; }
        public float Durability { get; }
        public float CurrentDurability { get; set; }

        private static readonly HashSet<BlockType> PickaxeEffective = new()
        {
            BlockType.Stone, BlockType.Granite, BlockType.Basalt, BlockType.Limestone,
            BlockType.Sandstone, BlockType.Slate, BlockType.Cobblestone,
            BlockType.CopperOre, BlockType.TinOre, BlockType.IronOre, BlockType.Coal
        };

        private static readonly HashSet<BlockType> AxeEffective = new()
        {
            BlockType.Wood, BlockType.OakLog, BlockType.PineLog, BlockType.BirchLog, BlockType.Planks
        };

        private static readonly HashSet<BlockType> ShovelEffective = new()
        {
            BlockType.Dirt, BlockType.Grass, BlockType.Sand, BlockType.Gravel,
            BlockType.Clay, BlockType.RedClay, BlockType.BlueClay, BlockType.FireClay
        };

        private static readonly HashSet<BlockType> HoeEffective = new()
        {
            BlockType.Dirt, BlockType.Grass
        };

        public ToolDefinition(string name, ToolType type, ToolTier tier, float speedMultiplier, float durability)
        {
            Name = name;
            Type = type;
            Tier = tier;
            SpeedMultiplier = speedMultiplier;
            Durability = durability;
            CurrentDurability = durability;
        }

        public float GetEffectiveness(BlockType blockType)
        {
            switch (Type)
            {
                case ToolType.Pickaxe:
                    return PickaxeEffective.Contains(blockType) ? 2.0f : 0.5f;
                case ToolType.Axe:
                    return AxeEffective.Contains(blockType) ? 2.0f : 0.5f;
                case ToolType.Shovel:
                    return ShovelEffective.Contains(blockType) ? 2.0f : 0.5f;
                case ToolType.Hoe:
                    return HoeEffective.Contains(blockType) ? 1.5f : 0.5f;
                case ToolType.None:
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Creates an independent copy of this tool definition with full durability.
        /// Use this when giving a tool to a player to avoid shared state.
        /// </summary>
        public ToolDefinition Clone()
        {
            return new ToolDefinition(Name, Type, Tier, SpeedMultiplier, Durability);
        }
    }

    public static class ToolRegistry
    {
        private const float BASE_BREAK_TIME = 1.0f;

        private static readonly Dictionary<string, ToolDefinition> _tools = new();

        static ToolRegistry()
        {
            RegisterDefaultTools();
        }

        private static void RegisterDefaultTools()
        {
            // Wood tier
            Register("wood_pickaxe", new ToolDefinition("Wood Pickaxe", ToolType.Pickaxe, ToolTier.Wood, 2.0f, 60));
            Register("wood_axe", new ToolDefinition("Wood Axe", ToolType.Axe, ToolTier.Wood, 2.0f, 60));
            Register("wood_shovel", new ToolDefinition("Wood Shovel", ToolType.Shovel, ToolTier.Wood, 2.0f, 60));
            Register("wood_hoe", new ToolDefinition("Wood Hoe", ToolType.Hoe, ToolTier.Wood, 2.0f, 60));

            // Stone tier
            Register("stone_pickaxe", new ToolDefinition("Stone Pickaxe", ToolType.Pickaxe, ToolTier.Stone, 4.0f, 132));
            Register("stone_axe", new ToolDefinition("Stone Axe", ToolType.Axe, ToolTier.Stone, 4.0f, 132));
            Register("stone_shovel", new ToolDefinition("Stone Shovel", ToolType.Shovel, ToolTier.Stone, 4.0f, 132));
            Register("stone_hoe", new ToolDefinition("Stone Hoe", ToolType.Hoe, ToolTier.Stone, 4.0f, 132));

            // Copper tier
            Register("copper_pickaxe", new ToolDefinition("Copper Pickaxe", ToolType.Pickaxe, ToolTier.Copper, 6.0f, 200));
            Register("copper_axe", new ToolDefinition("Copper Axe", ToolType.Axe, ToolTier.Copper, 6.0f, 200));
            Register("copper_shovel", new ToolDefinition("Copper Shovel", ToolType.Shovel, ToolTier.Copper, 6.0f, 200));
            Register("copper_hoe", new ToolDefinition("Copper Hoe", ToolType.Hoe, ToolTier.Copper, 6.0f, 200));

            // Bronze tier
            Register("bronze_pickaxe", new ToolDefinition("Bronze Pickaxe", ToolType.Pickaxe, ToolTier.Bronze, 8.0f, 300));
            Register("bronze_axe", new ToolDefinition("Bronze Axe", ToolType.Axe, ToolTier.Bronze, 8.0f, 300));
            Register("bronze_shovel", new ToolDefinition("Bronze Shovel", ToolType.Shovel, ToolTier.Bronze, 8.0f, 300));
            Register("bronze_hoe", new ToolDefinition("Bronze Hoe", ToolType.Hoe, ToolTier.Bronze, 8.0f, 300));

            // Iron tier
            Register("iron_pickaxe", new ToolDefinition("Iron Pickaxe", ToolType.Pickaxe, ToolTier.Iron, 10.0f, 500));
            Register("iron_axe", new ToolDefinition("Iron Axe", ToolType.Axe, ToolTier.Iron, 10.0f, 500));
            Register("iron_shovel", new ToolDefinition("Iron Shovel", ToolType.Shovel, ToolTier.Iron, 10.0f, 500));
            Register("iron_hoe", new ToolDefinition("Iron Hoe", ToolType.Hoe, ToolTier.Iron, 10.0f, 500));
        }

        public static void Register(string key, ToolDefinition tool)
        {
            _tools[key] = tool;
        }

        public static ToolDefinition? GetTool(string key)
        {
            return _tools.TryGetValue(key, out var tool) ? tool.Clone() : null;
        }

        public static Dictionary<string, ToolDefinition> GetAllTools()
        {
            return new Dictionary<string, ToolDefinition>(_tools);
        }

        /// <summary>
        /// Calculates the time in seconds to break a block with the given tool.
        /// Without a tool: baseHardness * BASE_BREAK_TIME
        /// With a tool: baseHardness * BASE_BREAK_TIME / (speedMultiplier * effectiveness)
        /// </summary>
        public static float CalculateBreakTime(BlockType blockType, ToolDefinition? tool)
        {
            float hardness = BlockRegistry.Get(blockType).Hardness;

            if (hardness <= 0f)
                return 0.01f; // Instant break for zero-hardness blocks

            float baseTime = hardness * BASE_BREAK_TIME;

            if (tool == null || tool.Type == ToolType.None)
                return baseTime;

            float effectiveness = tool.GetEffectiveness(blockType);
            float divisor = tool.SpeedMultiplier * effectiveness;

            if (divisor <= 0f)
                return baseTime;

            return baseTime / divisor;
        }
    }
}
