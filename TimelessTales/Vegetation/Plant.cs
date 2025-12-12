using Microsoft.Xna.Framework;
using System;

namespace TimelessTales.Vegetation
{
    /// <summary>
    /// Represents a single plant instance with growth tracking
    /// </summary>
    public class Plant
    {
        public Vector3 Position { get; set; }
        public VegetationType Type { get; set; }
        public GrowthStage Stage { get; private set; }
        public float GrowthProgress { get; private set; } // 0.0 to 1.0
        public float TimeToNextStage { get; private set; } // Seconds until next stage
        
        // Growth parameters
        private const float SEEDLING_TO_GROWING_TIME = 300f; // 5 minutes
        private const float GROWING_TO_MATURE_TIME = 600f;   // 10 minutes
        
        public Plant(Vector3 position, VegetationType type, GrowthStage initialStage = GrowthStage.Seedling)
        {
            Position = position;
            Type = type;
            Stage = initialStage;
            GrowthProgress = 0.0f;
            TimeToNextStage = GetTimeForStage(initialStage);
        }
        
        /// <summary>
        /// Update plant growth
        /// </summary>
        /// <param name="deltaTime">Time elapsed in seconds</param>
        /// <returns>True if the plant advanced to the next growth stage</returns>
        public bool Update(float deltaTime)
        {
            if (Stage == GrowthStage.Mature)
            {
                return false; // Already fully grown
            }
            
            TimeToNextStage -= deltaTime;
            
            if (TimeToNextStage <= 0)
            {
                // Advance to next stage
                Stage = (GrowthStage)((int)Stage + 1);
                GrowthProgress = 0.0f;
                
                if (Stage != GrowthStage.Mature)
                {
                    TimeToNextStage = GetTimeForStage(Stage);
                }
                else
                {
                    TimeToNextStage = 0;
                }
                
                return true; // Stage changed
            }
            
            // Update progress (0.0 to 1.0 for current stage)
            float stageTime = GetTimeForStage(Stage);
            GrowthProgress = 1.0f - (TimeToNextStage / stageTime);
            
            return false;
        }
        
        /// <summary>
        /// Get the time required for a growth stage in seconds
        /// </summary>
        private float GetTimeForStage(GrowthStage stage)
        {
            return stage switch
            {
                GrowthStage.Seedling => SEEDLING_TO_GROWING_TIME,
                GrowthStage.Growing => GROWING_TO_MATURE_TIME,
                GrowthStage.Mature => 0,
                _ => 0
            };
        }
        
        /// <summary>
        /// Force the plant to a specific growth stage
        /// </summary>
        public void SetStage(GrowthStage stage)
        {
            Stage = stage;
            GrowthProgress = stage == GrowthStage.Mature ? 1.0f : 0.0f;
            TimeToNextStage = stage == GrowthStage.Mature ? 0 : GetTimeForStage(stage);
        }
        
        /// <summary>
        /// Get the visual size multiplier based on growth stage
        /// </summary>
        public float GetSizeMultiplier()
        {
            return Stage switch
            {
                GrowthStage.Seedling => 0.3f + (GrowthProgress * 0.2f),  // 0.3 to 0.5
                GrowthStage.Growing => 0.5f + (GrowthProgress * 0.3f),   // 0.5 to 0.8
                GrowthStage.Mature => 0.8f + (GrowthProgress * 0.2f),    // 0.8 to 1.0
                _ => 1.0f
            };
        }
        
        /// <summary>
        /// Get the color tint based on growth stage
        /// </summary>
        public Color GetColorTint()
        {
            return Stage switch
            {
                GrowthStage.Seedling => Color.Lerp(new Color(200, 255, 200), Color.LightGreen, GrowthProgress),
                GrowthStage.Growing => Color.Lerp(Color.LightGreen, Color.Green, GrowthProgress),
                GrowthStage.Mature => Color.Green,
                _ => Color.White
            };
        }
    }
}
