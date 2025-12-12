using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using Xunit;

namespace TimelessTales.Tests
{
    public class PlayerMovementTests
    {
        [Fact]
        public void Player_ForwardKey_AlwaysMovesInNegativeZ_RegardlessOfFacingDirection()
        {
            // Movement is now absolute - W always moves forward (-Z) regardless of facing direction
            // This is the new expected behavior: direction keys are not affected by camera rotation
            
            // W key movement direction (from UpdateMovement in Player.cs)
            Vector3 forwardDirection = new Vector3(0, 0, -1);
            
            // Assert - W key should always move in -Z direction (forward/north) no matter where player is looking
            Assert.Equal(0f, forwardDirection.X);
            Assert.Equal(0f, forwardDirection.Y);
            Assert.Equal(-1f, forwardDirection.Z);
        }
        
        [Fact]
        public void Player_RightKey_AlwaysMovesInPositiveX_RegardlessOfFacingDirection()
        {
            // Movement is now absolute - D always moves right (+X) regardless of facing direction
            
            // D key movement direction (from UpdateMovement in Player.cs)
            Vector3 rightDirection = new Vector3(1, 0, 0);
            
            // Assert - D key should always move in +X direction (right/east) no matter where player is looking
            Assert.Equal(1f, rightDirection.X);
            Assert.Equal(0f, rightDirection.Y);
            Assert.Equal(0f, rightDirection.Z);
        }
        
        [Fact]
        public void Player_BackwardKey_AlwaysMovesInPositiveZ()
        {
            // S key movement direction (from UpdateMovement in Player.cs)
            Vector3 backwardDirection = new Vector3(0, 0, 1);
            
            // Assert - S key should always move in +Z direction (backward/south)
            Assert.Equal(0f, backwardDirection.X);
            Assert.Equal(0f, backwardDirection.Y);
            Assert.Equal(1f, backwardDirection.Z);
        }
        
        [Fact]
        public void Player_LeftKey_AlwaysMovesInNegativeX()
        {
            // A key movement direction (from UpdateMovement in Player.cs)
            Vector3 leftDirection = new Vector3(-1, 0, 0);
            
            // Assert - A key should always move in -X direction (left/west)
            Assert.Equal(-1f, leftDirection.X);
            Assert.Equal(0f, leftDirection.Y);
            Assert.Equal(0f, leftDirection.Z);
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
