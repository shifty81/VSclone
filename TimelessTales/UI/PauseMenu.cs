using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TimelessTales.UI
{
    /// <summary>
    /// Pause menu displayed when the game is paused
    /// </summary>
    public class PauseMenu
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture2D _pixelTexture;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        
        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;
        
        private MouseState _previousMouseState;
        
        public event Action? OnResume;
        public event Action? OnSettings;
        public event Action? OnMainMenu;
        
        public PauseMenu(GraphicsDevice graphicsDevice)
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
            int startY = _screenHeight / 2 - 50;
            int centerX = (_screenWidth - buttonWidth) / 2;
            
            _resumeButton = new Button(
                new Rectangle(centerX, startY, buttonWidth, buttonHeight),
                "RESUME"
            );
            
            _settingsButton = new Button(
                new Rectangle(centerX, startY + buttonHeight + buttonSpacing, buttonWidth, buttonHeight),
                "SETTINGS"
            );
            _settingsButton.IsEnabled = false; // Not implemented yet
            
            _mainMenuButton = new Button(
                new Rectangle(centerX, startY + (buttonHeight + buttonSpacing) * 2, buttonWidth, buttonHeight),
                "MAIN MENU"
            );
            
            _previousMouseState = Mouse.GetState();
        }
        
        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            
            // Update buttons
            if (_resumeButton.Update(mouseState, _previousMouseState))
            {
                OnResume?.Invoke();
            }
            
            if (_settingsButton.Update(mouseState, _previousMouseState))
            {
                OnSettings?.Invoke();
            }
            
            if (_mainMenuButton.Update(mouseState, _previousMouseState))
            {
                OnMainMenu?.Invoke();
            }
            
            _previousMouseState = mouseState;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw semi-transparent overlay
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, 0, _screenWidth, _screenHeight),
                Color.Black * 0.6f);
            
            // Draw title
            string title = "PAUSED";
            int titleScale = 3;
            int titleWidth = title.Length * 4 * titleScale;
            int titleX = (_screenWidth - titleWidth) / 2;
            int titleY = _screenHeight / 4;
            
            DrawLargeText(spriteBatch, title, titleX, titleY, titleScale, new Color(220, 180, 100));
            
            // Draw buttons
            _resumeButton.Draw(spriteBatch, _pixelTexture);
            _settingsButton.Draw(spriteBatch, _pixelTexture);
            _mainMenuButton.Draw(spriteBatch, _pixelTexture);
            
            _resumeButton.DrawText(spriteBatch, _pixelTexture);
            _settingsButton.DrawText(spriteBatch, _pixelTexture);
            _mainMenuButton.DrawText(spriteBatch, _pixelTexture);
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
        
        private bool[,] GetCharPixels(char c)
        {
            return c switch
            {
                'A' => new bool[,] { { true, true, true }, { true, false, true }, { true, true, true }, { true, false, true }, { true, false, true } },
                'D' => new bool[,] { { true, true, false }, { true, false, true }, { true, false, true }, { true, false, true }, { true, true, false } },
                'E' => new bool[,] { { true, true, true }, { true, false, false }, { true, true, true }, { true, false, false }, { true, true, true } },
                'P' => new bool[,] { { true, true, false }, { true, false, true }, { true, true, false }, { true, false, false }, { true, false, false } },
                'S' => new bool[,] { { false, true, true }, { true, false, false }, { false, true, false }, { false, false, true }, { true, true, false } },
                'U' => new bool[,] { { true, false, true }, { true, false, true }, { true, false, true }, { true, false, true }, { false, true, false } },
                ' ' => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } },
                _ => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } }
            };
        }
    }
}
