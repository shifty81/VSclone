using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.Entities;
using TimelessTales.Blocks;

namespace TimelessTales.UI
{
    /// <summary>
    /// Manages the game UI including HUD, crosshair, and inventory display
    /// </summary>
    public class UIManager
    {
        private readonly SpriteBatch _spriteBatch;
        private SpriteFont? _font;
        private Texture2D _pixelTexture;
        
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private Player? _player;
        private bool _inventoryOpen;

        public UIManager(SpriteBatch spriteBatch, ContentManager content)
        {
            _spriteBatch = spriteBatch;
            _screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            _screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
            
            // Create a 1x1 white pixel texture for drawing shapes
            _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime, Player player, bool isPaused, bool inventoryOpen = false)
        {
            _player = player;
            _inventoryOpen = inventoryOpen;
            // UI update logic if needed
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawCrosshair(spriteBatch);
            DrawHUD(spriteBatch);
            if (_player != null)
            {
                if (_inventoryOpen)
                {
                    DrawFullInventory(spriteBatch, _player);
                }
                else
                {
                    DrawInventory(spriteBatch, _player);
                }
                DrawBlockBreakProgress(spriteBatch, _player);
            }
        }

        private void DrawCrosshair(SpriteBatch spriteBatch)
        {
            int centerX = _screenWidth / 2;
            int centerY = _screenHeight / 2;
            int crosshairSize = 10;
            int thickness = 2;
            
            // Horizontal line
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(centerX - crosshairSize, centerY - thickness / 2, crosshairSize * 2, thickness),
                Color.White);
            
