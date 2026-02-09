using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.World;
using TimelessTales.Blocks;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Specialized renderer for translucent water blocks with Gerstner wave animation,
    /// foam effects, and depth-based coloring
    /// </summary>
    public class WaterRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly WorldManager _worldManager;
        private readonly BasicEffect _waterEffect;
        private readonly Dictionary<(int, int), WaterMesh> _waterMeshes;
        private float _time;
        
        // Track which chunks need wave vertex updates (dirty when time changes)
        private float _lastWaveUpdateTime;
        private const float WAVE_UPDATE_INTERVAL = 0.033f; // ~30 updates/sec for wave animation

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
            // Use default depth stencil for proper depth testing with transparency
            // This allows water to correctly occlude objects behind it while still being transparent
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone; // Render both sides of water

            // Throttle wave mesh rebuilds for performance (~30 fps for wave animation)
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
                _waterMeshes.Remove(key);
            }

            // Build/update water meshes
            foreach (var chunk in loadedChunks)
            {
                var key = (chunk.ChunkX, chunk.ChunkZ);

                if (!_waterMeshes.TryGetValue(key, out var mesh) || chunk.NeedsMeshRebuild || needsWaveUpdate)
                {
                    mesh = BuildWaterMesh(chunk);
                    _waterMeshes[key] = mesh;
                }

                // Draw water mesh
                if (mesh != null && mesh.VertexCount > 0)
                {
                    _waterEffect.World = Matrix.Identity;

                    foreach (var pass in _waterEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        _graphicsDevice.DrawUserPrimitives(
                            PrimitiveType.TriangleList,
                            mesh.Vertices,
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
            // Pre-allocate vertex list with estimated capacity to reduce allocations
            // Water chunks typically have fewer visible faces than regular blocks
            var vertices = new List<VertexPositionColor>(3000);

            int worldX = chunk.ChunkX * Chunk.CHUNK_SIZE;
            int worldZ = chunk.ChunkZ * Chunk.CHUNK_SIZE;

            // Build mesh for water blocks only
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        BlockType block = chunk.GetBlock(x, y, z);
                        
                        // Only render water blocks
                        if (block != BlockType.Water && block != BlockType.Saltwater)
                            continue;

                        // Check neighboring blocks to determine which faces to render
                        bool renderTop = !IsWaterBlock(chunk, x, y + 1, z);
                        bool renderBottom = !IsWaterBlock(chunk, x, y - 1, z);
                        bool renderNorth = !IsWaterBlock(chunk, x, y, z + 1);
                        bool renderSouth = !IsWaterBlock(chunk, x, y, z - 1);
                        bool renderEast = !IsWaterBlock(chunk, x + 1, y, z);
                        bool renderWest = !IsWaterBlock(chunk, x - 1, y, z);

                        Vector3 blockPos = new Vector3(worldX + x, y, worldZ + z);
                        
                        // Calculate depth-based color (deeper = darker blue)
                        Color waterColor = GetDepthBasedWaterColor(y, block);

                        // Add wave animation to top surface
                        float waveOffset = 0;
                        if (renderTop)
                        {
                            waveOffset = CalculateWaveOffset(worldX + x, worldZ + z);
                        }

                        AddWaterFaces(vertices, blockPos, waterColor, waveOffset,
                                    renderTop, renderBottom, renderNorth, renderSouth, renderEast, renderWest);
                    }
                }
            }

            return new WaterMesh(vertices.ToArray());
        }

        private bool IsWaterBlock(Chunk chunk, int x, int y, int z)
        {
            // Check vertical bounds
            if (y < 0 || y >= Chunk.CHUNK_HEIGHT) return false;
            
            // Check if within current chunk bounds
            if (x >= 0 && x < Chunk.CHUNK_SIZE && z >= 0 && z < Chunk.CHUNK_SIZE)
            {
                BlockType block = chunk.GetBlock(x, y, z);
                return block == BlockType.Water || block == BlockType.Saltwater;
            }
            
            // Handle cross-chunk boundaries
            // Calculate world coordinates
            int worldX = chunk.ChunkX * Chunk.CHUNK_SIZE + x;
            int worldZ = chunk.ChunkZ * Chunk.CHUNK_SIZE + z;
            
            // Get block from world manager (which handles chunk boundaries)
            BlockType neighborBlock = _worldManager.GetBlock(worldX, y, worldZ);
            return neighborBlock == BlockType.Water || neighborBlock == BlockType.Saltwater;
        }

        private Color GetDepthBasedWaterColor(int y, BlockType waterType)
        {
            // Calculate depth from sea level
            int depthFromSurface = SEA_LEVEL - y;

            // Determine base color
            Color baseColor;
            if (waterType == BlockType.Saltwater)
            {
                // Ocean water - deeper blue
                baseColor = new Color(20, 60, 160);
            }
            else
            {
                // Fresh water - lighter blue/green
                baseColor = new Color(30, 100, 200);
            }

            // Calculate depth factor
            float depthFactor = MathHelper.Clamp(depthFromSurface / MAX_DEPTH_FOR_COLOR_CALCULATION, 0, 1);
            
            // Apply cel shading - quantize depth into discrete bands
            float celDepthFactor = CelShadingUtility.QuantizeToNBands(depthFactor, CEL_SHADING_BANDS);
            
            // Shallow water is clearer (more transparent and lighter)
            // Deep water is darker and more opaque
            float brightness = 1.0f - (celDepthFactor * 0.5f);
            
            // Alpha also uses cel shading for consistent look
            int alpha = (int)(150 + celDepthFactor * 105); // Alpha: 150 (shallow) to 255 (deep)
            
            return new Color(
                (int)(baseColor.R * brightness),
                (int)(baseColor.G * brightness),
                (int)(baseColor.B * brightness),
                alpha
            );
        }

        private float CalculateWaveOffset(int worldX, int worldZ)
        {
            // Gerstner wave model for more realistic ocean-like wave motion
            // Produces sharper crests and flatter troughs compared to simple sine waves
            float totalOffset = 0f;
            
            foreach (var wave in GerstnerWaves)
            {
                float frequency = 2.0f * MathF.PI / wave.Wavelength;
                float phase = frequency * (wave.DirX * worldX + wave.DirZ * worldZ) + _time * MathF.Sqrt(GRAVITY_CONSTANT * frequency);
                // Gerstner vertical displacement: steepness * sin(phase) / frequency
                totalOffset += wave.Steepness * MathF.Sin(phase) * WAVE_HEIGHT;
            }
            
            return totalOffset;
        }
        
        /// <summary>
        /// Calculates foam intensity at a given position based on wave crest height.
        /// Returns a value 0-1 indicating how much foam to apply.
        /// </summary>
        public static float CalculateFoamFactor(float waveOffset, float waveHeight)
        {
            // Foam appears at wave crests (high positive displacement)
            float normalizedHeight = waveOffset / (waveHeight * FOAM_NORMALIZATION_RANGE); // Normalize to max possible offset
            if (normalizedHeight > FOAM_THRESHOLD)
            {
                return MathHelper.Clamp((normalizedHeight - FOAM_THRESHOLD) / (1.0f - FOAM_THRESHOLD), 0f, 1f) * FOAM_INTENSITY;
            }
            return 0f;
        }

        private void AddWaterFaces(List<VertexPositionColor> vertices, Vector3 pos, Color color, float waveOffset,
                                   bool top, bool bottom, bool north, bool south, bool east, bool west)
        {
            // Apply cel shading to side/bottom faces but NOT to top surface to avoid grid lines
            // Top surface should be smooth to prevent visible banding/grid effect
            Color topColor = Color.Lerp(color, Color.White, 0.3f); // Lighter top, no cel shading
            
            // Apply foam brightening to top surface at wave crests
            float foamFactor = CalculateFoamFactor(waveOffset, WAVE_HEIGHT);
            if (foamFactor > 0f)
            {
                topColor = Color.Lerp(topColor, Color.White, foamFactor);
            }
            
            Color bottomColor = CelShadingUtility.ApplyCelShading(Color.Lerp(color, Color.Black, 0.2f), CEL_SHADING_BANDS);
            Color sideColor = CelShadingUtility.ApplyCelShading(color, CEL_SHADING_BANDS);
            
            // Increased overlap to eliminate visible seams between water blocks
            const float OVERLAP = 0.01f;
            
            // Top face (Y+) - with wave animation
            if (top)
            {
                AddQuad(vertices, pos,
                    new Vector3(-OVERLAP, 1 + waveOffset, -OVERLAP), 
                    new Vector3(1 + OVERLAP, 1 + waveOffset, -OVERLAP),
                    new Vector3(1 + OVERLAP, 1 + waveOffset, 1 + OVERLAP), 
                    new Vector3(-OVERLAP, 1 + waveOffset, 1 + OVERLAP), topColor);
            }

            // Bottom face (Y-) - extend to hide seams
            if (bottom)
            {
                AddQuad(vertices, pos,
                    new Vector3(-OVERLAP, -OVERLAP, 1 + OVERLAP), 
                    new Vector3(1 + OVERLAP, -OVERLAP, 1 + OVERLAP),
                    new Vector3(1 + OVERLAP, -OVERLAP, -OVERLAP), 
                    new Vector3(-OVERLAP, -OVERLAP, -OVERLAP), bottomColor);
            }

            // North face (Z+) - extend to hide seams
            if (north)
            {
                AddQuad(vertices, pos,
                    new Vector3(-OVERLAP, -OVERLAP, 1 + OVERLAP), 
                    new Vector3(-OVERLAP, 1 + OVERLAP, 1 + OVERLAP),
                    new Vector3(1 + OVERLAP, 1 + OVERLAP, 1 + OVERLAP), 
                    new Vector3(1 + OVERLAP, -OVERLAP, 1 + OVERLAP), sideColor);
            }

            // South face (Z-) - extend to hide seams
            if (south)
            {
                AddQuad(vertices, pos,
                    new Vector3(1 + OVERLAP, -OVERLAP, -OVERLAP), 
                    new Vector3(1 + OVERLAP, 1 + OVERLAP, -OVERLAP),
                    new Vector3(-OVERLAP, 1 + OVERLAP, -OVERLAP), 
                    new Vector3(-OVERLAP, -OVERLAP, -OVERLAP), sideColor);
            }

            // East face (X+) - extend to hide seams
            if (east)
            {
                AddQuad(vertices, pos,
                    new Vector3(1 + OVERLAP, -OVERLAP, 1 + OVERLAP), 
                    new Vector3(1 + OVERLAP, 1 + OVERLAP, 1 + OVERLAP),
                    new Vector3(1 + OVERLAP, 1 + OVERLAP, -OVERLAP), 
                    new Vector3(1 + OVERLAP, -OVERLAP, -OVERLAP), sideColor);
            }

            // West face (X-) - extend to hide seams
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
            // First triangle
            vertices.Add(new VertexPositionColor(basePos + v1, color));
            vertices.Add(new VertexPositionColor(basePos + v2, color));
            vertices.Add(new VertexPositionColor(basePos + v3, color));

            // Second triangle
            vertices.Add(new VertexPositionColor(basePos + v3, color));
            vertices.Add(new VertexPositionColor(basePos + v4, color));
            vertices.Add(new VertexPositionColor(basePos + v1, color));
        }
    }

    /// <summary>
    /// Contains mesh data for water in a chunk
    /// </summary>
    public class WaterMesh
    {
        public VertexPositionColor[] Vertices { get; }
        public int VertexCount => Vertices.Length;

        public WaterMesh(VertexPositionColor[] vertices)
        {
            Vertices = vertices;
        }
    }
}
