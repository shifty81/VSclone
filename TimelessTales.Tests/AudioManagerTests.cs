using TimelessTales.Audio;
using Xunit;

namespace TimelessTales.Tests
{
    public class AudioManagerTests
    {
        [Fact]
        public void AudioManager_InitialTransitionIsZero()
        {
            // Arrange & Act
            var manager = new AudioManager();

            // Assert
            Assert.Equal(0.0f, manager.CurrentTransition);
            Assert.False(manager.IsUnderwater);
        }

        [Fact]
        public void AudioManager_TransitionIncreasesWhenUnderwater()
        {
            // Arrange
            var manager = new AudioManager();
            manager.IsUnderwater = true;

            // Act - simulate several frames
            for (int i = 0; i < 10; i++)
            {
                manager.Update(0.016f); // ~60fps
            }

            // Assert - transition should have increased toward 1.0
            Assert.True(manager.CurrentTransition > 0.0f);
            Assert.True(manager.CurrentTransition <= 1.0f);
        }

        [Fact]
        public void AudioManager_TransitionDecreasesWhenSurfacing()
        {
            // Arrange
            var manager = new AudioManager();
            manager.IsUnderwater = true;

            // Go fully underwater
            for (int i = 0; i < 100; i++)
            {
                manager.Update(0.016f);
            }
            float underwaterTransition = manager.CurrentTransition;

            // Surface
            manager.IsUnderwater = false;
            for (int i = 0; i < 10; i++)
            {
                manager.Update(0.016f);
            }

            // Assert
            Assert.True(manager.CurrentTransition < underwaterTransition);
        }

        [Fact]
        public void AudioManager_TransitionReachesFullyUnderwater()
        {
            // Arrange
            var manager = new AudioManager();
            manager.IsUnderwater = true;

            // Act - simulate many frames
            for (int i = 0; i < 200; i++)
            {
                manager.Update(0.016f);
            }

            // Assert - should reach 1.0
            Assert.Equal(1.0f, manager.CurrentTransition, 0.01f);
        }

        [Fact]
        public void AudioManager_TransitionReachesFullySurfaced()
        {
            // Arrange
            var manager = new AudioManager();
            manager.IsUnderwater = true;

            // Go fully underwater
            for (int i = 0; i < 200; i++)
            {
                manager.Update(0.016f);
            }

            // Surface
            manager.IsUnderwater = false;
            for (int i = 0; i < 200; i++)
            {
                manager.Update(0.016f);
            }

            // Assert - should reach 0.0
            Assert.Equal(0.0f, manager.CurrentTransition, 0.01f);
        }

        [Fact]
        public void AudioManager_SubmersionDepthClampedToRange()
        {
            // Arrange
            var manager = new AudioManager();

            // Act & Assert
            manager.SubmersionDepth = 0.5f;
            Assert.Equal(0.5f, manager.SubmersionDepth, 0.01f);

            manager.SubmersionDepth = -0.5f;
            Assert.Equal(0.0f, manager.SubmersionDepth, 0.01f);

            manager.SubmersionDepth = 1.5f;
            Assert.Equal(1.0f, manager.SubmersionDepth, 0.01f);
        }

        [Fact]
        public void AudioManager_DefaultVolumeSettings()
        {
            // Arrange & Act
            var manager = new AudioManager();

            // Assert
            Assert.Equal(1.0f, manager.MasterVolume);
            Assert.Equal(1.0f, manager.SoundEffectVolume);
        }

        [Fact]
        public void AudioManager_SmoothTransitionIsGradual()
        {
            // Arrange
            var manager = new AudioManager();
            manager.IsUnderwater = true;

            // Act - single frame update
            manager.Update(0.016f);

            // Assert - should not jump to 1.0 immediately
            Assert.True(manager.CurrentTransition > 0.0f);
            Assert.True(manager.CurrentTransition < 1.0f);
        }
    }
}
