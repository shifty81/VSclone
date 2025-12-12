using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.World;
using TimelessTales.Blocks;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Renders the 3D voxel world
    /// </summary>
    public class WorldRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly WorldManager _worldManager;
        private readonly BasicEffect _effect;
        private readonly Dictionary<(int, int), ChunkMesh> _chunkMeshes;
        
        // Cel shading parameters for world blocks
        private const int CEL_SHADING_BANDS = 4; // Number of discrete color bands for toon shading

        public WorldRenderer(GraphicsDevice graphicsDevice, WorldManager worldManager)
        {
            _graphicsDevice = graphicsDevice;
            _worldManager = worldManager;
            _chunkMeshes = new Dictionary<(int, int), ChunkMesh>();
            
            // Initialize basic effect for rendering
            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        public void Draw(Camera camera, GameTime gameTime)
        {
            // Update camera matrices
            camera.Update();
            _effect.View = camera.ViewMatrix;
            _effect.Projection = camera.ProjectionMatrix;
            
            // Set render states
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            // Build/update chunk meshes
            foreach (var chunk in _worldManager.GetLoadedChunks())
            {
                var key = (chunk.ChunkX, chunk.ChunkZ);
                
                if (!_chunkMeshes.TryGetValue(key, out var mesh) || chunk.NeedsMeshRebuild)
                {
                    mesh = BuildChunkMesh(chunk);
                    _chunkMeshes[key] = mesh;
                    chunk.NeedsMeshRebuild = false;
                }
                
                // Draw chunk mesh
                if (mesh != null && mesh.VertexCount > 0)
                {
                    _effect.World = Matrix.Identity;
                    
                    foreach (var pass in _effect.CurrentTechnique.Passes)
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
        }

        private ChunkMesh BuildChunkMesh(Chunk chunk)
        {
            var vertices = new List<VertexPositionColor>();
            
            int worldX = chunk.ChunkX * Chunk.CHUNK_SIZE;
            int worldZ = chunk.ChunkZ * Chunk.CHUNK_SIZE;
            
            // Build mesh for each block
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
                {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        BlockType block = chunk.GetBlock(x, y, z);
                        if (block == BlockType.Air) continue;
                        
                        // Skip water blocks - they're rendered separately by WaterRenderer
                        if (block == BlockType.Water || block == BlockType.Saltwater) continue;
                        
                        // Check neighboring blocks to determine which faces to render
                        bool renderTop = !IsBlockOpaque(chunk, x, y + 1, z);
                        bool renderBottom = !IsBlockOpaque(chunk, x, y - 1, z);
                        bool renderNorth = !IsBlockOpaque(chunk, x, y, z + 1);
                        bool renderSouth = !IsBlockOpaque(chunk, x, y, z - 1);
                        bool renderEast = !IsBlockOpaque(chunk, x + 1, y, z);
                        bool renderWest = !IsBlockOpaque(chunk, x - 1, y, z);
                        
                        Vector3 blockPos = new Vector3(worldX + x, y, worldZ + z);
                        Color blockColor = BlockRegistry.Get(block).Color;
                        
                        AddBlockFaces(vertices, blockPos, blockColor,
                                    renderTop, renderBottom, renderNorth, renderSouth, renderEast, renderWest);
                    }
                }
            }
            
            return new ChunkMesh(vertices.ToArray());
        }

        private bool IsBlockOpaque(Chunk chunk, int x, int y, int z)
        {
            // Check bounds - treat out of bounds as transparent to see into adjacent chunks
            if (y < 0 || y >= Chunk.CHUNK_HEIGHT) return false;
            if (x < 0 || x >= Chunk.CHUNK_SIZE || z < 0 || z >= Chunk.CHUNK_SIZE) return false;
            
            BlockType block = chunk.GetBlock(x, y, z);
            return !BlockRegistry.IsTransparent(block);
        }

        private void AddBlockFaces(List<VertexPositionColor> vertices, Vector3 pos, Color color,
                                   bool top, bool bottom, bool north, bool south, bool east, bool west)
        {
            // Apply cel shading to all face colors for toon-like appearance
            Color topColor = ApplyCelShading(Color.Lerp(color, Color.White, 0.2f));
            Color bottomColor = ApplyCelShading(Color.Lerp(color, Color.Black, 0.3f));
            Color sideColor = ApplyCelShading(Color.Lerp(color, Color.Black, 0.1f));
            
            // Top face (Y+)
            if (top)
            {
                AddQuad(vertices, pos,
                    new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                    new Vector3(1, 1, 1), new Vector3(0, 1, 1), topColor);
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
                    new Vector3(1, 1, 0), new Vector3(1, 0, 0), color);
            }
            
            // West face (X-)
            if (west)
            {
                AddQuad(vertices, pos,
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0),
                    new Vector3(0, 1, 1), new Vector3(0, 0, 1), color);
            }
        }
        
        /// <summary>
        /// Applies cel shading to a color by quantizing the RGB values into discrete bands
        /// </summary>
        private Color ApplyCelShading(Color color)
        {
            // Quantize each color channel to create cel shading effect
            int r = QuantizeColorChannel(color.R, CEL_SHADING_BANDS);
            int g = QuantizeColorChannel(color.G, CEL_SHADING_BANDS);
            int b = QuantizeColorChannel(color.B, CEL_SHADING_BANDS);
            
            return new Color(r, g, b, color.A);
        }
        
        /// <summary>
        /// Quantizes a color channel (0-255) into discrete bands for cel shading
        /// </summary>
        private int QuantizeColorChannel(int value, int bands)
        {
            float normalized = value / 255.0f;
            float bandSize = 1.0f / bands;
            float bandIndex = MathF.Floor(normalized / bandSize);
            
            // Return the center of the band
            float quantized = (bandIndex + 0.5f) * bandSize;
            return (int)MathHelper.Clamp(quantized * 255, 0, 255);
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
    /// Contains mesh data for a chunk
    /// </summary>
    public class ChunkMesh
    {
        public VertexPositionColor[] Vertices { get; }
        public int VertexCount => Vertices.Length;

        public ChunkMesh(VertexPositionColor[] vertices)
        {
            Vertices = vertices;
        }
    }
}
