using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.Entities;

namespace TimelessTales.UI
{
    /// <summary>
    /// Character status display showing health, hunger, and thirst
    /// </summary>
    public class CharacterStatusDisplay
    {
        private readonly Texture2D _pixelTexture;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        
        // Bar dimensions
        private const int BAR_WIDTH = 150;
        private const int BAR_HEIGHT = 18;
        private const int BAR_SPACING = 6;
        private const int BAR_BORDER = 2;
        
        // Position (top-left corner)
        private const int MARGIN_X = 10;
        private const int MARGIN_Y = 10;
        
        public CharacterStatusDisplay(GraphicsDevice graphicsDevice)
        {
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            
            // Create pixel texture
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        
        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            int currentY = MARGIN_Y;
            
            // Draw health bar (red)
            DrawStatusBar(spriteBatch, MARGIN_X, currentY, player.Health, player.MaxHealth, 
                new Color(180, 40, 40), Color.DarkRed, "HEALTH");
            currentY += BAR_HEIGHT + BAR_SPACING;
            
            // Draw hunger bar (orange/brown)
            DrawStatusBar(spriteBatch, MARGIN_X, currentY, player.Hunger, player.MaxHunger, 
                new Color(200, 120, 40), new Color(100, 60, 20), "HUNGER");
            currentY += BAR_HEIGHT + BAR_SPACING;
            
            // Draw thirst bar (blue)
            DrawStatusBar(spriteBatch, MARGIN_X, currentY, player.Thirst, player.MaxThirst, 
                new Color(60, 140, 220), new Color(30, 70, 110), "THIRST");
        }
        
        private void DrawStatusBar(SpriteBatch spriteBatch, int x, int y, float current, float max, 
            Color fillColor, Color backgroundColor, string label)
        {
            // Calculate fill percentage
            float percentage = max > 0 ? current / max : 0;
            percentage = MathHelper.Clamp(percentage, 0f, 1f);
            
            // Draw background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x, y, BAR_WIDTH, BAR_HEIGHT),
                Color.Black * 0.7f);
            
            // Draw empty bar background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(x + BAR_BORDER, y + BAR_BORDER, 
                    BAR_WIDTH - BAR_BORDER * 2, BAR_HEIGHT - BAR_BORDER * 2),
                backgroundColor * 0.6f);
            
            // Draw filled portion
            int fillWidth = (int)((BAR_WIDTH - BAR_BORDER * 2) * percentage);
            if (fillWidth > 0)
            {
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x + BAR_BORDER, y + BAR_BORDER, 
                        fillWidth, BAR_HEIGHT - BAR_BORDER * 2),
                    fillColor);
            }
            
            // Draw border
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, BAR_WIDTH, BAR_BORDER), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y + BAR_HEIGHT - BAR_BORDER, BAR_WIDTH, BAR_BORDER), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(x, y, BAR_BORDER, BAR_HEIGHT), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(x + BAR_WIDTH - BAR_BORDER, y, BAR_BORDER, BAR_HEIGHT), Color.White);
            
            // Draw label (small text inside bar)
            int labelX = x + 4;
            int labelY = y + (BAR_HEIGHT - 5) / 2;
            DrawSmallText(spriteBatch, label, labelX, labelY, Color.White);
            
            // Draw value text (right-aligned)
            string valueText = $"{(int)current}/{(int)max}";
            int valueWidth = valueText.Length * 4;
            int valueX = x + BAR_WIDTH - valueWidth - 4;
            int valueY = y + (BAR_HEIGHT - 5) / 2;
            DrawSmallText(spriteBatch, valueText, valueX, valueY, Color.White);
        }
        
        private void DrawSmallText(SpriteBatch spriteBatch, string text, int x, int y, Color color)
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
                case 'L':
                    pattern = new bool[,] {
                        { true, false, false },
                        { true, false, false },
                        { true, false, false },
                        { true, false, false },
                        { true, true, true }
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
                case ' ':
                    // Already initialized as all false
                    break;
            }
            
            return pattern;
        }
    }
}
