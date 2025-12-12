using Microsoft.Xna.Framework;

namespace TimelessTales.Core
{
    /// <summary>
    /// Manages the game's day/night cycle and time progression
    /// </summary>
    public class TimeManager
    {
        private float _timeOfDay; // 0.0 to 1.0 representing full day cycle
        private int _dayCount;
        
        // Day cycle configuration
        private const float DAY_LENGTH_SECONDS = 600f; // 10 minutes for a full day/night cycle
        private const float SUNRISE = 0.25f;
        private const float SUNSET = 0.75f;
        
        public TimeManager()
        {
            _timeOfDay = 0.3f; // Start at morning
            _dayCount = 0;
        }
        
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Advance time
            _timeOfDay += deltaTime / DAY_LENGTH_SECONDS;
            
            // Handle day rollover
            if (_timeOfDay >= 1.0f)
            {
                _timeOfDay -= 1.0f;
                _dayCount++;
            }
        }
        
        /// <summary>
        /// Gets the current time of day (0.0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset, 1.0 = midnight)
        /// </summary>
        public float TimeOfDay => _timeOfDay;
        
        /// <summary>
        /// Gets the current day count since game start
        /// </summary>
        public int DayCount => _dayCount;
        
        /// <summary>
        /// Returns true if it's currently daytime
        /// </summary>
        public bool IsDaytime => _timeOfDay >= SUNRISE && _timeOfDay < SUNSET;
        
        /// <summary>
        /// Returns true if it's currently nighttime
        /// </summary>
        public bool IsNighttime => !IsDaytime;
        
        /// <summary>
        /// Gets the sun position angle in radians (0 = horizon east, PI = horizon west)
        /// </summary>
        public float GetSunAngle()
        {
            // Map time of day to sun angle
            // 0.0 (midnight) -> -PI/2 (below horizon)
            // 0.25 (sunrise) -> 0 (horizon east)
            // 0.5 (noon) -> PI/2 (zenith)
            // 0.75 (sunset) -> PI (horizon west)
            // 1.0 (midnight) -> 3*PI/2 (below horizon)
            
            return (_timeOfDay * MathHelper.TwoPi) - MathHelper.PiOver2;
        }
        
        /// <summary>
        /// Gets the moon position angle in radians (opposite of sun)
        /// </summary>
        public float GetMoonAngle()
        {
            return GetSunAngle() + MathHelper.Pi;
        }
        
        /// <summary>
        /// Gets the sky color based on time of day
        /// </summary>
        public Color GetSkyColor()
        {
            float time = _timeOfDay;
            
            // Night (midnight to pre-dawn): Dark blue
            if (time < 0.2f || time > 0.8f)
            {
                return new Color(10, 15, 30);
            }
            // Dawn (sunrise): Orange-pink gradient
            else if (time >= 0.2f && time < 0.3f)
            {
                float t = (time - 0.2f) / 0.1f;
                return Color.Lerp(new Color(10, 15, 30), new Color(255, 150, 100), t);
            }
            // Morning to noon: Light blue
            else if (time >= 0.3f && time < 0.5f)
            {
                float t = (time - 0.3f) / 0.2f;
                return Color.Lerp(new Color(255, 150, 100), new Color(135, 206, 235), t);
            }
            // Noon to afternoon: Bright blue
            else if (time >= 0.5f && time < 0.7f)
            {
                return new Color(135, 206, 235);
            }
            // Dusk (sunset): Orange-red gradient
            else if (time >= 0.7f && time < 0.8f)
            {
                float t = (time - 0.7f) / 0.1f;
                return Color.Lerp(new Color(135, 206, 235), new Color(255, 100, 80), t);
            }
            
            return new Color(135, 206, 235); // Default day sky
        }
        
        /// <summary>
        /// Gets the horizon color for atmospheric effect
        /// </summary>
        public Color GetHorizonColor()
        {
            float time = _timeOfDay;
            
            // Night
            if (time < 0.2f || time > 0.8f)
            {
                return new Color(20, 25, 40);
            }
            // Dawn
            else if (time >= 0.2f && time < 0.3f)
            {
                float t = (time - 0.2f) / 0.1f;
                return Color.Lerp(new Color(20, 25, 40), new Color(255, 180, 120), t);
            }
            // Day
            else if (time >= 0.3f && time < 0.7f)
            {
                return new Color(200, 220, 255);
            }
            // Dusk
            else if (time >= 0.7f && time < 0.8f)
            {
                float t = (time - 0.7f) / 0.1f;
                return Color.Lerp(new Color(200, 220, 255), new Color(255, 120, 100), t);
            }
            
            return new Color(200, 220, 255);
        }
        
        /// <summary>
        /// Gets the ambient light level (0.0 = full darkness, 1.0 = full brightness)
        /// Now includes moon lighting at night
        /// </summary>
        public float GetAmbientLight()
        {
            float time = _timeOfDay;
            float baseLight;
            
            // Night: base dim light
            if (time < 0.2f || time > 0.8f)
            {
                baseLight = 0.2f; // Slightly darker base to make moon more noticeable
            }
            // Dawn
            else if (time >= 0.2f && time < 0.3f)
            {
                float t = (time - 0.2f) / 0.1f;
                baseLight = MathHelper.Lerp(0.2f, 1.0f, t);
            }
            // Day
            else if (time >= 0.3f && time < 0.7f)
            {
                baseLight = 1.0f;
            }
            // Dusk
            else if (time >= 0.7f && time < 0.8f)
            {
                float t = (time - 0.7f) / 0.1f;
                baseLight = MathHelper.Lerp(1.0f, 0.2f, t);
            }
            else
            {
                baseLight = 1.0f;
            }
            
            // Add moon lighting during nighttime
            if (time < 0.2f || time > 0.8f)
            {
                float moonAngle = GetMoonAngle();
                float moonHeight = MathF.Sin(moonAngle);
                
                if (moonHeight > 0)
                {
                    // Moon provides additional light when above horizon (up to 0.3 additional)
                    float moonLight = moonHeight * 0.3f;
                    baseLight = Math.Min(baseLight + moonLight, 0.8f); // Cap at 0.8 for nighttime
                }
            }
            
            return baseLight;
        }
    }
}
