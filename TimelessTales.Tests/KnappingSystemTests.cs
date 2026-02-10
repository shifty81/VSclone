using System.Collections.Generic;
using System.Linq;
using TimelessTales.Core;
using TimelessTales.Entities;
using TimelessTales.Blocks;
using Xunit;

namespace TimelessTales.Tests
{
    public class KnappingSystemTests
    {
        [Fact]
        public void FlintBlockType_ExistsInRegistry()
        {
            var blockDef = BlockRegistry.Get(BlockType.Flint);

            Assert.Equal("Flint", blockDef.Name);
            Assert.False(blockDef.IsTransparent);
        }

        [Fact]
        public void FlintKnife_ExistsInRegistry()
        {
            var blockDef = BlockRegistry.Get(BlockType.FlintKnife);

            Assert.Equal("Flint Knife", blockDef.Name);
        }

        [Fact]
        public void FlintAxeHead_ExistsInRegistry()
        {
            var blockDef = BlockRegistry.Get(BlockType.FlintAxeHead);

            Assert.Equal("Flint Axe Head", blockDef.Name);
        }

        [Fact]
        public void FlintShovelHead_ExistsInRegistry()
        {
            var blockDef = BlockRegistry.Get(BlockType.FlintShovelHead);

            Assert.Equal("Flint Shovel Head", blockDef.Name);
        }

        [Fact]
        public void FlintHoeHead_ExistsInRegistry()
        {
            var blockDef = BlockRegistry.Get(BlockType.FlintHoeHead);

            Assert.Equal("Flint Hoe Head", blockDef.Name);
        }

        [Fact]
        public void CraftingSystem_HasKnappingRecipes()
        {
            var system = new CraftingSystem();
            var knappingRecipes = system.GetRecipesByCategory(CraftingCategory.Knapping);

            Assert.True(knappingRecipes.Count >= 4);
        }

        [Fact]
        public void KnappingRecipe_FlintKnife_RequiresFlint()
        {
            var system = new CraftingSystem();
            var knappingRecipes = system.GetRecipesByCategory(CraftingCategory.Knapping);
            var knifeRecipe = knappingRecipes.First(r => r.Name == "FLINT KNIFE");

            Assert.Contains(BlockType.Flint, knifeRecipe.Inputs.Keys);
            Assert.Equal(2, knifeRecipe.Inputs[BlockType.Flint]);
            Assert.Contains(BlockType.FlintKnife, knifeRecipe.Outputs.Keys);
        }

        [Fact]
        public void KnappingRecipe_FlintAxeHead_Requires3Flint()
        {
            var system = new CraftingSystem();
            var knappingRecipes = system.GetRecipesByCategory(CraftingCategory.Knapping);
            var axeRecipe = knappingRecipes.First(r => r.Name == "FLINT AXE HEAD");

            Assert.Equal(3, axeRecipe.Inputs[BlockType.Flint]);
            Assert.Contains(BlockType.FlintAxeHead, axeRecipe.Outputs.Keys);
        }

        [Fact]
        public void CanCraft_FlintKnife_WithSufficientFlint()
        {
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Flint, 5);

            var knifeRecipe = system.GetRecipesByCategory(CraftingCategory.Knapping)
                .First(r => r.Name == "FLINT KNIFE");

            Assert.True(system.CanCraft(knifeRecipe, inventory));
        }

        [Fact]
        public void CannotCraft_FlintKnife_WithInsufficientFlint()
        {
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Flint, 1);

            var knifeRecipe = system.GetRecipesByCategory(CraftingCategory.Knapping)
                .First(r => r.Name == "FLINT KNIFE");

            Assert.False(system.CanCraft(knifeRecipe, inventory));
        }

        [Fact]
        public void Craft_FlintKnife_ConsumesFlintAndProducesKnife()
        {
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Flint, 5);

            var knifeRecipe = system.GetRecipesByCategory(CraftingCategory.Knapping)
                .First(r => r.Name == "FLINT KNIFE");

            bool result = system.Craft(knifeRecipe, inventory);

            Assert.True(result);
            Assert.Equal(3, inventory.GetItemCount(BlockType.Flint));
            Assert.Equal(1, inventory.GetItemCount(BlockType.FlintKnife));
        }

        [Fact]
        public void Craft_FlintAxeHead_ConsumesCorrectAmount()
        {
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Flint, 6);

            var axeRecipe = system.GetRecipesByCategory(CraftingCategory.Knapping)
                .First(r => r.Name == "FLINT AXE HEAD");

            bool result = system.Craft(axeRecipe, inventory);

            Assert.True(result);
            Assert.Equal(3, inventory.GetItemCount(BlockType.Flint));
            Assert.Equal(1, inventory.GetItemCount(BlockType.FlintAxeHead));
        }

        [Fact]
        public void AllKnappingRecipes_AreInKnappingCategory()
        {
            var system = new CraftingSystem();
            var knappingRecipes = system.GetRecipesByCategory(CraftingCategory.Knapping);

            foreach (var recipe in knappingRecipes)
            {
                Assert.Equal(CraftingCategory.Knapping, recipe.Category);
            }
        }

        [Fact]
        public void TotalDefaultRecipes_IncludesKnappingRecipes()
        {
            var system = new CraftingSystem();
            var allRecipes = system.GetAllRecipes();

            // 5 original + 4 knapping = 9 total
            Assert.True(allRecipes.Count >= 9);
        }
    }
}
