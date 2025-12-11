using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TimelessTales.Core;
using TimelessTales.World;
using TimelessTales.Blocks;

namespace TimelessTales.Entities
{
    /// <summary>
    /// Represents the player character with movement, camera, and block interaction
    /// </summary>
    public class Player
    {
        public Vector3 Position { get; set; }
        public Vector2 Rotation { get; set; } // X = pitch, Y = yaw
        public Vector3 Velocity { get; private set; }
        
        // Player dimensions
        private const float PLAYER_HEIGHT = 1.8f;
        private const float PLAYER_WIDTH = 0.6f;
        private const float PLAYER_EYE_HEIGHT = 1.62f;
        
        // Movement parameters
        private const float MOVE_SPEED = 4.5f;
        private const float SPRINT_MULTIPLIER = 1.5f;
        private const float JUMP_FORCE = 8.0f;
        private const float GRAVITY = 20.0f;
        private const float MOUSE_SENSITIVITY = 0.003f;
        
        // Block interaction
        private const float REACH_DISTANCE = 5.0f;
        private const float BREAK_TIME = 1.0f;
        
        private bool _isOnGround;
        private float _breakProgress;
        private Vector3? _targetBlockPos;
        
        // Inventory
        public Inventory Inventory { get; private set; }
        public BlockType SelectedBlock { get; set; }

        public Player(Vector3 startPosition)
        {
            Position = startPosition;
            Rotation = Vector2.Zero;
            Velocity = Vector3.Zero;
            Inventory = new Inventory(40);
            SelectedBlock = BlockType.Stone;
            
            // Start with some basic blocks
            Inventory.AddItem(BlockType.Stone, 64);
            Inventory.AddItem(BlockType.Dirt, 64);
            Inventory.AddItem(BlockType.Planks, 64);
        }

        public void Update(GameTime gameTime, InputManager input, WorldManager world)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update camera rotation
            UpdateRotation(input);
            
            // Update movement
            UpdateMovement(input, deltaTime);
            
            // Apply physics
            ApplyPhysics(world, deltaTime);
            
            // Handle block interaction
            HandleBlockInteraction(input, world, deltaTime);
            
            // Handle hotbar
            UpdateHotbar(input);
        }

        private void UpdateRotation(InputManager input)
        {
            // Mouse look
            int deltaX = input.GetMouseDeltaX();
            int deltaY = input.GetMouseDeltaY();
            
            Rotation = new Vector2(
                MathHelper.Clamp(Rotation.X - deltaY * MOUSE_SENSITIVITY, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f),
                Rotation.Y - deltaX * MOUSE_SENSITIVITY
            );
        }

        private void UpdateMovement(InputManager input, float deltaTime)
        {
            Vector3 moveDirection = Vector3.Zero;
            
            // WASD movement
            if (input.IsKeyDown(Keys.W))
                moveDirection += new Vector3(MathF.Sin(Rotation.Y), 0, -MathF.Cos(Rotation.Y));
            if (input.IsKeyDown(Keys.S))
                moveDirection -= new Vector3(MathF.Sin(Rotation.Y), 0, -MathF.Cos(Rotation.Y));
            if (input.IsKeyDown(Keys.A))
                moveDirection -= new Vector3(MathF.Cos(Rotation.Y), 0, -MathF.Sin(Rotation.Y));
            if (input.IsKeyDown(Keys.D))
                moveDirection += new Vector3(MathF.Cos(Rotation.Y), 0, -MathF.Sin(Rotation.Y));
            
            // Normalize diagonal movement
            if (moveDirection.LengthSquared() > 0)
                moveDirection.Normalize();
            
            // Sprint
            float speed = MOVE_SPEED;
            if (input.IsKeyDown(Keys.LeftShift))
                speed *= SPRINT_MULTIPLIER;
            
            // Apply horizontal movement
            Velocity = new Vector3(
                moveDirection.X * speed,
                Velocity.Y,
                moveDirection.Z * speed
            );
            
            // Jump
            if (input.IsKeyPressed(Keys.Space) && _isOnGround)
            {
                Velocity = new Vector3(Velocity.X, JUMP_FORCE, Velocity.Z);
                _isOnGround = false;
            }
        }

        private void ApplyPhysics(WorldManager world, float deltaTime)
        {
            // Apply gravity
            if (!_isOnGround)
            {
                Velocity = new Vector3(Velocity.X, Velocity.Y - GRAVITY * deltaTime, Velocity.Z);
            }
            
            // Apply velocity
            Vector3 newPosition = Position + Velocity * deltaTime;
            
            // Collision detection (simple AABB)
            newPosition = ResolveCollisions(world, newPosition);
            
            Position = newPosition;
        }

