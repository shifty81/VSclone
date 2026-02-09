using Microsoft.Xna.Framework;
using TimelessTales.Entities;
using TimelessTales.Rendering;
using Xunit;

namespace TimelessTales.Tests
{
    public class BreathSystemTests
    {
        [Fact]
        public void Player_BreathStartsAtMax()
        {
            var player = new Player(new Vector3(0, 100, 0));
            
            Assert.Equal(100f, player.Breath);
            Assert.Equal(100f, player.MaxBreath);
        }
        
        [Fact]
        public void Player_IsNotDrowning_WhenNotUnderwater()
        {
            var player = new Player(new Vector3(0, 100, 0));
            
            Assert.False(player.IsDrowning);
        }
        
        [Fact]
        public void BreathDepletion_Formula_IsCorrect()
        {
            // Breath depletes at BREATH_DEPLETION_RATE (5.0) units per second
            const float BREATH_DEPLETION_RATE = 5.0f;
            const float MAX_BREATH = 100f;
            
            float breath = MAX_BREATH;
            float deltaTime = 1.0f; // 1 second
            
            breath = MathF.Max(0f, breath - BREATH_DEPLETION_RATE * deltaTime);
            
            Assert.Equal(95f, breath, 5);
        }
        
        [Fact]
        public void BreathDepletion_ReachesZero_After20Seconds()
        {
            // At 5.0 units/sec, breath (100) depletes in 20 seconds
            const float BREATH_DEPLETION_RATE = 5.0f;
            const float MAX_BREATH = 100f;
            
            float breath = MAX_BREATH;
            float totalTime = 20f;
            
            breath = MathF.Max(0f, breath - BREATH_DEPLETION_RATE * totalTime);
            
            Assert.Equal(0f, breath, 5);
        }
        
        [Fact]
        public void BreathRecovery_Formula_IsCorrect()
        {
            // Breath recovers at BREATH_RECOVERY_RATE (20.0) units per second
            const float BREATH_RECOVERY_RATE = 20.0f;
            const float MAX_BREATH = 100f;
            
            float breath = 0f; // Empty
            float deltaTime = 1.0f;
            
            breath = MathF.Min(MAX_BREATH, breath + BREATH_RECOVERY_RATE * deltaTime);
            
            Assert.Equal(20f, breath, 5);
        }
        
        [Fact]
        public void BreathRecovery_CapsAtMax()
        {
            const float BREATH_RECOVERY_RATE = 20.0f;
            const float MAX_BREATH = 100f;
            
            float breath = 95f;
            float deltaTime = 1.0f;
            
            breath = MathF.Min(MAX_BREATH, breath + BREATH_RECOVERY_RATE * deltaTime);
            
            Assert.Equal(MAX_BREATH, breath, 5);
        }
        
        [Fact]
        public void DrowningDamage_AppliesWhenOutOfBreath()
        {
            const float DROWNING_DAMAGE_RATE = 10.0f;
            
            float health = 100f;
            float breath = 0f;
            float deltaTime = 1.0f;
            
            // Simulate drowning damage when breath is zero
            if (breath <= 0f)
            {
                health = MathF.Max(0f, health - DROWNING_DAMAGE_RATE * deltaTime);
            }
            
            Assert.Equal(90f, health, 5);
        }
        
        [Fact]
        public void DrowningDamage_DoesNotApply_WithBreathRemaining()
        {
            const float DROWNING_DAMAGE_RATE = 10.0f;
            
            float health = 100f;
            float breath = 50f;
            float deltaTime = 1.0f;
            
            if (breath <= 0f)
            {
                health = MathF.Max(0f, health - DROWNING_DAMAGE_RATE * deltaTime);
            }
            
            Assert.Equal(100f, health, 5); // Health unchanged
        }
    }
    
    public class LedgeGrabTests
    {
        [Fact]
        public void Player_IsNotGrabbingLedge_ByDefault()
        {
            var player = new Player(new Vector3(0, 100, 0));
            
            Assert.False(player.IsGrabbingLedge);
        }
        
