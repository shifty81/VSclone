using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.Blocks;

namespace TimelessTales.UI
{
    /// <summary>
    /// Tooltip display for showing item information on hover
    /// </summary>
    public class Tooltip
    {
        private readonly Texture2D _pixelTexture;
        
        public Tooltip(GraphicsDevice graphicsDevice)
        {
            // Create pixel texture
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        
        /// <summary>
        /// Draw a tooltip for a block type at the mouse position
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, BlockType blockType, int count, int mouseX, int mouseY)
        {
            if (blockType == BlockType.Air)
                return;
            
            // Get block info
            var block = BlockRegistry.Get(blockType);
            string name = GetBlockDisplayName(blockType);
            string hardnessText = $"HARDNESS: {block.Hardness:F1}";
            string countText = $"COUNT: {count}";
            
            // Calculate tooltip size
            int padding = 6;
            int lineHeight = 13;
            int maxWidth = System.Math.Max(name.Length, System.Math.Max(hardnessText.Length, countText.Length)) * 4 + padding * 2;
            int tooltipHeight = lineHeight * 3 + padding * 2;
            
            // Position tooltip near mouse (offset to avoid cursor)
            int tooltipX = mouseX + 15;
            int tooltipY = mouseY + 15;
            
            // Keep tooltip on screen
            int screenWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            int screenHeight = spriteBatch.GraphicsDevice.Viewport.Height;
            
            if (tooltipX + maxWidth > screenWidth)
                tooltipX = mouseX - maxWidth - 5;
            if (tooltipY + tooltipHeight > screenHeight)
                tooltipY = mouseY - tooltipHeight - 5;
            
            // Draw background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(tooltipX, tooltipY, maxWidth, tooltipHeight),
                Color.Black * 0.85f);
            
            // Draw border
            int borderWidth = 2;
            spriteBatch.Draw(_pixelTexture, new Rectangle(tooltipX, tooltipY, maxWidth, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(tooltipX, tooltipY + tooltipHeight - borderWidth, maxWidth, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(tooltipX, tooltipY, borderWidth, tooltipHeight), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(tooltipX + maxWidth - borderWidth, tooltipY, borderWidth, tooltipHeight), Color.White);
            
            // Draw text
            int textX = tooltipX + padding;
            int textY = tooltipY + padding;
            
            // Item name (yellow)
            DrawText(spriteBatch, name, textX, textY, new Color(255, 220, 100));
            textY += lineHeight;
            
            // Hardness (gray)
            DrawText(spriteBatch, hardnessText, textX, textY, Color.LightGray);
            textY += lineHeight;
            
            // Count (white)
            DrawText(spriteBatch, countText, textX, textY, Color.White);
        }
        
        private string GetBlockDisplayName(BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Stone => "STONE",
                BlockType.Dirt => "DIRT",
                BlockType.Grass => "GRASS",
                BlockType.Sand => "SAND",
                BlockType.Gravel => "GRAVEL",
                BlockType.Clay => "CLAY",
                BlockType.Granite => "GRANITE",
                BlockType.Limestone => "LIMESTONE",
                BlockType.Basalt => "BASALT",
                BlockType.Sandstone => "SANDSTONE",
                BlockType.Slate => "SLATE",
                BlockType.Cobblestone => "COBBLESTONE",
                BlockType.Wood => "WOOD",
                BlockType.Planks => "PLANKS",
                BlockType.Leaves => "LEAVES",
                BlockType.OakLog => "OAK LOG",
                BlockType.OakLeaves => "OAK LEAVES",
                BlockType.PineLog => "PINE LOG",
                BlockType.PineLeaves => "PINE LEAVES",
                BlockType.BirchLog => "BIRCH LOG",
                BlockType.BirchLeaves => "BIRCH LEAVES",
                BlockType.CopperOre => "COPPER ORE",
                BlockType.TinOre => "TIN ORE",
                BlockType.IronOre => "IRON ORE",
                BlockType.Coal => "COAL",
                BlockType.Water => "WATER",
                BlockType.Saltwater => "SALTWATER",
                _ => blockType.ToString().ToUpper()
            };
        }
        
        private void DrawText(SpriteBatch spriteBatch, string text, int x, int y, Color color)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                int charX = x + i * 4;
                
                bool[,] pattern = GetCharPattern(c);
                
                for (int py = 0; py < 5; py++)
                {
                    for (int px = 0; px < 3; px++)
                    {
                        if (pattern[py, px])
                        {
                            spriteBatch.Draw(_pixelTexture,
                                new Rectangle(charX + px, y + py, 1, 1),
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
                        { true, false, true },
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
                        { false, true, false },
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { false, true, false }
                    };
                    break;
                case 'P':
                    pattern = new bool[,] {
                        { true, true, false },
                        { true, false, true },
                        { true, true, false },
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
                        { true, false, true },
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
                case '.':
                    pattern = new bool[,] {
                        { false, false, false },
                        { false, false, false },
                        { false, false, false },
                        { false, false, false },
                        { false, true, false }
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
                case ' ':
                    // Already initialized as all false
                    break;
            }
            
            return pattern;
        }
    }
}
