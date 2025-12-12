using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using Xunit;

namespace TimelessTales.Tests
{
    public class PlayerMovementTests
    {
        [Fact]
        public void Player_ForwardKey_MovesInFacingDirection_WhenFacingNorth()
        {
            // Movement is now relative to player's facing direction (yaw)
            // When facing north (yaw = 0), W should move in -Z direction
            
            // Simulate the transformation with yaw = 0
            Vector3 localForward = new Vector3(0, 0, -1);
            float yaw = 0f; // Facing north
            Matrix yawRotation = Matrix.CreateRotationY(yaw);
            Vector3 worldDirection = Vector3.Transform(localForward, yawRotation);
            
            // Assert - When facing north, W moves in -Z (forward/north)
            Assert.Equal(0f, worldDirection.X, 5); // precision 5 decimal places
            Assert.Equal(0f, worldDirection.Y, 5);
            Assert.Equal(-1f, worldDirection.Z, 5);
        }
        
        [Fact]
        public void Player_ForwardKey_MovesInFacingDirection_WhenFacingEast()
        {
            // When facing east (yaw = -90 degrees), W should move in +X direction
            
            Vector3 localForward = new Vector3(0, 0, -1);
            float yaw = -MathHelper.PiOver2; // Facing east (turned right from north)
            Matrix yawRotation = Matrix.CreateRotationY(yaw);
            Vector3 worldDirection = Vector3.Transform(localForward, yawRotation);
            
            // Assert - When facing east, W moves in +X (east)
            Assert.Equal(1f, worldDirection.X, 5);
            Assert.Equal(0f, worldDirection.Y, 5);
            Assert.Equal(0f, worldDirection.Z, 5);
        }
        
        [Fact]
        public void Player_ForwardKey_MovesInFacingDirection_WhenFacingSouth()
        {
            // When facing south (yaw = 180 degrees), W should move in +Z direction
            
            Vector3 localForward = new Vector3(0, 0, -1);
            float yaw = MathHelper.Pi; // Facing south (180 degrees)
            Matrix yawRotation = Matrix.CreateRotationY(yaw);
            Vector3 worldDirection = Vector3.Transform(localForward, yawRotation);
            
            // Assert - When facing south, W moves in +Z (south)
            Assert.Equal(0f, worldDirection.X, 5);
            Assert.Equal(0f, worldDirection.Y, 5);
            Assert.Equal(1f, worldDirection.Z, 5);
        }
        
        [Fact]
        public void Player_ForwardKey_MovesInFacingDirection_WhenFacingWest()
        {
            // When facing west (yaw = 90 degrees), W should move in -X direction
            
            Vector3 localForward = new Vector3(0, 0, -1);
            float yaw = MathHelper.PiOver2; // Facing west (turned left from north)
            Matrix yawRotation = Matrix.CreateRotationY(yaw);
            Vector3 worldDirection = Vector3.Transform(localForward, yawRotation);
            
            // Assert - When facing west, W moves in -X (west)
            Assert.Equal(-1f, worldDirection.X, 5);
            Assert.Equal(0f, worldDirection.Y, 5);
            Assert.Equal(0f, worldDirection.Z, 5);
        }
        
        [Fact]
        public void Player_RightKey_MovesRightRelativeToFacing_WhenFacingNorth()
        {
            // When facing north (yaw = 0), D should move in +X direction (right)
            
            Vector3 localRight = new Vector3(1, 0, 0);
            float yaw = 0f; // Facing north
            Matrix yawRotation = Matrix.CreateRotationY(yaw);
            Vector3 worldDirection = Vector3.Transform(localRight, yawRotation);
            
            // Assert - When facing north, D moves in +X (east/right)
            Assert.Equal(1f, worldDirection.X, 5);
            Assert.Equal(0f, worldDirection.Y, 5);
            Assert.Equal(0f, worldDirection.Z, 5);
        }
        
        [Fact]
        public void Player_LeftKey_MovesLeftRelativeToFacing_WhenFacingNorth()
        {
            // When facing north (yaw = 0), A should move in -X direction (left)
            
            Vector3 localLeft = new Vector3(-1, 0, 0);
            float yaw = 0f; // Facing north
            Matrix yawRotation = Matrix.CreateRotationY(yaw);
            Vector3 worldDirection = Vector3.Transform(localLeft, yawRotation);
            
            // Assert - When facing north, A moves in -X (west/left)
            Assert.Equal(-1f, worldDirection.X, 5);
            Assert.Equal(0f, worldDirection.Y, 5);
            Assert.Equal(0f, worldDirection.Z, 5);
        }
        
        [Fact]
        public void Player_BackwardKey_MovesBackwardRelativeToFacing_WhenFacingNorth()
        {
            // When facing north (yaw = 0), S should move in +Z direction (backward)
            
            Vector3 localBackward = new Vector3(0, 0, 1);
            float yaw = 0f; // Facing north
            Matrix yawRotation = Matrix.CreateRotationY(yaw);
            Vector3 worldDirection = Vector3.Transform(localBackward, yawRotation);
            
            // Assert - When facing north, S moves in +Z (south/backward)
            Assert.Equal(0f, worldDirection.X, 5);
            Assert.Equal(0f, worldDirection.Y, 5);
            Assert.Equal(1f, worldDirection.Z, 5);
        }
        
        [Fact]
        public void Player_InitialPosition_IsSetCorrectly()
        {
            // Arrange
            var startPosition = new Vector3(10, 20, 30);
            
            // Act
            var player = new Player(startPosition);
            
            // Assert
            Assert.Equal(startPosition, player.Position);
        }
        
        [Fact]
        public void Player_InitialRotation_IsZero()
        {
            // Arrange & Act
            var player = new Player(Vector3.Zero);
            
            // Assert
            Assert.Equal(Vector2.Zero, player.Rotation);
        }
    }
}
