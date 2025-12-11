using Microsoft.Xna.Framework;
using TimelessTales.Core;
using Xunit;

namespace TimelessTales.Tests
{
    public class TimeManagerTests
    {
        [Fact]
        public void TimeManager_StartsAtMorning()
        {
            // Arrange & Act
            var timeManager = new TimeManager();
            
            // Assert
            Assert.Equal(0.3f, timeManager.TimeOfDay);
            Assert.Equal(0, timeManager.DayCount);
            Assert.True(timeManager.IsDaytime);
        }
        
        [Fact]
        public void TimeManager_UpdatesTimeOfDay()
        {
            // Arrange
            var timeManager = new TimeManager();
            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(60));
            
            // Act
            timeManager.Update(gameTime);
            
            // Assert
            Assert.True(timeManager.TimeOfDay > 0.3f);
        }
        
        [Fact]
        public void TimeManager_RollsOverAfterFullDay()
        {
            // Arrange
            var timeManager = new TimeManager();
            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(600)); // Full day cycle
            
            // Act
            timeManager.Update(gameTime);
            
            // Assert
            Assert.Equal(1, timeManager.DayCount);
            Assert.True(timeManager.TimeOfDay < 1.0f);
        }
        
        [Fact]
        public void TimeManager_IsDaytimeDuringDay()
        {
            // Arrange
            var timeManager = new TimeManager();
            
            // Act & Assert - at 0.5 (noon) should be daytime
            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(120)); // Move forward
            timeManager.Update(gameTime);
            
            Assert.True(timeManager.IsDaytime);
        }
        
        [Fact]
        public void TimeManager_GetSunAngleReturnsValidRange()
        {
            // Arrange
            var timeManager = new TimeManager();
            
            // Act
            float sunAngle = timeManager.GetSunAngle();
            
            // Assert
            Assert.True(sunAngle >= -MathHelper.Pi && sunAngle <= MathHelper.TwoPi);
        }
        
        [Fact]
        public void TimeManager_GetMoonAngleIsOppositeSun()
        {
            // Arrange
            var timeManager = new TimeManager();
            
            // Act
            float sunAngle = timeManager.GetSunAngle();
            float moonAngle = timeManager.GetMoonAngle();
            
            // Assert - Moon should be roughly PI radians from sun
            float difference = Math.Abs(moonAngle - sunAngle);
            Assert.True(Math.Abs(difference - MathHelper.Pi) < 0.01f);
        }
        
        [Fact]
        public void TimeManager_GetSkyColorReturnsValidColor()
        {
            // Arrange
            var timeManager = new TimeManager();
            
            // Act
            Color skyColor = timeManager.GetSkyColor();
            
            // Assert
            Assert.NotEqual(Color.Transparent, skyColor);
        }
        
        [Fact]
        public void TimeManager_GetAmbientLightReturnsBetweenZeroAndOne()
        {
            // Arrange
            var timeManager = new TimeManager();
            
            // Act
            float ambientLight = timeManager.GetAmbientLight();
            
            // Assert
            Assert.True(ambientLight >= 0.0f && ambientLight <= 1.0f);
        }
    }
}
