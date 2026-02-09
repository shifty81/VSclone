using Microsoft.Xna.Framework;
using System;

namespace TimelessTales.Entities
{
    /// <summary>
    /// Animation types for the player character
    /// </summary>
    public enum AnimationType
    {
        Idle,
        Walking,
        Running,
        Breaking,
        Jumping,
        TreadingWater,
        Swimming,
        LedgeGrab
    }

    /// <summary>
    /// Controls animations for the player character with improved natural arm movement
    /// and ledge grab support
    /// </summary>
    public class AnimationController
    {
        private readonly Skeleton _skeleton;
        private AnimationType _currentAnimation = AnimationType.Idle;
        private float _animationTime = 0f;
        private bool _isBreaking = false;
        private float _breakingProgress = 0f;
        private float _ledgeGrabProgress = 0f;
        
        // Animation parameters
        private const float WALK_SPEED = 2.0f;
        private const float RUN_SPEED = 3.0f;
        private const float ARM_SWING_AMOUNT = 0.5f;
        private const float LEG_SWING_AMOUNT = 0.4f;
        private const float BREAK_SWING_AMOUNT = 1.2f;
        private const float TREAD_WATER_SPEED = 1.5f;
        private const float SWIM_SPEED = 2.5f;
        
        // Improved arm motion parameters for more natural movement
        private const float ARM_SECONDARY_MOTION = 0.15f; // Secondary shoulder/elbow motion
        private const float ARM_LATERAL_SWAY = 0.08f; // Slight lateral arm sway during walk
        
        public AnimationController(Skeleton skeleton)
        {
            _skeleton = skeleton;
        }
        
        public void Update(float deltaTime, bool isMoving, bool isSprinting, bool isBreaking, float breakProgress, bool isInWater = false, bool isSwimming = false, bool isGrabbingLedge = false, float ledgeGrabProgress = 0f)
        {
            _animationTime += deltaTime;
            _isBreaking = isBreaking;
            _breakingProgress = breakProgress;
            _ledgeGrabProgress = ledgeGrabProgress;
            
            // Determine current animation
            if (isGrabbingLedge)
            {
                _currentAnimation = AnimationType.LedgeGrab;
            }
            else if (isInWater)
            {
                _currentAnimation = isSwimming ? AnimationType.Swimming : AnimationType.TreadingWater;
            }
            else if (isBreaking)
            {
                _currentAnimation = AnimationType.Breaking;
            }
            else if (isMoving)
            {
                _currentAnimation = isSprinting ? AnimationType.Running : AnimationType.Walking;
            }
            else
            {
                _currentAnimation = AnimationType.Idle;
                _animationTime = 0f; // Reset animation when idle
            }
            
            // Apply animation
            ApplyAnimation();
        }
        
        private void ApplyAnimation()
        {
            switch (_currentAnimation)
            {
                case AnimationType.Idle:
                    ApplyIdleAnimation();
                    break;
                case AnimationType.Walking:
                    ApplyWalkingAnimation(WALK_SPEED);
                    break;
                case AnimationType.Running:
                    ApplyWalkingAnimation(RUN_SPEED);
                    break;
                case AnimationType.Breaking:
                    ApplyBreakingAnimation();
                    break;
                case AnimationType.TreadingWater:
                    ApplyTreadingWaterAnimation();
                    break;
                case AnimationType.Swimming:
                    ApplySwimmingAnimation();
                    break;
                case AnimationType.LedgeGrab:
                    ApplyLedgeGrabAnimation();
                    break;
            }
            
            _skeleton.UpdateAllTransforms();
        }
        
        private void ApplyIdleAnimation()
        {
            // Subtle breathing motion
            float breathe = MathF.Sin(_animationTime * 1.5f) * 0.02f;
            
            Bone? torso = _skeleton.GetBone("torso");
            if (torso != null)
            {
                torso.SetRotation(new Vector3(breathe, 0, 0));
            }
            
            // Reset arms and legs to neutral position
            ResetLimbs();
        }
        
