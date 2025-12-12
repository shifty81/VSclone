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
        Jumping
    }

    /// <summary>
    /// Controls animations for the player character
    /// </summary>
    public class AnimationController
    {
        private readonly Skeleton _skeleton;
        private AnimationType _currentAnimation = AnimationType.Idle;
        private float _animationTime = 0f;
        private bool _isBreaking = false;
        private float _breakingProgress = 0f;
        
        // Animation parameters
        private const float WALK_SPEED = 2.0f;
        private const float RUN_SPEED = 3.0f;
        private const float ARM_SWING_AMOUNT = 0.5f;
        private const float LEG_SWING_AMOUNT = 0.4f;
        private const float BREAK_SWING_AMOUNT = 1.2f;
        
        public AnimationController(Skeleton skeleton)
        {
            _skeleton = skeleton;
        }
        
        public void Update(float deltaTime, bool isMoving, bool isSprinting, bool isBreaking, float breakProgress)
        {
            _animationTime += deltaTime;
            _isBreaking = isBreaking;
            _breakingProgress = breakProgress;
            
            // Determine current animation
            if (isBreaking)
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
            
            // Arms swing opposite to legs
            Bone? rightArm = _skeleton.GetBone("right_arm");
            Bone? leftArm = _skeleton.GetBone("left_arm");
            Bone? rightLeg = _skeleton.GetBone("right_leg");
            Bone? leftLeg = _skeleton.GetBone("left_leg");
            
            if (rightArm != null)
            {
                float armSwing = MathF.Sin(phase) * ARM_SWING_AMOUNT;
                rightArm.SetRotation(new Vector3(armSwing, 0, 0));
            }
            
            if (leftArm != null)
            {
                float armSwing = MathF.Sin(phase + MathF.PI) * ARM_SWING_AMOUNT;
                leftArm.SetRotation(new Vector3(armSwing, 0, 0));
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
            
            // Add slight torso rotation
            Bone? torso = _skeleton.GetBone("torso");
            if (torso != null)
            {
                float torsoRotation = MathF.Sin(phase * 2) * 0.05f;
                torso.SetRotation(new Vector3(0, torsoRotation, 0));
            }
        }
        
        private void ApplyBreakingAnimation()
        {
            // Swing right arm based on breaking progress
            Bone? rightArm = _skeleton.GetBone("right_arm");
            if (rightArm != null)
            {
                // Swing from up to down as breaking progresses
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
    }
}
