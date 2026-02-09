using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace TimelessTales.Audio
{
    /// <summary>
    /// Manages audio playback and effects including underwater filtering
    /// </summary>
    public class AudioManager
    {
        private readonly Dictionary<string, SoundEffect> _soundEffects;
        private readonly Dictionary<string, SoundEffectInstance> _loopingSounds;
        private readonly Dictionary<string, float> _originalVolumes; // Track original volumes
        private bool _isUnderwater;
        
        // Audio parameters
        private const float UNDERWATER_VOLUME_MULTIPLIER = 0.6f;
        private const float UNDERWATER_PITCH_SHIFT = -0.3f; // Lower pitch underwater
        
        // Depth-based audio attenuation
        private float _submersionDepth; // 0.0 (surface) to 1.0 (fully submerged)
        private float _currentTransition; // Smooth transition 0.0 to 1.0
        private const float TRANSITION_SPEED = 3.0f; // Transition speed per second
        private const float DEEP_VOLUME_MULTIPLIER = 0.3f; // Extra reduction at full depth
        private const float DEEP_PITCH_SHIFT = -0.5f; // More muffled at full depth
        
        public bool IsUnderwater
        {
            get => _isUnderwater;
            set
            {
                if (_isUnderwater != value)
                {
                    _isUnderwater = value;
                    // Don't apply immediately - let smooth transition handle it
                }
            }
        }
        
        /// <summary>
        /// Submersion depth from 0.0 (surface) to 1.0 (fully submerged)
        /// Used for depth-based audio attenuation
        /// </summary>
        public float SubmersionDepth
        {
            get => _submersionDepth;
            set => _submersionDepth = Math.Clamp(value, 0.0f, 1.0f);
        }
        
        /// <summary>
        /// Current transition value (0 = normal, 1 = fully underwater)
        /// Smoothly interpolates for gradual audio changes
        /// </summary>
        public float CurrentTransition => _currentTransition;
        
        public float MasterVolume { get; set; } = 1.0f;
        public float SoundEffectVolume { get; set; } = 1.0f;
        
        public AudioManager()
        {
            _soundEffects = new Dictionary<string, SoundEffect>();
            _loopingSounds = new Dictionary<string, SoundEffectInstance>();
            _originalVolumes = new Dictionary<string, float>();
            _isUnderwater = false;
            _currentTransition = 0.0f;
        }
        
        /// <summary>
        /// Load a sound effect and store it with a key
        /// </summary>
        public void LoadSound(string key, SoundEffect soundEffect)
        {
            _soundEffects[key] = soundEffect;
        }
        
        /// <summary>
        /// Play a one-shot sound effect
        /// </summary>
        public void PlaySound(string key, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
        {
            if (_soundEffects.TryGetValue(key, out var sound))
            {
                float finalVolume = volume * SoundEffectVolume * MasterVolume;
                float finalPitch = pitch;
                
                // Apply underwater effect with depth-based attenuation
                if (_currentTransition > 0.01f)
                {
                    float volumeMultiplier = Lerp(1.0f, 
                        Lerp(UNDERWATER_VOLUME_MULTIPLIER, DEEP_VOLUME_MULTIPLIER, _submersionDepth), 
                        _currentTransition);
                    float pitchShift = Lerp(0.0f, 
                        Lerp(UNDERWATER_PITCH_SHIFT, DEEP_PITCH_SHIFT, _submersionDepth), 
                        _currentTransition);
                    
                    finalVolume *= volumeMultiplier;
                    finalPitch += pitchShift;
                }
                
                // Clamp values
                finalVolume = Math.Clamp(finalVolume, 0.0f, 1.0f);
                finalPitch = Math.Clamp(finalPitch, -1.0f, 1.0f);
                pan = Math.Clamp(pan, -1.0f, 1.0f);
                
                sound.Play(finalVolume, finalPitch, pan);
            }
        }
        
        /// <summary>
        /// Start a looping sound effect
        /// </summary>
        public void PlayLoopingSound(string key, float volume = 1.0f)
        {
            if (_soundEffects.TryGetValue(key, out var sound) && !_loopingSounds.ContainsKey(key))
            {
                var instance = sound.CreateInstance();
                instance.IsLooped = true;
                
                float finalVolume = volume * SoundEffectVolume * MasterVolume;
                instance.Volume = finalVolume;
                
                // Store original volume for this instance
                _originalVolumes[key] = finalVolume;
                
                if (_currentTransition > 0.01f)
                {
                    ApplyUnderwaterToInstance(instance, finalVolume);
                }
                
                instance.Play();
                _loopingSounds[key] = instance;
            }
        }
        
        /// <summary>
        /// Stop a looping sound
        /// </summary>
        public void StopLoopingSound(string key)
        {
            if (_loopingSounds.TryGetValue(key, out var instance))
            {
                instance.Stop();
                instance.Dispose();
                _loopingSounds.Remove(key);
                _originalVolumes.Remove(key);
            }
        }
        
        /// <summary>
        /// Stop all looping sounds
        /// </summary>
        public void StopAllLoopingSounds()
        {
            foreach (var instance in _loopingSounds.Values)
            {
                instance.Stop();
                instance.Dispose();
            }
            _loopingSounds.Clear();
            _originalVolumes.Clear();
        }
        
        /// <summary>
        /// Apply underwater effect to a single sound instance using current transition
        /// </summary>
        private void ApplyUnderwaterToInstance(SoundEffectInstance instance, float originalVolume)
        {
            float volumeMultiplier = Lerp(1.0f, 
                Lerp(UNDERWATER_VOLUME_MULTIPLIER, DEEP_VOLUME_MULTIPLIER, _submersionDepth), 
                _currentTransition);
            float pitchShift = Lerp(0.0f, 
                Lerp(UNDERWATER_PITCH_SHIFT, DEEP_PITCH_SHIFT, _submersionDepth), 
                _currentTransition);
            
            instance.Volume = Math.Clamp(originalVolume * volumeMultiplier, 0.0f, 1.0f);
            instance.Pitch = Math.Clamp(pitchShift, -1.0f, 1.0f);
        }
        
        /// <summary>
        /// Apply smooth underwater transition to all looping sounds
        /// </summary>
        private void ApplyUnderwaterTransition()
        {
            foreach (var kvp in _loopingSounds)
            {
                string key = kvp.Key;
                SoundEffectInstance instance = kvp.Value;
                
                float originalVolume = _originalVolumes.TryGetValue(key, out float vol) 
                    ? vol 
                    : SoundEffectVolume * MasterVolume;
                
                ApplyUnderwaterToInstance(instance, originalVolume);
            }
        }
        
        /// <summary>
        /// Update audio system (call each frame)
        /// Handles smooth underwater transition
        /// </summary>
        public void Update(float deltaTime = 0.016f)
        {
            // Smooth transition towards target state
            float target = _isUnderwater ? 1.0f : 0.0f;
            
            if (Math.Abs(_currentTransition - target) > 0.001f)
            {
                if (_currentTransition < target)
                {
                    _currentTransition = Math.Min(_currentTransition + TRANSITION_SPEED * deltaTime, target);
                }
                else
                {
                    _currentTransition = Math.Max(_currentTransition - TRANSITION_SPEED * deltaTime, target);
                }
                
                ApplyUnderwaterTransition();
            }
        }
        
        /// <summary>
        /// Linear interpolation helper
        /// </summary>
        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        
        /// <summary>
        /// Clean up audio resources
        /// </summary>
        public void Dispose()
        {
            StopAllLoopingSounds();
            
            foreach (var sound in _soundEffects.Values)
            {
                sound.Dispose();
            }
            _soundEffects.Clear();
        }
    }
}
