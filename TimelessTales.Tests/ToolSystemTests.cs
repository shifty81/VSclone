using Microsoft.Xna.Framework;
using TimelessTales.Core;
using TimelessTales.Entities;
using TimelessTales.Blocks;
using Xunit;

namespace TimelessTales.Tests
{
    public class ToolSystemTests
    {
        [Fact]
        public void ToolRegistry_HasDefaultToolsRegistered()
        {
            var tools = ToolRegistry.GetAllTools();
            Assert.True(tools.Count >= 20);
        }

        [Fact]
        public void ToolRegistry_GetTool_ReturnsCorrectTool()
        {
            var tool = ToolRegistry.GetTool("wood_pickaxe");
            Assert.NotNull(tool);
            Assert.Equal("Wood Pickaxe", tool.Name);
            Assert.Equal(ToolType.Pickaxe, tool.Type);
            Assert.Equal(ToolTier.Wood, tool.Tier);
        }

        [Fact]
        public void ToolRegistry_GetTool_ReturnsNullForUnknownKey()
        {
            var tool = ToolRegistry.GetTool("diamond_pickaxe");
            Assert.Null(tool);
        }

        [Fact]
        public void CalculateBreakTime_WithoutTool_UsesHardnessOnly()
        {
            // Stone has hardness 1.5
            float breakTime = ToolRegistry.CalculateBreakTime(BlockType.Stone, null);
            Assert.Equal(1.5f, breakTime);
        }

        [Fact]
        public void CalculateBreakTime_WithMatchingTool_IsFaster()
        {
            float bareHandTime = ToolRegistry.CalculateBreakTime(BlockType.Stone, null);
            var pickaxe = new ToolDefinition("Wood Pickaxe", ToolType.Pickaxe, ToolTier.Wood, 2.0f, 60);
            float toolTime = ToolRegistry.CalculateBreakTime(BlockType.Stone, pickaxe);

            Assert.True(toolTime < bareHandTime, "Tool should break matching blocks faster than bare hands");
        }

        [Fact]
        public void CalculateBreakTime_WithWrongTool_IsSlowerThanMatchingTool()
        {
            // Axe on stone (wrong tool) vs pickaxe on stone (right tool)
            var axe = new ToolDefinition("Wood Axe", ToolType.Axe, ToolTier.Wood, 2.0f, 60);
            var pickaxe = new ToolDefinition("Wood Pickaxe", ToolType.Pickaxe, ToolTier.Wood, 2.0f, 60);
            float axeTime = ToolRegistry.CalculateBreakTime(BlockType.Stone, axe);
            float pickaxeTime = ToolRegistry.CalculateBreakTime(BlockType.Stone, pickaxe);

            Assert.True(axeTime > pickaxeTime, "Wrong tool should be slower than matching tool");
        }

        [Fact]
        public void Pickaxe_IsEffectiveOnStone()
        {
            var pickaxe = new ToolDefinition("Test Pickaxe", ToolType.Pickaxe, ToolTier.Wood, 2.0f, 60);
            Assert.Equal(2.0f, pickaxe.GetEffectiveness(BlockType.Stone));
            Assert.Equal(2.0f, pickaxe.GetEffectiveness(BlockType.IronOre));
            Assert.Equal(2.0f, pickaxe.GetEffectiveness(BlockType.Coal));
            Assert.Equal(0.5f, pickaxe.GetEffectiveness(BlockType.Dirt));
        }

        [Fact]
        public void Axe_IsEffectiveOnWood()
        {
            var axe = new ToolDefinition("Test Axe", ToolType.Axe, ToolTier.Wood, 2.0f, 60);
            Assert.Equal(2.0f, axe.GetEffectiveness(BlockType.Wood));
            Assert.Equal(2.0f, axe.GetEffectiveness(BlockType.OakLog));
            Assert.Equal(2.0f, axe.GetEffectiveness(BlockType.PineLog));
            Assert.Equal(2.0f, axe.GetEffectiveness(BlockType.BirchLog));
            Assert.Equal(2.0f, axe.GetEffectiveness(BlockType.Planks));
            Assert.Equal(0.5f, axe.GetEffectiveness(BlockType.Stone));
        }

        [Fact]
        public void Shovel_IsEffectiveOnDirtAndSand()
        {
            var shovel = new ToolDefinition("Test Shovel", ToolType.Shovel, ToolTier.Wood, 2.0f, 60);
            Assert.Equal(2.0f, shovel.GetEffectiveness(BlockType.Dirt));
            Assert.Equal(2.0f, shovel.GetEffectiveness(BlockType.Sand));
            Assert.Equal(2.0f, shovel.GetEffectiveness(BlockType.Gravel));
            Assert.Equal(2.0f, shovel.GetEffectiveness(BlockType.Clay));
            Assert.Equal(0.5f, shovel.GetEffectiveness(BlockType.Stone));
        }

