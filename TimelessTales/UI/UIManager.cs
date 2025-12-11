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

        public UIManager(SpriteBatch spriteBatch, ContentManager content)
        {
            _spriteBatch = spriteBatch;
            _screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            _screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
            
            // Create a 1x1 white pixel texture for drawing shapes
            _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime, Player player, bool isPaused)
        {
            _player = player;
            // UI update logic if needed
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawCrosshair(spriteBatch);
            DrawHUD(spriteBatch);
            if (_player != null)
            {
                DrawInventory(spriteBatch, _player);
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
            
            // Get the 5 basic block types for hotbar
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
    }
}
