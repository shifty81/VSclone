using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using Xunit;

namespace TimelessTales.Tests
{
    public class SurvivalSystemTests
    {
        private Player CreatePlayer()
        {
            return new Player(Vector3.Zero);
        }

        [Fact]
        public void Player_StartsWithFullStats()
        {
            var player = CreatePlayer();

            Assert.Equal(100f, player.Health);
            Assert.Equal(100f, player.Hunger);
            Assert.Equal(100f, player.Thirst);
        }

        [Fact]
        public void Hunger_DepletesOverTime()
        {
            var player = CreatePlayer();
            var system = new SurvivalSystem();

            system.Update(player, 10f, isSprinting: false, isSwimming: false);

            Assert.True(player.Hunger < 100f);
            float expected = 100f - SurvivalSystem.BaseHungerDepletion * 10f;
            Assert.Equal(expected, player.Hunger, 3);
        }

        [Fact]
        public void Thirst_DepletesOverTime()
        {
            var player = CreatePlayer();
            var system = new SurvivalSystem();

            system.Update(player, 10f, isSprinting: false, isSwimming: false);

            Assert.True(player.Thirst < 100f);
            float expected = 100f - SurvivalSystem.BaseThirstDepletion * 10f;
            Assert.Equal(expected, player.Thirst, 3);
        }

        [Fact]
        public void Hunger_DepletsFasterWhenSprinting()
        {
            var playerIdle = CreatePlayer();
            var playerSprint = CreatePlayer();
            var system = new SurvivalSystem();

            system.Update(playerIdle, 10f, isSprinting: false, isSwimming: false);
            system.Update(playerSprint, 10f, isSprinting: true, isSwimming: false);

            Assert.True(playerSprint.Hunger < playerIdle.Hunger);
        }

        [Fact]
        public void Thirst_DepletsFasterWhenSwimming()
        {
            var playerIdle = CreatePlayer();
            var playerSwim = CreatePlayer();
            var system = new SurvivalSystem();

            system.Update(playerIdle, 10f, isSprinting: false, isSwimming: false);
            system.Update(playerSwim, 10f, isSprinting: false, isSwimming: true);

            Assert.True(playerSwim.Thirst < playerIdle.Thirst);
        }

        [Fact]
        public void Hunger_DoesNotGoBelowZero()
        {
            var player = CreatePlayer();
            var system = new SurvivalSystem();

            // Simulate a very long time to fully deplete hunger
            system.Update(player, 100000f, isSprinting: false, isSwimming: false);

            Assert.Equal(0f, player.Hunger);
        }

        [Fact]
        public void Thirst_DoesNotGoBelowZero()
        {
            var player = CreatePlayer();
            var system = new SurvivalSystem();

            system.Update(player, 100000f, isSprinting: false, isSwimming: false);

            Assert.Equal(0f, player.Thirst);
        }

        [Fact]
        public void Health_TakesDamage_WhenStarving()
        {
            var player = CreatePlayer();
            player.Hunger = 0f;
            var system = new SurvivalSystem();

            // Thirst still full, so only starvation damage applies
            system.Update(player, 10f, isSprinting: false, isSwimming: false);

            Assert.True(player.Health < 100f);
            float expectedDamage = SurvivalSystem.StarvationDamage * 10f;
            Assert.Equal(100f - expectedDamage, player.Health, 2);
        }

        [Fact]
        public void Health_TakesDamage_WhenDehydrated()
        {
            var player = CreatePlayer();
            player.Thirst = 0f;
            var system = new SurvivalSystem();

            system.Update(player, 10f, isSprinting: false, isSwimming: false);

            Assert.True(player.Health < 100f);
            float expectedDamage = SurvivalSystem.DehydrationDamage * 10f;
            Assert.Equal(100f - expectedDamage, player.Health, 2);
        }

        [Fact]
        public void Health_TakesCombinedDamage_WhenStarvingAndDehydrated()
        {
            var player = CreatePlayer();
            player.Hunger = 0f;
            player.Thirst = 0f;
            var system = new SurvivalSystem();

            system.Update(player, 10f, isSprinting: false, isSwimming: false);

            float expectedDamage = (SurvivalSystem.StarvationDamage + SurvivalSystem.DehydrationDamage) * 10f;
            Assert.Equal(100f - expectedDamage, player.Health, 2);
        }

        [Fact]
        public void Health_DoesNotGoBelowZero()
        {
            var player = CreatePlayer();
            player.Hunger = 0f;
            player.Thirst = 0f;
            var system = new SurvivalSystem();

            system.Update(player, 100000f, isSprinting: false, isSwimming: false);

            Assert.Equal(0f, player.Health);
        }

        [Fact]
        public void Health_RegeneratesWhenWellFedAndHydrated()
        {
            var player = CreatePlayer();
            player.Health = 50f;
            player.Hunger = 90f;
            player.Thirst = 90f;
            var system = new SurvivalSystem();

            system.Update(player, 10f, isSprinting: false, isSwimming: false);

            Assert.True(player.Health > 50f);
        }

        [Fact]
        public void Health_DoesNotRegenerateAboveMax()
        {
            var player = CreatePlayer();
            player.Health = 99.9f;
            player.Hunger = 100f;
            player.Thirst = 100f;
            var system = new SurvivalSystem();

            system.Update(player, 10f, isSprinting: false, isSwimming: false);

            Assert.Equal(player.MaxHealth, player.Health);
        }

        [Fact]
        public void Health_DoesNotRegenerate_WhenHungerBelowThreshold()
        {
            var player = CreatePlayer();
            player.Health = 50f;
            player.Hunger = 50f; // Below regen threshold
            player.Thirst = 90f;
            var system = new SurvivalSystem();

            system.Update(player, 10f, isSprinting: false, isSwimming: false);

            // Health should not have increased (hunger is depleting too so it stays below threshold)
            Assert.True(player.Health <= 50f);
        }

        [Fact]
        public void SprintAndSwim_StackDepletionMultipliers()
        {
            var playerNormal = CreatePlayer();
            var playerBoth = CreatePlayer();
            var system = new SurvivalSystem();

            system.Update(playerNormal, 10f, isSprinting: false, isSwimming: false);
            system.Update(playerBoth, 10f, isSprinting: true, isSwimming: true);

            // Both multipliers should stack, so depletion is much faster
            float normalDepletion = 100f - playerNormal.Hunger;
            float bothDepletion = 100f - playerBoth.Hunger;
            float expectedMultiplier = SurvivalSystem.SprintMultiplier * SurvivalSystem.SwimMultiplier;
            Assert.Equal(bothDepletion, normalDepletion * expectedMultiplier, 2);
        }
    }
}
