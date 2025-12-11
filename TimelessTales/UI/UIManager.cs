using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.Entities;
using TimelessTales.Blocks;
using TimelessTales.Core;
using TimelessTales.World;
using System;

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
        private TimeManager? _timeManager;
        private WorldManager? _worldManager;
        private bool _inventoryOpen;
        private bool _worldMapOpen;

        public UIManager(SpriteBatch spriteBatch, ContentManager content)
        {
            _spriteBatch = spriteBatch;
            _screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            _screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
            
            // Create a 1x1 white pixel texture for drawing shapes
            _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime, Player player, TimeManager timeManager, WorldManager worldManager, bool isPaused, bool inventoryOpen = false, bool worldMapOpen = false)
        {
            _player = player;
            _timeManager = timeManager;
            _worldManager = worldManager;
            _inventoryOpen = inventoryOpen;
            _worldMapOpen = worldMapOpen;
            // UI update logic if needed
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawCrosshair(spriteBatch);
            DrawHUD(spriteBatch);
            
            if (_player != null)
            {
                if (_worldMapOpen)
                {
                    DrawWorldMap(spriteBatch, _player);
                }
                else if (_inventoryOpen)
                {
                    DrawFullInventory(spriteBatch, _player);
                }
                else
                {
                    DrawInventory(spriteBatch, _player);
                    DrawMinimap(spriteBatch, _player);
                    DrawClock(spriteBatch);
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
        
        private void DrawMinimap(SpriteBatch spriteBatch, Player player)
        {
            // Minimap in upper right corner
            int minimapSize = 150;
            int padding = 10;
            int minimapX = _screenWidth - minimapSize - padding;
            int minimapY = padding;
            
            // Draw minimap background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(minimapX, minimapY, minimapSize, minimapSize),
                Color.Black * 0.7f);
            
            // Draw minimap border
            int borderWidth = 2;
            spriteBatch.Draw(_pixelTexture, new Rectangle(minimapX, minimapY, minimapSize, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(minimapX, minimapY + minimapSize - borderWidth, minimapSize, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(minimapX, minimapY, borderWidth, minimapSize), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(minimapX + minimapSize - borderWidth, minimapY, borderWidth, minimapSize), Color.White);
            
            // Draw player position indicator (red dot at center)
            int playerDotSize = 6;
            int centerX = minimapX + minimapSize / 2 - playerDotSize / 2;
            int centerY = minimapY + minimapSize / 2 - playerDotSize / 2;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(centerX, centerY, playerDotSize, playerDotSize),
                Color.Red);
            
            // Draw simple terrain representation if world manager is available
            if (_worldManager != null)
            {
                int mapRange = 50; // Blocks to show on each side
                if (mapRange <= 0) mapRange = 1; // Prevent division by zero
                
                int pixelsPerBlock = minimapSize / (mapRange * 2);
                
                Vector3 playerPos = player.Position;
                int playerX = (int)Math.Floor(playerPos.X);
                int playerZ = (int)Math.Floor(playerPos.Z);
                
                // Draw a simplified top-down view with reduced sampling for performance
                int step = Math.Max(1, mapRange / 25); // Sample approximately 50x50 grid
                for (int dx = -mapRange; dx < mapRange; dx += step)
                {
                    for (int dz = -mapRange; dz < mapRange; dz += step)
                    {
                        int worldX = playerX + dx;
                        int worldZ = playerZ + dz;
                        
                        // Find highest solid block
                        int highestY = -1;
                        for (int y = 100; y >= 0; y--)
                        {
                            if (_worldManager.IsBlockSolid(worldX, y, worldZ))
                            {
                                highestY = y;
                                break;
                            }
                        }
                        
                        if (highestY >= 0)
                        {
                            // Color based on height
                            float heightFactor = MathHelper.Clamp(highestY / 100f, 0f, 1f);
                            Color terrainColor = Color.Lerp(new Color(0, 100, 0), new Color(150, 150, 150), heightFactor);
                            
                            int pixelX = minimapX + (dx + mapRange) * pixelsPerBlock;
                            int pixelZ = minimapY + (dz + mapRange) * pixelsPerBlock;
                            
                            if (pixelsPerBlock > 0)
                            {
                                spriteBatch.Draw(_pixelTexture,
                                    new Rectangle(pixelX, pixelZ, Math.Max(1, pixelsPerBlock * step), Math.Max(1, pixelsPerBlock * step)),
                                    terrainColor * 0.6f);
                            }
                        }
                    }
                }
            }
            
            // Draw coordinates below minimap
            int coordY = minimapY + minimapSize + 5;
            int coordBoxHeight = 20;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(minimapX, coordY, minimapSize, coordBoxHeight),
                Color.Black * 0.7f);
            
            // Draw coordinate text representation with bars (since we don't have font)
            // Display X, Y, Z coordinates as visual bars
            Vector3 pos = player.Position;
            int barWidth = 3;
            int barSpacing = 2;
            int barStartX = minimapX + 5;
            
            // X coordinate (Red bars)
            int xBars = Math.Abs((int)pos.X) % 20;
            for (int i = 0; i < xBars; i++)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(barStartX + i * (barWidth + barSpacing), coordY + 2, barWidth, 5),
                    Color.Red);
            }
            
            // Y coordinate (Green bars) 
            int yBars = Math.Abs((int)pos.Y) % 20;
            for (int i = 0; i < yBars; i++)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(barStartX + i * (barWidth + barSpacing), coordY + 9, barWidth, 5),
                    Color.Green);
            }
            
            // Z coordinate (Blue bars)
            int zBars = Math.Abs((int)pos.Z) % 20;
            for (int i = 0; i < zBars; i++)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(barStartX + i * (barWidth + barSpacing), coordY + 16, barWidth, 5),
                    Color.Blue);
            }
        }
        
        private void DrawClock(SpriteBatch spriteBatch)
        {
            if (_timeManager == null) return;
            
            // Clock below minimap
            int minimapSize = 150;
            int padding = 10;
            int clockX = _screenWidth - minimapSize - padding;
            int clockY = padding + minimapSize + 30; // Below coordinates
            int clockWidth = minimapSize;
            int clockHeight = 40;
            
            // Draw clock background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(clockX, clockY, clockWidth, clockHeight),
                Color.Black * 0.7f);
            
            // Draw clock border
            int borderWidth = 2;
            spriteBatch.Draw(_pixelTexture, new Rectangle(clockX, clockY, clockWidth, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(clockX, clockY + clockHeight - borderWidth, clockWidth, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(clockX, clockY, borderWidth, clockHeight), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(clockX + clockWidth - borderWidth, clockY, borderWidth, clockHeight), Color.White);
            
            // Draw time of day gauge
            float timeOfDay = _timeManager.TimeOfDay;
            int gaugeWidth = (int)(clockWidth * timeOfDay);
            
            // Color based on time of day
            Color gaugeColor;
            if (_timeManager.IsDaytime)
            {
                gaugeColor = Color.Yellow;
            }
            else
            {
                gaugeColor = Color.DarkBlue;
            }
            
            // Draw gauge fill
            if (gaugeWidth > 0)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(clockX + 2, clockY + 2, gaugeWidth - 4, clockHeight - 4),
                    gaugeColor * 0.6f);
            }
            
            // Draw sun/moon indicator
            int indicatorSize = 10;
            int indicatorX = clockX + (int)((clockWidth - indicatorSize) * timeOfDay);
            int indicatorY = clockY + clockHeight / 2 - indicatorSize / 2;
            
            Color indicatorColor = _timeManager.IsDaytime ? Color.Yellow : Color.LightBlue;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(indicatorX, indicatorY, indicatorSize, indicatorSize),
                indicatorColor);
            
            // Draw day count indicator (small bars)
            int dayCount = _timeManager.DayCount;
            int dayBarWidth = 4;
            int dayBarHeight = 8;
            int dayBarY = clockY + 5;
            int dayBarX = clockX + 5;
            
            for (int i = 0; i < Math.Min(dayCount, 10); i++)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(dayBarX + i * (dayBarWidth + 2), dayBarY, dayBarWidth, dayBarHeight),
                    Color.White * 0.8f);
            }
        }
        
        private void DrawWorldMap(SpriteBatch spriteBatch, Player player)
        {
            // Full screen world map overlay
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, 0, _screenWidth, _screenHeight),
                Color.Black * 0.9f);
            
            // Draw title bar
            int titleHeight = 50;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, 0, _screenWidth, titleHeight),
                Color.DarkSlateGray * 0.9f);
            
            // Calculate map area (centered)
            int mapSize = Math.Min(_screenWidth - 100, _screenHeight - 150);
            int mapX = (_screenWidth - mapSize) / 2;
            int mapY = (_screenHeight - mapSize) / 2 + 25;
            
            // Draw map background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(mapX, mapY, mapSize, mapSize),
                Color.Gray * 0.3f);
            
            // Draw map border
            int borderWidth = 3;
            spriteBatch.Draw(_pixelTexture, new Rectangle(mapX, mapY, mapSize, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(mapX, mapY + mapSize - borderWidth, mapSize, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(mapX, mapY, borderWidth, mapSize), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(mapX + mapSize - borderWidth, mapY, borderWidth, mapSize), Color.White);
            
            // Draw terrain on world map if world manager is available
            if (_worldManager != null)
            {
                int mapRange = 200; // Show larger area on world map
                if (mapRange <= 0) mapRange = 1; // Prevent division by zero
                
                int pixelsPerBlock = mapSize / (mapRange * 2);
                
                Vector3 playerPos = player.Position;
                int playerX = (int)Math.Floor(playerPos.X);
                int playerZ = (int)Math.Floor(playerPos.Z);
                
                // Draw a simplified top-down view with optimized sampling
                // Step size adjusts based on map range for consistent performance
                int step = Math.Max(2, mapRange / 50); // Sample approximately 100x100 grid
                for (int dx = -mapRange; dx < mapRange; dx += step)
                {
                    for (int dz = -mapRange; dz < mapRange; dz += step)
                    {
                        int worldX = playerX + dx;
                        int worldZ = playerZ + dz;
                        
                        // Find highest solid block
                        int highestY = -1;
                        for (int y = 100; y >= 0; y--)
                        {
                            if (_worldManager.IsBlockSolid(worldX, y, worldZ))
                            {
                                highestY = y;
                                break;
                            }
                        }
                        
                        if (highestY >= 0)
                        {
                            // Color based on height
                            float heightFactor = MathHelper.Clamp(highestY / 100f, 0f, 1f);
                            Color terrainColor = Color.Lerp(new Color(0, 100, 0), new Color(200, 200, 200), heightFactor);
                            
                            int pixelX = mapX + ((dx + mapRange) * pixelsPerBlock) / step;
                            int pixelZ = mapY + ((dz + mapRange) * pixelsPerBlock) / step;
                            
                            if (pixelsPerBlock > 0)
                            {
                                spriteBatch.Draw(_pixelTexture,
                                    new Rectangle(pixelX, pixelZ, Math.Max(2, pixelsPerBlock * step), Math.Max(2, pixelsPerBlock * step)),
                                    terrainColor * 0.7f);
                            }
                        }
                    }
                }
            }
            
            // Draw player position on map (large red marker)
            int playerMarkerSize = 12;
            int playerMarkerX = mapX + mapSize / 2 - playerMarkerSize / 2;
            int playerMarkerY = mapY + mapSize / 2 - playerMarkerSize / 2;
            
            // Draw red circle (approximated with a filled square)
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(playerMarkerX, playerMarkerY, playerMarkerSize, playerMarkerSize),
                Color.Red);
            
            // Draw outer ring
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(playerMarkerX - 2, playerMarkerY - 2, playerMarkerSize + 4, 2),
                Color.White);
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(playerMarkerX - 2, playerMarkerY + playerMarkerSize, playerMarkerSize + 4, 2),
                Color.White);
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(playerMarkerX - 2, playerMarkerY, 2, playerMarkerSize),
                Color.White);
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(playerMarkerX + playerMarkerSize, playerMarkerY, 2, playerMarkerSize),
                Color.White);
            
            // Draw "Press M to close" indicator at bottom
            int messageY = _screenHeight - 40;
            int messageWidth = 200;
            int messageX = (_screenWidth - messageWidth) / 2;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(messageX, messageY, messageWidth, 30),
                Color.DarkGray * 0.8f);
        }
    }
}
