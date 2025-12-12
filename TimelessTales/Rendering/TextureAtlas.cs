using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Represents UV coordinates for a texture within an atlas
    /// </summary>
    public struct TextureCoordinates
    {
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomLeft;
        public Vector2 BottomRight;

        public TextureCoordinates(float u, float v, float width, float height)
        {
            TopLeft = new Vector2(u, v);
            TopRight = new Vector2(u + width, v);
            BottomLeft = new Vector2(u, v + height);
            BottomRight = new Vector2(u + width, v + height);
        }

        public TextureCoordinates(Vector2 topLeft, Vector2 bottomRight)
        {
            TopLeft = topLeft;
            TopRight = new Vector2(bottomRight.X, topLeft.Y);
            BottomLeft = new Vector2(topLeft.X, bottomRight.Y);
            BottomRight = bottomRight;
        }
    }

    /// <summary>
    /// Manages a texture atlas for efficient block rendering
    /// </summary>
    public class TextureAtlas : IDisposable
    {
        private readonly Texture2D _texture;
        private readonly int _textureSize;
        private readonly int _tileSize;
        private readonly int _tilesPerRow;
        private readonly Dictionary<string, TextureCoordinates> _textureMap;
        
        // Cache for texture coordinates by index to avoid dictionary lookups
        private readonly TextureCoordinates[] _coordinateCache;
        private const int MAX_TEXTURE_INDEX = 64; // Support up to 64 different textures
        private bool _disposed;

        public Texture2D Texture => _texture;
        public int TileSize => _tileSize;

        public TextureAtlas(GraphicsDevice graphicsDevice, int textureSize = 256, int tileSize = 16)
        {
            _textureSize = textureSize;
            _tileSize = tileSize;
            _tilesPerRow = textureSize / tileSize;
            _textureMap = new Dictionary<string, TextureCoordinates>();
            _coordinateCache = new TextureCoordinates[MAX_TEXTURE_INDEX];
            
            // Create the texture atlas
            _texture = new Texture2D(graphicsDevice, textureSize, textureSize);
            
            // Initialize with procedurally generated textures
            GenerateProceduralTextures(graphicsDevice);
        }

        /// <summary>
        /// Generates procedural textures for all block types
        /// </summary>
        private void GenerateProceduralTextures(GraphicsDevice graphicsDevice)
        {
            Color[] textureData = new Color[_textureSize * _textureSize];
            
            // Initialize with transparent background
            for (int i = 0; i < textureData.Length; i++)
            {
                textureData[i] = Color.Transparent;
            }

            int currentTile = 0;

            // Generate textures for each block type
            GenerateStoneTexture(textureData, currentTile++, new Color(128, 128, 128)); // Stone
            GenerateDirtTexture(textureData, currentTile++, new Color(139, 69, 19)); // Dirt
            GenerateGrassTexture(textureData, currentTile++, Color.Green, new Color(139, 69, 19)); // Grass
            GenerateSandTexture(textureData, currentTile++, Color.SandyBrown); // Sand
            GenerateGravelTexture(textureData, currentTile++, Color.DarkGray); // Gravel
            GenerateDirtTexture(textureData, currentTile++, new Color(178, 140, 110)); // Clay
            
            // Geological rocks
            GenerateStoneTexture(textureData, currentTile++, new Color(100, 100, 100)); // Granite
            GenerateStoneTexture(textureData, currentTile++, new Color(200, 200, 180)); // Limestone
            GenerateStoneTexture(textureData, currentTile++, new Color(60, 60, 70)); // Basalt
            GenerateStoneTexture(textureData, currentTile++, new Color(210, 180, 140)); // Sandstone
            GenerateStoneTexture(textureData, currentTile++, new Color(70, 80, 90)); // Slate
            
            // Ores
            GenerateOreTexture(textureData, currentTile++, new Color(128, 128, 128), new Color(184, 115, 51)); // Copper Ore
            GenerateOreTexture(textureData, currentTile++, new Color(128, 128, 128), new Color(150, 150, 150)); // Tin Ore
            GenerateOreTexture(textureData, currentTile++, new Color(128, 128, 128), new Color(139, 90, 90)); // Iron Ore
            GenerateOreTexture(textureData, currentTile++, new Color(60, 60, 60), new Color(30, 30, 30)); // Coal
            
            // Wood blocks
            GenerateBarkTexture(textureData, currentTile++, new Color(139, 90, 43)); // Generic Wood
            GenerateLeavesTexture(textureData, currentTile++, Color.DarkGreen); // Generic Leaves
            
            // Tree types
            GenerateBarkTexture(textureData, currentTile++, new Color(101, 67, 33)); // Oak Log
            GenerateLeavesTexture(textureData, currentTile++, new Color(34, 139, 34)); // Oak Leaves
            GenerateBarkTexture(textureData, currentTile++, new Color(85, 53, 24)); // Pine Log
            GenerateLeavesTexture(textureData, currentTile++, new Color(28, 95, 28)); // Pine Leaves
            GenerateBarkTexture(textureData, currentTile++, new Color(220, 220, 200)); // Birch Log
            GenerateLeavesTexture(textureData, currentTile++, new Color(50, 150, 50)); // Birch Leaves
            
            // Crafted blocks
            GenerateWoodPlanksTexture(textureData, currentTile++, new Color(160, 110, 60)); // Planks
            GenerateCobblestoneTexture(textureData, currentTile++, new Color(120, 120, 120)); // Cobblestone
            
            // Water (simple blue with noise)
            GenerateWaterTexture(textureData, currentTile++, new Color(30, 100, 200, 180)); // Water
            GenerateWaterTexture(textureData, currentTile++, new Color(20, 60, 160, 180)); // Saltwater

            // Set the texture data
            _texture.SetData(textureData);
        }

        private void GenerateStoneTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Add noise variation to create rocky texture
                    float noise = (float)(rand.NextDouble() * 0.3 - 0.15);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    // Add occasional darker spots for mineral variations
                    if (rand.NextDouble() < 0.05)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.Black, 0.5f);
                    }
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateDirtTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Add noise for earthy texture
                    float noise = (float)(rand.NextDouble() * 0.4 - 0.2);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateGrassTexture(Color[] textureData, int tileIndex, Color grassColor, Color dirtColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Top half is grass, bottom is dirt (for side view)
                    Color baseColor = y < _tileSize / 2 ? grassColor : dirtColor;
                    
                    // Add noise
                    float noise = (float)(rand.NextDouble() * 0.3 - 0.15);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    // Add grass blades at the top
                    if (y < 3 && rand.NextDouble() < 0.2)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.DarkGreen, 0.7f);
                    }
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateSandTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Fine grain sand texture
                    float noise = (float)(rand.NextDouble() * 0.2 - 0.1);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateGravelTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Rocky gravel texture with distinct pebbles
                    float noise = (float)(rand.NextDouble() * 0.5 - 0.25);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    // Add occasional light spots for pebbles
                    if (rand.NextDouble() < 0.1)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.LightGray, 0.6f);
                    }
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateOreTexture(Color[] textureData, int tileIndex, Color stoneColor, Color oreColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Start with stone texture
                    float noise = (float)(rand.NextDouble() * 0.3 - 0.15);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(stoneColor.R + stoneColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(stoneColor.G + stoneColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(stoneColor.B + stoneColor.B * noise, 0, 255)
                    );
                    
                    // Add ore veins (about 30% of the texture)
                    if (rand.NextDouble() < 0.3)
                    {
                        pixelColor = Color.Lerp(pixelColor, oreColor, 0.8f);
                    }
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateBarkTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Create vertical wood grain
                    float verticalNoise = MathF.Sin(x * 0.5f) * 0.1f;
                    float noise = (float)(rand.NextDouble() * 0.3 - 0.15) + verticalNoise;
                    
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    // Add horizontal bark lines
                    if (y % 4 == 0 && rand.NextDouble() < 0.3)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.Black, 0.3f);
                    }
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateLeavesTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Create leafy texture with transparency
                    float noise = (float)(rand.NextDouble() * 0.4 - 0.2);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    // Add small gaps for leaf texture
                    if (rand.NextDouble() < 0.15)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.Transparent, 0.5f);
                    }
                    
                    // Add lighter veins
                    if (rand.NextDouble() < 0.05)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.LightGreen, 0.6f);
                    }
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateWoodPlanksTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Create horizontal planks
                    float horizontalNoise = MathF.Sin(y * 0.4f) * 0.1f;
                    float noise = (float)(rand.NextDouble() * 0.2 - 0.1) + horizontalNoise;
                    
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    // Add plank separators
                    if (y % 5 == 0)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.Black, 0.4f);
                    }
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateCobblestoneTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Create cobblestone pattern
                    float noise = (float)(rand.NextDouble() * 0.5 - 0.25);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255)
                    );
                    
                    // Add dark lines for cobble edges
                    if ((x % 4 == 0 || y % 4 == 0) && rand.NextDouble() < 0.5)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.Black, 0.5f);
                    }
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void GenerateWaterTexture(Color[] textureData, int tileIndex, Color baseColor)
        {
            int tileX = (tileIndex % _tilesPerRow) * _tileSize;
            int tileY = (tileIndex / _tilesPerRow) * _tileSize;
            
            Random rand = new Random(tileIndex * 1000);
            
            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    // Simple water texture with slight variation
                    float noise = (float)(rand.NextDouble() * 0.1 - 0.05);
                    Color pixelColor = new Color(
                        (int)MathHelper.Clamp(baseColor.R + baseColor.R * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.G + baseColor.G * noise, 0, 255),
                        (int)MathHelper.Clamp(baseColor.B + baseColor.B * noise, 0, 255),
                        baseColor.A
                    );
                    
                    int index = (tileY + y) * _textureSize + (tileX + x);
                    textureData[index] = pixelColor;
                }
            }
            
            RegisterTexture($"tile_{tileIndex}", tileIndex);
        }

        private void RegisterTexture(string name, int tileIndex)
        {
            int tileX = tileIndex % _tilesPerRow;
            int tileY = tileIndex / _tilesPerRow;
            
            float u = (float)tileX / _tilesPerRow;
            float v = (float)tileY / _tilesPerRow;
            float width = 1.0f / _tilesPerRow;
            float height = 1.0f / _tilesPerRow;
            
            var coords = new TextureCoordinates(u, v, width, height);
            _textureMap[name] = coords;
            
            // Cache coordinates by index for fast lookups
            if (tileIndex < MAX_TEXTURE_INDEX)
            {
                _coordinateCache[tileIndex] = coords;
            }
        }

        public TextureCoordinates GetTextureCoordinates(string textureName)
        {
            if (_textureMap.TryGetValue(textureName, out var coords))
            {
                return coords;
            }
            
            // Return default texture (stone texture as fallback)
            const string DEFAULT_TEXTURE = "tile_0";
            if (_textureMap.TryGetValue(DEFAULT_TEXTURE, out var defaultCoords))
            {
                return defaultCoords;
            }
            
            // If even default is missing, return full atlas (shouldn't happen)
            return new TextureCoordinates(0, 0, 1, 1);
        }

        public TextureCoordinates GetTextureCoordinates(int tileIndex)
        {
            // Use cached coordinates for fast lookup without dictionary access
            if (tileIndex >= 0 && tileIndex < MAX_TEXTURE_INDEX)
            {
                TextureCoordinates cached = _coordinateCache[tileIndex];
                // Verify the cache entry was initialized (TopLeft won't be Vector2.Zero for valid entries)
                if (cached.TopLeft != Vector2.Zero || tileIndex == 0)
                {
                    return cached;
                }
            }
            
            // Fallback to dictionary lookup for out-of-range or uninitialized indices
            return GetTextureCoordinates($"tile_{tileIndex}");
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _texture?.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
