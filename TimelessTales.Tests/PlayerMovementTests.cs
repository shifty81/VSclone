using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using Xunit;

namespace TimelessTales.Tests
{
    public class PlayerMovementTests
    {
        [Theory]
        [InlineData(0f, 0f, -1f)]            // Facing North (yaw=0) - should move in -Z direction
        [InlineData(MathF.PI / 2, 1f, 0f)]   // Facing East (yaw=90deg) - should move in +X direction
        [InlineData(MathF.PI, 0f, 1f)]       // Facing South (yaw=180deg) - should move in +Z direction
        [InlineData(-MathF.PI / 2, -1f, 0f)] // Facing West (yaw=-90deg) - should move in -X direction
        public void Player_FacingDirection_ForwardMovement_MovesInCorrectDirection(float yaw, float expectedXSign, float expectedZSign)
        {
            // This test verifies the movement direction calculations match the coordinate system
            // where North is -Z, East is +X, South is +Z, West is -X
            
            // Calculate forward direction using the same formula as UpdateMovement for W key
            float forwardX = MathF.Sin(yaw);
            float forwardZ = -MathF.Cos(yaw);
            
            // Normalize the expected direction
            Vector2 expected = new Vector2(expectedXSign, expectedZSign);
            if (expected.LengthSquared() > 0)
                expected.Normalize();
            
            Vector2 actual = new Vector2(forwardX, forwardZ);
            if (actual.LengthSquared() > 0)
                actual.Normalize();
            
            // Assert - directions should match within a small tolerance
            Assert.True(Vector2.Distance(expected, actual) < 0.01f, 
                $"Expected forward direction ({expected.X:F3}, {expected.Y:F3}) but got ({actual.X:F3}, {actual.Y:F3}) when facing yaw={yaw:F3}");
        }
        
        [Theory]
        [InlineData(0f, 1f, 0f)]             // Facing North, strafe right should move in +X direction (East)
        [InlineData(MathF.PI / 2, 0f, 1f)]   // Facing East, strafe right should move in +Z direction (South)
        [InlineData(MathF.PI, -1f, 0f)]      // Facing South, strafe right should move in -X direction (West)
        [InlineData(-MathF.PI / 2, 0f, -1f)] // Facing West, strafe right should move in -Z direction (North)
        public void Player_FacingDirection_RightStrafeMovement_MovesInCorrectDirection(float yaw, float expectedXSign, float expectedZSign)
        {
            // Calculate right direction using the same formula as UpdateMovement for D key
            float rightX = MathF.Cos(yaw);
            float rightZ = MathF.Sin(yaw);
            
            // Normalize the expected direction
            Vector2 expected = new Vector2(expectedXSign, expectedZSign);
            if (expected.LengthSquared() > 0)
                expected.Normalize();
            
            Vector2 actual = new Vector2(rightX, rightZ);
            if (actual.LengthSquared() > 0)
                actual.Normalize();
            
            // Assert - directions should match within a small tolerance
            Assert.True(Vector2.Distance(expected, actual) < 0.01f,
                $"Expected right direction ({expected.X:F3}, {expected.Y:F3}) but got ({actual.X:F3}, {actual.Y:F3}) when facing yaw={yaw:F3}");
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