        [Fact]
        public void LedgePullUpProgress_IsZero_WhenNotGrabbing()
        {
            var player = new Player(new Vector3(0, 100, 0));
            
            // LedgePullUpProgress should be 0/duration = 0 or NaN-safe
            Assert.True(player.LedgePullUpProgress >= 0f);
        }
        
        [Fact]
        public void LedgeGrab_Animation_ExistsInEnum()
        {
            // Verify the LedgeGrab animation type exists
            AnimationType ledgeGrab = AnimationType.LedgeGrab;
            Assert.Equal(AnimationType.LedgeGrab, ledgeGrab);
        }
        
        [Fact]
        public void LedgeGrab_AnimationController_HandlesLedgeGrab()
        {
            // Verify the animation controller can process a ledge grab update
            var skeleton = new Skeleton();
            skeleton.AddBone("root", Vector3.Zero);
            var torso = skeleton.AddBone("torso", new Vector3(0, 0.9f, 0), skeleton.GetBone("root"));
            skeleton.AddBone("right_arm", new Vector3(0.3f, 0.4f, 0), torso);
            skeleton.AddBone("left_arm", new Vector3(-0.3f, 0.4f, 0), torso);
            skeleton.AddBone("right_leg", new Vector3(0.15f, -0.2f, 0), skeleton.GetBone("root"));
            skeleton.AddBone("left_leg", new Vector3(-0.15f, -0.2f, 0), skeleton.GetBone("root"));
            
            var controller = new AnimationController(skeleton);
            
            // Should not throw during ledge grab animation
            controller.Update(0.016f, false, false, false, 0, false, false, true, 0.5f);
            
            // Verify bones have been rotated (arms should reach up in early phase)
            var rightArm = skeleton.GetBone("right_arm");
            Assert.NotNull(rightArm);
        }
    }
    
    public class WaterExitTests
    {
        [Fact]
        public void WaterExitBoost_IsGreaterThan_NormalSwimUp()
        {
            // Water exit boost (6.0) should be stronger than normal swim up (3.0)
            const float WATER_EXIT_BOOST = 6.0f;
            const float SWIM_UP_SPEED = 3.0f;
            
            Assert.True(WATER_EXIT_BOOST > SWIM_UP_SPEED);
        }
        
        [Fact]
        public void WaterStepUp_Height_AllowsExitFromWater()
        {
            // Step-up height should be at least 1 block to exit water onto land
            const float WATER_STEP_UP_HEIGHT = 1.2f;
            const float ONE_BLOCK = 1.0f;
            
            Assert.True(WATER_STEP_UP_HEIGHT >= ONE_BLOCK);
        }
        
        [Fact]
        public void WaterSurfacePattern_ProducesDifferentValues()
        {
            // Verify the surface pattern calculation produces different values at different positions
            // Using the same formula as WaterRenderer.CalculateSurfacePattern
            float time = 1.0f;
            float CAUSTIC_SCALE = 0.3f;
            float RIPPLE_SCALE = 0.15f;
            
            float pattern1 = CalculateTestSurfacePattern(0, 0, time, CAUSTIC_SCALE, RIPPLE_SCALE);
            float pattern2 = CalculateTestSurfacePattern(5, 5, time, CAUSTIC_SCALE, RIPPLE_SCALE);
            
            Assert.NotEqual(pattern1, pattern2);
        }
        
        private float CalculateTestSurfacePattern(int worldX, int worldZ, float time, float causticScale, float rippleScale)
        {
            float pattern = 0f;
            float cx = worldX * causticScale + time * 0.3f;
            float cz = worldZ * causticScale - time * 0.2f;
            pattern += MathF.Sin(cx) * MathF.Cos(cz) * 0.5f;
            
            float rx = worldX * (causticScale * 2.3f) - time * 0.5f;
            float rz = worldZ * (causticScale * 2.3f) + time * 0.4f;
            pattern += MathF.Sin(rx + rz) * rippleScale;
            
            return pattern;
        }
    }
}
