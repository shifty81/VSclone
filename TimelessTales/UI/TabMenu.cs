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
            // Three-panel layout:
            // LEFT: Inventory grid | CENTER: Full-body character portrait | RIGHT: Stats & Equipment
            int panelSpacing = 10;
            int leftPanelWidth = (width - panelSpacing * 2) * 35 / 100;   // 35% for inventory
            int centerPanelWidth = (width - panelSpacing * 2) * 30 / 100; // 30% for portrait
            int rightPanelWidth = width - leftPanelWidth - centerPanelWidth - panelSpacing * 2; // remaining for stats
            
            int leftX = x + MENU_PADDING;
            int centerX = leftX + leftPanelWidth + panelSpacing;
            int rightX = centerX + centerPanelWidth + panelSpacing;
            int contentY = y + MENU_PADDING;
            int panelHeight = height - MENU_PADDING * 2;
            
            // --- LEFT PANEL: Inventory ---
            DrawPanel(spriteBatch, leftX, contentY, leftPanelWidth, panelHeight, "INVENTORY");
            DrawInventoryGrid(spriteBatch, player, leftX + 5, contentY + 25, leftPanelWidth - 10, panelHeight - 30);
            
            // --- CENTER PANEL: Character Portrait ---
            DrawPanel(spriteBatch, centerX, contentY, centerPanelWidth, panelHeight, player.CharacterName.ToUpper());
            DrawFullBodyPortrait(spriteBatch, player, centerX + 5, contentY + 25, centerPanelWidth - 10, panelHeight - 30);
            
            // --- RIGHT PANEL: Stats & Equipment ---
            DrawPanel(spriteBatch, rightX, contentY, rightPanelWidth, panelHeight, "STATUS");
            DrawCharacterStats(spriteBatch, player, rightX + 5, contentY + 25, rightPanelWidth - 10, panelHeight - 30);
        }
        
        /// <summary>
        /// Draws a panel background with title
        /// </summary>
        private void DrawPanel(SpriteBatch spriteBatch, int x, int y, int width, int height, string title)
        {
            // Panel background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x, y, width, height),
                new Color(25, 25, 30) * 0.9f);
            DrawBorder(spriteBatch, x, y, width, height, new Color(80, 80, 90), 1);
            
            // Title bar
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x, y, width, 20),
                new Color(50, 50, 60));
            DrawText(spriteBatch, title, x + 5, y + 5, Color.Yellow);
        }
        
        /// <summary>
        /// Draws the inventory grid in the left panel
        /// </summary>
        private void DrawInventoryGrid(SpriteBatch spriteBatch, Player player, int x, int y, int availableWidth, int availableHeight)
        {
            var items = player.Inventory.GetAllItems();
            BlockType selectedBlock = player.SelectedBlock;
            
            // Calculate slot size to fit in available width
            int slotsPerRow = 4;
            int slotSize = Math.Min(INVENTORY_SLOT_SIZE, (availableWidth - (slotsPerRow - 1) * 4) / slotsPerRow);
            int spacing = 4;
            int slotsDrawn = 0;
            
            foreach (var kvp in items)
            {
                int row = slotsDrawn / slotsPerRow;
                int col = slotsDrawn % slotsPerRow;
                
                int slotX = x + col * (slotSize + spacing);
                int slotY = y + row * (slotSize + spacing);
                
                if (slotY + slotSize > y + availableHeight)
                    break;
                
                bool isSelected = kvp.Key == selectedBlock;
                Color slotColor = isSelected ? new Color(60, 60, 40) : new Color(30, 30, 30);
                
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(slotX, slotY, slotSize, slotSize),
                    slotColor);
                DrawBorder(spriteBatch, slotX, slotY, slotSize, slotSize, 
                    isSelected ? Color.Yellow : Color.Gray, 1);
                
                // Draw block color swatch
                Color blockColor = BlockRegistry.Get(kvp.Key).Color;
                int padding = 4;
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(slotX + padding, slotY + padding, slotSize - padding * 2, slotSize - padding * 2 - 8),
                    blockColor);
                
                // Item count
                DrawText(spriteBatch, kvp.Value.ToString(), 
                    slotX + slotSize - kvp.Value.ToString().Length * CHAR_WIDTH - 2, 
                    slotY + slotSize - 7, 
                    Color.White);
                
                slotsDrawn++;
            }
            
            // Total slots info at bottom
            int totalY = y + availableHeight - 12;
            DrawText(spriteBatch, $"{items.Count}/40 SLOTS", x, totalY, Color.LightGray);
        }
        
        /// <summary>
        /// Draws a full-body character portrait centered in the panel.
        /// Shows head, hair, eyes, torso, arms, legs, and equipment placeholders.
        /// </summary>
        private void DrawFullBodyPortrait(SpriteBatch spriteBatch, Player player, int x, int y, int availableWidth, int availableHeight)
        {
            // Scale character to fill the portrait area nicely
            int charHeight = (int)(availableHeight * 0.85f);
            int charWidth = charHeight / 3; // ~3:1 aspect ratio for human body
            
            // Center horizontally
            int charX = x + (availableWidth - charWidth) / 2;
            int charY = y + (availableHeight - charHeight) / 6; // Slight top offset
            
            // Proportions relative to character height
            int headSize = charHeight * 18 / 100;
            int torsoHeight = charHeight * 35 / 100;
            int armWidth = charWidth * 20 / 100;
            int armHeight = torsoHeight * 90 / 100;
            int legHeight = charHeight * 40 / 100;
            int legWidth = charWidth * 35 / 100;
            int handSize = armWidth + 2;
            int footHeight = headSize / 3;
            
            int currentY = charY;
            
            // --- Hair (on top of head) ---
            int hairHeight = headSize / 3;
            switch (player.Appearance.HairStyle)
            {
                case HairStyle.Long:
                    hairHeight = headSize * 2 / 3;
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(charX + charWidth / 2 - headSize / 2 - 2, currentY - 2, headSize + 4, hairHeight),
                        player.Appearance.HairColor);
                    break;
                case HairStyle.Medium:
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(charX + charWidth / 2 - headSize / 2, currentY, headSize, hairHeight),
                        player.Appearance.HairColor);
                    break;
                case HairStyle.Short:
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(charX + charWidth / 2 - headSize / 2 + 2, currentY + 2, headSize - 4, hairHeight - 2),
                        player.Appearance.HairColor);
                    break;
                case HairStyle.Ponytail:
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(charX + charWidth / 2 - headSize / 2, currentY, headSize, hairHeight),
                        player.Appearance.HairColor);
                    // Ponytail extension
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(charX + charWidth / 2 + headSize / 2 - 4, currentY + hairHeight, 6, headSize / 2),
                        player.Appearance.HairColor);
                    break;
                case HairStyle.Braided:
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(charX + charWidth / 2 - headSize / 2, currentY, headSize, hairHeight),
                        player.Appearance.HairColor);
                    // Braid strands
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(charX + charWidth / 2 - 4, currentY + headSize, 3, headSize / 2),
                        player.Appearance.HairColor);
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(charX + charWidth / 2 + 1, currentY + headSize, 3, headSize / 2),
                        player.Appearance.HairColor);
                    break;
                default: // Bald
                    break;
            }
            
            // --- Head ---
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth / 2 - headSize / 2, currentY, headSize, headSize),
                player.Appearance.SkinTone);
            
            // Eyes
            int eyeY = currentY + headSize * 40 / 100;
            int eyeSize = Math.Max(2, headSize / 8);
            int eyeSpacing = headSize / 4;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth / 2 - eyeSpacing, eyeY, eyeSize, eyeSize),
                player.Appearance.EyeColor);
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth / 2 + eyeSpacing - eyeSize, eyeY, eyeSize, eyeSize),
                player.Appearance.EyeColor);
            
            currentY += headSize + 2;
            
            // --- Neck ---
            int neckWidth = charWidth / 4;
            int neckHeight = charHeight * 3 / 100;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth / 2 - neckWidth / 2, currentY, neckWidth, neckHeight),
                player.Appearance.SkinTone);
            currentY += neckHeight;
            
            // --- Torso ---
            Color torsoColor = player.Appearance.ShirtColor;
            // Check for chest equipment
            var equipped = player.Equipment.GetAllEquipped();
            if (equipped.TryGetValue(EquipmentSlot.Chest, out var chestItem) && chestItem.HasValue)
            {
                torsoColor = Color.Lerp(torsoColor, BlockRegistry.Get(chestItem.Value).Color, 0.5f);
            }
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX, currentY, charWidth, torsoHeight),
                torsoColor);
            
            // --- Arms (on sides of torso) ---
            Color armColor = player.Appearance.SkinTone;
            // Left arm
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX - armWidth - 2, currentY, armWidth, armHeight),
                armColor);
            // Right arm
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth + 2, currentY, armWidth, armHeight),
                armColor);
            
            // Hands
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX - armWidth - 2, currentY + armHeight, handSize, handSize),
                player.Appearance.SkinTone);
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth + 2, currentY + armHeight, handSize, handSize),
                player.Appearance.SkinTone);
            
            // Show held item in right hand
            var mainHandItem = player.Equipment.GetEquipped(EquipmentSlot.MainHand);
            if (mainHandItem.HasValue)
            {
                Color itemColor = BlockRegistry.Get(mainHandItem.Value).Color;
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(charX + charWidth + armWidth + 4, currentY + armHeight - 4, 8, 8),
                    itemColor);
            }
            
            currentY += torsoHeight;
            
            // --- Legs ---
            Color legColor = player.Appearance.PantsColor;
            if (equipped.TryGetValue(EquipmentSlot.Legs, out var legItem) && legItem.HasValue)
            {
                legColor = Color.Lerp(legColor, BlockRegistry.Get(legItem.Value).Color, 0.5f);
            }
            int legGap = charWidth / 8;
            // Left leg
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth / 2 - legWidth - legGap / 2, currentY, legWidth, legHeight),
                legColor);
            // Right leg
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth / 2 + legGap / 2, currentY, legWidth, legHeight),
                legColor);
            
            currentY += legHeight;
            
            // --- Feet ---
            Color footColor = new Color(80, 50, 30); // Default boot color
            if (equipped.TryGetValue(EquipmentSlot.Feet, out var footItem) && footItem.HasValue)
            {
                footColor = BlockRegistry.Get(footItem.Value).Color;
            }
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth / 2 - legWidth - legGap / 2 - 2, currentY, legWidth + 4, footHeight),
                footColor);
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(charX + charWidth / 2 + legGap / 2 - 2, currentY, legWidth + 4, footHeight),
                footColor);
            
            // --- Equipment indicators ---
            // Head slot indicator
            if (equipped.TryGetValue(EquipmentSlot.Head, out var headItem) && headItem.HasValue)
            {
                Color hatColor = BlockRegistry.Get(headItem.Value).Color;
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(charX + charWidth / 2 - headSize / 2 - 2, charY - 6, headSize + 4, 8),
                    hatColor);
            }
        }
        
        /// <summary>
        /// Draws character stats and equipment list in the right panel
        /// </summary>
        private void DrawCharacterStats(SpriteBatch spriteBatch, Player player, int x, int y, int availableWidth, int availableHeight)
        {
            int lineY = y;
            int lineHeight = 14;
            
            // Health bar
            DrawText(spriteBatch, "HEALTH", x, lineY, Color.Red);
            lineY += lineHeight;
            DrawStatBar(spriteBatch, x, lineY, availableWidth - 10, 8, player.Health / player.MaxHealth, Color.Red);
            lineY += lineHeight;
            
            // Hunger bar
            DrawText(spriteBatch, "HUNGER", x, lineY, new Color(200, 120, 40));
            lineY += lineHeight;
            DrawStatBar(spriteBatch, x, lineY, availableWidth - 10, 8, player.Hunger / player.MaxHunger, new Color(200, 120, 40));
            lineY += lineHeight;
            
            // Thirst bar
            DrawText(spriteBatch, "THIRST", x, lineY, new Color(60, 140, 220));
            lineY += lineHeight;
            DrawStatBar(spriteBatch, x, lineY, availableWidth - 10, 8, player.Thirst / player.MaxThirst, new Color(60, 140, 220));
            lineY += lineHeight + 8;
            
            // Equipment list
            DrawText(spriteBatch, "EQUIPMENT", x, lineY, Color.Yellow);
            lineY += lineHeight + 4;
            
            var equippedItems = player.Equipment.GetAllEquipped();
            string[] slotNames = { "HEAD", "CHEST", "LEGS", "FEET", "HANDS", "BACK", "MAIN", "OFF" };
            int slotIndex = 0;
            
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (lineY + 20 > y + availableHeight)
                    break;
                
                string label = slotNames[slotIndex];
                bool hasItem = equippedItems.TryGetValue(slot, out var item) && item.HasValue;
                
                // Compact slot display
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x, lineY, availableWidth - 10, 16),
                    new Color(20, 20, 20));
                DrawBorder(spriteBatch, x, lineY, availableWidth - 10, 16, Color.DarkGray, 1);
                
                DrawText(spriteBatch, label, x + 2, lineY + 3, Color.LightGray);
                if (hasItem)
                {
                    string itemName = item!.Value.ToString();
                    if (itemName.Length > 8) itemName = itemName[..8];
                    DrawText(spriteBatch, itemName, x + 40, lineY + 3, Color.Yellow);
                }
                else
                {
                    DrawText(spriteBatch, "-", x + 40, lineY + 3, Color.DarkGray);
                }
                
                lineY += 18;
                slotIndex++;
            }
        }
        
        /// <summary>
        /// Draws a horizontal stat bar
        /// </summary>
        private void DrawStatBar(SpriteBatch spriteBatch, int x, int y, int width, int height, float fillPercent, Color fillColor)
        {
            fillPercent = MathHelper.Clamp(fillPercent, 0f, 1f);
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, width, height), new Color(20, 20, 20));
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, (int)(width * fillPercent), height), fillColor);
            DrawBorder(spriteBatch, x, y, width, height, Color.DarkGray, 1);
        }

        private void DrawInventoryTab(SpriteBatch spriteBatch, Player player, int x, int y, int width, int height)
        {
            // Same three-panel layout as character tab: Inventory left, Portrait centered
            int panelSpacing = 10;
            int leftPanelWidth = (width - panelSpacing * 2) * 40 / 100;   // Wider inventory panel
            int centerPanelWidth = (width - panelSpacing * 2) * 30 / 100; // Character portrait
            int rightPanelWidth = width - leftPanelWidth - centerPanelWidth - panelSpacing * 2;
            
            int leftX = x + MENU_PADDING;
            int centerX = leftX + leftPanelWidth + panelSpacing;
            int rightX = centerX + centerPanelWidth + panelSpacing;
            int contentY = y + MENU_PADDING;
            int panelHeight = height - MENU_PADDING * 2;
            
            // --- LEFT PANEL: Inventory Grid ---
            DrawPanel(spriteBatch, leftX, contentY, leftPanelWidth, panelHeight, "INVENTORY");
            DrawInventoryGrid(spriteBatch, player, leftX + 5, contentY + 25, leftPanelWidth - 10, panelHeight - 30);
            
            // --- CENTER PANEL: Character Portrait ---
            DrawPanel(spriteBatch, centerX, contentY, centerPanelWidth, panelHeight, player.CharacterName.ToUpper());
            DrawFullBodyPortrait(spriteBatch, player, centerX + 5, contentY + 25, centerPanelWidth - 10, panelHeight - 30);
            
            // --- RIGHT PANEL: Equipment ---
            DrawPanel(spriteBatch, rightX, contentY, rightPanelWidth, panelHeight, "EQUIPMENT");
            DrawCharacterStats(spriteBatch, player, rightX + 5, contentY + 25, rightPanelWidth - 10, panelHeight - 30);
        }

        private void DrawCraftingTab(SpriteBatch spriteBatch, Player player, int x, int y, int width, int height)
        {
            // Three-panel layout for crafting:
            // LEFT: Inventory (materials) | CENTER: Crafting grid | RIGHT: Output & recipes
            int panelSpacing = 10;
            int leftPanelWidth = (width - panelSpacing * 2) * 30 / 100;
            int centerPanelWidth = (width - panelSpacing * 2) * 40 / 100;
            int rightPanelWidth = width - leftPanelWidth - centerPanelWidth - panelSpacing * 2;
            
            int leftX = x + MENU_PADDING;
            int centerX = leftX + leftPanelWidth + panelSpacing;
            int rightX = centerX + centerPanelWidth + panelSpacing;
            int contentY = y + MENU_PADDING;
            int panelHeight = height - MENU_PADDING * 2;
            
            // --- LEFT PANEL: Materials ---
            DrawPanel(spriteBatch, leftX, contentY, leftPanelWidth, panelHeight, "MATERIALS");
            DrawInventoryGrid(spriteBatch, player, leftX + 5, contentY + 25, leftPanelWidth - 10, panelHeight - 30);
            
            // --- CENTER PANEL: Hand Crafting Grid ---
            DrawPanel(spriteBatch, centerX, contentY, centerPanelWidth, panelHeight, "HAND CRAFTING");
            DrawHandCraftingGrid(spriteBatch, player, centerX + 5, contentY + 25, centerPanelWidth - 10, panelHeight - 30);
            
            // --- RIGHT PANEL: Material Pouch & Recipes ---
            DrawPanel(spriteBatch, rightX, contentY, rightPanelWidth, panelHeight, "RECIPES");
            DrawCraftingRecipes(spriteBatch, player, rightX + 5, contentY + 25, rightPanelWidth - 10, panelHeight - 30);
        }
        
        /// <summary>
        /// Draws a 3x3 hand crafting grid for basic recipes
        /// </summary>
        private void DrawHandCraftingGrid(SpriteBatch spriteBatch, Player player, int x, int y, int availableWidth, int availableHeight)
        {
            int lineY = y;
            
            DrawText(spriteBatch, "2X2 CRAFTING", x, lineY, Color.White);
            lineY += 18;
            
            // Draw 2x2 crafting grid (hand crafting)
            int gridSize = 2;
            int slotSize = Math.Min(50, (availableWidth - 80) / gridSize);
            int spacing = 4;
            int gridWidth = gridSize * (slotSize + spacing);
            int gridStartX = x + (availableWidth - gridWidth - 60) / 2;
            
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    int slotX = gridStartX + col * (slotSize + spacing);
                    int slotY = lineY + row * (slotSize + spacing);
                    
                    spriteBatch.Draw(_pixelTexture,
                        new Rectangle(slotX, slotY, slotSize, slotSize),
                        new Color(30, 30, 30));
                    DrawBorder(spriteBatch, slotX, slotY, slotSize, slotSize, Color.Gray, 1);
                }
            }
            
            // Arrow pointing to output slot
            int arrowX = gridStartX + gridWidth + 8;
            int arrowY = lineY + (gridSize * (slotSize + spacing)) / 2 - 3;
            DrawText(spriteBatch, "=>", arrowX, arrowY, Color.White);
            
            // Output slot
            int outputX = arrowX + 25;
            int outputY = lineY + (gridSize * (slotSize + spacing)) / 2 - slotSize / 2;
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(outputX, outputY, slotSize, slotSize),
                new Color(40, 40, 20));
            DrawBorder(spriteBatch, outputX, outputY, slotSize, slotSize, Color.Yellow, 2);
            
            lineY += gridSize * (slotSize + spacing) + 20;
            
            // Material pouch display
            DrawText(spriteBatch, "MATERIAL POUCH", x, lineY, Color.Yellow);
            lineY += 16;
            
            float pouchFill = player.MaterialPouch.GetFillPercentage();
            DrawText(spriteBatch, $"{pouchFill * 100:F0}% FULL", x, lineY, Color.White);
            lineY += 14;
            
            // Progress bar for pouch
            int barWidth = Math.Min(200, availableWidth - 10);
            DrawStatBar(spriteBatch, x, lineY, barWidth, 12, pouchFill, new Color(200, 120, 40));
            lineY += 24;
            
            // Crafting categories
            DrawText(spriteBatch, "CRAFTING TYPES:", x, lineY, Color.Yellow);
            lineY += 16;
            
            string[] categories = { "KNAPPING", "POTTERY", "CARPENTRY", "SMELTING" };
            Color[] catColors = { Color.Gray, new Color(180, 120, 80), new Color(140, 100, 60), new Color(200, 80, 40) };
            
            for (int i = 0; i < categories.Length; i++)
            {
                if (lineY + 14 > y + availableHeight) break;
                
                // Category icon
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x, lineY, 8, 8),
                    catColors[i]);
                DrawText(spriteBatch, categories[i], x + 12, lineY, Color.LightGray);
                lineY += 14;
            }
        }
        
        /// <summary>
        /// Draws available crafting recipes in the right panel
        /// </summary>
        private void DrawCraftingRecipes(SpriteBatch spriteBatch, Player player, int x, int y, int availableWidth, int availableHeight)
        {
            int lineY = y;
            
            DrawText(spriteBatch, "HAND RECIPES", x, lineY, Color.White);
            lineY += 18;
            
            // Show basic hand-craftable recipes based on available materials
            var items = player.Inventory.GetAllItems();
            
            // Recipe: 1 Wood -> 4 Planks
            if (items.ContainsKey(BlockType.Wood))
            {
                DrawRecipeEntry(spriteBatch, x, lineY, availableWidth, "WOOD > PLANK", "1 WOOD = 4 PLANKS", Color.Green);
                lineY += 30;
            }
            
            // Recipe: 2 Planks -> 4 Sticks (if sticks exist)
            if (items.ContainsKey(BlockType.Planks) && items[BlockType.Planks] >= 2)
            {
                DrawRecipeEntry(spriteBatch, x, lineY, availableWidth, "PLANK > STICK", "2 PLANKS = 4 STICKS", Color.Green);
                lineY += 30;
            }
            
            // Recipe: 4 Clay -> Clay Block (always show if clay available)
            if (items.ContainsKey(BlockType.Clay) && items[BlockType.Clay] >= 4)
            {
                DrawRecipeEntry(spriteBatch, x, lineY, availableWidth, "CLAY BLOCK", "4 CLAY = 1 BLOCK", Color.Green);
                lineY += 30;
            }
            
            // Show unavailable recipes dimmed
            if (!items.ContainsKey(BlockType.Wood))
            {
                DrawRecipeEntry(spriteBatch, x, lineY, availableWidth, "WOOD > PLANK", "NEED: WOOD", Color.DarkGray);
                lineY += 30;
            }
            
            if (lineY < y + availableHeight - 20)
            {
                lineY = y + availableHeight - 16;
                DrawText(spriteBatch, "MORE COMING SOON", x, lineY, Color.DarkGray);
            }
        }
        
        /// <summary>
        /// Draws a single recipe entry with title and description
        /// </summary>
        private void DrawRecipeEntry(SpriteBatch spriteBatch, int x, int y, int width, string title, string description, Color color)
        {
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x, y, width - 4, 26),
                new Color(25, 25, 25));
            DrawBorder(spriteBatch, x, y, width - 4, 26, color * 0.5f, 1);
            DrawText(spriteBatch, title, x + 4, y + 3, color);
            DrawText(spriteBatch, description, x + 4, y + 14, Color.LightGray);
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
