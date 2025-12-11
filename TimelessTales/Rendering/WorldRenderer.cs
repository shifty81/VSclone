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
            // Check bounds
            if (y < 0 || y >= Chunk.CHUNK_HEIGHT) return false;
            if (x < 0 || x >= Chunk.CHUNK_SIZE || z < 0 || z >= Chunk.CHUNK_SIZE) return true;
            
            BlockType block = chunk.GetBlock(x, y, z);
            return !BlockRegistry.IsTransparent(block);
        }

        private void AddBlockFaces(List<VertexPositionColor> vertices, Vector3 pos, Color color,
                                   bool top, bool bottom, bool north, bool south, bool east, bool west)
        {
            // Top face (Y+)
            if (top)
            {
                Color topColor = Color.Lerp(color, Color.White, 0.2f);
                AddQuad(vertices, pos,
                    new Vector3(0, 1, 0), new Vector3(1, 1, 0),
                    new Vector3(1, 1, 1), new Vector3(0, 1, 1), topColor);
            }
            
            // Bottom face (Y-)
            if (bottom)
            {
                Color bottomColor = Color.Lerp(color, Color.Black, 0.3f);
                AddQuad(vertices, pos,
                    new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(1, 0, 0), new Vector3(0, 0, 0), bottomColor);
            }
            
            // North face (Z+)
            if (north)
            {
                Color sideColor = Color.Lerp(color, Color.Black, 0.1f);
                AddQuad(vertices, pos,
                    new Vector3(0, 0, 1), new Vector3(0, 1, 1),
                    new Vector3(1, 1, 1), new Vector3(1, 0, 1), sideColor);
            }
            
            // South face (Z-)
            if (south)
            {
                Color sideColor = Color.Lerp(color, Color.Black, 0.1f);
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