            // Vertical line
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(centerX - thickness / 2, centerY - crosshairSize, thickness, crosshairSize * 2),
                Color.White);
        }

        private void DrawHUD(SpriteBatch spriteBatch)
        {
            // Draw simple text HUD
            if (_font == null)
            {
                // For now, just draw basic shapes
                // Font would be loaded from Content in a real implementation
                return;
            }
            
            // Draw controls info in the future
            // string controls = "WASD: Move | Space: Jump | Shift: Sprint | LMB: Break | RMB: Place | 1-5: Select Block | P: Pause";
            // spriteBatch.DrawString(_font, controls, new Vector2(10, 10), Color.White);
        }

        private void DrawInventory(SpriteBatch spriteBatch, Player player)
        {
            // Draw hotbar at bottom of screen
            int hotbarY = _screenHeight - 70;
            int slotSize = 50;
            int spacing = 8;
            int hotbarSlots = 9;
            int startX = (_screenWidth - (slotSize + spacing) * hotbarSlots + spacing) / 2;
            
            var items = player.Inventory.GetAllItems();
            
            // Get the 9 basic block types for hotbar
            var hotbarItems = new[] 
            { 
                BlockType.Stone, 
                BlockType.Dirt, 
                BlockType.Planks, 
                BlockType.Cobblestone, 
                BlockType.Wood,
                BlockType.Grass,
                BlockType.Sand,
                BlockType.Gravel,
                BlockType.Clay
            };
            
            for (int i = 0; i < hotbarSlots; i++)
            {
                int x = startX + (slotSize + spacing) * i;
                
                // Determine if this slot is selected
                bool isSelected = (i < hotbarItems.Length && hotbarItems[i] == player.SelectedBlock);
                
                // Draw slot background
                Color slotColor = isSelected ? Color.White * 0.9f : Color.DarkGray * 0.7f;
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x, hotbarY, slotSize, slotSize),
                    slotColor);
                
                // Draw slot border
                int borderWidth = isSelected ? 3 : 2;
                Color borderColor = isSelected ? Color.Yellow : Color.Black;
                
                // Top border
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, hotbarY, slotSize, borderWidth), borderColor);
                // Bottom border
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, hotbarY + slotSize - borderWidth, slotSize, borderWidth), borderColor);
                // Left border
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, hotbarY, borderWidth, slotSize), borderColor);
                // Right border
                spriteBatch.Draw(_pixelTexture, new Rectangle(x + slotSize - borderWidth, hotbarY, borderWidth, slotSize), borderColor);
                
                // Draw block color if item exists in inventory
                if (i < hotbarItems.Length)
                {
                    BlockType blockType = hotbarItems[i];
                    int count = items.ContainsKey(blockType) ? items[blockType] : 0;
                    
                    if (count > 0 || blockType == player.SelectedBlock)
                    {
                        Color blockColor = BlockRegistry.Get(blockType).Color;
                        int padding = 8;
                        spriteBatch.Draw(_pixelTexture,
                            new Rectangle(x + padding, hotbarY + padding, slotSize - padding * 2, slotSize - padding * 2),
                            blockColor);
                        
                        // Draw count if available (simplified - would use font in real implementation)
                        // For now, we'll just show a visual indicator
                    }
                }
                
                // Draw slot number (1-9)
                // Would use font here, but for now just a visual indicator at bottom
            }
        }
        
        private void DrawBlockBreakProgress(SpriteBatch spriteBatch, Player player)
        {
            // Draw block break progress indicator near crosshair
            float progress = player.GetBreakProgress();
            
            if (progress > 0 && player.GetTargetBlockPos().HasValue)
            {
                int centerX = _screenWidth / 2;
                int centerY = _screenHeight / 2;
                int barWidth = 100;
                int barHeight = 10;
                int barX = centerX - barWidth / 2;
                int barY = centerY + 30; // Below crosshair
                
                // Draw background
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(barX - 2, barY - 2, barWidth + 4, barHeight + 4),
                    Color.Black * 0.7f);
                
                // Draw progress bar background
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(barX, barY, barWidth, barHeight),
                    Color.Gray * 0.5f);
                
                // Draw progress
                int progressWidth = (int)(barWidth * progress);
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(barX, barY, progressWidth, barHeight),
                    Color.White);
            }
        }
        
        private void DrawFullInventory(SpriteBatch spriteBatch, Player player)
        {
            // Draw semi-transparent overlay
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, 0, _screenWidth, _screenHeight),
                Color.Black * 0.7f);
            
            // Draw inventory title area
            int titleHeight = 60;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, 0, _screenWidth, titleHeight),
                Color.DarkSlateGray * 0.9f);
            
            // Draw inventory grid
            int slotSize = 60;
            int spacing = 10;
            int slotsPerRow = 8;
            int rows = 5;
            
            int gridWidth = (slotSize + spacing) * slotsPerRow + spacing;
            int gridHeight = (slotSize + spacing) * rows + spacing;
            int gridX = (_screenWidth - gridWidth) / 2;
            int gridY = (_screenHeight - gridHeight) / 2 + 30;
            
            // Draw inventory background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(gridX - 20, gridY - 20, gridWidth + 40, gridHeight + 40),
                Color.DarkGray * 0.9f);
            
            var items = player.Inventory.GetAllItems();
            
            // Draw inventory slots - iterate directly over dictionary
            int index = 0;
            foreach (var item in items)
            {
                if (index >= rows * slotsPerRow)
                    break;
                    
                int row = index / slotsPerRow;
                int col = index % slotsPerRow;
                int x = gridX + spacing + (slotSize + spacing) * col;
                int y = gridY + spacing + (slotSize + spacing) * row;
                
                // Draw slot background
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x, y, slotSize, slotSize),
                    Color.Gray * 0.6f);
                
                // Draw slot border
                int borderWidth = 2;
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, slotSize, borderWidth), Color.Black);
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + slotSize - borderWidth, slotSize, borderWidth), Color.Black);
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, borderWidth, slotSize), Color.Black);
                spriteBatch.Draw(_pixelTexture, new Rectangle(x + slotSize - borderWidth, y, borderWidth, slotSize), Color.Black);
                
                // Draw item
                Color blockColor = BlockRegistry.Get(item.Key).Color;
                int padding = 8;
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x + padding, y + padding, slotSize - padding * 2, slotSize - padding * 2),
                    blockColor);
                
                index++;
            }
            
            // Draw empty slots for remaining spaces
            for (; index < rows * slotsPerRow; index++)
            {
                int row = index / slotsPerRow;
                int col = index % slotsPerRow;
                int x = gridX + spacing + (slotSize + spacing) * col;
                int y = gridY + spacing + (slotSize + spacing) * row;
                
                // Draw slot background
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x, y, slotSize, slotSize),
                    Color.Gray * 0.6f);
                
                // Draw slot border
                int borderWidth = 2;
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, slotSize, borderWidth), Color.Black);
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + slotSize - borderWidth, slotSize, borderWidth), Color.Black);
                spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, borderWidth, slotSize), Color.Black);
                spriteBatch.Draw(_pixelTexture, new Rectangle(x + slotSize - borderWidth, y, borderWidth, slotSize), Color.Black);
            }
            
            // Draw "Press I to close" message
            // (Would use font here in real implementation)
        }
    }
}
