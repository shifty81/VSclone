using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TimelessTales.Entities;
using TimelessTales.World;

namespace TimelessTales.UI
{
    /// <summary>
    /// Debug overlay showing FPS, position, and other debug info (F3 to toggle)
    /// </summary>
    public class DebugOverlay
    {
        private readonly Texture2D _pixelTexture;
        private readonly int _screenWidth;
        private readonly int _screenHeight;
        
        private double _frameTime;
        private int _frameCount;
        private double _fpsUpdateTimer;
        private int _currentFPS;
        
        public bool IsVisible { get; set; } = false;
        
        public DebugOverlay(GraphicsDevice graphicsDevice)
        {
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            
            // Create pixel texture
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        
        public void Update(GameTime gameTime)
        {
            // Calculate FPS
            _frameTime += gameTime.ElapsedGameTime.TotalSeconds;
            _frameCount++;
            _fpsUpdateTimer += gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_fpsUpdateTimer >= 0.5) // Update FPS twice per second
            {
                _currentFPS = (int)(_frameCount / _fpsUpdateTimer);
                _frameCount = 0;
                _fpsUpdateTimer = 0;
            }
        }
        
        public void Draw(SpriteBatch spriteBatch, Player player, WorldManager worldManager)
        {
            if (!IsVisible) return;
            
            // Draw semi-transparent background panel
            int panelWidth = 250;
            int panelHeight = 165;
            int panelX = 10;
            int panelY = 244; // Below character status bars (4 bars)
            
            spriteBatch.Draw(_pixelTexture,
                new Rectangle(panelX, panelY, panelWidth, panelHeight),
                Color.Black * 0.7f);
            
            // Draw border
            int borderWidth = 2;
            spriteBatch.Draw(_pixelTexture, new Rectangle(panelX, panelY, panelWidth, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(panelX, panelY + panelHeight - borderWidth, panelWidth, borderWidth), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(panelX, panelY, borderWidth, panelHeight), Color.White);
            spriteBatch.Draw(_pixelTexture, new Rectangle(panelX + panelWidth - borderWidth, panelY, borderWidth, panelHeight), Color.White);
            
            // Draw debug info
            int textX = panelX + 6;
            int textY = panelY + 6;
            int lineHeight = 13;
            
            // FPS
            DrawText(spriteBatch, $"FPS: {_currentFPS}", textX, textY, Color.Yellow);
            textY += lineHeight;
            
            // Position
            Vector3 pos = player.Position;
            DrawText(spriteBatch, $"X: {pos.X:F2}", textX, textY, Color.Red);
            textY += lineHeight;
            DrawText(spriteBatch, $"Y: {pos.Y:F2}", textX, textY, Color.Green);
            textY += lineHeight;
            DrawText(spriteBatch, $"Z: {pos.Z:F2}", textX, textY, Color.Blue);
            textY += lineHeight;
            
            // Rotation
            DrawText(spriteBatch, $"YAW: {player.Rotation.Y:F2}", textX, textY, Color.Cyan);
            textY += lineHeight;
            DrawText(spriteBatch, $"PITCH: {player.Rotation.X:F2}", textX, textY, Color.Cyan);
            textY += lineHeight;
            
            // Chunk info
            int chunkX = (int)System.Math.Floor(pos.X / 16);
            int chunkZ = (int)System.Math.Floor(pos.Z / 16);
            DrawText(spriteBatch, $"CHUNK: {chunkX} {chunkZ}", textX, textY, Color.LightGray);
            textY += lineHeight;
            
            // Water status
            if (player.IsUnderwater)
            {
                DrawText(spriteBatch, $"SUBMERGED: {player.SubmersionDepth:F2}", textX, textY, Color.Aqua);
            }
            else
            {
                DrawText(spriteBatch, "NOT IN WATER", textX, textY, Color.Gray);
            }
            textY += lineHeight;
            
            // Temperature status
            string tempStatus = Entities.TemperatureSystem.GetTemperatureStatus(player.BodyTemperature);
            Color tempColor = player.BodyTemperature < 30f ? Color.CornflowerBlue : 
                              player.BodyTemperature > 70f ? Color.OrangeRed : Color.LightGreen;
            DrawText(spriteBatch, $"TEMP: {player.BodyTemperature:F1} {tempStatus}", textX, textY, tempColor);
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
                case 'W':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { true, false, true },
                        { true, true, true },
                        { true, false, true }
                    };
                    break;
                case 'X':
                    pattern = new bool[,] {
                        { true, false, true },
                        { true, false, true },
                        { false, true, false },
                        { true, false, true },
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
                case 'Z':
                    pattern = new bool[,] {
                        { true, true, true },
                        { false, false, true },
                        { false, true, false },
                        { true, false, false },
                        { true, true, true }
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
                case '-':
                    pattern = new bool[,] {
                        { false, false, false },
                        { false, false, false },
                        { true, true, true },
                        { false, false, false },
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
