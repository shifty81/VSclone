using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TimelessTales.UI
{
    /// <summary>
    /// Title screen with main menu options
    /// </summary>
    public class TitleScreen
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture2D _pixelTexture;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        
        private Button _newGameButton;
        private Button _loadGameButton;
        private Button _joinButton;
        private Button _settingsButton;
        
        private MouseState _previousMouseState;
        
        public event Action? OnNewGame;
        public event Action? OnLoadGame;
        public event Action? OnJoin;
        public event Action? OnSettings;
        
        public TitleScreen(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            
            // Create pixel texture
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            // Create buttons
            int buttonWidth = 200;
            int buttonHeight = 50;
            int buttonSpacing = 20;
            int startY = _screenHeight / 2;
            int centerX = (_screenWidth - buttonWidth) / 2;
            
            _newGameButton = new Button(
                new Rectangle(centerX, startY, buttonWidth, buttonHeight),
                "NEW GAME"
            );
            
            _loadGameButton = new Button(
                new Rectangle(centerX, startY + buttonHeight + buttonSpacing, buttonWidth, buttonHeight),
                "LOAD GAME"
            );
            _loadGameButton.IsEnabled = false; // Not implemented yet
            
            _joinButton = new Button(
                new Rectangle(centerX, startY + (buttonHeight + buttonSpacing) * 2, buttonWidth, buttonHeight),
                "JOIN"
            );
            _joinButton.IsEnabled = false; // Not implemented yet
            
            _settingsButton = new Button(
                new Rectangle(centerX, startY + (buttonHeight + buttonSpacing) * 3, buttonWidth, buttonHeight),
                "SETTINGS"
            );
            _settingsButton.IsEnabled = false; // Not implemented yet
            
            _previousMouseState = Mouse.GetState();
        }
        
        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            
            // Update buttons
            if (_newGameButton.Update(mouseState, _previousMouseState))
            {
                OnNewGame?.Invoke();
            }
            
            if (_loadGameButton.Update(mouseState, _previousMouseState))
            {
                OnLoadGame?.Invoke();
            }
            
            if (_joinButton.Update(mouseState, _previousMouseState))
            {
                OnJoin?.Invoke();
            }
            
            if (_settingsButton.Update(mouseState, _previousMouseState))
            {
                OnSettings?.Invoke();
            }
            
            _previousMouseState = mouseState;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(20, 30, 40));
            
            spriteBatch.Begin();
            
            // Draw title
            string title = "TIMELESS TALES";
            int titleScale = 3;
            int titleWidth = title.Length * 4 * titleScale;
            int titleX = (_screenWidth - titleWidth) / 2;
            int titleY = _screenHeight / 4;
            
            DrawLargeText(spriteBatch, title, titleX, titleY, titleScale, new Color(220, 180, 100));
            
            // Draw subtitle
            string subtitle = "A VINTAGE STORY CLONE";
            int subtitleX = (_screenWidth - subtitle.Length * 4) / 2;
            int subtitleY = titleY + 60;
            DrawPixelText(spriteBatch, subtitle, subtitleX, subtitleY, new Color(150, 150, 150));
            
            // Draw buttons
            _newGameButton.Draw(spriteBatch, _pixelTexture);
            _loadGameButton.Draw(spriteBatch, _pixelTexture);
            _joinButton.Draw(spriteBatch, _pixelTexture);
            _settingsButton.Draw(spriteBatch, _pixelTexture);
            
            _newGameButton.DrawText(spriteBatch, _pixelTexture);
            _loadGameButton.DrawText(spriteBatch, _pixelTexture);
            _joinButton.DrawText(spriteBatch, _pixelTexture);
            _settingsButton.DrawText(spriteBatch, _pixelTexture);
            
            // Draw version info
            string version = "ALPHA 0.1";
            DrawPixelText(spriteBatch, version, _screenWidth - version.Length * 4 - 10, _screenHeight - 15, Color.Gray);
            
            spriteBatch.End();
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
                    if (pixels[px, py])
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
                    if (pixels[px, py])
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
                ' ' => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } },
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
                '.' => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, true, false } },
                _ => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } }
            };
        }
    }
}
