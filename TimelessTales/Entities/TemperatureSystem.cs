using System;
using TimelessTales.World;
using TimelessTales.Core;

namespace TimelessTales.Entities
{
    /// <summary>
    /// Manages temperature mechanics for the player, including environmental
    /// temperature calculation based on biome, altitude, time of day, and
    /// submersion state. Body temperature drifts toward environmental temperature,
    /// and extreme body temperatures cause health effects.
    /// </summary>
    public class TemperatureSystem
    {
        // Environmental temperature ranges by biome (in °C-like units, 0-100 scale)
        // 0 = freezing cold, 50 = comfortable, 100 = extreme heat
        private const float TUNDRA_BASE_TEMP = 15f;
        private const float BOREAL_BASE_TEMP = 25f;
        private const float TEMPERATE_BASE_TEMP = 45f;
        private const float DESERT_BASE_TEMP = 75f;
        private const float TROPICAL_BASE_TEMP = 65f;
        private const float OCEAN_BASE_TEMP = 35f;

        // Comfortable body temperature range
        private const float COMFORTABLE_MIN = 35f;
        private const float COMFORTABLE_MAX = 65f;

        // Danger thresholds for health effects
        private const float HYPOTHERMIA_THRESHOLD = 20f;
        private const float COLD_THRESHOLD = 30f;
        private const float HEAT_THRESHOLD = 70f;
        private const float HEATSTROKE_THRESHOLD = 80f;

        // Health damage rates (units per second)
        private const float HYPOTHERMIA_DAMAGE = 0.6f;
        private const float HEATSTROKE_DAMAGE = 0.5f;

        // Body temperature drift rate (units per second toward environment temp)
        private const float BODY_TEMP_DRIFT_RATE = 0.8f;

        // Modifiers
        private const float NIGHT_TEMP_REDUCTION = 12f;
        private const float ALTITUDE_TEMP_REDUCTION_PER_BLOCK = 0.15f;
        private const float WATER_TEMP_REDUCTION = 15f;
        private const float SEA_LEVEL = 64f;

        /// <summary>Gets the base temperature for Tundra biome.</summary>
        public static float TundraBaseTemp => TUNDRA_BASE_TEMP;

        /// <summary>Gets the base temperature for Desert biome.</summary>
        public static float DesertBaseTemp => DESERT_BASE_TEMP;

        /// <summary>Gets the hypothermia damage rate in units per second.</summary>
        public static float HypothermiaDamage => HYPOTHERMIA_DAMAGE;

        /// <summary>Gets the heatstroke damage rate in units per second.</summary>
        public static float HeatstrokeDamage => HEATSTROKE_DAMAGE;

        /// <summary>Gets the body temperature drift rate in units per second.</summary>
        public static float BodyTempDriftRate => BODY_TEMP_DRIFT_RATE;

        /// <summary>Gets the hypothermia threshold temperature.</summary>
        public static float HypothermiaThreshold => HYPOTHERMIA_THRESHOLD;

        /// <summary>Gets the heatstroke threshold temperature.</summary>
        public static float HeatstrokeThreshold => HEATSTROKE_THRESHOLD;

        /// <summary>Gets the comfortable minimum temperature.</summary>
        public static float ComfortableMin => COMFORTABLE_MIN;

        /// <summary>Gets the comfortable maximum temperature.</summary>
        public static float ComfortableMax => COMFORTABLE_MAX;

        /// <summary>Gets the night temperature reduction amount.</summary>
        public static float NightTempReduction => NIGHT_TEMP_REDUCTION;

        /// <summary>Gets the water temperature reduction amount.</summary>
        public static float WaterTempReduction => WATER_TEMP_REDUCTION;

