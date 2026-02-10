using System.Collections.Generic;
using TimelessTales.Core;
using TimelessTales.Entities;
using TimelessTales.Blocks;
using Xunit;

namespace TimelessTales.Tests
{
    public class CraftingSystemTests
    {
        [Fact]
        public void CraftingSystem_RegistersDefaultRecipes()
        {
            var system = new CraftingSystem();
            var recipes = system.GetAllRecipes();

            Assert.True(recipes.Count >= 5);
        }

        [Fact]
        public void CraftingSystem_CanRegisterCustomRecipe()
        {
            var system = new CraftingSystem();
            int initialCount = system.GetAllRecipes().Count;

            var recipe = new CraftingRecipe(
                "Test Recipe", "A test",
                new Dictionary<BlockType, int> { { BlockType.Dirt, 1 } },
                new Dictionary<BlockType, int> { { BlockType.Grass, 1 } });

            system.RegisterRecipe(recipe);

            Assert.Equal(initialCount + 1, system.GetAllRecipes().Count);
        }

        [Fact]
        public void CanCraft_ReturnsTrueWithSufficientMaterials()
        {
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Wood, 5);

            var recipes = system.GetAllRecipes();
            var woodToPlanks = recipes[0]; // Wood > Plank

            Assert.True(system.CanCraft(woodToPlanks, inventory));
        }

        [Fact]
        public void CanCraft_ReturnsFalseWithInsufficientMaterials()
        {
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            // No wood in inventory

            var recipes = system.GetAllRecipes();
            var woodToPlanks = recipes[0];

            Assert.False(system.CanCraft(woodToPlanks, inventory));
        }

        [Fact]
        public void CanCraft_ReturnsFalseWithNullInventory()
        {
            var system = new CraftingSystem();
            var recipes = system.GetAllRecipes();

            Assert.False(system.CanCraft(recipes[0], null));
        }

        [Fact]
        public void CanCraft_ReturnsFalseWithNullRecipe()
        {
            var system = new CraftingSystem();
            var inventory = new Inventory(40);

            Assert.False(system.CanCraft(null, inventory));
        }

