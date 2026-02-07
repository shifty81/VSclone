using System;

namespace TimelessTales.Entities
{
    /// <summary>
    /// Manages survival mechanics including hunger depletion, thirst depletion,
    /// and health effects based on the player's current state.
    /// </summary>
    public class SurvivalSystem
    {
        // Depletion rates (units per second)
        private const float BASE_HUNGER_DEPLETION = 0.08f;   // ~100 in ~20 minutes
        private const float BASE_THIRST_DEPLETION = 0.12f;   // ~100 in ~14 minutes
        private const float SPRINT_DEPLETION_MULTIPLIER = 2.0f;
        private const float SWIM_DEPLETION_MULTIPLIER = 1.5f;

        // Health effect thresholds
        private const float STARVATION_THRESHOLD = 0f;
        private const float DEHYDRATION_THRESHOLD = 0f;
        private const float REGEN_HUNGER_THRESHOLD = 80f;
        private const float REGEN_THIRST_THRESHOLD = 80f;

        // Health change rates (units per second)
        private const float STARVATION_DAMAGE = 0.5f;
        private const float DEHYDRATION_DAMAGE = 0.8f;
        private const float HEALTH_REGEN_RATE = 0.3f;

        /// <summary>
        /// Gets the base hunger depletion rate in units per second.
        /// </summary>
        public static float BaseHungerDepletion => BASE_HUNGER_DEPLETION;

        /// <summary>
        /// Gets the base thirst depletion rate in units per second.
        /// </summary>
        public static float BaseThirstDepletion => BASE_THIRST_DEPLETION;

        /// <summary>
        /// Gets the sprint activity multiplier for depletion rates.
        /// </summary>
        public static float SprintMultiplier => SPRINT_DEPLETION_MULTIPLIER;

        /// <summary>
        /// Gets the swimming activity multiplier for depletion rates.
        /// </summary>
        public static float SwimMultiplier => SWIM_DEPLETION_MULTIPLIER;

        /// <summary>
        /// Gets the hunger threshold above which health regeneration occurs.
        /// </summary>
        public static float RegenHungerThreshold => REGEN_HUNGER_THRESHOLD;

        /// <summary>
        /// Gets the thirst threshold above which health regeneration occurs.
        /// </summary>
        public static float RegenThirstThreshold => REGEN_THIRST_THRESHOLD;

        /// <summary>
        /// Gets the health regeneration rate in units per second.
        /// </summary>
        public static float HealthRegenRate => HEALTH_REGEN_RATE;

        /// <summary>
        /// Gets the starvation damage rate in units per second.
        /// </summary>
        public static float StarvationDamage => STARVATION_DAMAGE;

        /// <summary>
        /// Gets the dehydration damage rate in units per second.
        /// </summary>
        public static float DehydrationDamage => DEHYDRATION_DAMAGE;

        /// <summary>
        /// Updates the survival stats for the player based on elapsed time and activity.
        /// </summary>
        /// <param name="player">The player whose stats to update.</param>
        /// <param name="deltaTime">Elapsed time in seconds since last update.</param>
        /// <param name="isSprinting">Whether the player is currently sprinting.</param>
        /// <param name="isSwimming">Whether the player is currently swimming.</param>
        public void Update(Player player, float deltaTime, bool isSprinting, bool isSwimming)
        {
            UpdateHunger(player, deltaTime, isSprinting, isSwimming);
            UpdateThirst(player, deltaTime, isSprinting, isSwimming);
            UpdateHealth(player, deltaTime);
        }

        private void UpdateHunger(Player player, float deltaTime, bool isSprinting, bool isSwimming)
        {
            float rate = BASE_HUNGER_DEPLETION;
            if (isSprinting)
                rate *= SPRINT_DEPLETION_MULTIPLIER;
            if (isSwimming)
                rate *= SWIM_DEPLETION_MULTIPLIER;

            player.Hunger = Math.Max(0f, player.Hunger - rate * deltaTime);
        }

        private void UpdateThirst(Player player, float deltaTime, bool isSprinting, bool isSwimming)
        {
            float rate = BASE_THIRST_DEPLETION;
            if (isSprinting)
                rate *= SPRINT_DEPLETION_MULTIPLIER;
            if (isSwimming)
                rate *= SWIM_DEPLETION_MULTIPLIER;

            player.Thirst = Math.Max(0f, player.Thirst - rate * deltaTime);
        }

        private void UpdateHealth(Player player, float deltaTime)
        {
            // Starvation damage
            if (player.Hunger <= STARVATION_THRESHOLD)
            {
                player.Health = Math.Max(0f, player.Health - STARVATION_DAMAGE * deltaTime);
            }

            // Dehydration damage
            if (player.Thirst <= DEHYDRATION_THRESHOLD)
            {
                player.Health = Math.Max(0f, player.Health - DEHYDRATION_DAMAGE * deltaTime);
            }

            // Health regeneration when well-fed and hydrated
            if (player.Hunger >= REGEN_HUNGER_THRESHOLD && player.Thirst >= REGEN_THIRST_THRESHOLD)
            {
                player.Health = Math.Min(player.MaxHealth, player.Health + HEALTH_REGEN_RATE * deltaTime);
            }
        }
    }
}
