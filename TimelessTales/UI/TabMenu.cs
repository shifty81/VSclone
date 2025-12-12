using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TimelessTales.Entities;
using TimelessTales.Core;
using TimelessTales.Blocks;
using System.Collections.Generic;

namespace TimelessTales.UI
{
    /// <summary>
    /// Tab types for the tabbed menu system
    /// </summary>
    public enum TabType
    {
        Character,
        Inventory,
        Crafting,
        Map
    }

    /// <summary>
    /// Tabbed menu system for character, inventory, crafting, etc.
    /// </summary>
    public class TabMenu
    {
        private readonly Texture2D _pixelTexture;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        
        private TabType _currentTab = TabType.Character;
        
        // Layout constants
        private const int TAB_HEIGHT = 40;
        private const int TAB_WIDTH = 120;
        private const int TAB_SPACING = 5;
        private const int MENU_MARGIN = 50;
        private const int MENU_PADDING = 20;
        
        // Equipment slot positions (for Character tab)
        private const int EQUIPMENT_SLOT_SIZE = 50;
        private const int EQUIPMENT_SPACING = 10;
        
        // Inventory slot positions
        private const int INVENTORY_SLOT_SIZE = 45;
        private const int INVENTORY_SPACING = 8;
        private const int INVENTORY_SLOTS_PER_ROW = 8;
        
        // Text rendering constants
        private const int CHAR_WIDTH = 4;  // 3 pixels + 1 spacing
        private const int TEXT_PADDING = 2;
        
        // Mouse interaction
        private int _hoveredEquipmentSlot = -1;
        private int _hoveredInventorySlot = -1;
        private MouseState _previousMouseState;

        public TabMenu(GraphicsDevice graphicsDevice)
        {
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            _previousMouseState = Mouse.GetState();
        }

        public void Update(InputManager input)
        {
            MouseState mouseState = Mouse.GetState();
            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;
            
            // Check if clicking on tabs
            if (mouseState.LeftButton == ButtonState.Pressed && 
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                int tabY = MENU_MARGIN;
                int tabX = MENU_MARGIN;
                
                // Check each tab
                for (int i = 0; i < 4; i++)
                {
                    Rectangle tabRect = new Rectangle(tabX, tabY, TAB_WIDTH, TAB_HEIGHT);
                    if (tabRect.Contains(mouseX, mouseY))
                    {
                        _currentTab = (TabType)i;
                        break;
                    }
                    tabX += TAB_WIDTH + TAB_SPACING;
                }
            }
            
            _previousMouseState = mouseState;
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            // Draw semi-transparent background overlay
            spriteBatch.Draw(_pixelTexture, 
                new Rectangle(0, 0, _screenWidth, _screenHeight),
                Color.Black * 0.7f);
            
            // Calculate menu area
            int menuWidth = _screenWidth - MENU_MARGIN * 2;
            int menuHeight = _screenHeight - MENU_MARGIN * 2;
            int menuX = MENU_MARGIN;
            int menuY = MENU_MARGIN + TAB_HEIGHT;
            
            // Draw tabs
            DrawTabs(spriteBatch);
            
            // Draw menu background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(menuX, menuY, menuWidth, menuHeight),
                new Color(40, 40, 40) * 0.95f);
            
            // Draw border
            DrawBorder(spriteBatch, menuX, menuY, menuWidth, menuHeight, Color.White, 2);
            
            // Draw content based on selected tab
            switch (_currentTab)
            {
                case TabType.Character:
                    DrawCharacterTab(spriteBatch, player, menuX, menuY, menuWidth, menuHeight);
                    break;
                case TabType.Inventory:
                    DrawInventoryTab(spriteBatch, player, menuX, menuY, menuWidth, menuHeight);
                    break;
                case TabType.Crafting:
                    DrawCraftingTab(spriteBatch, player, menuX, menuY, menuWidth, menuHeight);
                    break;
                case TabType.Map:
                    DrawMapTab(spriteBatch, player, menuX, menuY, menuWidth, menuHeight);
                    break;
            }
        }