        [Fact]
        public void Hoe_IsEffectiveOnDirtAndGrass()
        {
            var hoe = new ToolDefinition("Test Hoe", ToolType.Hoe, ToolTier.Wood, 2.0f, 60);
            Assert.Equal(1.5f, hoe.GetEffectiveness(BlockType.Dirt));
            Assert.Equal(1.5f, hoe.GetEffectiveness(BlockType.Grass));
            Assert.Equal(0.5f, hoe.GetEffectiveness(BlockType.Stone));
        }

        [Fact]
        public void BareHands_HasUniformEffectiveness()
        {
            var hand = new ToolDefinition("Hand", ToolType.None, ToolTier.Hand, 1.0f, 0);
            Assert.Equal(1.0f, hand.GetEffectiveness(BlockType.Stone));
            Assert.Equal(1.0f, hand.GetEffectiveness(BlockType.Dirt));
            Assert.Equal(1.0f, hand.GetEffectiveness(BlockType.Wood));
        }

        [Fact]
        public void ToolDurability_DecreasesOnUse()
        {
            var pickaxe = new ToolDefinition("Wood Pickaxe", ToolType.Pickaxe, ToolTier.Wood, 2.0f, 60);
            Assert.Equal(60f, pickaxe.CurrentDurability);

            pickaxe.CurrentDurability -= 1;
            Assert.Equal(59f, pickaxe.CurrentDurability);
        }

        [Fact]
        public void ToolTier_SpeedMultipliers_AreCorrect()
        {
            var wood = ToolRegistry.GetTool("wood_pickaxe")!;
            var stone = ToolRegistry.GetTool("stone_pickaxe")!;
            var copper = ToolRegistry.GetTool("copper_pickaxe")!;
            var bronze = ToolRegistry.GetTool("bronze_pickaxe")!;
            var iron = ToolRegistry.GetTool("iron_pickaxe")!;

            Assert.Equal(2.0f, wood.SpeedMultiplier);
            Assert.Equal(4.0f, stone.SpeedMultiplier);
            Assert.Equal(6.0f, copper.SpeedMultiplier);
            Assert.Equal(8.0f, bronze.SpeedMultiplier);
            Assert.Equal(10.0f, iron.SpeedMultiplier);
        }

        [Fact]
        public void ToolTier_Durabilities_AreCorrect()
        {
            var wood = ToolRegistry.GetTool("wood_axe")!;
            var stone = ToolRegistry.GetTool("stone_axe")!;
            var copper = ToolRegistry.GetTool("copper_axe")!;
            var bronze = ToolRegistry.GetTool("bronze_axe")!;
            var iron = ToolRegistry.GetTool("iron_axe")!;

            Assert.Equal(60f, wood.Durability);
            Assert.Equal(132f, stone.Durability);
            Assert.Equal(200f, copper.Durability);
            Assert.Equal(300f, bronze.Durability);
            Assert.Equal(500f, iron.Durability);
        }

        [Fact]
        public void SoftBlocks_BreakFasterThanHardBlocks()
        {
            // Dirt (0.5 hardness) vs Iron Ore (3.0 hardness) with bare hands
            float dirtTime = ToolRegistry.CalculateBreakTime(BlockType.Dirt, null);
            float ironOreTime = ToolRegistry.CalculateBreakTime(BlockType.IronOre, null);

            Assert.True(dirtTime < ironOreTime, "Dirt should break faster than Iron Ore");
        }

        [Fact]
        public void HigherTierTool_BreaksFasterThanLowerTier()
        {
            var woodPick = new ToolDefinition("Wood Pickaxe", ToolType.Pickaxe, ToolTier.Wood, 2.0f, 60);
            var ironPick = new ToolDefinition("Iron Pickaxe", ToolType.Pickaxe, ToolTier.Iron, 10.0f, 500);

            float woodTime = ToolRegistry.CalculateBreakTime(BlockType.Stone, woodPick);
            float ironTime = ToolRegistry.CalculateBreakTime(BlockType.Stone, ironPick);

            Assert.True(ironTime < woodTime, "Iron pickaxe should break stone faster than wood pickaxe");
        }

        [Fact]
        public void CalculateBreakTime_ZeroHardness_ReturnsSmallValue()
        {
            // Water has 0 hardness
            float breakTime = ToolRegistry.CalculateBreakTime(BlockType.Water, null);
            Assert.True(breakTime > 0f, "Break time should never be zero");
            Assert.True(breakTime <= 0.01f, "Zero-hardness blocks should break near-instantly");
        }

        [Fact]
        public void Player_HasCurrentToolProperty()
        {
            var player = new Player(Vector3.Zero);
            Assert.Null(player.CurrentTool);

            var tool = new ToolDefinition("Test Pickaxe", ToolType.Pickaxe, ToolTier.Wood, 2.0f, 60);
            player.CurrentTool = tool;
            Assert.NotNull(player.CurrentTool);
            Assert.Equal("Test Pickaxe", player.CurrentTool.Name);
        }
    }
}
