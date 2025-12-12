using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TimelessTales.World;
using TimelessTales.Blocks;

namespace TimelessTales.Rendering
{
    /// <summary>
    /// Renders the 3D voxel world with texture support
    /// </summary>
    public class WorldRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly WorldManager _worldManager;
        private readonly BasicEffect _effect;
        private readonly Dictionary<(int, int), ChunkMesh> _chunkMeshes;
        private readonly TextureAtlas _textureAtlas;
        
        // Cel shading parameters for world blocks
        private const int CEL_SHADING_BANDS = 4; // Number of discrete color bands for toon shading

        public WorldRenderer(GraphicsDevice graphicsDevice, WorldManager worldManager)
        {
            _graphicsDevice = graphicsDevice;
            _worldManager = worldManager;
            _chunkMeshes = new Dictionary<(int, int), ChunkMesh>();
            
            // Initialize texture atlas
            _textureAtlas = new TextureAtlas(graphicsDevice);
            
            // Initialize basic effect for rendering with textures
            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                TextureEnabled = true,
                Texture = _textureAtlas.Texture
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
            _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp; // Pixelated texture look
            
            // Get currently loaded chunks
            var loadedChunks = _worldManager.GetLoadedChunks().ToList();
            var loadedChunkKeys = new HashSet<(int, int)>(
                loadedChunks.Select(c => (c.ChunkX, c.ChunkZ))
            );
            
            // Remove meshes for unloaded chunks to free memory
            var meshKeysToRemove = _chunkMeshes.Keys
                .Where(key => !loadedChunkKeys.Contains(key))
                .ToList();
            
            foreach (var key in meshKeysToRemove)
            {
                _chunkMeshes.Remove(key);
            }
            
            // Build/update chunk meshes
            foreach (var chunk in loadedChunks)
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
            var vertices = new List<VertexPositionColorTexture>();
            
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
                        BlockDefinition blockDef = BlockRegistry.Get(block);
                        Color blockColor = blockDef.Color;
                        int textureIndex = blockDef.TextureIndex;
                        
                        AddBlockFaces(vertices, blockPos, blockColor, textureIndex,
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

        private void AddBlockFaces(List<VertexPositionColorTexture> vertices, Vector3 pos, Color color, int textureIndex,
                                   bool top, bool bottom, bool north, bool south, bool east, bool west)
        {
            // Apply cel shading to all face colors for toon-like appearance
            Color topColor = CelShadingUtility.ApplyCelShading(Color.Lerp(color, Color.White, 0.2f), CEL_SHADING_BANDS);
            Color bottomColor = CelShadingUtility.ApplyCelShading(Color.Lerp(color, Color.Black, 0.3f), CEL_SHADING_BANDS);
            Color sideColor = CelShadingUtility.ApplyCelShading(Color.Lerp(color, Color.Black, 0.1f), CEL_SHADING_BANDS);
            
            // Get texture coordinates for this block
            TextureCoordinates texCoords = _textureAtlas.GetTextureCoordinates(textureIndex);
            
            // Top face (Y+)
            if (top)
            {
                AddQuadWithTexture(vertices, pos,
                    new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                    new Vector3(1, 1, 1), new Vector3(0, 1, 1), topColor, texCoords);
            }
            
            // Bottom face (Y-)
            if (bottom)
            {
                AddQuadWithTexture(vertices, pos,
                    new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(1, 0, 0), new Vector3(0, 0, 0), bottomColor, texCoords);
            }
            
            // North face (Z+)
            if (north)
            {
                AddQuadWithTexture(vertices, pos,
                    new Vector3(0, 0, 1), new Vector3(0, 1, 1),
                    new Vector3(1, 1, 1), new Vector3(1, 0, 1), sideColor, texCoords);
            }
            
            // South face (Z-)
            if (south)
            {
                AddQuadWithTexture(vertices, pos,
                    new Vector3(1, 0, 0), new Vector3(1, 1, 0),
                    new Vector3(0, 1, 0), new Vector3(0, 0, 0), sideColor, texCoords);
            }
            
            // East face (X+)
            if (east)
            {
                AddQuadWithTexture(vertices, pos,
                    new Vector3(1, 0, 1), new Vector3(1, 1, 1),
                    new Vector3(1, 1, 0), new Vector3(1, 0, 0), sideColor, texCoords);
            }
            
            // West face (X-)
            if (west)
            {
                AddQuadWithTexture(vertices, pos,
                    new Vector3(0, 0, 0), new Vector3(0, 1, 0),
                    new Vector3(0, 1, 1), new Vector3(0, 0, 1), sideColor, texCoords);
            }
        }

        private void AddQuadWithTexture(List<VertexPositionColorTexture> vertices, Vector3 basePos,
                            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color color, TextureCoordinates texCoords)
        {
            // First triangle
            vertices.Add(new VertexPositionColorTexture(basePos + v1, color, texCoords.BottomLeft));
            vertices.Add(new VertexPositionColorTexture(basePos + v2, color, texCoords.TopLeft));
            vertices.Add(new VertexPositionColorTexture(basePos + v3, color, texCoords.TopRight));
            
            // Second triangle
            vertices.Add(new VertexPositionColorTexture(basePos + v3, color, texCoords.TopRight));
            vertices.Add(new VertexPositionColorTexture(basePos + v4, color, texCoords.BottomRight));
            vertices.Add(new VertexPositionColorTexture(basePos + v1, color, texCoords.BottomLeft));
        }
    }

    /// <summary>
    /// Contains mesh data for a chunk
    /// </summary>
    public class ChunkMesh
    {
        public VertexPositionColorTexture[] Vertices { get; }
        public int VertexCount => Vertices.Length;

        public ChunkMesh(VertexPositionColorTexture[] vertices)
        {
            Vertices = vertices;
        }
    }
}
