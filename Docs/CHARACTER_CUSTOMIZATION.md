# Character Customization & Tabbed Menu System

This document describes the character customization system and the new tabbed menu interface implemented in Timeless Tales.

## Overview

The character customization system makes the player character more human-like with realistic appearance options, while the tabbed menu provides an organized interface for managing character stats, inventory, equipment, and crafting.

## Character Customization

### Appearance System

The `CharacterAppearance` class provides the following customization options:

#### Skin Tone
- **Type**: Color (RGB)
- **Default**: Light tan (210, 180, 140)
- **Purpose**: Realistic human skin color representation
- **Usage**: Automatically applied to arms and exposed skin in first-person view

#### Hair
- **Color**: Customizable RGB color
- **Default**: Brown (101, 67, 33)
- **Style Options**:
  - Bald
  - Short
  - Medium
  - Long
  - Ponytail
  - Braided
- **Purpose**: Adds variety and personality to characters

#### Eyes
- **Color**: Customizable RGB color
- **Default**: Blue (70, 130, 180)
- **Purpose**: Character detail (visible in character preview)

#### Body Type
- **Body Scale**: Float value (0.8 - 1.2)
- **Default**: 1.0 (average)
- **Purpose**: Adjust character height and width for variety

#### Gender Presentation
- **Options**: Masculine, Feminine, Neutral
- **Default**: Neutral
- **Purpose**: Affects body proportions slightly (future implementation)

#### Clothing
- **Shirt Color**: Customizable RGB color
- **Default**: Blue (60, 100, 180)
- **Pants Color**: Customizable RGB color
- **Default**: Dark blue (50, 50, 120)
- **Purpose**: Visual variety when not wearing equipment

### Character Properties

#### Name
- **Type**: String
- **Default**: "Wanderer"
- **Purpose**: Identify your character in menus and (future) multiplayer

## Tabbed Menu System

Press **C** to open the tabbed menu. Press **ESC** to close.

### Character Tab

The Character tab displays comprehensive information about your character:

#### Stats Section
- **Health**: Current/Max (e.g., 100/100)
- **Hunger**: Current/Max (e.g., 100/100)
- **Thirst**: Current/Max (e.g., 100/100)

#### Appearance Section
Shows visual swatches of:
- Skin tone
- Hair (color and style name)
- Eye color

#### Equipment Section
Displays all 8 equipment slots:
1. **Head**: Helmets, hats
2. **Chest**: Armor, shirts
3. **Legs**: Pants, leg armor
4. **Feet**: Boots, shoes
5. **Hands**: Gloves
6. **Back**: Backpacks (increases inventory space - future)
7. **Main Hand**: Primary weapon/tool
8. **Off Hand**: Shield, torch, secondary tool

Each slot shows:
- Slot name
- Equipped item (or [EMPTY])
- Visual border

#### Character Preview
A voxel-style preview of your character showing:
- Head with hair
- Torso with shirt color
- Legs with pants color
- Visual representation of equipped items (future)

### Inventory Tab

Organized grid view of all inventory items:
- **Layout**: 8 items per row
- **Display**: Item name and quantity
- **Selection**: Currently selected item highlighted in yellow
- **Capacity**: Shows slots used (e.g., "SLOTS: 12/40")

### Crafting Tab

Crafting system interface (framework):

#### Current Features
- **Material Pouch Display**:
  - Capacity percentage
  - Visual progress bar
  - Color-coded (orange/brown)

#### Planned Systems
Preview of upcoming crafting systems:
- Knapping (Stone Tools)
- Pottery (Clay Forming)
- Metallurgy (Smelting)
- Carpentry (Wood Working)

### Map Tab

Quick access to world information:
- **World Map View**: Reminder to press M for full screen
- **Player Position**: Precise X, Y, Z coordinates (color-coded)

## Implementation Details

### File Structure

#### New Files
1. `TimelessTales/UI/TabMenu.cs` - Main tabbed menu implementation
2. `Docs/CHARACTER_CUSTOMIZATION.md` - This documentation

