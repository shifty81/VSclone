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
        
        // Water physics parameters
        private const float BUOYANT_FORCE = 15.0f; // Upward force when underwater
        private const float WATER_DRAG = 0.9f; // Resistance in water (0-1, lower = more drag)
        private const float WATER_GRAVITY_MULTIPLIER = 0.3f; // Reduced gravity in water
        private const int SEA_LEVEL = 64;
        
        // Block interaction
        private const float REACH_DISTANCE = 5.0f;
        private const float BREAK_TIME = 1.0f;
        
        private bool _isOnGround;
        private float _breakProgress;
        private Vector3? _targetBlockPos;
        
        // Water state
        private bool _isInWater;
        private float _submersionDepth; // 0 = not in water, 1 = fully submerged
        
        // Inventory and equipment
        public Inventory Inventory { get; private set; }
        public Equipment Equipment { get; private set; }
        public BlockType SelectedBlock { get; set; }
        
        // Character skeleton and animation
        public Skeleton Skeleton { get; private set; }
        public AnimationController AnimationController { get; private set; }

        public Player(Vector3 startPosition)
        {
            Position = startPosition;
            Rotation = Vector2.Zero;
            Velocity = Vector3.Zero;
            Inventory = new Inventory(40);
            Equipment = new Equipment();
            SelectedBlock = BlockType.Stone;
            
            // Initialize skeleton
            Skeleton = new Skeleton();
            InitializeSkeleton();
            
            // Initialize animation controller
            AnimationController = new AnimationController(Skeleton);
            
            // Start with some basic blocks
            Inventory.AddItem(BlockType.Stone, 64);
            Inventory.AddItem(BlockType.Dirt, 64);
            Inventory.AddItem(BlockType.Planks, 64);
            Inventory.AddItem(BlockType.Cobblestone, 32);
            Inventory.AddItem(BlockType.Wood, 32);
            Inventory.AddItem(BlockType.Grass, 32);
            Inventory.AddItem(BlockType.Sand, 32);
            Inventory.AddItem(BlockType.Gravel, 32);
            Inventory.AddItem(BlockType.Clay, 32);
        }
        
        private void InitializeSkeleton()
        {
            // Root bone (center of character)
            Bone root = Skeleton.AddBone("root", Vector3.Zero);
            
            // Torso (body)
            Bone torso = Skeleton.AddBone("torso", new Vector3(0, 0.9f, 0), root);
            
            // Head
            Bone head = Skeleton.AddBone("head", new Vector3(0, 0.6f, 0), torso);
            
            // Arms (attached to upper torso)
            Bone rightArm = Skeleton.AddBone("right_arm", new Vector3(0.3f, 0.4f, 0), torso);
            Bone leftArm = Skeleton.AddBone("left_arm", new Vector3(-0.3f, 0.4f, 0), torso);
            
            // Legs (attached to lower torso)
            Bone rightLeg = Skeleton.AddBone("right_leg", new Vector3(0.15f, -0.2f, 0), root);
            Bone leftLeg = Skeleton.AddBone("left_leg", new Vector3(-0.15f, -0.2f, 0), root);
        }

        public void Update(GameTime gameTime, InputManager input, WorldManager world)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Check if player is in water
            UpdateWaterState(world);
            
            // Update camera rotation
            UpdateRotation(input);
            
            // Check if player is moving for animation
            bool isMoving = input.IsKeyDown(Keys.W) || input.IsKeyDown(Keys.A) || 
                           input.IsKeyDown(Keys.S) || input.IsKeyDown(Keys.D);
            bool isSprinting = input.IsKeyDown(Keys.LeftShift);
            
            // Update movement
            UpdateMovement(input, deltaTime);
            
            // Apply physics (includes buoyancy if in water)
            ApplyPhysics(world, deltaTime);
            
            // Handle block interaction
            bool isBreaking = input.IsLeftMouseDown() && _targetBlockPos.HasValue;
            HandleBlockInteraction(input, world, deltaTime);
            
            // Handle hotbar
            UpdateHotbar(input);
            
            // Determine if player is swimming (moving in water)
            bool isSwimming = _isInWater && isMoving;
            
            // Update animations
            AnimationController.Update(deltaTime, isMoving, isSprinting, isBreaking, _breakProgress, _isInWater, isSwimming);
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
        
        private void UpdateWaterState(WorldManager world)
        {
            // Check multiple sample points in player's body to determine water submersion
            int samples = 0;
            int waterSamples = 0;
            
            // Sample at feet, waist, chest, and head level
            float[] sampleHeights = { 0.0f, 0.5f, 1.0f, 1.5f };
            
            foreach (float heightOffset in sampleHeights)
            {
                Vector3 samplePos = Position + new Vector3(0, heightOffset, 0);
                int x = (int)MathF.Floor(samplePos.X);
                int y = (int)MathF.Floor(samplePos.Y);
                int z = (int)MathF.Floor(samplePos.Z);
                
                BlockType block = world.GetBlock(x, y, z);
                samples++;
                
                if (block == BlockType.Water || block == BlockType.Saltwater)
                {
                    waterSamples++;
                }
            }
            
            // Calculate submersion depth (0 = not in water, 1 = fully submerged)
            _submersionDepth = waterSamples / (float)samples;
            _isInWater = _submersionDepth > 0;
        }

        private void UpdateMovement(InputManager input, float deltaTime)
        {
            Vector3 moveDirection = Vector3.Zero;
            
            // WASD movement - relative to player's facing direction (yaw only, not pitch)
            // W = forward in facing direction, S = backward from facing direction
            // A = left relative to facing direction, D = right relative to facing direction
            if (input.IsKeyDown(Keys.W))
                moveDirection += new Vector3(0, 0, -1); // Forward in local space
            if (input.IsKeyDown(Keys.S))
                moveDirection += new Vector3(0, 0, 1);  // Backward in local space
            if (input.IsKeyDown(Keys.A))
                moveDirection += new Vector3(-1, 0, 0); // Left in local space
            if (input.IsKeyDown(Keys.D))
                moveDirection += new Vector3(1, 0, 0);  // Right in local space
            
            // Normalize diagonal movement before rotation
            if (moveDirection.LengthSquared() > 0)
                moveDirection.Normalize();
            
            // Transform movement direction by player's yaw rotation (horizontal rotation only)
            // This makes WASD movement relative to where the player is looking
            Matrix yawRotation = Matrix.CreateRotationY(Rotation.Y);
            moveDirection = Vector3.Transform(moveDirection, yawRotation);
            
            // Sprint
            float speed = MOVE_SPEED;
            if (input.IsKeyDown(Keys.LeftShift))
                speed *= SPRINT_MULTIPLIER;
            
            // Reduce speed in water
            if (_isInWater)
                speed *= (0.5f + 0.5f * (1.0f - _submersionDepth)); // 50-100% speed based on submersion
            
            // Apply horizontal movement (maintain Y velocity for jumping)
            Velocity = new Vector3(
                moveDirection.X * speed,
                Velocity.Y,
                moveDirection.Z * speed
            );
            
            // Jump - can jump while moving, maintains horizontal momentum
            // In water, space makes you swim upward
            if (input.IsKeyDown(Keys.Space))
            {
                if (_isInWater)
                {
                    // Swimming upward - only when space is pressed
                    Velocity = new Vector3(Velocity.X, 3.0f, Velocity.Z);
                }
                else if (_isOnGround)
                {
                    // Normal jump on ground
                    Velocity = new Vector3(Velocity.X, JUMP_FORCE, Velocity.Z);
                    _isOnGround = false;
                }
            }
            
            // Crouch/dive in water - make player sink
            if (input.IsKeyDown(Keys.LeftControl) && _isInWater)
            {
                Velocity = new Vector3(Velocity.X, -3.0f, Velocity.Z);
            }
        }

        private void ApplyPhysics(WorldManager world, float deltaTime)
        {
            // Apply gravity (reduced in water)
            if (!_isOnGround)
            {
                float gravityMultiplier = _isInWater ? WATER_GRAVITY_MULTIPLIER : 1.0f;
                Velocity = new Vector3(Velocity.X, Velocity.Y - GRAVITY * gravityMultiplier * deltaTime, Velocity.Z);
            }
            
            // Apply buoyancy force when in water (keeps player floating at surface)
            if (_isInWater && _submersionDepth > 0.5f) // Only apply when mostly submerged
            {
                // Buoyancy force proportional to submersion depth
                // This creates a natural floating effect - player bobs at water surface
                float buoyancyForce = BUOYANT_FORCE * _submersionDepth * deltaTime;
                Velocity = new Vector3(Velocity.X, Velocity.Y + buoyancyForce, Velocity.Z);
                
                // Apply water drag to vertical velocity (prevents excessive bobbing)
                Velocity = new Vector3(Velocity.X, Velocity.Y * WATER_DRAG, Velocity.Z);
            }
            
            // Apply water drag to horizontal movement
            if (_isInWater)
            {
                Velocity = new Vector3(Velocity.X * WATER_DRAG, Velocity.Y, Velocity.Z * WATER_DRAG);
            }
            
            // Apply velocity
            Vector3 newPosition = Position + Velocity * deltaTime;
            
            // Collision detection (simple AABB)
            newPosition = ResolveCollisions(world, newPosition);
            
            Position = newPosition;
        }

        private Vector3 ResolveCollisions(WorldManager world, Vector3 targetPos)
        {
            // Resolve collisions axis by axis: X, Y, Z
            // This prevents issues where resolving one collision creates another
            Vector3 result = Position; // Start from current position
            
            // Reset ground state
            _isOnGround = false;
            
            // Try to move on X axis
            result.X = targetPos.X;
            if (CheckCollision(world, result))
            {
                result.X = Position.X; // Revert X movement if collision
            }
            
            // Try to move on Y axis
            result.Y = targetPos.Y;
            if (CheckCollision(world, result))
            {
                // Find the correct Y position
                if (Velocity.Y < 0)
                {
                    // Falling - find the highest solid block below player across their entire width
                    int minBlockY = (int)MathF.Floor(result.Y);
                    int maxBlockY = (int)MathF.Ceiling(result.Y + PLAYER_HEIGHT);
                    
                    // Check all blocks the player overlaps horizontally
                    int minX = (int)MathF.Floor(result.X - PLAYER_WIDTH / 2);
                    int maxX = (int)MathF.Ceiling(result.X + PLAYER_WIDTH / 2);
                    int minZ = (int)MathF.Floor(result.Z - PLAYER_WIDTH / 2);
                    int maxZ = (int)MathF.Ceiling(result.Z + PLAYER_WIDTH / 2);
                    
                    // Find highest solid block (sentinel value indicates no block found)
                    const int NO_BLOCK_FOUND = -1;
                    int highestBlockY = NO_BLOCK_FOUND;
                    for (int x = minX; x < maxX; x++)
                    {
                        for (int z = minZ; z < maxZ; z++)
                        {
                            for (int y = maxBlockY - 1; y >= minBlockY; y--)
                            {
                                if (world.IsBlockSolid(x, y, z))
                                {
                                    highestBlockY = highestBlockY == NO_BLOCK_FOUND ? y : Math.Max(highestBlockY, y);
                                    break; // Found highest in this column, move to next column
                                }
                            }
                        }
                    }
                    
                    if (highestBlockY != NO_BLOCK_FOUND)
                    {
                        result.Y = highestBlockY + 1; // Stand on top of highest block
                        _isOnGround = true;
                    }
                }
                else
                {
                    // Rising - hit ceiling, find the lowest ceiling block
                    int minX = (int)MathF.Floor(result.X - PLAYER_WIDTH / 2);
                    int maxX = (int)MathF.Ceiling(result.X + PLAYER_WIDTH / 2);
                    int minZ = (int)MathF.Floor(result.Z - PLAYER_WIDTH / 2);
                    int maxZ = (int)MathF.Ceiling(result.Z + PLAYER_WIDTH / 2);
                    int headY = (int)MathF.Ceiling(result.Y + PLAYER_HEIGHT);
                    
                    // Find the lowest Y coordinate of ceiling blocks (sentinel value indicates no ceiling)
                    const int NO_CEILING_FOUND = int.MaxValue;
                    int lowestCeilingY = NO_CEILING_FOUND;
                    for (int x = minX; x < maxX; x++)
                    {
                        for (int z = minZ; z < maxZ; z++)
                        {
                            // Check a few blocks above head for ceiling
                            for (int y = headY; y <= headY + 1; y++)
                            {
                                if (world.IsBlockSolid(x, y, z))
                                {
                                    lowestCeilingY = Math.Min(lowestCeilingY, y);
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (lowestCeilingY != NO_CEILING_FOUND)
                    {
                        result.Y = lowestCeilingY - PLAYER_HEIGHT;
                    }
                }
                Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
            }
            
            // Try to move on Z axis
            result.Z = targetPos.Z;
            if (CheckCollision(world, result))
            {
                result.Z = Position.Z; // Revert Z movement if collision
            }
            
            return result;
        }
        
        private bool CheckCollision(WorldManager world, Vector3 pos)
        {
            // Check blocks around player's bounding box at given position
            int minX = (int)MathF.Floor(pos.X - PLAYER_WIDTH / 2);
            int maxX = (int)MathF.Ceiling(pos.X + PLAYER_WIDTH / 2);
            int minY = (int)MathF.Floor(pos.Y);
            int maxY = (int)MathF.Ceiling(pos.Y + PLAYER_HEIGHT);
            int minZ = (int)MathF.Floor(pos.Z - PLAYER_WIDTH / 2);
            int maxZ = (int)MathF.Ceiling(pos.Z + PLAYER_WIDTH / 2);
            
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (world.IsBlockSolid(x, y, z))
                        {
                            // Get block bounds
                            float blockMinX = x;
                            float blockMaxX = x + 1;
                            float blockMinY = y;
                            float blockMaxY = y + 1;
                            float blockMinZ = z;
                            float blockMaxZ = z + 1;
                            
                            // Get player bounds
                            float playerMinX = pos.X - PLAYER_WIDTH / 2;
                            float playerMaxX = pos.X + PLAYER_WIDTH / 2;
                            float playerMinY = pos.Y;
                            float playerMaxY = pos.Y + PLAYER_HEIGHT;
                            float playerMinZ = pos.Z - PLAYER_WIDTH / 2;
                            float playerMaxZ = pos.Z + PLAYER_WIDTH / 2;
                            
                            // Check if player AABB intersects with block AABB
                            bool collisionX = playerMaxX > blockMinX && playerMinX < blockMaxX;
                            bool collisionY = playerMaxY > blockMinY && playerMinY < blockMaxY;
                            bool collisionZ = playerMaxZ > blockMinZ && playerMinZ < blockMaxZ;
                            
                            if (collisionX && collisionY && collisionZ)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            
            return false;
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
            Vector3 rayStart = Position + new Vector3(0, PLAYER_EYE_HEIGHT, 0);
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
            // Number keys to select blocks (1-9 for hotbar slots)
            if (input.IsKeyPressed(Keys.D1)) SelectedBlock = BlockType.Stone;
            if (input.IsKeyPressed(Keys.D2)) SelectedBlock = BlockType.Dirt;
            if (input.IsKeyPressed(Keys.D3)) SelectedBlock = BlockType.Planks;
            if (input.IsKeyPressed(Keys.D4)) SelectedBlock = BlockType.Cobblestone;
            if (input.IsKeyPressed(Keys.D5)) SelectedBlock = BlockType.Wood;
            if (input.IsKeyPressed(Keys.D6)) SelectedBlock = BlockType.Grass;
            if (input.IsKeyPressed(Keys.D7)) SelectedBlock = BlockType.Sand;
            if (input.IsKeyPressed(Keys.D8)) SelectedBlock = BlockType.Gravel;
            if (input.IsKeyPressed(Keys.D9)) SelectedBlock = BlockType.Clay;
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

    /// <summary>
    /// Equipment slot types
    /// </summary>
    public enum EquipmentSlot
    {
        Head,
        Chest,
        Legs,
        Feet,
        Hands,
        Back,  // For backpacks
        MainHand,
        OffHand
    }

    /// <summary>
    /// Player equipment system
    /// </summary>
    public class Equipment
    {
        private readonly Dictionary<EquipmentSlot, BlockType?> _equippedItems;

        public Equipment()
        {
            _equippedItems = new Dictionary<EquipmentSlot, BlockType?>();
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                _equippedItems[slot] = null;
            }
        }

        public void Equip(EquipmentSlot slot, BlockType item)
        {
            _equippedItems[slot] = item;
        }

        public BlockType? Unequip(EquipmentSlot slot)
        {
            BlockType? item = _equippedItems[slot];
            _equippedItems[slot] = null;
            return item;
        }

        public BlockType? GetEquipped(EquipmentSlot slot)
        {
            return _equippedItems.TryGetValue(slot, out var item) ? item : null;
        }

        public bool IsSlotEmpty(EquipmentSlot slot)
        {
            return _equippedItems[slot] == null;
        }
    }
}