        private void ApplyWalkingAnimation(float speed)
        {
            float phase = _animationTime * speed;
            
            // Arms swing opposite to legs with improved natural motion
            Bone? rightArm = _skeleton.GetBone("right_arm");
            Bone? leftArm = _skeleton.GetBone("left_arm");
            Bone? rightLeg = _skeleton.GetBone("right_leg");
            Bone? leftLeg = _skeleton.GetBone("left_leg");
            
            if (rightArm != null)
            {
                // Primary swing + secondary shoulder rotation + slight lateral sway
                float armSwing = MathF.Sin(phase) * ARM_SWING_AMOUNT;
                float secondaryMotion = MathF.Sin(phase * 2f) * ARM_SECONDARY_MOTION;
                float lateralSway = MathF.Cos(phase) * ARM_LATERAL_SWAY;
                rightArm.SetRotation(new Vector3(armSwing + secondaryMotion, lateralSway, 0));
            }
            
            if (leftArm != null)
            {
                float armSwing = MathF.Sin(phase + MathF.PI) * ARM_SWING_AMOUNT;
                float secondaryMotion = MathF.Sin((phase + MathF.PI) * 2f) * ARM_SECONDARY_MOTION;
                float lateralSway = MathF.Cos(phase + MathF.PI) * ARM_LATERAL_SWAY;
                leftArm.SetRotation(new Vector3(armSwing + secondaryMotion, lateralSway, 0));
            }
            
            if (rightLeg != null)
            {
                float legSwing = MathF.Sin(phase + MathF.PI) * LEG_SWING_AMOUNT;
                rightLeg.SetRotation(new Vector3(legSwing, 0, 0));
            }
            
            if (leftLeg != null)
            {
                float legSwing = MathF.Sin(phase) * LEG_SWING_AMOUNT;
                leftLeg.SetRotation(new Vector3(legSwing, 0, 0));
            }
            
            // Add slight torso rotation for natural gait
            Bone? torso = _skeleton.GetBone("torso");
            if (torso != null)
            {
                float torsoRotation = MathF.Sin(phase * 2) * 0.05f;
                float torsoSway = MathF.Sin(phase) * 0.02f;
                torso.SetRotation(new Vector3(torsoSway, torsoRotation, 0));
            }
        }
        
        private void ApplyBreakingAnimation()
        {
            // Swing right arm based on breaking progress
            Bone? rightArm = _skeleton.GetBone("right_arm");
            if (rightArm != null)
            {
                float swingPhase = _breakingProgress * MathF.PI;
                float armRotation = -BREAK_SWING_AMOUNT + MathF.Cos(swingPhase) * BREAK_SWING_AMOUNT;
                rightArm.SetRotation(new Vector3(armRotation, 0, 0));
            }
            
            // Keep left arm neutral
            Bone? leftArm = _skeleton.GetBone("left_arm");
            if (leftArm != null)
            {
                leftArm.SetRotation(Vector3.Zero);
            }
            
            // Lean torso slightly forward
            Bone? torso = _skeleton.GetBone("torso");
            if (torso != null)
            {
                torso.SetRotation(new Vector3(0.1f, 0, 0));
            }
        }
        
        private void ResetLimbs()
        {
            Bone? rightArm = _skeleton.GetBone("right_arm");
            Bone? leftArm = _skeleton.GetBone("left_arm");
            Bone? rightLeg = _skeleton.GetBone("right_leg");
            Bone? leftLeg = _skeleton.GetBone("left_leg");
            
            rightArm?.SetRotation(Vector3.Zero);
            leftArm?.SetRotation(Vector3.Zero);
            rightLeg?.SetRotation(Vector3.Zero);
            leftLeg?.SetRotation(Vector3.Zero);
        }
        
        private void ApplyTreadingWaterAnimation()
        {
            float phase = _animationTime * TREAD_WATER_SPEED;
            
            // Arms gently move in circular motion (like treading water)
            // Improved with more natural figure-8 pattern
            Bone? rightArm = _skeleton.GetBone("right_arm");
            Bone? leftArm = _skeleton.GetBone("left_arm");
            
            if (rightArm != null)
            {
                float armX = MathF.Sin(phase) * 0.3f;
                float armY = MathF.Cos(phase) * 0.2f;
                float armZ = MathF.Sin(phase * 0.5f) * 0.1f; // Slight roll for figure-8 motion
                rightArm.SetRotation(new Vector3(armX, armY, armZ));
            }
            
            if (leftArm != null)
            {
                float armX = MathF.Sin(phase + MathF.PI) * 0.3f;
                float armY = MathF.Cos(phase + MathF.PI) * 0.2f;
                float armZ = MathF.Sin((phase + MathF.PI) * 0.5f) * 0.1f;
                leftArm.SetRotation(new Vector3(armX, armY, armZ));
            }
            
            // Legs kick gently
            Bone? rightLeg = _skeleton.GetBone("right_leg");
            Bone? leftLeg = _skeleton.GetBone("left_leg");
            
            if (rightLeg != null)
            {
                float legSwing = MathF.Sin(phase * 1.5f) * 0.25f;
                rightLeg.SetRotation(new Vector3(legSwing, 0, 0));
            }
            
            if (leftLeg != null)
            {
                float legSwing = MathF.Sin(phase * 1.5f + MathF.PI) * 0.25f;
                leftLeg.SetRotation(new Vector3(legSwing, 0, 0));
            }
            
            // Body stays mostly upright with slight bobbing
            Bone? torso = _skeleton.GetBone("torso");
            if (torso != null)
            {
                float bob = MathF.Sin(phase * 2) * 0.03f;
                torso.SetRotation(new Vector3(bob, 0, 0));
            }
        }
        