        private void DrawTabs(SpriteBatch spriteBatch)
        {
            string[] tabNames = { "CHARACTER", "INVENTORY", "CRAFTING", "MAP" };
            int tabX = MENU_MARGIN;
            int tabY = MENU_MARGIN;
            
            for (int i = 0; i < tabNames.Length; i++)
            {
                bool isSelected = (TabType)i == _currentTab;
                Color tabColor = isSelected ? new Color(60, 60, 60) : new Color(30, 30, 30);
                Color textColor = isSelected ? Color.Yellow : Color.LightGray;
                
                // Draw tab background
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(tabX, tabY, TAB_WIDTH, TAB_HEIGHT),
                    tabColor);
                
                // Draw tab border
                DrawBorder(spriteBatch, tabX, tabY, TAB_WIDTH, TAB_HEIGHT, 
                    isSelected ? Color.Yellow : Color.Gray, 2);
                
                // Draw tab text
                DrawText(spriteBatch, tabNames[i], 
                    tabX + TAB_WIDTH / 2 - tabNames[i].Length * CHAR_WIDTH / 2, 
                    tabY + TAB_HEIGHT / 2 - 5, 
                    textColor);
                
                tabX += TAB_WIDTH + TAB_SPACING;
            }
        }

        private void DrawCharacterTab(SpriteBatch spriteBatch, Player player, int x, int y, int width, int height)
        {
            int contentX = x + MENU_PADDING;
            int contentY = y + MENU_PADDING;
            
            // Title
            DrawText(spriteBatch, "CHARACTER", contentX, contentY, Color.Yellow, 2);
            contentY += 30;
            
            // Character name
            DrawText(spriteBatch, $"NAME: {player.CharacterName}", contentX, contentY, Color.White);
            contentY += 20;
            
            // Character stats
            DrawText(spriteBatch, $"HEALTH: {(int)player.Health}/{(int)player.MaxHealth}", contentX, contentY, Color.Red);
            contentY += 15;
            DrawText(spriteBatch, $"HUNGER: {(int)player.Hunger}/{(int)player.MaxHunger}", contentX, contentY, new Color(200, 120, 40));
            contentY += 15;
            DrawText(spriteBatch, $"THIRST: {(int)player.Thirst}/{(int)player.MaxThirst}", contentX, contentY, new Color(60, 140, 220));
            contentY += 30;
            
            // Character appearance section
            DrawText(spriteBatch, "APPEARANCE", contentX, contentY, Color.Yellow);
            contentY += 20;
            DrawText(spriteBatch, $"SKIN TONE: ", contentX, contentY, Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(contentX + 100, contentY, 40, 10), player.Appearance.SkinTone);
            contentY += 15;
            DrawText(spriteBatch, $"HAIR: ", contentX, contentY, Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(contentX + 100, contentY, 40, 10), player.Appearance.HairColor);
            DrawText(spriteBatch, player.Appearance.HairStyle.ToString(), contentX + 150, contentY, Color.LightGray);
            contentY += 15;
            DrawText(spriteBatch, $"EYES: ", contentX, contentY, Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(contentX + 100, contentY, 40, 10), player.Appearance.EyeColor);
            contentY += 30;
            
            // Equipment section
            DrawText(spriteBatch, "EQUIPMENT", contentX, contentY, Color.Yellow);
            contentY += 25;
            
            DrawEquipmentSlots(spriteBatch, player, contentX, contentY);
            
            // Character preview (on the right side)
            int previewX = x + width - 200;
            int previewY = y + MENU_PADDING + 60;
            DrawCharacterPreview(spriteBatch, player, previewX, previewY);
        }

        private void DrawEquipmentSlots(SpriteBatch spriteBatch, Player player, int startX, int startY)
        {
            var equippedItems = player.Equipment.GetAllEquipped();
            string[] slotNames = { "HEAD", "CHEST", "LEGS", "FEET", "HANDS", "BACK", "MAIN HAND", "OFF HAND" };
            
            int slotY = startY;
            int slotIndex = 0;
            
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                // Draw slot background
                Color slotColor = new Color(20, 20, 20);
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(startX, slotY, 250, EQUIPMENT_SLOT_SIZE),
                    slotColor);
                
                // Draw slot border
                DrawBorder(spriteBatch, startX, slotY, 250, EQUIPMENT_SLOT_SIZE, Color.Gray, 1);
                
                // Draw slot label
                DrawText(spriteBatch, slotNames[slotIndex], startX + 5, slotY + 5, Color.LightGray);
                
                // Draw equipped item if any
                if (equippedItems.TryGetValue(slot, out var item) && item.HasValue)
                {
                    DrawText(spriteBatch, item.Value.ToString(), 
                        startX + 5, slotY + 25, Color.Yellow);
                }
                else
                {
                    DrawText(spriteBatch, "[EMPTY]", startX + 5, slotY + 25, Color.DarkGray);
                }
                
                slotY += EQUIPMENT_SLOT_SIZE + EQUIPMENT_SPACING;
                slotIndex++;
            }
        }

        private void DrawCharacterPreview(SpriteBatch spriteBatch, Player player, int x, int y)
        {
            // Draw a simple voxel character preview
            DrawText(spriteBatch, "PREVIEW", x + 40, y - 20, Color.Yellow);
            
            // Character preview box
            int previewSize = 150;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x, y, previewSize, previewSize),
                new Color(30, 30, 30));
            DrawBorder(spriteBatch, x, y, previewSize, previewSize, Color.Gray, 1);
            
            // Simple character representation (voxel style)
            // Head
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x + 55, y + 20, 40, 40),
                player.Appearance.SkinTone);
            // Hair
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x + 55, y + 15, 40, 15),
                player.Appearance.HairColor);
            // Body
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x + 50, y + 60, 50, 60),
                player.Appearance.ShirtColor);
            // Legs
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x + 55, y + 120, 20, 25),
                player.Appearance.PantsColor);
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x + 75, y + 120, 20, 25),
                player.Appearance.PantsColor);
        }

        private void DrawInventoryTab(SpriteBatch spriteBatch, Player player, int x, int y, int width, int height)
        {
            int contentX = x + MENU_PADDING;
            int contentY = y + MENU_PADDING;
            
            // Title
            DrawText(spriteBatch, "INVENTORY", contentX, contentY, Color.Yellow, 2);
            contentY += 30;
            
            // Draw inventory grid
            var items = player.Inventory.GetAllItems();
            BlockType selectedBlock = player.SelectedBlock; // Cache for performance
            int slotsDrawn = 0;
            
            foreach (var kvp in items)
            {
                int row = slotsDrawn / INVENTORY_SLOTS_PER_ROW;
                int col = slotsDrawn % INVENTORY_SLOTS_PER_ROW;
                
                int slotX = contentX + col * (INVENTORY_SLOT_SIZE + INVENTORY_SPACING);
                int slotY = contentY + row * (INVENTORY_SLOT_SIZE + INVENTORY_SPACING);
                
                // Don't draw beyond visible area
                if (slotY + INVENTORY_SLOT_SIZE > y + height - MENU_PADDING)
                    break;
                
                // Draw slot background
                bool isSelected = kvp.Key == selectedBlock;
                Color slotColor = isSelected ? new Color(60, 60, 40) : new Color(30, 30, 30);
                
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(slotX, slotY, INVENTORY_SLOT_SIZE, INVENTORY_SLOT_SIZE),
                    slotColor);
                
                // Draw border
                Color borderColor = isSelected ? Color.Yellow : Color.Gray;
                DrawBorder(spriteBatch, slotX, slotY, INVENTORY_SLOT_SIZE, INVENTORY_SLOT_SIZE, borderColor, 2);
                
                // Draw item count
                DrawText(spriteBatch, kvp.Value.ToString(), 
                    slotX + INVENTORY_SLOT_SIZE - kvp.Value.ToString().Length * CHAR_WIDTH - TEXT_PADDING, 
                    slotY + INVENTORY_SLOT_SIZE - 7, 
                    Color.White);
                
                // Draw item name (small)
                string itemName = kvp.Key.ToString();
                if (itemName.Length > 8)
                    itemName = itemName.Substring(0, 8);
                DrawText(spriteBatch, itemName, slotX + TEXT_PADDING, slotY + TEXT_PADDING, Color.LightGray);
                
                slotsDrawn++;
            }
            
            // Show total slots used
            int totalY = y + height - MENU_PADDING - 20;
            DrawText(spriteBatch, $"SLOTS: {items.Count}/40", contentX, totalY, Color.LightGray);
        }

        private void DrawCraftingTab(SpriteBatch spriteBatch, Player player, int x, int y, int width, int height)
        {
            int contentX = x + MENU_PADDING;
            int contentY = y + MENU_PADDING;
            
            // Title
            DrawText(spriteBatch, "CRAFTING", contentX, contentY, Color.Yellow, 2);
            contentY += 30;
            
            // Crafting info
            DrawText(spriteBatch, "BASIC CRAFTING SYSTEM", contentX, contentY, Color.White);
            contentY += 25;
            
            DrawText(spriteBatch, "COMING SOON:", contentX, contentY, Color.Gray);
            contentY += 20;
            DrawText(spriteBatch, "- Knapping (Stone Tools)", contentX + 10, contentY, Color.LightGray);
            contentY += 15;
            DrawText(spriteBatch, "- Pottery (Clay Forming)", contentX + 10, contentY, Color.LightGray);
            contentY += 15;
            DrawText(spriteBatch, "- Metallurgy (Smelting)", contentX + 10, contentY, Color.LightGray);
            contentY += 15;
            DrawText(spriteBatch, "- Carpentry (Wood Working)", contentX + 10, contentY, Color.LightGray);
            contentY += 30;
            
            // Material pouch display
            DrawText(spriteBatch, "MATERIAL POUCH", contentX, contentY, Color.Yellow);
            contentY += 20;
            
            float pouchFill = player.MaterialPouch.GetFillPercentage();
            DrawText(spriteBatch, $"CAPACITY: {pouchFill * 100:F0}%", contentX, contentY, Color.White);
            
            // Draw progress bar for pouch
            int barWidth = 200;
            int barHeight = 20;
            int barY = contentY + 15;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(contentX, barY, barWidth, barHeight),
                new Color(20, 20, 20));
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(contentX, barY, (int)(barWidth * pouchFill), barHeight),
                new Color(200, 120, 40));
            DrawBorder(spriteBatch, contentX, barY, barWidth, barHeight, Color.Gray, 1);
        }

        private void DrawMapTab(SpriteBatch spriteBatch, Player player, int x, int y, int width, int height)
        {
            int contentX = x + MENU_PADDING;
            int contentY = y + MENU_PADDING;
            
            // Title
            DrawText(spriteBatch, "MAP", contentX, contentY, Color.Yellow, 2);
            contentY += 30;
            
            // Map info
            DrawText(spriteBatch, "WORLD MAP VIEW", contentX, contentY, Color.White);
            contentY += 25;
            
            DrawText(spriteBatch, "Press M for full screen map", contentX, contentY, Color.LightGray);
            contentY += 20;
            
            // Player position
            DrawText(spriteBatch, $"POSITION: X={player.Position.X:F1} Y={player.Position.Y:F1} Z={player.Position.Z:F1}", 
                contentX, contentY, Color.Green);
        }

        private void DrawBorder(SpriteBatch spriteBatch, int x, int y, int width, int height, Color color, int thickness)
        {
            // Top
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, width, thickness), color);
            // Bottom
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + height - thickness, width, thickness), color);
            // Left
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, thickness, height), color);
            // Right
            spriteBatch.Draw(_pixelTexture, new Rectangle(x + width - thickness, y, thickness, height), color);
        }

        private void DrawText(SpriteBatch spriteBatch, string text, int x, int y, Color color, int scale = 1)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                int charX = x + i * CHAR_WIDTH * scale;
                
                bool[,] pattern = GetCharPattern(c);
                
                for (int py = 0; py < 5; py++)
                {
                    for (int px = 0; px < 3; px++)
                    {
                        if (pattern[py, px])
                        {
                            spriteBatch.Draw(_pixelTexture,
                                new Rectangle(charX + px * scale, y + py * scale, scale, scale),
                                color);
                        }
                    }
                }
            }
        }

        private bool[,] GetCharPattern(char c)
        {
            bool[,] pattern = new bool[5, 3];
            
            switch (char.ToUpper(c))
            {
                case 'A':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, true },
                        { true, true, true },
                        { true, false, true },
                        { true, false, true }
                    };
                    break;
                case 'B':
                    pattern = new bool[,] {
                        { true, true, false },
                        { true, false, true },
                        { true, true, false },
                        { true, false, true },
                        { true, true, false }
                    };
                    break;
                case 'C':
                    pattern = new bool[,] {
                        { false, true, true },
                        { true, false, false },
                        { true, false, false },
                        { true, false, false },
                        { false, true, true }
                    };
                    break;
                case 'D':
                    pattern = new bool[,] {
                        { true, true, false },
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { true, true, false }
                    };
                    break;
                case 'E':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, false },
                        { true, true, true },
                        { true, false, false },
                        { true, true, true }
                    };
                    break;
                case 'F':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, false },
                        { true, true, true },
                        { true, false, false },
                        { true, false, false }
                    };
                    break;
                case 'G':
                    pattern = new bool[,] {
                        { false, true, true },
                        { true, false, false },
                        { true, false, true },
                        { true, false, true },
                        { false, true, true }
                    };
                    break;
                case 'H':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { true, true, true },
                        { true, false, true },
                        { true, false, true }
                    };
                    break;
                case 'I':
                    pattern = new bool[,] {
                        { true, true, true },
                        { false, true, false },
                        { false, true, false },
                        { false, true, false },
                        { true, true, true }
                    };
                    break;
                case 'K':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { true, true, false },
                        { true, false, true },
                        { true, false, true }
                    };
                    break;
                case 'L':
                    pattern = new bool[,] {
                        { true, false, false },
                        { true, false, false },
                        { true, false, false },
                        { true, false, false },
                        { true, true, true }
                    };
                    break;
                case 'M':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, true, true },
                        { true, true, true },
                        { true, false, true },
                        { true, false, true }
                    };
                    break;
                case 'N':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, true, true },
                        { true, true, true },
                        { true, false, true },
                        { true, false, true }
                    };
                    break;
                case 'O':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { true, true, true }
                    };
                    break;
                case 'P':
                    pattern = new bool[,] {
                        { true, true, true },
                        { true, false, true },
                        { true, true, true },
                        { true, false, false },
                        { true, false, false }
                    };
                    break;
                case 'R':
                    pattern = new bool[,] {
                        { true, true, false },
                        { true, false, true },
                        { true, true, false },
                        { true, false, true },
                        { true, false, true }
                    };
                    break;
                case 'S':
                    pattern = new bool[,] {
                        { false, true, true },
                        { true, false, false },
                        { false, true, false },
                        { false, false, true },
                        { true, true, false }
                    };
                    break;
                case 'T':
                    pattern = new bool[,] {
                        { true, true, true },
                        { false, true, false },
                        { false, true, false },
                        { false, true, false },
                        { false, true, false }
                    };
                    break;
                case 'U':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { false, true, false }
                    };
                    break;
                case 'V':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { false, true, false }
                    };
                    break;
                case 'W':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { true, true, true },
                        { true, true, true },
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
                case '/':
                    pattern = new bool[,] {
                        { false, false, true },
                        { false, false, true },
                        { false, true, false },
                        { true, false, false },
                        { true, false, false }
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
                case '=':
                    pattern = new bool[,] {
                        { false, false, false },
                        { true, true, true },
                        { false, false, false },
                        { true, true, true },
                        { false, false, false }
                    };
                    break;
                case '.':
                    pattern = new bool[,] {
                        { false, false, false },
                        { false, false, false },
                        { false, false, false },
                        { false, false, false },
                        { false, true, false }
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
                case '(':
                    pattern = new bool[,] {
                        { false, true, false },
                        { true, false, false },
                        { true, false, false },
                        { true, false, false },
                        { false, true, false }
                    };
                    break;
                case ')':
                    pattern = new bool[,] {
                        { false, true, false },
                        { false, false, true },
                        { false, false, true },
                        { false, false, true },
                        { false, true, false }
                    };
                    break;
                case '[':
                    pattern = new bool[,] {
                        { true, true, false },
                        { true, false, false },
                        { true, false, false },
                        { true, false, false },
                        { true, true, false }
                    };
                    break;
                case ']':
                    pattern = new bool[,] {
                        { false, true, true },
                        { false, false, true },
                        { false, false, true },
                        { false, false, true },
                        { false, true, true }
                    };
                    break;
                case '%':
                    pattern = new bool[,] {
                        { true, false, true },
                        { false, false, true },
                        { false, true, false },
                        { true, false, false },
                        { true, false, true }
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
