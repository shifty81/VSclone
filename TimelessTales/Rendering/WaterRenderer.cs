using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.World;
using TimelessTales.Blocks;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Specialized renderer for translucent water blocks with wave animation and depth-based coloring
    /// </summary>
    public class WaterRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly WorldManager _worldManager;
        private readonly BasicEffect _waterEffect;
        private readonly Dictionary<(int, int), WaterMesh> _waterMeshes;
        private float _time;

        // Water visual parameters
        private const int SEA_LEVEL = 64;
        private const float WAVE_SPEED = 0.3f;
        private const float WAVE_HEIGHT = 0.05f;
        private const float MAX_DEPTH_FOR_COLOR_CALCULATION = 20.0f;
        
        // Cel shading parameters
        private const int CEL_SHADING_BANDS = 4; // Number of discrete color bands

        public WaterRenderer(GraphicsDevice graphicsDevice, WorldManager worldManager)
        {
            _graphicsDevice = graphicsDevice;
            _worldManager = worldManager;
            _waterMeshes = new Dictionary<(int, int), WaterMesh>();
            _time = 0;

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

            // Build/update water meshes
            foreach (var chunk in _worldManager.GetLoadedChunks())
            {
                var key = (chunk.ChunkX, chunk.ChunkZ);

                if (!_waterMeshes.TryGetValue(key, out var mesh) || chunk.NeedsMeshRebuild)
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

            // Restore default render states
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        private WaterMesh BuildWaterMesh(Chunk chunk)
        {
            var vertices = new List<VertexPositionColor>();

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
            // Simple sine wave pattern for water surface animation
            float wave1 = MathF.Sin(worldX * 0.3f + _time) * WAVE_HEIGHT;
            float wave2 = MathF.Sin(worldZ * 0.4f + _time * 1.2f) * WAVE_HEIGHT;
            return wave1 + wave2;
        }

        private void AddWaterFaces(List<VertexPositionColor> vertices, Vector3 pos, Color color, float waveOffset,
                                   bool top, bool bottom, bool north, bool south, bool east, bool west)
        {
            // Apply cel shading to edge colors to create toon-like appearance
            Color topColor = CelShadingUtility.ApplyCelShading(Color.Lerp(color, Color.White, 0.3f), CEL_SHADING_BANDS); // Lighter top
            Color bottomColor = CelShadingUtility.ApplyCelShading(Color.Lerp(color, Color.Black, 0.2f), CEL_SHADING_BANDS);
            Color sideColor = CelShadingUtility.ApplyCelShading(color, CEL_SHADING_BANDS);
            
            // Top face (Y+) - with wave animation
            // Extend slightly beyond block boundary to hide seams
            const float OVERLAP = 0.001f;
            if (top)
            {
                AddQuad(vertices, pos,
                    new Vector3(-OVERLAP, 1 + waveOffset, -OVERLAP), 
                    new Vector3(1 + OVERLAP, 1 + waveOffset, -OVERLAP),
                    new Vector3(1 + OVERLAP, 1 + waveOffset, 1 + OVERLAP), 
                    new Vector3(-OVERLAP, 1 + waveOffset, 1 + OVERLAP), topColor);
            }

            // Bottom face (Y-)
            if (bottom)
            {
                AddQuad(vertices, pos,
                    new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(1, 0, 0), new Vector3(0, 0, 0), bottomColor);
            }

            // North face (Z+)
            if (north)
            {
                AddQuad(vertices, pos,
                    new Vector3(0, 0, 1), new Vector3(0, 1, 1),
                    new Vector3(1, 1, 1), new Vector3(1, 0, 1), sideColor);
            }

            // South face (Z-)
            if (south)
            {
                AddQuad(vertices, pos,
                    new Vector3(1, 0, 0), new Vector3(1, 1, 0),
                    new Vector3(0, 1, 0), new Vector3(0, 0, 0), sideColor);
            }

            // East face (X+)
            if (east)
            {
                AddQuad(vertices, pos,
                    new Vector3(1, 0, 1), new Vector3(1, 1, 1),
                    new Vector3(1, 1, 0), new Vector3(1, 0, 0), sideColor);
            }

            // West face (X-)
            if (west)
            {
                AddQuad(vertices, pos,
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0),
                    new Vector3(0, 1, 1), new Vector3(0, 0, 1), sideColor);
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