        private void ApplySwimmingAnimation()
        {
            float phase = _animationTime * SWIM_SPEED;
            
            // Improved swimming animation - more natural arm stroke with reach/pull phases
            Bone? rightArm = _skeleton.GetBone("right_arm");
            Bone? leftArm = _skeleton.GetBone("left_arm");
            
            if (rightArm != null)
            {
                // Freestyle-like stroke: reach forward, pull back
                float reach = MathF.Sin(phase) * 0.8f;
                float lateralPull = MathF.Cos(phase) * 0.25f; // Arm sweeps out during pull phase
                float rollMotion = MathF.Sin(phase * 0.5f) * 0.15f; // Body roll effect
                rightArm.SetRotation(new Vector3(reach, lateralPull, rollMotion));
            }
            
            if (leftArm != null)
            {
                float reach = MathF.Sin(phase + MathF.PI) * 0.8f;
                float lateralPull = MathF.Cos(phase + MathF.PI) * 0.25f;
                float rollMotion = MathF.Sin((phase + MathF.PI) * 0.5f) * 0.15f;
                leftArm.SetRotation(new Vector3(reach, lateralPull, rollMotion));
            }
            
            // Flutter kick
            Bone? rightLeg = _skeleton.GetBone("right_leg");
            Bone? leftLeg = _skeleton.GetBone("left_leg");
            
            if (rightLeg != null)
            {
                float legSwing = MathF.Sin(phase * 2) * 0.4f;
                rightLeg.SetRotation(new Vector3(legSwing, 0, 0));
            }
            
            if (leftLeg != null)
            {
                float legSwing = MathF.Sin(phase * 2 + MathF.PI) * 0.4f;
                leftLeg.SetRotation(new Vector3(legSwing, 0, 0));
            }
            
            // Torso tilts forward with slight body roll for realistic freestyle
            Bone? torso = _skeleton.GetBone("torso");
            if (torso != null)
            {
                float tilt = 0.2f; // Lean forward
                float bodyRoll = MathF.Sin(phase) * 0.08f; // Subtle body roll
                torso.SetRotation(new Vector3(tilt, 0, bodyRoll));
            }
        }
        
        /// <summary>
        /// Ledge grab and pull-up animation - arms reach up and pull body over ledge
        /// </summary>
        private void ApplyLedgeGrabAnimation()
        {
            float progress = _ledgeGrabProgress;
            
            Bone? rightArm = _skeleton.GetBone("right_arm");
            Bone? leftArm = _skeleton.GetBone("left_arm");
            Bone? rightLeg = _skeleton.GetBone("right_leg");
            Bone? leftLeg = _skeleton.GetBone("left_leg");
            Bone? torso = _skeleton.GetBone("torso");
            
            if (progress < 0.4f)
            {
                // Phase 1: Arms reach up to grab ledge
                float grabPhase = progress / 0.4f;
                float armReach = MathHelper.Lerp(0f, -1.2f, grabPhase); // Arms reach overhead
                
                rightArm?.SetRotation(new Vector3(armReach, 0.1f, 0));
                leftArm?.SetRotation(new Vector3(armReach, -0.1f, 0));
                
                // Legs dangle
                rightLeg?.SetRotation(new Vector3(0.1f, 0, 0));
                leftLeg?.SetRotation(new Vector3(-0.1f, 0, 0));
            }
            else
            {
                // Phase 2: Pull up - arms pull down, body rises
                float pullPhase = (progress - 0.4f) / 0.6f;
                float armPull = MathHelper.Lerp(-1.2f, 0.3f, pullPhase); // Arms pull body up
                float legKick = MathF.Sin(pullPhase * MathF.PI) * 0.5f; // Legs kick during pull-up
                
                rightArm?.SetRotation(new Vector3(armPull, 0.1f, 0));
                leftArm?.SetRotation(new Vector3(armPull, -0.1f, 0));
                
                rightLeg?.SetRotation(new Vector3(legKick, 0, 0));
                leftLeg?.SetRotation(new Vector3(-legKick * 0.5f, 0, 0));
            }
            
            // Torso leans forward during pull-up
            if (torso != null)
            {
                float torsoPitch = progress < 0.4f ? 0.1f : MathHelper.Lerp(0.3f, 0f, (progress - 0.4f) / 0.6f);
                torso.SetRotation(new Vector3(torsoPitch, 0, 0));
            }
        }
    }
}
