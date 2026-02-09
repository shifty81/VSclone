using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.World;
using TimelessTales.Blocks;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Specialized renderer for translucent water blocks with Gerstner wave animation,
    /// foam effects, depth-based coloring, and procedural surface patterns.
    /// Uses GPU vertex buffers for improved performance.
    /// </summary>
    public class WaterRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly WorldManager _worldManager;
        private readonly BasicEffect _waterEffect;
        private readonly Dictionary<(int, int), WaterMesh> _waterMeshes;
        private float _time;
        private bool _disposed;
        
        // Track which chunks need wave vertex updates (dirty when time changes)
        private float _lastWaveUpdateTime;
        private const float WAVE_UPDATE_INTERVAL = 0.05f; // ~20 updates/sec for wave animation (reduced from 30)

        // Water visual parameters
        private const int SEA_LEVEL = 64;
        private const float WAVE_SPEED = 0.5f;
        private const float WAVE_HEIGHT = 0.08f;
        private const float MAX_DEPTH_FOR_COLOR_CALCULATION = 20.0f;
        
        // Gerstner wave parameters for more realistic wave motion
        // Each wave component: (direction_x, direction_z, steepness, wavelength)
        private static readonly (float DirX, float DirZ, float Steepness, float Wavelength)[] GerstnerWaves = new[]
        {
            (1.0f, 0.0f, 0.15f, 8.0f),   // Primary wave - long wavelength
            (0.0f, 1.0f, 0.10f, 5.0f),   // Secondary cross-wave
            (0.7f, 0.7f, 0.08f, 3.0f),   // Diagonal chop
        };
        
        // Physical constants for Gerstner wave dispersion relation
        private const float GRAVITY_CONSTANT = 9.81f; // m/s², standard gravity for wave dispersion
        
        // Foam parameters
        private const float FOAM_THRESHOLD = 0.6f;  // Wave height ratio to trigger foam
        private const float FOAM_INTENSITY = 0.4f;   // How much foam brightens the water
        private const float FOAM_NORMALIZATION_RANGE = 2.5f; // Max possible offset normalizer for foam calculation
        
        // Cel shading parameters
        private const int CEL_SHADING_BANDS = 4; // Number of discrete color bands
        
        // Procedural surface pattern parameters for more realistic water appearance
        private const float CAUSTIC_SCALE = 0.3f;     // Scale of caustic-like patterns on surface
        private const float CAUSTIC_INTENSITY = 0.08f; // Subtle brightness variation
        private const float RIPPLE_SCALE = 0.15f;      // Scale of surface ripple patterns
        private const float SPECULAR_POWER = 0.12f;    // Specular highlight intensity on crests

        public WaterRenderer(GraphicsDevice graphicsDevice, WorldManager worldManager)
        {
            _graphicsDevice = graphicsDevice;
            _worldManager = worldManager;
            _waterMeshes = new Dictionary<(int, int), WaterMesh>();
            _time = 0;
            _lastWaveUpdateTime = 0;

            // Initialize effect for water rendering with alpha blending
            _waterEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                Alpha = 0.7f // Water transparency
            };
        }

        public void Update(GameTime gameTime)
        {
            _time += (float)gameTime.ElapsedGameTime.TotalSeconds * WAVE_SPEED;
        }

        public void Draw(Camera camera)
        {
            // Update camera matrices
            camera.Update();
            _waterEffect.View = camera.ViewMatrix;
            _waterEffect.Projection = camera.ProjectionMatrix;

            // Set render states for transparent water
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Throttle wave mesh rebuilds for performance (~20 fps for wave animation)
            bool needsWaveUpdate = (_time - _lastWaveUpdateTime) >= WAVE_UPDATE_INTERVAL;

            // Get currently loaded chunks
            var loadedChunks = _worldManager.GetLoadedChunks().ToList();
            var loadedChunkKeys = new HashSet<(int, int)>(
                loadedChunks.Select(c => (c.ChunkX, c.ChunkZ))
            );
            
            // Remove meshes for unloaded chunks to free memory
            var meshKeysToRemove = _waterMeshes.Keys
                .Where(key => !loadedChunkKeys.Contains(key))
                .ToList();
            
            foreach (var key in meshKeysToRemove)
            {
                var mesh = _waterMeshes[key];
                mesh.Dispose();
                _waterMeshes.Remove(key);
            }

            // Build/update water meshes
            foreach (var chunk in loadedChunks)
            {
                var key = (chunk.ChunkX, chunk.ChunkZ);

                if (!_waterMeshes.TryGetValue(key, out var mesh) || chunk.NeedsMeshRebuild || needsWaveUpdate)
                {
                    // Dispose old mesh before replacing
                    if (mesh != null)
                    {
                        mesh.Dispose();
                    }
                    mesh = BuildWaterMesh(chunk);
                    _waterMeshes[key] = mesh;
                }

                // Draw water mesh using GPU vertex buffer
                if (mesh != null && mesh.VertexCount > 0 && mesh.VertexBuffer != null)
                {
                    _waterEffect.World = Matrix.Identity;

                    foreach (var pass in _waterEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        _graphicsDevice.SetVertexBuffer(mesh.VertexBuffer);
                        _graphicsDevice.DrawPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            mesh.VertexCount / 3
                        );
                    }
                }
            }

            if (needsWaveUpdate)
            {
                _lastWaveUpdateTime = _time;
            }

            // Restore default render states
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        private WaterMesh BuildWaterMesh(Chunk chunk)
        {
            var vertices = new List<VertexPositionColor>(3000);

            int worldX = chunk.ChunkX * Chunk.CHUNK_SIZE;
            int worldZ = chunk.ChunkZ * Chunk.CHUNK_SIZE;

            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        BlockType block = chunk.GetBlock(x, y, z);
                        
                        if (block != BlockType.Water && block != BlockType.Saltwater)
                            continue;

                        bool renderTop = !IsWaterBlock(chunk, x, y + 1, z);
                        bool renderBottom = !IsWaterBlock(chunk, x, y - 1, z);
                        bool renderNorth = !IsWaterBlock(chunk, x, y, z + 1);
                        bool renderSouth = !IsWaterBlock(chunk, x, y, z - 1);
                        bool renderEast = !IsWaterBlock(chunk, x + 1, y, z);
                        bool renderWest = !IsWaterBlock(chunk, x - 1, y, z);

                        Vector3 blockPos = new Vector3(worldX + x, y, worldZ + z);
                        
                        Color waterColor = GetDepthBasedWaterColor(y, block);

                        float waveOffset = 0;
                        if (renderTop)
                        {
                            waveOffset = CalculateWaveOffset(worldX + x, worldZ + z);
                        }

                        AddWaterFaces(vertices, blockPos, waterColor, waveOffset, worldX + x, worldZ + z,
                                    renderTop, renderBottom, renderNorth, renderSouth, renderEast, renderWest);
                    }
                }
            }

            return new WaterMesh(_graphicsDevice, vertices.ToArray());
        }

        private bool IsWaterBlock(Chunk chunk, int x, int y, int z)
        {
            if (y < 0 || y >= Chunk.CHUNK_HEIGHT) return false;
            
            if (x >= 0 && x < Chunk.CHUNK_SIZE && z >= 0 && z < Chunk.CHUNK_SIZE)
            {
                BlockType block = chunk.GetBlock(x, y, z);
                return block == BlockType.Water || block == BlockType.Saltwater;
            }
            
            int worldX = chunk.ChunkX * Chunk.CHUNK_SIZE + x;
            int worldZ = chunk.ChunkZ * Chunk.CHUNK_SIZE + z;
            
            BlockType neighborBlock = _worldManager.GetBlock(worldX, y, worldZ);
            return neighborBlock == BlockType.Water || neighborBlock == BlockType.Saltwater;
        }

        private Color GetDepthBasedWaterColor(int y, BlockType waterType)
        {
            int depthFromSurface = SEA_LEVEL - y;

            // Use richer base colors with slight green tint for more realistic water
            Color baseColor;
            if (waterType == BlockType.Saltwater)
            {
                // Ocean water - deeper blue-green with more realistic tones
                baseColor = new Color(15, 70, 140);
            }
            else
            {
                // Fresh water - blue-green with clarity
                baseColor = new Color(25, 110, 180);
            }

            float depthFactor = MathHelper.Clamp(depthFromSurface / MAX_DEPTH_FOR_COLOR_CALCULATION, 0, 1);
            float celDepthFactor = CelShadingUtility.QuantizeToNBands(depthFactor, CEL_SHADING_BANDS);
            
            float brightness = 1.0f - (celDepthFactor * 0.5f);
            
            // Alpha: shallow water more transparent, deep water more opaque
            int alpha = (int)(140 + celDepthFactor * 115); // Alpha: 140 (shallow) to 255 (deep)
            
            return new Color(
                (int)(baseColor.R * brightness),
                (int)(baseColor.G * brightness),
                (int)(baseColor.B * brightness),
                alpha
            );
        }

        private float CalculateWaveOffset(int worldX, int worldZ)
        {
            float totalOffset = 0f;
            
            foreach (var wave in GerstnerWaves)
            {
                float frequency = 2.0f * MathF.PI / wave.Wavelength;
                float phase = frequency * (wave.DirX * worldX + wave.DirZ * worldZ) + _time * MathF.Sqrt(GRAVITY_CONSTANT * frequency);
                totalOffset += wave.Steepness * MathF.Sin(phase) * WAVE_HEIGHT;
            }
            
            return totalOffset;
        }
        
        /// <summary>
        /// Calculates a procedural surface pattern value for a water surface position.
        /// Creates caustic-like light patterns that shift over time.
        /// </summary>
        private float CalculateSurfacePattern(int worldX, int worldZ)
        {
            float pattern = 0f;
            
            // Primary caustic pattern - large slow-moving light bands
            float cx = worldX * CAUSTIC_SCALE + _time * 0.3f;
            float cz = worldZ * CAUSTIC_SCALE - _time * 0.2f;
            pattern += MathF.Sin(cx) * MathF.Cos(cz) * 0.5f;
            
            // Secondary ripple pattern - smaller faster-moving detail
            float rx = worldX * (CAUSTIC_SCALE * 2.3f) - _time * 0.5f;
            float rz = worldZ * (CAUSTIC_SCALE * 2.3f) + _time * 0.4f;
            pattern += MathF.Sin(rx + rz) * RIPPLE_SCALE;
            
            // Tertiary fine detail - very small high-frequency ripples
            float fx = worldX * (CAUSTIC_SCALE * 4.7f) + _time * 0.7f;
            float fz = worldZ * (CAUSTIC_SCALE * 4.7f) - _time * 0.6f;
            pattern += MathF.Sin(fx) * MathF.Sin(fz) * 0.08f;
            
            return pattern;
        }
        
        /// <summary>
        /// Calculates foam intensity at a given position based on wave crest height.
        /// Returns a value 0-1 indicating how much foam to apply.
        /// </summary>
        public static float CalculateFoamFactor(float waveOffset, float waveHeight)
        {
            float normalizedHeight = waveOffset / (waveHeight * FOAM_NORMALIZATION_RANGE);
            if (normalizedHeight > FOAM_THRESHOLD)
            {
                return MathHelper.Clamp((normalizedHeight - FOAM_THRESHOLD) / (1.0f - FOAM_THRESHOLD), 0f, 1f) * FOAM_INTENSITY;
            }
            return 0f;
        }

        private void AddWaterFaces(List<VertexPositionColor> vertices, Vector3 pos, Color color, float waveOffset,
                                    int worldX, int worldZ,
                                    bool top, bool bottom, bool north, bool south, bool east, bool west)
        {
            Color topColor = Color.Lerp(color, Color.White, 0.3f);
            
            // Apply procedural surface pattern to top for more realistic water look
            if (top)
            {
                float surfacePattern = CalculateSurfacePattern(worldX, worldZ);
                
                // Modulate top color with caustic-like pattern
                float patternBrightness = 1.0f + surfacePattern * CAUSTIC_INTENSITY;
                topColor = new Color(
                    (int)MathHelper.Clamp(topColor.R * patternBrightness, 0, 255),
                    (int)MathHelper.Clamp(topColor.G * patternBrightness, 0, 255),
                    (int)MathHelper.Clamp(topColor.B * patternBrightness, 0, 255),
                    topColor.A
                );
                
                // Add specular-like highlight at wave crests
                if (waveOffset > 0)
                {
                    float specular = MathF.Pow(waveOffset / (WAVE_HEIGHT * 0.33f), 2.0f) * SPECULAR_POWER;
                    specular = MathHelper.Clamp(specular, 0f, 0.3f);
                    topColor = Color.Lerp(topColor, Color.White, specular);
                }
            }
            
            // Apply foam brightening to top surface at wave crests
            float foamFactor = CalculateFoamFactor(waveOffset, WAVE_HEIGHT);
            if (foamFactor > 0f)
            {
                topColor = Color.Lerp(topColor, Color.White, foamFactor);
            }
            
            Color bottomColor = CelShadingUtility.ApplyCelShading(Color.Lerp(color, Color.Black, 0.2f), CEL_SHADING_BANDS);
            Color sideColor = CelShadingUtility.ApplyCelShading(color, CEL_SHADING_BANDS);
            
            const float OVERLAP = 0.01f;
            
            if (top)
            {
                AddQuad(vertices, pos,
                    new Vector3(-OVERLAP, 1 + waveOffset, -OVERLAP), 
                    new Vector3(1 + OVERLAP, 1 + waveOffset, -OVERLAP),
                    new Vector3(1 + OVERLAP, 1 + waveOffset, 1 + OVERLAP), 
                    new Vector3(-OVERLAP, 1 + waveOffset, 1 + OVERLAP), topColor);
            }

            if (bottom)
            {
                AddQuad(vertices, pos,
                    new Vector3(-OVERLAP, -OVERLAP, 1 + OVERLAP), 
                    new Vector3(1 + OVERLAP, -OVERLAP, 1 + OVERLAP),
                    new Vector3(1 + OVERLAP, -OVERLAP, -OVERLAP), 
                    new Vector3(-OVERLAP, -OVERLAP, -OVERLAP), bottomColor);
            }

            if (north)
            {
                AddQuad(vertices, pos,
                    new Vector3(-OVERLAP, -OVERLAP, 1 + OVERLAP), 
                    new Vector3(-OVERLAP, 1 + OVERLAP, 1 + OVERLAP),
                    new Vector3(1 + OVERLAP, 1 + OVERLAP, 1 + OVERLAP), 
                    new Vector3(1 + OVERLAP, -OVERLAP, 1 + OVERLAP), sideColor);
            }

            if (south)
            {
                AddQuad(vertices, pos,
                    new Vector3(1 + OVERLAP, -OVERLAP, -OVERLAP), 
                    new Vector3(1 + OVERLAP, 1 + OVERLAP, -OVERLAP),
                    new Vector3(-OVERLAP, 1 + OVERLAP, -OVERLAP), 
                    new Vector3(-OVERLAP, -OVERLAP, -OVERLAP), sideColor);
            }

            if (east)
            {
                AddQuad(vertices, pos,
                    new Vector3(1 + OVERLAP, -OVERLAP, 1 + OVERLAP), 
                    new Vector3(1 + OVERLAP, 1 + OVERLAP, 1 + OVERLAP),
                    new Vector3(1 + OVERLAP, 1 + OVERLAP, -OVERLAP), 
                    new Vector3(1 + OVERLAP, -OVERLAP, -OVERLAP), sideColor);
            }

            if (west)
            {
                AddQuad(vertices, pos,
                    new Vector3(-OVERLAP, -OVERLAP, -OVERLAP), 
                    new Vector3(-OVERLAP, 1 + OVERLAP, -OVERLAP),
                    new Vector3(-OVERLAP, 1 + OVERLAP, 1 + OVERLAP), 
                    new Vector3(-OVERLAP, -OVERLAP, 1 + OVERLAP), sideColor);
            }
        }

        private void AddQuad(List<VertexPositionColor> vertices, Vector3 basePos,
                            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color color)
        {
            vertices.Add(new VertexPositionColor(basePos + v1, color));
            vertices.Add(new VertexPositionColor(basePos + v2, color));
            vertices.Add(new VertexPositionColor(basePos + v3, color));

            vertices.Add(new VertexPositionColor(basePos + v3, color));
            vertices.Add(new VertexPositionColor(basePos + v4, color));
            vertices.Add(new VertexPositionColor(basePos + v1, color));
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var mesh in _waterMeshes.Values)
                {
                    mesh.Dispose();
                }
                _waterMeshes.Clear();
                _waterEffect?.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Contains mesh data for water in a chunk, using a GPU vertex buffer for performance
    /// </summary>
    public class WaterMesh : IDisposable
    {
        public VertexPositionColor[] Vertices { get; }
        public int VertexCount => Vertices.Length;
        public VertexBuffer? VertexBuffer { get; private set; }
        private bool _disposed;

        public WaterMesh(GraphicsDevice graphicsDevice, VertexPositionColor[] vertices)
        {
            Vertices = vertices;
            if (vertices.Length > 0)
            {
                VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
                VertexBuffer.SetData(vertices);
            }
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                VertexBuffer?.Dispose();
                VertexBuffer = null;
                _disposed = true;
            }
        }
    }
}
