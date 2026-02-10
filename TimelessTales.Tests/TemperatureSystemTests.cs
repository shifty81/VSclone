using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using TimelessTales.World;
using Xunit;

namespace TimelessTales.Tests
{
    public class TemperatureSystemTests
    {
        private Player CreatePlayer()
        {
            return new Player(Vector3.Zero);
        }

        [Fact]
        public void Player_StartsWithComfortableTemperature()
        {
            var player = CreatePlayer();

            Assert.Equal(50f, player.BodyTemperature);
        }

        [Fact]
        public void EnvironmentTemp_TundraIsCold()
        {
            var system = new TemperatureSystem();

            float temp = system.CalculateEnvironmentTemperature(BiomeType.Tundra, 64f, 0.5f, false);

            Assert.True(temp < 30f);
        }

        [Fact]
        public void EnvironmentTemp_DesertIsHot()
        {
            var system = new TemperatureSystem();

            float temp = system.CalculateEnvironmentTemperature(BiomeType.Desert, 64f, 0.5f, false);

            Assert.True(temp > 60f);
        }

        [Fact]
        public void EnvironmentTemp_TemperateIsComfortable()
        {
            var system = new TemperatureSystem();

            float temp = system.CalculateEnvironmentTemperature(BiomeType.Temperate, 64f, 0.5f, false);

            Assert.InRange(temp, 30f, 60f);
        }

        [Fact]
        public void EnvironmentTemp_NightIsCoolerThanDay()
        {
            var system = new TemperatureSystem();

            float dayTemp = system.CalculateEnvironmentTemperature(BiomeType.Temperate, 64f, 0.5f, false);
            float nightTemp = system.CalculateEnvironmentTemperature(BiomeType.Temperate, 64f, 0.1f, false);

            Assert.True(nightTemp < dayTemp);
        }

        [Fact]
        public void EnvironmentTemp_HighAltitudeIsColder()
        {
            var system = new TemperatureSystem();

            float seaLevelTemp = system.CalculateEnvironmentTemperature(BiomeType.Temperate, 64f, 0.5f, false);
            float mountainTemp = system.CalculateEnvironmentTemperature(BiomeType.Temperate, 120f, 0.5f, false);

            Assert.True(mountainTemp < seaLevelTemp);
        }

        [Fact]
        public void EnvironmentTemp_WaterMakesColder()
        {
            var system = new TemperatureSystem();

            float dryTemp = system.CalculateEnvironmentTemperature(BiomeType.Temperate, 64f, 0.5f, false);
            float wetTemp = system.CalculateEnvironmentTemperature(BiomeType.Temperate, 64f, 0.5f, true);

            Assert.True(wetTemp < dryTemp);
        }

        [Fact]
        public void BodyTemp_DriftsTowardEnvironment()
        {
            var player = CreatePlayer();
            player.BodyTemperature = 50f;
            var system = new TemperatureSystem();

            // Cold environment should drift body temp down
            system.Update(player, 10f, 10f);

            Assert.True(player.BodyTemperature < 50f);
        }

        [Fact]
        public void BodyTemp_DriftsUpInHotEnvironment()
        {
            var player = CreatePlayer();
            player.BodyTemperature = 50f;
            var system = new TemperatureSystem();

            // Hot environment should drift body temp up
            system.Update(player, 90f, 10f);

            Assert.True(player.BodyTemperature > 50f);
        }

        [Fact]
        public void Health_TakesDamage_WhenHypothermic()
        {
            var player = CreatePlayer();
            player.BodyTemperature = 15f; // Below hypothermia threshold
            var system = new TemperatureSystem();

            system.Update(player, 5f, 10f);

            Assert.True(player.Health < 100f);
        }

        [Fact]
        public void Health_TakesDamage_WhenOverheated()
        {
            var player = CreatePlayer();
            player.BodyTemperature = 85f; // Above heatstroke threshold
            var system = new TemperatureSystem();

            system.Update(player, 95f, 10f);

            Assert.True(player.Health < 100f);
        }

        [Fact]
        public void Health_NoDamage_AtComfortableTemp()
        {
            var player = CreatePlayer();
            player.BodyTemperature = 50f;
            var system = new TemperatureSystem();

            system.Update(player, 50f, 10f);

            Assert.Equal(100f, player.Health);
        }

        [Fact]
        public void Health_DoesNotGoBelowZero_FromHypothermia()
        {
            var player = CreatePlayer();
            player.BodyTemperature = 5f;
            var system = new TemperatureSystem();

            system.Update(player, 0f, 100000f);

            Assert.Equal(0f, player.Health);
        }

        [Fact]
        public void TemperatureStatus_ReturnsCorrectLabels()
        {
            Assert.Equal("FREEZING", TemperatureSystem.GetTemperatureStatus(15f));
            Assert.Equal("COLD", TemperatureSystem.GetTemperatureStatus(25f));
            Assert.Equal("COOL", TemperatureSystem.GetTemperatureStatus(33f));
            Assert.Equal("COMFORTABLE", TemperatureSystem.GetTemperatureStatus(50f));
            Assert.Equal("WARM", TemperatureSystem.GetTemperatureStatus(67f));
            Assert.Equal("HOT", TemperatureSystem.GetTemperatureStatus(72f));
            Assert.Equal("OVERHEATING", TemperatureSystem.GetTemperatureStatus(85f));
        }

        [Fact]
        public void BodyTemp_ClampsBetweenZeroAndHundred()
        {
            var player = CreatePlayer();
            player.BodyTemperature = 2f;
            var system = new TemperatureSystem();

            // Extreme cold for a long time
            system.Update(player, 0f, 10000f);

            Assert.InRange(player.BodyTemperature, 0f, 100f);
        }

        [Fact]
        public void EnvironmentTemp_ClampsBetweenZeroAndHundred()
        {
            var system = new TemperatureSystem();

            // Extreme conditions
            float temp = system.CalculateEnvironmentTemperature(BiomeType.Tundra, 200f, 0.0f, true);

            Assert.InRange(temp, 0f, 100f);
        }

        [Fact]
        public void BodyTemp_DriftRateIsCorrect()
        {
            var player = CreatePlayer();
            player.BodyTemperature = 50f;
            var system = new TemperatureSystem();

            // Environment is at 40, delta is 1 second
            system.Update(player, 40f, 1f);

            // Drift should be min(|10|, 0.8 * 1) = 0.8
            float expected = 50f - TemperatureSystem.BodyTempDriftRate;
            Assert.Equal(expected, player.BodyTemperature, 3);
        }

        [Fact]
        public void AllBiomes_ReturnValidTemperature()
        {
            var system = new TemperatureSystem();

            foreach (BiomeType biome in System.Enum.GetValues(typeof(BiomeType)))
            {
                float temp = system.CalculateEnvironmentTemperature(biome, 64f, 0.5f, false);
                Assert.InRange(temp, 0f, 100f);
            }
        }
    }
}
