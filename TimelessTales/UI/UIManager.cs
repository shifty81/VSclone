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
        private Texture2D _pixelTexture;
        
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private Player? _player;
        private TimeManager? _timeManager;
        private WorldManager? _worldManager;
        private bool _inventoryOpen;
        private bool _worldMapOpen;
        
        // Simple text rendering constants
        private const int CHAR_WIDTH = 4;  // 3 pixels + 1 spacing
        private const int CHAR_HEIGHT = 5;
        private const int PIXEL_SIZE = 1;

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
            // For now, just draw basic shapes
            // Font would be loaded from Content in a real implementation
            
            // Draw controls info in the future
            // string controls = "WASD: Move | Space: Jump | Shift: Sprint | LMB: Break | RMB: Place | 1-5: Select Block | P: Pause";
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
            
            // Draw cardinal direction markers on minimap edges
            int markerSize = 8;
            int markerThickness = 3;
            int compassCenterX = minimapX + minimapSize / 2;
            int compassCenterY = minimapY + minimapSize / 2;
            
            // North (top) - White marker
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(compassCenterX - markerThickness / 2, minimapY + 2, markerThickness, markerSize),
                Color.White);
            
            // South (bottom) - Gray marker
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(compassCenterX - markerThickness / 2, minimapY + minimapSize - markerSize - 2, markerThickness, markerSize),
                Color.Gray);
            
            // East (right) - White marker
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(minimapX + minimapSize - markerSize - 2, compassCenterY - markerThickness / 2, markerSize, markerThickness),
                Color.White);
            
            // West (left) - Gray marker
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(minimapX + 2, compassCenterY - markerThickness / 2, markerSize, markerThickness),
                Color.Gray);
            
            // Draw player position indicator (red dot at center)
            int playerDotSize = 6;
            int centerX = minimapX + minimapSize / 2 - playerDotSize / 2;
            int centerY = minimapY + minimapSize / 2 - playerDotSize / 2;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(centerX, centerY, playerDotSize, playerDotSize),
                Color.Red);
            
            // Draw player facing direction indicator
            float yaw = player.Rotation.Y;
            int directionLineLength = 20;
            // Calculate endpoint of direction line (yaw: 0 = forward/north, increases clockwise)
            // In screen coordinates: +X is right, +Y is down
            // We want: yaw 0 = point up (north), yaw PI/2 = point right (east)
            float dirEndX = centerX + playerDotSize / 2 + MathF.Sin(yaw) * directionLineLength;
            float dirEndY = centerY + playerDotSize / 2 - MathF.Cos(yaw) * directionLineLength;
            
            // Draw direction line from player dot to direction point
            DrawLine(spriteBatch, 
                centerX + playerDotSize / 2, centerY + playerDotSize / 2,
                (int)dirEndX, (int)dirEndY, 
                Color.Yellow, 2);
            
            // Draw simple terrain representation if world manager is available
            if (_worldManager != null)
            {
                int mapRange = 50; // Blocks to show on each side
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
                        
                        // Get top surface block (ignores caves and underground)
                        var (surfaceY, blockType) = _worldManager.GetTopSurfaceBlock(worldX, worldZ);
                        
                        if (surfaceY >= 0)
                        {
                            // Color based on block type and height for better visual distinction
                            Color terrainColor = GetTerrainColorForMap(blockType, surfaceY);
                            
                            int pixelX = minimapX + (dx + mapRange) * pixelsPerBlock;
                            int pixelZ = minimapY + (dz + mapRange) * pixelsPerBlock;
                            int pixelSize = Math.Max(1, pixelsPerBlock * step);
                            
                            if (pixelsPerBlock > 0)
                            {
                                spriteBatch.Draw(_pixelTexture,
                                    new Rectangle(pixelX, pixelZ, pixelSize, pixelSize),
                                    terrainColor);
                            }
                        }
                    }
                }
            }
            
            // Draw coordinates below minimap with numeric display
            int coordY = minimapY + minimapSize + 5;
            int coordBoxHeight = 45; // Increased height for 3 rows of coordinates
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(minimapX, coordY, minimapSize, coordBoxHeight),
                Color.Black * 0.7f);
            
            // Display X, Y, Z coordinates as numbers using simple digit rendering
            Vector3 pos = player.Position;
            int textStartX = minimapX + 5;
            int lineHeight = 13;
            
            // X coordinate (Red)
            string xText = $"X: {(int)pos.X}";
            DrawSimpleText(spriteBatch, xText, textStartX, coordY + 3, Color.Red);
            
            // Y coordinate (Green) 
            string yText = $"Y: {(int)pos.Y}";
            DrawSimpleText(spriteBatch, yText, textStartX, coordY + 3 + lineHeight, Color.Green);
            
            // Z coordinate (Blue)
            string zText = $"Z: {(int)pos.Z}";
            DrawSimpleText(spriteBatch, zText, textStartX, coordY + 3 + lineHeight * 2, Color.Blue);
            
            // Draw compass direction below coordinates
            int compassY = coordY + coordBoxHeight + 3;
            int compassBoxHeight = 18;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(minimapX, compassY, minimapSize, compassBoxHeight),
                Color.Black * 0.7f);
            
            // Determine cardinal direction based on player yaw
            // Yaw: 0 = North, PI/2 = East, PI = South, 3PI/2 = West
            float playerYaw = player.Rotation.Y;
            // Normalize yaw to 0-2PI range
            while (playerYaw < 0) playerYaw += MathHelper.TwoPi;
            while (playerYaw >= MathHelper.TwoPi) playerYaw -= MathHelper.TwoPi;
            
            // Determine which cardinal direction (with 45 degree segments)
            string direction;
            if (playerYaw < MathHelper.PiOver4 || playerYaw >= 7 * MathHelper.PiOver4)
                direction = "N";  // North
            else if (playerYaw < 3 * MathHelper.PiOver4)
                direction = "E";  // East
            else if (playerYaw < 5 * MathHelper.PiOver4)
                direction = "S";  // South
            else
                direction = "W";  // West
            
            // Draw direction indicator bars
            // We'll create a simple visual pattern for each direction
            int dirBarStartX = minimapX + minimapSize / 2 - 20;
            int dirBarY = compassY + 3;
            int dirBarHeight = 12;
            
            // Visual pattern based on direction (since no font available)
            // N = 2 tall bars, E = 3 medium bars, S = 1 wide bar, W = 2 wide bars
            switch (direction)
            {
                case "N": // North - two tall bars (like "II")
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(dirBarStartX + 10, dirBarY, 4, dirBarHeight),
                        Color.Cyan);
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(dirBarStartX + 26, dirBarY, 4, dirBarHeight),
                        Color.Cyan);
                    break;
                    
                case "E": // East - three medium bars (like "III")
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(dirBarStartX + 5, dirBarY, 3, dirBarHeight),
                        Color.Yellow);
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(dirBarStartX + 17, dirBarY, 3, dirBarHeight),
                        Color.Yellow);
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(dirBarStartX + 29, dirBarY, 3, dirBarHeight),
                        Color.Yellow);
                    break;
                    
                case "S": // South - one wide horizontal bar
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(dirBarStartX + 5, dirBarY + 4, 30, 4),
                        Color.Orange);
                    break;
                    
                case "W": // West - two wide bars stacked
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(dirBarStartX + 5, dirBarY + 1, 30, 4),
                        Color.Magenta);
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(dirBarStartX + 5, dirBarY + 7, 30, 4),
                        Color.Magenta);
                    break;
            }
        }
        
        private void DrawClock(SpriteBatch spriteBatch)
        {
            if (_timeManager == null) return;
            
            // Clock below minimap and compass
            int minimapSize = 150;
            int padding = 10;
            int clockX = _screenWidth - minimapSize - padding;
            // Calculate position: minimap (150) + spacing (5) + coords (20) + spacing (3) + compass (18) + spacing (3)
            int clockY = padding + minimapSize + 5 + 20 + 3 + 18 + 3;
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
                        
                        // Get top surface block (ignores caves and underground)
                        var (surfaceY, blockType) = _worldManager.GetTopSurfaceBlock(worldX, worldZ);
                        
                        if (surfaceY >= 0)
                        {
                            // Color based on block type and height for better visual distinction
                            Color terrainColor = GetTerrainColorForMap(blockType, surfaceY);
                            
                            int pixelX = mapX + (dx + mapRange) * pixelsPerBlock;
                            int pixelZ = mapY + (dz + mapRange) * pixelsPerBlock;
                            int pixelSize = Math.Max(2, pixelsPerBlock * step);
                            
                            if (pixelsPerBlock > 0)
                            {
                                spriteBatch.Draw(_pixelTexture,
                                    new Rectangle(pixelX, pixelZ, pixelSize, pixelSize),
                                    terrainColor);
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
        
        /// <summary>
        /// Helper method to draw a line using the pixel texture
        /// </summary>
        private void DrawLine(SpriteBatch spriteBatch, int x1, int y1, int x2, int y2, Color color, int thickness)
        {
            // Calculate line parameters
            int dx = x2 - x1;
            int dy = y2 - y1;
            float length = MathF.Sqrt(dx * dx + dy * dy);
            
            if (length < 1) return; // Line too short to draw
            
            // For simplicity, we'll approximate with multiple small rectangles for angled lines
            // Clamp steps to prevent excessive draw calls for long lines
            int steps = Math.Min((int)length, 50);
            for (int i = 0; i <= steps; i++)
            {
                float t = steps > 0 ? i / (float)steps : 0;
                int x = (int)(x1 + dx * t);
                int y = (int)(y1 + dy * t);
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x - thickness / 2, y - thickness / 2, thickness, thickness),
                    color);
            }
        }
        
        /// <summary>
        /// Helper method to draw simple text using pixel patterns (no font required)
        /// Uses a simple 3x5 pixel pattern for each character
        /// </summary>
        private void DrawSimpleText(SpriteBatch spriteBatch, string text, int x, int y, Color color)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                int charX = x + i * CHAR_WIDTH;
                
                // Define simple 3x5 pixel patterns for each character
                bool[,] pattern = GetCharPattern(c);
                
                // Draw the character pattern
                for (int py = 0; py < CHAR_HEIGHT; py++)
                {
                    for (int px = 0; px < 3; px++)
                    {
                        if (pattern[py, px])
                        {
                            spriteBatch.Draw(_pixelTexture,
                                new Rectangle(charX + px * PIXEL_SIZE, y + py * PIXEL_SIZE, PIXEL_SIZE, PIXEL_SIZE),
                                color);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Get terrain color for map display based on block type and height
        /// Provides better visual distinction than simple height-based coloring
        /// </summary>
        private Color GetTerrainColorForMap(BlockType blockType, int surfaceY)
        {
            // Base colors for different terrain types
            Color baseColor;
            
            switch (blockType)
            {
                // Water blocks - distinct blue colors
                case BlockType.Water:
                    baseColor = new Color(60, 140, 220); // Light blue for fresh water
                    break;
                case BlockType.Saltwater:
                    baseColor = new Color(30, 80, 180); // Deep blue for ocean
                    break;
                
                // Vegetation - green tones
                case BlockType.Grass:
                    baseColor = new Color(80, 180, 60); // Vibrant green
                    break;
                case BlockType.OakLeaves:
                case BlockType.PineLeaves:
                case BlockType.BirchLeaves:
                case BlockType.Leaves:
                    baseColor = new Color(40, 140, 40); // Dark green for forests
                    break;
                
                // Desert/beach - sandy colors
                case BlockType.Sand:
                    baseColor = new Color(230, 200, 140); // Sandy yellow
                    break;
                
                // Rocky terrain
                case BlockType.Gravel:
                    baseColor = new Color(140, 140, 140); // Gray for tundra/rocky areas
                    break;
                case BlockType.Stone:
                case BlockType.Cobblestone:
                    baseColor = new Color(120, 120, 120); // Stone gray
                    break;
                
                // Clay/wetlands
                case BlockType.Clay:
                    baseColor = new Color(160, 130, 100); // Brown-gray for clay
                    break;
                
                // Dirt - brown
                case BlockType.Dirt:
                    baseColor = new Color(120, 80, 50); // Brown
                    break;
                
                // Wood/trees
                case BlockType.OakLog:
                case BlockType.PineLog:
                case BlockType.BirchLog:
                case BlockType.Wood:
                    baseColor = new Color(100, 70, 40); // Brown for tree trunks
                    break;
                
                // Default for other blocks
                default:
                    baseColor = new Color(100, 100, 100); // Neutral gray
                    break;
            }
            
            // Apply subtle height-based shading for depth perception
            // Higher elevations get slightly brighter, lower get slightly darker
            const int SEA_LEVEL = 64;
            float heightOffset = (surfaceY - SEA_LEVEL) / 40f; // -1.6 to +4.8 range approx
            heightOffset = MathHelper.Clamp(heightOffset, -0.3f, 0.3f);
            
            // Brighten high areas, darken low areas (except water which stays consistent)
            if (blockType != BlockType.Water && blockType != BlockType.Saltwater)
            {
                float brightness = 1.0f + heightOffset;
                baseColor = new Color(
                    (int)(baseColor.R * brightness),
                    (int)(baseColor.G * brightness),
                    (int)(baseColor.B * brightness)
                );
            }
            
            return baseColor;
        }

        /// <summary>
        /// Get 3x5 pixel pattern for a character
        /// </summary>
        private bool[,] GetCharPattern(char c)
        {
            // Default pattern (space)
            bool[,] pattern = new bool[5, 3];
            
            switch (char.ToUpper(c))
            {
                case '0':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { true, true, true }
                    };
                    break;
                case '1':
                    pattern = new bool[,] {
                        { false, true, false },
                        { true, true, false },
                        { false, true, false },
                        { false, true, false },
                        { true, true, true }
                    };
                    break;
                case '2':
                    pattern = new bool[,] {
                        { true, true, true },
                        { false, false, true },
                        { true, true, true },
                        { true, false, false },
                        { true, true, true }
                    };
                    break;
                case '3':
                    pattern = new bool[,] {
                        { true, true, true },
                        { false, false, true },
                        { true, true, true },
                        { false, false, true },
                        { true, true, true }
                    };
                    break;
                case '4':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { true, true, true },
                        { false, false, true },
                        { false, false, true }
                    };
                    break;
                case '5':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, false },
                        { true, true, true },
                        { false, false, true },
                        { true, true, true }
                    };
                    break;
                case '6':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, false },
                        { true, true, true },
                        { true, false, true },
                        { true, true, true }
                    };
                    break;
                case '7':
                    pattern = new bool[,] {
                        { true, true, true },
                        { false, false, true },
                        { false, false, true },
                        { false, false, true },
                        { false, false, true }
                    };
                    break;
                case '8':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, true },
                        { true, true, true },
                        { true, false, true },
                        { true, true, true }
                    };
                    break;
                case '9':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, true },
                        { true, true, true },
                        { false, false, true },
                        { true, true, true }
                    };
                    break;
                case 'X':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { false, true, false },
                        { true, false, true },
                        { true, false, true }
                    };
                    break;
                case 'Y':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { false, true, false },
                        { false, true, false },
                        { false, true, false }
                    };
                    break;
                case 'Z':
                    pattern = new bool[,] {
                        { true, true, true },
                        { false, false, true },
                        { false, true, false },
                        { true, false, false },
                        { true, true, true }
                    };
                    break;
                case ':':
                    pattern = new bool[,] {
                        { false, false, false },
                        { false, true, false },
                        { false, false, false },
                        { false, true, false },
                        { false, false, false }
                    };
                    break;
                case '-':
                    pattern = new bool[,] {
                        { false, false, false },
                        { false, false, false },
                        { true, true, true },
                        { false, false, false },
                        { false, false, false }
                    };
                    break;
                case ' ':
                    // Already initialized as all false
                    break;
            }
            
            return pattern;
        }
    }
}