        [Fact]
        public void Craft_WoodToPlanks_RemovesInputAndAddsOutput()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Wood, 3);

            var woodToPlanks = system.GetAllRecipes()[0];

            // Act
            bool result = system.Craft(woodToPlanks, inventory);

            // Assert
            Assert.True(result);
            Assert.Equal(2, inventory.GetItemCount(BlockType.Wood));
            Assert.Equal(4, inventory.GetItemCount(BlockType.Planks));
        }

        [Fact]
        public void Craft_PlanksToSticks_RemovesInputAndAddsOutput()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Planks, 6);

            var planksToSticks = system.GetAllRecipes()[1];

            // Act
            bool result = system.Craft(planksToSticks, inventory);

            // Assert
            Assert.True(result);
            Assert.Equal(4, inventory.GetItemCount(BlockType.Planks));
            Assert.Equal(4, inventory.GetItemCount(BlockType.Stick));
        }

        [Fact]
        public void Craft_RedClay_RemovesInputAndAddsOutput()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Clay, 8);

            var redClay = system.GetAllRecipes()[2];

            // Act
            bool result = system.Craft(redClay, inventory);

            // Assert
            Assert.True(result);
            Assert.Equal(4, inventory.GetItemCount(BlockType.Clay)); // 8 - 4
            Assert.Equal(2, inventory.GetItemCount(BlockType.RedClay));
        }

        [Fact]
        public void Craft_FailsWithInsufficientMaterials()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Wood, 0);

            var woodToPlanks = system.GetAllRecipes()[0];

            // Act
            bool result = system.Craft(woodToPlanks, inventory);

            // Assert
            Assert.False(result);
            Assert.Equal(0, inventory.GetItemCount(BlockType.Planks));
        }

        [Fact]
        public void Craft_DoesNotModifyInventoryOnFailure()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Planks, 1); // Need 2 for sticks

            var planksToSticks = system.GetAllRecipes()[1];

            // Act
            bool result = system.Craft(planksToSticks, inventory);

            // Assert
            Assert.False(result);
            Assert.Equal(1, inventory.GetItemCount(BlockType.Planks));
        }

        [Fact]
        public void GetAvailableRecipes_FiltersCorrectly()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Wood, 1);

            // Act
            var available = system.GetAvailableRecipes(inventory);

            // Assert - only Wood > Plank should be available
            Assert.Single(available);
            Assert.Equal("WOOD > PLANK", available[0].Name);
        }

        [Fact]
        public void GetAvailableRecipes_ReturnsEmptyForEmptyInventory()
        {
            var system = new CraftingSystem();
            var inventory = new Inventory(40);

            var available = system.GetAvailableRecipes(inventory);

            Assert.Empty(available);
        }

        [Fact]
        public void GetAvailableRecipes_ReturnsMultipleWhenMaterialsSuffice()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Wood, 1);
            inventory.AddItem(BlockType.Planks, 2);

            // Act
            var available = system.GetAvailableRecipes(inventory);

            // Assert - Wood > Plank and Plank > Stick should be available
            Assert.Equal(2, available.Count);
        }

        [Fact]
        public void GetRecipesByCategory_FiltersCorrectly()
        {
            var system = new CraftingSystem();

            var handCrafting = system.GetRecipesByCategory(CraftingCategory.HandCrafting);
            var pottery = system.GetRecipesByCategory(CraftingCategory.Pottery);
            var knapping = system.GetRecipesByCategory(CraftingCategory.Knapping);

            Assert.True(handCrafting.Count >= 4);
            Assert.Single(pottery);
            Assert.Empty(knapping);
        }

        [Fact]
        public void CraftingRecipe_HasCorrectProperties()
        {
            var recipe = new CraftingRecipe(
                "Test", "Description",
                new Dictionary<BlockType, int> { { BlockType.Stone, 2 } },
                new Dictionary<BlockType, int> { { BlockType.Cobblestone, 1 } },
                CraftingCategory.Knapping);

            Assert.Equal("Test", recipe.Name);
            Assert.Equal("Description", recipe.Description);
            Assert.Equal(CraftingCategory.Knapping, recipe.Category);
            Assert.Single(recipe.Inputs);
            Assert.Single(recipe.Outputs);
        }

        [Fact]
        public void Craft_Sandstone_RequiresMultipleInputTypes()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Sand, 4);
            inventory.AddItem(BlockType.Clay, 1);

            var sandstone = system.GetAllRecipes()[4]; // Sandstone recipe

            // Act
            bool result = system.Craft(sandstone, inventory);

            // Assert
            Assert.True(result);
            Assert.Equal(0, inventory.GetItemCount(BlockType.Sand));
            Assert.Equal(0, inventory.GetItemCount(BlockType.Clay));
            Assert.Equal(2, inventory.GetItemCount(BlockType.Sandstone));
        }

        [Fact]
        public void Craft_Sandstone_FailsWithPartialInputs()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Sand, 4);
            // Missing clay

            var sandstone = system.GetAllRecipes()[4];

            // Act
            bool result = system.Craft(sandstone, inventory);

            // Assert
            Assert.False(result);
            Assert.Equal(4, inventory.GetItemCount(BlockType.Sand)); // Unchanged
        }

        [Fact]
        public void Craft_MultipleTimes_AccumulatesOutput()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Wood, 3);

            var woodToPlanks = system.GetAllRecipes()[0];

            // Act
            system.Craft(woodToPlanks, inventory);
            system.Craft(woodToPlanks, inventory);
            system.Craft(woodToPlanks, inventory);

            // Assert
            Assert.Equal(0, inventory.GetItemCount(BlockType.Wood));
            Assert.Equal(12, inventory.GetItemCount(BlockType.Planks));
        }

        [Fact]
        public void Craft_ChainedRecipes_WoodToPlanksToSticks()
        {
            // Arrange
            var system = new CraftingSystem();
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Wood, 1);

            var woodToPlanks = system.GetAllRecipes()[0];
            var planksToSticks = system.GetAllRecipes()[1];

            // Act - craft wood into planks, then planks into sticks
            system.Craft(woodToPlanks, inventory);
            Assert.Equal(4, inventory.GetItemCount(BlockType.Planks));

            system.Craft(planksToSticks, inventory);

            // Assert
            Assert.Equal(0, inventory.GetItemCount(BlockType.Wood));
            Assert.Equal(2, inventory.GetItemCount(BlockType.Planks));
            Assert.Equal(4, inventory.GetItemCount(BlockType.Stick));
        }
    }
}
