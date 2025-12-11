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
            // UI update logic if needed
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawCrosshair(spriteBatch);
            DrawHUD(spriteBatch);
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
            // Draw hotbar
            int hotbarY = _screenHeight - 60;
            int slotSize = 40;
            int spacing = 5;
            int startX = (_screenWidth - (slotSize + spacing) * 9) / 2;
            
            var items = player.Inventory.GetAllItems();
            int index = 0;
            
            foreach (var item in items.Take(9))
            {
                int x = startX + (slotSize + spacing) * index;
                
                // Draw slot background
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x, hotbarY, slotSize, slotSize),
                    Color.DarkGray * 0.7f);
                
                // Draw block color (temporary representation)
                Color blockColor = BlockRegistry.Get(item.Key).Color;
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x + 5, hotbarY + 5, slotSize - 10, slotSize - 10),
                    blockColor);
                
                index++;
            }
        }
    }
}
