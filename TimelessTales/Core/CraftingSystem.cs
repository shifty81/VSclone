using System;
using System.Collections.Generic;
using System.Linq;
using TimelessTales.Blocks;
using TimelessTales.Entities;

namespace TimelessTales.Core
{
    public enum CraftingCategory
    {
        HandCrafting,
        Knapping,
        Pottery,
        Carpentry,
        Smelting
    }

    public class CraftingRecipe
    {
        public string Name { get; }
        public string Description { get; }
        public Dictionary<BlockType, int> Inputs { get; }
        public Dictionary<BlockType, int> Outputs { get; }
        public CraftingCategory Category { get; }

        public CraftingRecipe(string name, string description,
            Dictionary<BlockType, int> inputs,
            Dictionary<BlockType, int> outputs,
            CraftingCategory category = CraftingCategory.HandCrafting)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            Outputs = outputs ?? throw new ArgumentNullException(nameof(outputs));
            Category = category;
        }
    }

    public class CraftingSystem
    {
        private readonly List<CraftingRecipe> _recipes = new List<CraftingRecipe>();

        public CraftingSystem()
        {
            RegisterDefaultRecipes();
        }

        public void RegisterRecipe(CraftingRecipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            _recipes.Add(recipe);
        }

        public List<CraftingRecipe> GetAllRecipes()
        {
            return new List<CraftingRecipe>(_recipes);
        }

        public List<CraftingRecipe> GetRecipesByCategory(CraftingCategory category)
        {
            return _recipes.Where(r => r.Category == category).ToList();
        }

        public List<CraftingRecipe> GetAvailableRecipes(Inventory inventory)
        {
            return _recipes.Where(r => CanCraft(r, inventory)).ToList();
        }

        public bool CanCraft(CraftingRecipe recipe, Inventory inventory)
        {
            if (recipe == null || inventory == null) return false;

            foreach (var input in recipe.Inputs)
            {
                if (inventory.GetItemCount(input.Key) < input.Value)
                    return false;
            }
            return true;
        }

        public bool Craft(CraftingRecipe recipe, Inventory inventory)
        {
            if (!CanCraft(recipe, inventory))
                return false;

            // Remove inputs
            foreach (var input in recipe.Inputs)
            {
                if (!inventory.RemoveItem(input.Key, input.Value))
                {
                    // This shouldn't happen since CanCraft passed, but handle gracefully
                    return false;
                }
            }

            // Add outputs
            foreach (var output in recipe.Outputs)
            {
                inventory.AddItem(output.Key, output.Value);
            }

            return true;
        }

        private void RegisterDefaultRecipes()
        {
            // 1 Wood -> 4 Planks
            RegisterRecipe(new CraftingRecipe(
                "WOOD > PLANK",
                "1 WOOD = 4 PLANKS",
                new Dictionary<BlockType, int> { { BlockType.Wood, 1 } },
                new Dictionary<BlockType, int> { { BlockType.Planks, 4 } },
                CraftingCategory.HandCrafting));

            // 2 Planks -> 4 Sticks
            RegisterRecipe(new CraftingRecipe(
                "PLANK > STICK",
                "2 PLANKS = 4 STICKS",
                new Dictionary<BlockType, int> { { BlockType.Planks, 2 } },
                new Dictionary<BlockType, int> { { BlockType.Stick, 4 } },
                CraftingCategory.HandCrafting));

            // 4 Clay -> 1 Clay Block (compresses clay)
            RegisterRecipe(new CraftingRecipe(
                "CLAY BLOCK",
                "4 CLAY = 1 CLAY BLOCK",
                new Dictionary<BlockType, int> { { BlockType.Clay, 4 } },
                new Dictionary<BlockType, int> { { BlockType.Clay, 1 } },
                CraftingCategory.HandCrafting));

            // 2 Stone -> 1 Cobblestone
            RegisterRecipe(new CraftingRecipe(
                "COBBLESTONE",
                "2 STONE = 1 COBBLESTONE",
                new Dictionary<BlockType, int> { { BlockType.Stone, 2 } },
                new Dictionary<BlockType, int> { { BlockType.Cobblestone, 1 } },
                CraftingCategory.HandCrafting));

            // 4 Sand + 1 Clay -> 2 Sandstone
            RegisterRecipe(new CraftingRecipe(
                "SANDSTONE",
                "4 SAND + 1 CLAY = 2 SANDSTONE",
                new Dictionary<BlockType, int> { { BlockType.Sand, 4 }, { BlockType.Clay, 1 } },
                new Dictionary<BlockType, int> { { BlockType.Sandstone, 2 } },
                CraftingCategory.HandCrafting));
        }
    }
}
