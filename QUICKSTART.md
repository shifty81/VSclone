# Quick Start Guide - Timeless Tales

Welcome to Timeless Tales! This guide will help you get started playing the game.

## Installation

### System Requirements

**Minimum:**
- OS: Windows 10 (64-bit)
- Processor: Dual-core 2.5 GHz
- Memory: 4 GB RAM
- Graphics: DirectX 11 compatible GPU
- Storage: 2 GB available space
- .NET 8.0 Runtime

**Recommended:**
- OS: Windows 11 (64-bit)
- Processor: Quad-core 3.0 GHz
- Memory: 8 GB RAM
- Graphics: DirectX 11 GPU with 2GB VRAM
- Storage: 2 GB available space (SSD)

### Installing .NET Runtime

If you don't have .NET installed:
1. Download .NET 8.0 Runtime from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run the installer
3. Restart your computer

### Running the Game

**Option 1: Run from source**
```bash
cd TimelessTales
dotnet run
```

**Option 2: Build and run executable**
```bash
cd TimelessTales
dotnet build -c Release
cd bin/Release/net8.0
./TimelessTales.exe
```

## First Steps

### Understanding the World

When you first spawn, you'll find yourself in a procedurally generated world with:
- **Different biomes**: Tundra (cold), Boreal (forests), Temperate (grasslands), Desert (sandy), Tropical (lush)
- **Realistic geology**: The ground has multiple rock layers - sedimentary rocks near the surface, metamorphic rocks deeper, and igneous rocks at the deepest levels
- **Ores**: Hidden within specific rock types at certain depths

### Basic Controls

| Key | Action |
|-----|--------|
| **W** | Move forward |
| **A** | Move left |
| **S** | Move backward |
| **D** | Move right |
| **Mouse** | Look around |
| **Space** | Jump |
| **Left Shift** | Sprint (hold while moving) |
| **Left Mouse Button** | Break block (hold to mine) |
| **Right Mouse Button** | Place block |
| **1-5** | Select different block types from inventory |
| **P** | Pause game |
| **Escape** | Exit game |

### Your First Minutes

1. **Look Around**: Move your mouse to look in different directions
2. **Walk Around**: Use WASD keys to explore
3. **Break a Block**: Point at the ground and hold Left Mouse Button
   - You'll see the crosshair in the center of your screen
   - Aim at a block and hold left-click
   - The block will break after ~1 second
4. **Place a Block**: 
   - Select a block type with number keys (1-5)
   - Right-click to place it
   - You can only place blocks adjacent to existing blocks
5. **Build Something**: Try making a simple shelter!

### Gameplay Tips

**Resource Gathering:**
- Different blocks take different times to break based on hardness
- Stone is harder than dirt
- Ores are harder than regular stone

**Building:**
- Plan your structures before building
- You start with some basic blocks in your inventory
- Place blocks strategically to create structures

**Exploration:**
- Look for different biomes
- Different rock types appear at different depths
- Ores are not randomly distributed - they appear in specific rock layers

**Survival (Future Feature):**
- Temperature management will matter based on biome
- Seasons will affect resource availability
- Food and water will be essential

## Understanding the HUD

- **Crosshair**: Center of screen - shows where you're aiming
- **Inventory**: Shows your collected blocks (press 1-5 to select)
- **Break Progress**: Visual indicator when breaking blocks

## Block Types

### Natural Terrain
- **Grass**: Green surface blocks in temperate areas
- **Dirt**: Brown subsurface layer
- **Sand**: Found in deserts and beaches (affected by gravity)
- **Gravel**: Gray loose stone (affected by gravity)
- **Clay**: Brownish material for future crafting

### Rock Layers (by depth)
- **Limestone**: Light colored sedimentary rock (upper levels)
- **Sandstone**: Sandy colored sedimentary rock (upper levels)
- **Slate**: Dark metamorphic rock (mid levels)
- **Granite**: Speckled igneous rock (deep levels)
- **Basalt**: Dark volcanic rock (deepest levels)

### Ores
- **Coal**: Black ore in sandstone layers (Y: 40-70)
- **Copper Ore**: Reddish-brown ore in limestone (Y: 30-60)
- **Tin Ore**: Gray ore in granite (Y: 15-35)
- **Iron Ore**: Dark red ore in basalt (Y: 5-30)

*Note: Y-coordinate represents depth, with 0 at bedrock and 64 at sea level*

### Crafted/Processed
- **Planks**: Wooden building material
- **Cobblestone**: Processed stone
- **Wood**: Tree logs
- **Leaves**: Tree foliage (partially transparent)

## Hotbar Quick Reference

Press number keys to switch between block types:
- **1**: Stone (default starting material)
- **2**: Dirt (easy to work with)
- **3**: Planks (building material)
- **4**: Cobblestone (processed stone)
- **5**: Wood (from trees)

## Frequently Asked Questions

**Q: How do I mine ores?**
A: Break stone blocks underground. Ores appear as different colored blocks within specific rock types.

**Q: Why can't I place a block?**
A: Make sure you have that block type in your inventory, and you're clicking on an existing block's face (not in mid-air).

**Q: How do I know what type of block I'm looking at?**
A: Currently, blocks are identified by color. Future updates will add a UI tooltip.

**Q: Is there a map?**
A: Not yet. Navigate by memory and landmarks. Future updates may add mapping.

**Q: Can I save my world?**
A: Saving/loading is planned for a future update. Currently, worlds reset when you restart.

**Q: Are there enemies?**
A: Not in this alpha version. Future updates will add hostile creatures (Drifters) and temporal instability mechanics.

**Q: What's the goal?**
A: Currently it's a creative sandbox. Future updates will add survival mechanics, crafting progression, and exploration goals.

## Troubleshooting

**Game won't start:**
- Ensure .NET 8.0 Runtime is installed
- Update graphics drivers
- Try running in windowed mode (edit code if needed)

**Low FPS:**
- The game is in early alpha and not fully optimized
- Reduce render distance (requires code change currently)
- Close other applications

**Controls not working:**
- Make sure the game window has focus
- Check keyboard/mouse are properly connected

## What's Next?

This is Alpha 0.1 - a foundational release. Upcoming features include:
- Textures for all blocks
- Crafting system (knapping, pottery, metallurgy)
- Survival mechanics (hunger, thirst, temperature)
- Day/night cycle
- Lighting system
- Hostile creatures
- Sound and music
- Save/load system

## Getting Help

- Check the README.md for technical details
- See DEVELOPER.md for code-level information
- Report issues on the GitHub repository

## Credits

Inspired by **Vintage Story** by Anego Studios.
Built with MonoGame framework.

---

Happy building and exploring! 🎮⛏️🏗️