        private Vector3 ResolveCollisions(WorldManager world, Vector3 targetPos)
        {
            Vector3 result = targetPos;
            
            // Check ground collision
            _isOnGround = false;
            
            // Check blocks around player's bounding box
            int minX = (int)MathF.Floor(result.X - PLAYER_WIDTH / 2);
            int maxX = (int)MathF.Ceiling(result.X + PLAYER_WIDTH / 2);
            int minY = (int)MathF.Floor(result.Y);
            int maxY = (int)MathF.Ceiling(result.Y + PLAYER_HEIGHT);
            int minZ = (int)MathF.Floor(result.Z - PLAYER_WIDTH / 2);
            int maxZ = (int)MathF.Ceiling(result.Z + PLAYER_WIDTH / 2);
            
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (world.IsBlockSolid(x, y, z))
                        {
                            // Simple collision response
                            if (result.Y < y + 1 && result.Y > y)
                            {
                                result.Y = y + 1;
                                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                                _isOnGround = true;
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        private void HandleBlockInteraction(InputManager input, WorldManager world, float deltaTime)
        {
            // Raycast to find target block
            var raycastResult = RaycastBlock(world);
            
            if (raycastResult.HasValue)
            {
                _targetBlockPos = raycastResult.Value.blockPos;
                
                // Left click - break block
                if (input.IsLeftMouseDown())
                {
                    _breakProgress += deltaTime / BREAK_TIME;
                    
                    if (_breakProgress >= 1.0f)
                    {
                        // Break block
                        int x = (int)_targetBlockPos.Value.X;
                        int y = (int)_targetBlockPos.Value.Y;
                        int z = (int)_targetBlockPos.Value.Z;
                        
                        BlockType brokenBlock = world.GetBlock(x, y, z);
                        world.SetBlock(x, y, z, BlockType.Air);
                        
                        // Add to inventory
                        Inventory.AddItem(brokenBlock, 1);
                        
                        _breakProgress = 0;
                    }
                }
                else
                {
                    _breakProgress = 0;
                }
                
                // Right click - place block
                if (input.IsRightMousePressed() && raycastResult.Value.placePos.HasValue)
                {
                    Vector3 placePos = raycastResult.Value.placePos.Value;
                    int x = (int)placePos.X;
                    int y = (int)placePos.Y;
                    int z = (int)placePos.Z;
                    
                    // Check if player is not in the way
                    if (!IsPlayerAt(placePos))
                    {
                        if (Inventory.RemoveItem(SelectedBlock, 1))
                        {
                            world.SetBlock(x, y, z, SelectedBlock);
                        }
                    }
                }
            }
            else
            {
                _targetBlockPos = null;
                _breakProgress = 0;
            }
        }

        private (Vector3 blockPos, Vector3? placePos)? RaycastBlock(WorldManager world)
        {
            // Cast ray from player's eye position
            Vector3 rayStart = Position + new Vector3(0, PLAYER_EYE_HEIGHT - PLAYER_HEIGHT, 0);
            Vector3 rayDir = GetLookDirection();
            
            // TODO: Consider using DDA algorithm for more efficient raycasting
            // Current implementation uses fixed 0.1f steps which may be inefficient
            // Step through ray
            for (float t = 0; t < REACH_DISTANCE; t += 0.1f)
            {
                Vector3 pos = rayStart + rayDir * t;
                int x = (int)MathF.Floor(pos.X);
                int y = (int)MathF.Floor(pos.Y);
                int z = (int)MathF.Floor(pos.Z);
                
                if (world.IsBlockSolid(x, y, z))
                {
                    // Found a block - calculate place position
                    Vector3 prevPos = rayStart + rayDir * (t - 0.1f);
                    int px = (int)MathF.Floor(prevPos.X);
                    int py = (int)MathF.Floor(prevPos.Y);
                    int pz = (int)MathF.Floor(prevPos.Z);
                    
                    return (new Vector3(x, y, z), new Vector3(px, py, pz));
                }
            }
            
            return null;
        }

        private Vector3 GetLookDirection()
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y);
            return Vector3.Transform(Vector3.Forward, rotationMatrix);
        }

        private bool IsPlayerAt(Vector3 blockPos)
        {
            // Check if block would intersect with player
            float dist = Vector3.Distance(Position, blockPos + new Vector3(0.5f, 0.5f, 0.5f));
            return dist < PLAYER_WIDTH;
        }

        private void UpdateHotbar(InputManager input)
        {
            // Number keys to select blocks
            if (input.IsKeyPressed(Keys.D1)) SelectedBlock = BlockType.Stone;
            if (input.IsKeyPressed(Keys.D2)) SelectedBlock = BlockType.Dirt;
            if (input.IsKeyPressed(Keys.D3)) SelectedBlock = BlockType.Planks;
            if (input.IsKeyPressed(Keys.D4)) SelectedBlock = BlockType.Cobblestone;
            if (input.IsKeyPressed(Keys.D5)) SelectedBlock = BlockType.Wood;
        }

        public Vector3? GetTargetBlockPos() => _targetBlockPos;
        public float GetBreakProgress() => _breakProgress;
    }

    /// <summary>
    /// Player inventory system
    /// </summary>
    public class Inventory
    {
        private readonly Dictionary<BlockType, int> _items;
        private readonly int _maxSlots;

        public Inventory(int maxSlots)
        {
            _maxSlots = maxSlots;
            _items = new Dictionary<BlockType, int>();
        }

        public bool AddItem(BlockType type, int amount)
        {
            if (type == BlockType.Air) return false;
            
            if (_items.ContainsKey(type))
                _items[type] += amount;
            else
                _items[type] = amount;
            
            return true;
        }

        public bool RemoveItem(BlockType type, int amount)
        {
            if (!_items.ContainsKey(type) || _items[type] < amount)
                return false;
            
            _items[type] -= amount;
            if (_items[type] <= 0)
                _items.Remove(type);
            
            return true;
        }

        public int GetItemCount(BlockType type)
        {
            return _items.TryGetValue(type, out int count) ? count : 0;
        }

        public Dictionary<BlockType, int> GetAllItems() => new Dictionary<BlockType, int>(_items);
    }
}
