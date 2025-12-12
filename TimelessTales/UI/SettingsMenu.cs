using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TimelessTales.UI
{
    /// <summary>
    /// Settings and options menu
    /// </summary>
    public class SettingsMenu
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Texture2D _pixelTexture;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        
        private Button _graphicsButton;
        private Button _audioButton;
        private Button _controlsButton;
        private Button _backButton;
        
        private MouseState _previousMouseState;
        
        public event Action? OnGraphics;
        public event Action? OnAudio;
        public event Action? OnControls;
        public event Action? OnBack;
        
        public SettingsMenu(GraphicsDevice graphicsDevice)
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
            
            _graphicsButton = new Button(
                new Rectangle(centerX, startY, buttonWidth, buttonHeight),
                "GRAPHICS"
            );
            _graphicsButton.IsEnabled = false; // Not implemented yet
            
            _audioButton = new Button(
                new Rectangle(centerX, startY + buttonHeight + buttonSpacing, buttonWidth, buttonHeight),
                "AUDIO"
            );
            _audioButton.IsEnabled = false; // Not implemented yet
            
            _controlsButton = new Button(
                new Rectangle(centerX, startY + (buttonHeight + buttonSpacing) * 2, buttonWidth, buttonHeight),
                "CONTROLS"
            );
            
            _backButton = new Button(
                new Rectangle(centerX, startY + (buttonHeight + buttonSpacing) * 3, buttonWidth, buttonHeight),
                "BACK"
            );
            
            _previousMouseState = Mouse.GetState();
        }
        
        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            
            // Update buttons
            if (_graphicsButton.Update(mouseState, _previousMouseState))
            {
                OnGraphics?.Invoke();
            }
            
            if (_audioButton.Update(mouseState, _previousMouseState))
            {
                OnAudio?.Invoke();
            }
            
            if (_controlsButton.Update(mouseState, _previousMouseState))
            {
                OnControls?.Invoke();
            }
            
            if (_backButton.Update(mouseState, _previousMouseState))
            {
                OnBack?.Invoke();
            }
            
            _previousMouseState = mouseState;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw semi-transparent overlay
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(0, 0, _screenWidth, _screenHeight),
                Color.Black * 0.8f);
            
            // Draw title
            string title = "SETTINGS";
            int titleScale = 2;
            int titleWidth = title.Length * 4 * titleScale;
            int titleX = (_screenWidth - titleWidth) / 2;
            int titleY = _screenHeight / 4;
            
            DrawLargeText(spriteBatch, title, titleX, titleY, titleScale, new Color(220, 180, 100));
            
            // Draw buttons
            _graphicsButton.Draw(spriteBatch, _pixelTexture);
            _audioButton.Draw(spriteBatch, _pixelTexture);
            _controlsButton.Draw(spriteBatch, _pixelTexture);
            _backButton.Draw(spriteBatch, _pixelTexture);
            
            _graphicsButton.DrawText(spriteBatch, _pixelTexture);
            _audioButton.DrawText(spriteBatch, _pixelTexture);
            _controlsButton.DrawText(spriteBatch, _pixelTexture);
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
                _ => new bool[,] { { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false }, { false, false, false } }
            };
        }
    }
}