#### Modified Files
1. `TimelessTales/Entities/Player.cs`:
   - Added `CharacterName` property
   - Added `CharacterAppearance` property
   - Added `CharacterAppearance` class
   - Added `HairStyle` enum
   - Added `Gender` enum
   - Enhanced `Equipment` class with `GetAllEquipped()`

2. `TimelessTales/Rendering/PlayerRenderer.cs`:
   - Updated to use character appearance colors
   - Skin tone, shirt, and pants now use customized values

3. `TimelessTales/Core/GameState.cs`:
   - Added `TabMenu` state

4. `TimelessTales/Core/TimelessTalesGame.cs`:
   - Added `_tabMenu` field
   - Initialized TabMenu in LoadContent
   - Added C key handler to open tab menu
   - Added ESC handler to close tab menu
   - Added update logic for TabMenu state
   - Added draw logic for TabMenu state

### Controls

| Key | Action |
|-----|--------|
| **C** | Open/toggle tabbed menu |
| **ESC** | Close tabbed menu |
| **Mouse** | Click tabs to switch between them |
| **Mouse** | Hover over items for tooltips (future) |

### Visual Design

The tabbed menu uses a consistent design language:
- **Tabs**: Dark gray with yellow highlight when selected
- **Background**: Semi-transparent dark overlay (70% opacity)
- **Colors**:
  - Yellow: Selected items, headings, important info
  - White: Primary text
  - Light Gray: Secondary text
  - Gray: Borders, inactive elements
  - Color swatches: Show actual appearance colors

### Technical Notes

#### Rendering
- Uses pixel art font system (3x5 character patterns)
- Maintains voxel/low-poly aesthetic
- All UI elements drawn with primitive rectangles
- No external texture/font files required

#### Performance
- Minimal draw calls using texture batching
- Efficient update logic (only when menu is open)
- Mouse input handled through InputManager

#### Future Enhancements

1. **Character Customization**:
   - In-game appearance editor
   - Customization presets
   - Save/load custom appearances
   - More hair styles and options

2. **Equipment**:
   - Visual rendering of equipped items on character
   - Equipment tooltips
   - Equipment comparison
   - Drag-and-drop equip/unequip

3. **Inventory**:
   - Drag-and-drop item management
   - Item sorting and filtering
   - Item categories
   - Search functionality

4. **Crafting**:
   - Recipe browser
   - Crafting queue
   - Material requirements display
   - Skill-based crafting

5. **UI Polish**:
   - Smooth tab transitions
   - Animation effects
   - Sound effects
   - Gamepad support

## Usage Examples

### Opening the Menu
```
Press C while playing → Tab menu opens on Character tab
```

### Viewing Equipment
```
Press C → Character tab shows all equipment slots
Equipment slots display what's currently equipped
Empty slots show [EMPTY]
```

### Checking Inventory
```
Press C → Click "INVENTORY" tab
View all items in organized grid
Currently selected item highlighted
Bottom shows slot usage (e.g., 12/40)
```

### Viewing Appearance
```
Press C → Character tab shows:
- Skin tone color swatch
- Hair color swatch and style name
- Eye color swatch
- Character preview with your colors
```

### Checking Crafting Materials
```
Press C → Click "CRAFTING" tab
See material pouch capacity (percentage)
Visual progress bar shows how full
```

## Integration with Game Systems

### Player Stats
- Health, Hunger, Thirst values from Player class
- Real-time updates (menu reflects current values)
- Color-coded for easy recognition

### Equipment System
- Connects to Player.Equipment
- 8 slots match EquipmentSlot enum
- Future: Visual rendering in game world

### Inventory System
- Shows all items from Player.Inventory
- Syncs with hotbar selection
- Future: Drag-and-drop management

### Material Pouch
- Displays Player.MaterialPouch capacity
- Shows percentage and progress bar
- Used for storing crafting materials

## Compatibility

### Save System (Future)
Character appearance and equipment will be:
- Saved with world data
- Loaded when resuming game
- Preserved across sessions

### Multiplayer (Future)
Character customization will:
- Display to other players
- Sync across network
- Allow unique identification

## Conclusion

The character customization and tabbed menu system adds depth and organization to Timeless Tales, making the character feel more human and providing better access to game features. The system is designed to be expandable, with many planned enhancements for future updates.
