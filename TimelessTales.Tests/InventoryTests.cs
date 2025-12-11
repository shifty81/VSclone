using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using TimelessTales.Blocks;
using Xunit;

namespace TimelessTales.Tests
{
    public class InventoryTests
    {
        [Fact]
        public void Inventory_CanAddItems()
        {
            // Arrange
            var inventory = new Inventory(40);
            
            // Act
            bool result = inventory.AddItem(BlockType.Stone, 10);
            
            // Assert
            Assert.True(result);
            Assert.Equal(10, inventory.GetItemCount(BlockType.Stone));
        }
        
        [Fact]
        public void Inventory_CanRemoveItems()
        {
            // Arrange
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Stone, 10);
            
            // Act
            bool result = inventory.RemoveItem(BlockType.Stone, 5);
            
            // Assert
            Assert.True(result);
            Assert.Equal(5, inventory.GetItemCount(BlockType.Stone));
        }
        
        [Fact]
        public void Inventory_CannotRemoveMoreThanExists()
        {
            // Arrange
            var inventory = new Inventory(40);
            inventory.AddItem(BlockType.Stone, 10);
            
            // Act
            bool result = inventory.RemoveItem(BlockType.Stone, 15);
            
            // Assert
            Assert.False(result);
            Assert.Equal(10, inventory.GetItemCount(BlockType.Stone));
        }
        
        [Fact]
        public void Inventory_DoesNotAddAirBlocks()
        {
            // Arrange
            var inventory = new Inventory(40);
            
            // Act
            bool result = inventory.AddItem(BlockType.Air, 10);
            
            // Assert
            Assert.False(result);
            Assert.Equal(0, inventory.GetItemCount(BlockType.Air));
        }
        
        [Fact]
        public void Inventory_StacksItemsOfSameType()
        {
            // Arrange
            var inventory = new Inventory(40);
            
            // Act
            inventory.AddItem(BlockType.Stone, 10);
            inventory.AddItem(BlockType.Stone, 20);
            
            // Assert
            Assert.Equal(30, inventory.GetItemCount(BlockType.Stone));
        }
        
        [Fact]
        public void Player_StartsWithInitialInventory()
        {
            // Arrange & Act
            var player = new Player(Vector3.Zero);
            
            // Assert
            Assert.NotNull(player.Inventory);
            Assert.True(player.Inventory.GetItemCount(BlockType.Stone) > 0);
            Assert.True(player.Inventory.GetItemCount(BlockType.Dirt) > 0);
            Assert.True(player.Inventory.GetItemCount(BlockType.Planks) > 0);
        }
        
        [Fact]
        public void Player_HasSelectedBlockInitialized()
        {
            // Arrange & Act
            var player = new Player(Vector3.Zero);
            
            // Assert
            Assert.NotEqual(BlockType.Air, player.SelectedBlock);
        }
    }
}
