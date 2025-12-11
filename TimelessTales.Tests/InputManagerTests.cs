using Xunit;
using Microsoft.Xna.Framework;
using TimelessTales.Core;

namespace TimelessTales.Tests
{
    public class InputManagerTests
    {
        [Fact]
        public void InputManager_InitializesCorrectly()
        {
            // Arrange & Act
            var inputManager = new InputManager();
            
            // Assert
            Assert.NotNull(inputManager);
            Assert.Equal(0, inputManager.GetMouseDeltaX());
            Assert.Equal(0, inputManager.GetMouseDeltaY());
        }
        
        [Fact]
        public void InputManager_SetScreenCenter_StoresCorrectly()
        {
            // Arrange
            var inputManager = new InputManager();
            
            // Act
            inputManager.SetScreenCenter(640, 360);
            
            // Assert - if center is set, delta should still be 0 initially
            Assert.Equal(0, inputManager.GetMouseDeltaX());
            Assert.Equal(0, inputManager.GetMouseDeltaY());
        }
        
        [Fact]
        public void InputManager_SetMouseCaptured_DoesNotThrow()
        {
            // Arrange
            var inputManager = new InputManager();
            
            // Act & Assert - should not throw
            inputManager.SetMouseCaptured(true);
            inputManager.SetMouseCaptured(false);
        }
    }
}
