using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace TimelessTales.Audio
{
    /// <summary>
    /// Manages audio playback and effects
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
        
        public bool IsUnderwater
        {
            get => _isUnderwater;
            set
            {
                if (_isUnderwater != value)
                {
                    _isUnderwater = value;
                    ApplyUnderwaterEffect();
                }
            }
        }
        
        public float MasterVolume { get; set; } = 1.0f;
        public float SoundEffectVolume { get; set; } = 1.0f;
        
        public AudioManager()
        {
            _soundEffects = new Dictionary<string, SoundEffect>();
            _loopingSounds = new Dictionary<string, SoundEffectInstance>();
            _originalVolumes = new Dictionary<string, float>();
            _isUnderwater = false;
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
                
                // Apply underwater effect
                if (_isUnderwater)
                {
                    finalVolume *= UNDERWATER_VOLUME_MULTIPLIER;
                    finalPitch += UNDERWATER_PITCH_SHIFT;
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
                
                if (_isUnderwater)
                {
                    instance.Volume *= UNDERWATER_VOLUME_MULTIPLIER;
                    instance.Pitch = UNDERWATER_PITCH_SHIFT;
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
        /// Apply underwater effect to all currently playing looping sounds
        /// </summary>
        private void ApplyUnderwaterEffect()
        {
            foreach (var kvp in _loopingSounds)
            {
                string key = kvp.Key;
                SoundEffectInstance instance = kvp.Value;
                
                // Get original volume or use current volume as fallback
                float originalVolume = _originalVolumes.TryGetValue(key, out float vol) 
                    ? vol 
                    : SoundEffectVolume * MasterVolume;
                
                if (_isUnderwater)
                {
                    instance.Volume = originalVolume * UNDERWATER_VOLUME_MULTIPLIER;
                    instance.Pitch = UNDERWATER_PITCH_SHIFT;
                }
                else
                {
                    instance.Volume = originalVolume;
                    instance.Pitch = 0.0f;
                }
            }
        }
        
        /// <summary>
        /// Update audio system (call each frame)
        /// </summary>
        public void Update()
        {
            // Future: Update 3D audio positions, fade effects, etc.
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
