using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace TimelessTales.UI
{
    /// <summary>
    /// Controls help screen showing key bindings
    /// </summary>
    public class ControlsScreen
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture2D _pixelTexture;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        
        private Button _backButton;
        private MouseState _previousMouseState;
        
        public event Action? OnBack;
        
        // Control mapping data
        private readonly List<(string action, string key)> _controls = new List<(string, string)>
        {
            ("MOVEMENT", ""),
            ("Move Forward", "W"),
            ("Move Left", "A"),
            ("Move Backward", "S"),
            ("Move Right", "D"),
            ("Sprint", "Left Shift"),
            ("Jump", "Space"),
            ("Swim Up", "Space"),
            ("Dive Down", "Left Ctrl"),
            ("", ""),
            ("ACTIONS", ""),
            ("Break Block", "Left Click"),
            ("Place Block", "Right Click"),
            ("", ""),
            ("INTERFACE", ""),
            ("Inventory", "I"),
            ("World Map", "M"),
            ("Pause", "P"),
            ("", ""),
            ("HOTBAR", ""),
            ("Select Slot 1-9", "1-9 Keys"),
            ("", ""),
            ("SYSTEM", ""),
            ("Fullscreen", "F11"),
            ("Exit Game", "Escape")
        };
        
        public ControlsScreen(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            
            // Create pixel texture
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            // Create back button
            int buttonWidth = 150;
            int buttonHeight = 40;
            int centerX = (_screenWidth - buttonWidth) / 2;
            int buttonY = _screenHeight - 80;
            
            _backButton = new Button(
                new Rectangle(centerX, buttonY, buttonWidth, buttonHeight),
                "BACK"
            );
            
            _previousMouseState = Mouse.GetState();
        }
        
        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            
            if (_backButton.Update(mouseState, _previousMouseState))
            {
                OnBack?.Invoke();
            }
            
            _previousMouseState = mouseState;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw semi-transparent background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, 0, _screenWidth, _screenHeight),
                Color.Black * 0.85f);
            
            // Draw title
            string title = "CONTROLS";
            int titleScale = 2;
            int titleWidth = title.Length * 4 * titleScale;
            int titleX = (_screenWidth - titleWidth) / 2;
            int titleY = 40;
            
            DrawLargeText(spriteBatch, title, titleX, titleY, titleScale, new Color(220, 180, 100));
            
            // Draw controls panel
            int panelWidth = 600;
            int panelHeight = 420;
            int panelX = (_screenWidth - panelWidth) / 2;
            int panelY = 110;
            
            // Panel background
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(panelX, panelY, panelWidth, panelHeight),
                Color.DarkGray * 0.5f);
            
            // Panel border
            int borderWidth = 2;
            spriteBatch.Draw(_pixelTexture, new Rectangle(panelX, panelY, panelWidth, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(panelX, panelY + panelHeight - borderWidth, panelWidth, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(panelX, panelY, borderWidth, panelHeight), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(panelX + panelWidth - borderWidth, panelY, borderWidth, panelHeight), Color.White);
            
            // Draw controls list
            int textY = panelY + 15;
            int lineHeight = 13;
            int leftColumnX = panelX + 30;
            int rightColumnX = panelX + panelWidth - 150;
            
            foreach (var (action, key) in _controls)
            {
                if (string.IsNullOrEmpty(action))
                {
                    // Empty line for spacing
                    textY += lineHeight;
                    continue;
                }
                
                if (string.IsNullOrEmpty(key))
                {
                    // Section header
                    DrawPixelText(spriteBatch, action, leftColumnX, textY, new Color(255, 200, 100));
                    textY += lineHeight + 3; // Extra spacing after headers
                }
                else
                {
                    // Control binding
                    DrawPixelText(spriteBatch, action, leftColumnX + 15, textY, Color.LightGray);
                    DrawPixelText(spriteBatch, key, rightColumnX, textY, Color.White);
                    textY += lineHeight;
                }
            }
            
            // Draw back button
            _backButton.Draw(spriteBatch, _pixelTexture);
            _backButton.DrawText(spriteBatch, _pixelTexture);
        }
        
        private void DrawLargeText(SpriteBatch spriteBatch, string text, int x, int y, int scale, Color color)
        {
            int currentX = x;
            foreach (char c in text.ToUpper())
            {
                DrawLargeChar(spriteBatch, c, currentX, y, scale, color);
                currentX += 4 * scale;
            }
        }
        
        private void DrawLargeChar(SpriteBatch spriteBatch, char c, int x, int y, int scale, Color color)
        {
            bool[,] pixels = GetCharPixels(c);
            
            for (int py = 0; py < 5; py++)
            {
                for (int px = 0; px < 3; px++)
                {
                    if (pixels[py, px])
                    {
                        spriteBatch.Draw(_pixelTexture, 
                            new Rectangle(x + px * scale, y + py * scale, scale, scale), 
                            color);
                    }
                }
            }
        }
        
        private void DrawPixelText(SpriteBatch spriteBatch, string text, int x, int y, Color color)
        {
            int currentX = x;
            foreach (char c in text.ToUpper())
            {
                DrawChar(spriteBatch, c, currentX, y, color);
                currentX += 4;
            }
        }
        
        private void DrawChar(SpriteBatch spriteBatch, char c, int x, int y, Color color)
        {
            bool[,] pixels = GetCharPixels(c);
            
            for (int py = 0; py < 5; py++)
            {
                for (int px = 0; px < 3; px++)
                {
                    if (pixels[py, px])
                    {
                        spriteBatch.Draw(_pixelTexture, new Rectangle(x + px, y + py, 1, 1), color);
                    }
                }
            }
        }
        
        private bool[,] GetCharPixels(char c)
        {
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
                '0' => new bool[,] { { false, true, false }, { true, false, true }, { true, false, true }, { true, false, true }, { false, true, false } },
                '1' => new bool[,] { { false, true, false }, { true, true, false }, { false, true, false }, { false, true, false }, { true, true, true } },
                '2' => new bool[,] { { true, true, false }, { false, false, true }, { false, true, false }, { true, false, false }, { true, true, true } },
                '3' => new bool[,] { { true, true, true }, { false, false, true }, { false, true, true }, { false, false, true }, { true, true, true } },
                '4' => new bool[,] { { true, false, true }, { true, false, true }, { true, true, true }, { false, false, true }, { false, false, true } },
                '5' => new bool[,] { { true, true, true }, { true, false, false }, { true, true, true }, { false, false, true }, { true, true, false } },
                '6' => new bool[,] { { false, true, true }, { true, false, false }, { true, true, true }, { true, false, true }, { true, true, true } },
                '7' => new bool[,] { { true, true, true }, { false, false, true }, { false, true, false }, { false, true, false }, { false, true, false } },
                '8' => new bool[,] { { true, true, true }, { true, false, true }, { true, true, true }, { true, false, true }, { true, true, true } },
                '9' => new bool[,] { { true, true, true }, { true, false, true }, { true, true, true }, { false, false, true }, { true, true, false } },
                ' ' => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } },
                '-' => new bool[,] { { false, false, false }, { false, false, false }, { true, true, true }, { false, false, false }, { false, false, false } },
                _ => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } }
            };
        }
    }
}
