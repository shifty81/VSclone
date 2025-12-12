using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TimelessTales.UI
{
    /// <summary>
    /// A simple clickable button for UI
    /// </summary>
    public class Button
    {
        public Rectangle Bounds { get; set; }
        public string Text { get; set; }
        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressedColor { get; set; }
        public bool IsEnabled { get; set; }
        
        private bool _isHovered;
        private bool _wasPressed;
        
        public Button(Rectangle bounds, string text)
        {
            Bounds = bounds;
            Text = text;
            NormalColor = new Color(60, 60, 60, 200);
            HoverColor = new Color(80, 80, 80, 220);
            PressedColor = new Color(100, 100, 100, 240);
            IsEnabled = true;
        }
        
        public bool Update(MouseState mouseState, MouseState previousMouseState)
        {
            if (!IsEnabled)
            {
                _isHovered = false;
                return false;
            }
            
            Point mousePosition = new Point(mouseState.X, mouseState.Y);
            _isHovered = Bounds.Contains(mousePosition);
            
            // Check for click (mouse was pressed and released on button)
            bool clicked = false;
            if (_isHovered)
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    _wasPressed = true;
                }
                else if (_wasPressed && previousMouseState.LeftButton == ButtonState.Pressed)
                {
                    clicked = true;
                    _wasPressed = false;
                }
            }
            else
            {
                _wasPressed = false;
            }
            
            return clicked;
        }
        
        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            Color color = NormalColor;
            if (!IsEnabled)
            {
                color = new Color(40, 40, 40, 150);
            }
            else if (_wasPressed)
            {
                color = PressedColor;
            }
            else if (_isHovered)
            {
                color = HoverColor;
            }
            
            // Draw button background
            spriteBatch.Draw(pixelTexture, Bounds, color);
            
            // Draw button border
            int borderThickness = 2;
            Color borderColor = IsEnabled ? Color.White : new Color(100, 100, 100);
            
            // Top
            spriteBatch.Draw(pixelTexture, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, borderThickness), borderColor);
            // Bottom
            spriteBatch.Draw(pixelTexture, new Rectangle(Bounds.X, Bounds.Y + Bounds.Height - borderThickness, Bounds.Width, borderThickness), borderColor);
            // Left
            spriteBatch.Draw(pixelTexture, new Rectangle(Bounds.X, Bounds.Y, borderThickness, Bounds.Height), borderColor);
            // Right
            spriteBatch.Draw(pixelTexture, new Rectangle(Bounds.X + Bounds.Width - borderThickness, Bounds.Y, borderThickness, Bounds.Height), borderColor);
        }
        
        public void DrawText(SpriteBatch spriteBatch, Texture2D pixelTexture)
        {
            Color textColor = IsEnabled ? Color.White : new Color(150, 150, 150);
            
            // Calculate centered position
            int textWidth = Text.Length * 4; // Approximate width (4 pixels per char including spacing)
            int textHeight = 5;
            int textX = Bounds.X + (Bounds.Width - textWidth) / 2;
            int textY = Bounds.Y + (Bounds.Height - textHeight) / 2;
            
            DrawPixelText(spriteBatch, pixelTexture, Text, textX, textY, textColor);
        }
        
        private void DrawPixelText(SpriteBatch spriteBatch, Texture2D pixelTexture, string text, int x, int y, Color color)
        {
            // Simple pixel font rendering (3x5 characters)
            int currentX = x;
            
            foreach (char c in text.ToUpper())
            {
                DrawChar(spriteBatch, pixelTexture, c, currentX, y, color);
                currentX += 4; // 3 pixel width + 1 spacing
            }
        }
        
        private void DrawChar(SpriteBatch spriteBatch, Texture2D pixelTexture, char c, int x, int y, Color color)
        {
            // Simple 3x5 pixel font for basic characters
            bool[,] pixels = GetCharPixels(c);
            
            for (int py = 0; py < 5; py++)
            {
                for (int px = 0; px < 3; px++)
                {
                    if (pixels[px, py])
                    {
                        spriteBatch.Draw(pixelTexture, new Rectangle(x + px, y + py, 1, 1), color);
                    }
                }
            }
        }
        
        private bool[,] GetCharPixels(char c)
        {
            // 3x5 bitmap font (simplified)
            return c switch
            {
                'A' => new bool[,] { { true, true, true }, { true, false, true }, { true, true, true }, { true, false, true }, { true, false, true } },
                'B' => new bool[,] { { true, true, false }, { true, false, true }, { true, true, false }, { true, false, true }, { true, true, false } },
                'C' => new bool[,] { { false, true, true }, { true, false, false }, { true, false, false }, { true, false, false }, { false, true, true } },
                'D' => new bool[,] { { true, true, false }, { true, false, true }, { true, false, true }, { true, false, true }, { true, true, false } },
                'E' => new bool[,] { { true, true, true }, { true, false, false }, { true, true, true }, { true, false, false }, { true, true, true } },
                'F' => new bool[,] { { true, true, true }, { true, false, false }, { true, true, true }, { true, false, false }, { true, false, false } },
                'G' => new bool[,] { { false, true, true }, { true, false, false }, { true, false, true }, { true, false, true }, { false, true, true } },
                'H' => new bool[,] { { true, false, true }, { true, false, true }, { true, true, true }, { true, false, true }, { true, false, true } },
                'I' => new bool[,] { { true, true, true }, { false, true, false }, { false, true, false }, { false, true, false }, { true, true, true } },
                'J' => new bool[,] { { false, false, true }, { false, false, true }, { false, false, true }, { true, false, true }, { false, true, false } },
                'K' => new bool[,] { { true, false, true }, { true, false, true }, { true, true, false }, { true, false, true }, { true, false, true } },
                'L' => new bool[,] { { true, false, false }, { true, false, false }, { true, false, false }, { true, false, false }, { true, true, true } },
                'M' => new bool[,] { { true, false, true }, { true, true, true }, { true, false, true }, { true, false, true }, { true, false, true } },
                'N' => new bool[,] { { true, false, true }, { true, true, true }, { true, true, true }, { true, false, true }, { true, false, true } },
                'O' => new bool[,] { { false, true, false }, { true, false, true }, { true, false, true }, { true, false, true }, { false, true, false } },
                'P' => new bool[,] { { true, true, false }, { true, false, true }, { true, true, false }, { true, false, false }, { true, false, false } },
                'Q' => new bool[,] { { false, true, false }, { true, false, true }, { true, false, true }, { true, true, false }, { false, true, true } },
                'R' => new bool[,] { { true, true, false }, { true, false, true }, { true, true, false }, { true, false, true }, { true, false, true } },
                'S' => new bool[,] { { false, true, true }, { true, false, false }, { false, true, false }, { false, false, true }, { true, true, false } },
                'T' => new bool[,] { { true, true, true }, { false, true, false }, { false, true, false }, { false, true, false }, { false, true, false } },
                'U' => new bool[,] { { true, false, true }, { true, false, true }, { true, false, true }, { true, false, true }, { false, true, false } },
                'V' => new bool[,] { { true, false, true }, { true, false, true }, { true, false, true }, { true, false, true }, { false, true, false } },
                'W' => new bool[,] { { true, false, true }, { true, false, true }, { true, false, true }, { true, true, true }, { true, false, true } },
                'X' => new bool[,] { { true, false, true }, { true, false, true }, { false, true, false }, { true, false, true }, { true, false, true } },
                'Y' => new bool[,] { { true, false, true }, { true, false, true }, { false, true, false }, { false, true, false }, { false, true, false } },
                'Z' => new bool[,] { { true, true, true }, { false, false, true }, { false, true, false }, { true, false, false }, { true, true, true } },
                ' ' => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } },
                _ => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } }
            };
        }
    }
}