        /// <summary>
        /// Calculates the environmental temperature at the player's location.
        /// </summary>
        /// <param name="biome">The biome at the player's position.</param>
        /// <param name="altitude">The player's Y position (altitude).</param>
        /// <param name="timeOfDay">Time of day (0.0 to 1.0).</param>
        /// <param name="isInWater">Whether the player is submerged in water.</param>
        /// <returns>Environmental temperature (0-100 scale).</returns>
        public float CalculateEnvironmentTemperature(BiomeType biome, float altitude, float timeOfDay, bool isInWater)
        {
            // Start with biome base temperature
            float temp = GetBiomeBaseTemperature(biome);

            // Altitude modifier: higher = colder
            float altitudeAboveSea = Math.Max(0, altitude - SEA_LEVEL);
            temp -= altitudeAboveSea * ALTITUDE_TEMP_REDUCTION_PER_BLOCK;

            // Day/night modifier: nighttime is colder
            if (!IsDaytime(timeOfDay))
            {
                temp -= NIGHT_TEMP_REDUCTION;
            }

            // Water modifier: being in water makes it colder
            if (isInWater)
            {
                temp -= WATER_TEMP_REDUCTION;
            }

            return Math.Clamp(temp, 0f, 100f);
        }

        /// <summary>
        /// Updates the player's body temperature, drifting toward the environmental temperature,
        /// and applies health effects from extreme temperatures.
        /// </summary>
        /// <param name="player">The player to update.</param>
        /// <param name="environmentTemp">The current environmental temperature.</param>
        /// <param name="deltaTime">Elapsed time in seconds.</param>
        public void Update(Player player, float environmentTemp, float deltaTime)
        {
            // Drift body temperature toward environment temperature
            float tempDiff = environmentTemp - player.BodyTemperature;
            float drift = Math.Sign(tempDiff) * Math.Min(Math.Abs(tempDiff), BODY_TEMP_DRIFT_RATE * deltaTime);
            player.BodyTemperature = Math.Clamp(player.BodyTemperature + drift, 0f, 100f);

            // Apply health effects from extreme body temperature
            if (player.BodyTemperature <= HYPOTHERMIA_THRESHOLD)
            {
                player.Health = Math.Max(0f, player.Health - HYPOTHERMIA_DAMAGE * deltaTime);
            }
            else if (player.BodyTemperature >= HEATSTROKE_THRESHOLD)
            {
                player.Health = Math.Max(0f, player.Health - HEATSTROKE_DAMAGE * deltaTime);
            }
        }

        /// <summary>
        /// Gets the temperature status description for display.
        /// </summary>
        public static string GetTemperatureStatus(float bodyTemperature)
        {
            if (bodyTemperature <= HYPOTHERMIA_THRESHOLD)
                return "FREEZING";
            if (bodyTemperature <= COLD_THRESHOLD)
                return "COLD";
            if (bodyTemperature <= COMFORTABLE_MAX && bodyTemperature >= COMFORTABLE_MIN)
                return "COMFORTABLE";
            if (bodyTemperature >= HEATSTROKE_THRESHOLD)
                return "OVERHEATING";
            if (bodyTemperature >= HEAT_THRESHOLD)
                return "HOT";
            // Between COLD_THRESHOLD and COMFORTABLE_MIN, or COMFORTABLE_MAX and HEAT_THRESHOLD
            if (bodyTemperature < COMFORTABLE_MIN)
                return "COOL";
            return "WARM";
        }

        private float GetBiomeBaseTemperature(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Tundra => TUNDRA_BASE_TEMP,
                BiomeType.Boreal => BOREAL_BASE_TEMP,
                BiomeType.Temperate => TEMPERATE_BASE_TEMP,
                BiomeType.Desert => DESERT_BASE_TEMP,
                BiomeType.Tropical => TROPICAL_BASE_TEMP,
                BiomeType.Ocean => OCEAN_BASE_TEMP,
                _ => TEMPERATE_BASE_TEMP
            };
        }

        private bool IsDaytime(float timeOfDay)
        {
            return timeOfDay >= 0.25f && timeOfDay < 0.75f;
        }
    }
}
