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
        
        // Wind sway parameters
        public float WindSwayAmplitude { get; private set; }
        public float WindSwayFrequency { get; private set; }
        
        // Berry yield (for berry shrubs)
        public int BerryCount { get; private set; }
        public bool HasBerries => Type == VegetationType.BerryShrub && Stage == GrowthStage.Mature && BerryCount > 0;
        
        // Growth parameters
        private const float SEEDLING_TO_GROWING_TIME = 300f; // 5 minutes
        private const float GROWING_TO_MATURE_TIME = 600f;   // 10 minutes
        private const int MAX_BERRIES = 5;
        private const float BERRY_REGROW_TIME = 180f; // 3 minutes to regrow berries
        private float _berryRegrowTimer;
        
        public Plant(Vector3 position, VegetationType type, GrowthStage initialStage = GrowthStage.Seedling)
        {
            Position = position;
            Type = type;
            Stage = initialStage;
            GrowthProgress = 0.0f;
            TimeToNextStage = GetTimeForStage(initialStage);
            
            // Set wind sway based on vegetation type
            SetWindSwayParameters();
            
            // Initialize berries for mature berry shrubs
            if (type == VegetationType.BerryShrub && initialStage == GrowthStage.Mature)
            {
                BerryCount = MAX_BERRIES;
            }
        }
        
        /// <summary>
        /// Set wind sway parameters based on vegetation type
        /// </summary>
        private void SetWindSwayParameters()
        {
            switch (Type)
            {
                case VegetationType.Grass:
                    WindSwayAmplitude = 0.05f;
                    WindSwayFrequency = 2.0f;
                    break;
                case VegetationType.TallGrass:
                    WindSwayAmplitude = 0.1f;
                    WindSwayFrequency = 1.5f;
                    break;
                case VegetationType.Shrub:
                case VegetationType.BerryShrub:
                    WindSwayAmplitude = 0.03f;
                    WindSwayFrequency = 1.0f;
                    break;
                case VegetationType.Flowers:
                    WindSwayAmplitude = 0.07f;
                    WindSwayFrequency = 1.8f;
                    break;
                case VegetationType.Kelp:
                    WindSwayAmplitude = 0.15f; // Strong underwater sway
                    WindSwayFrequency = 0.8f;
                    break;
                case VegetationType.Seaweed:
                case VegetationType.SeaGrass:
                    WindSwayAmplitude = 0.1f;
                    WindSwayFrequency = 1.2f;
                    break;
                default:
                    WindSwayAmplitude = 0.0f;
                    WindSwayFrequency = 0.0f;
                    break;
            }
        }
        
        /// <summary>
        /// Update plant growth
        /// </summary>
        /// <param name="deltaTime">Time elapsed in seconds</param>
        /// <returns>True if the plant advanced to the next growth stage</returns>
        public bool Update(float deltaTime)
        {
            // Handle berry regrowth
            if (Type == VegetationType.BerryShrub && Stage == GrowthStage.Mature && BerryCount < MAX_BERRIES)
            {
                _berryRegrowTimer += deltaTime;
                if (_berryRegrowTimer >= BERRY_REGROW_TIME)
                {
                    BerryCount = Math.Min(BerryCount + 1, MAX_BERRIES);
                    _berryRegrowTimer -= BERRY_REGROW_TIME;
                }
            }
            
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
                    
                    // Initialize berries when berry shrub matures
                    if (Type == VegetationType.BerryShrub)
                    {
                        BerryCount = MAX_BERRIES;
                    }
                }
                
                return true; // Stage changed
            }
            
            // Update progress (0.0 to 1.0 for current stage)
            float stageTime = GetTimeForStage(Stage);
            GrowthProgress = 1.0f - (TimeToNextStage / stageTime);
            
            return false;
        }
        
        /// <summary>
        /// Harvest berries from this plant (returns number harvested)
        /// </summary>
        public int HarvestBerries()
        {
            if (!HasBerries)
                return 0;
            
            int harvested = BerryCount;
            BerryCount = 0;
            _berryRegrowTimer = 0;
            return harvested;
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
            
            if (Type == VegetationType.BerryShrub && stage == GrowthStage.Mature)
            {
                BerryCount = MAX_BERRIES;
            }
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
            // Underwater plants have different color schemes
            if (IsUnderwaterPlant())
            {
                return Type switch
                {
                    VegetationType.Kelp => new Color(40, 100, 60),      // Dark green
                    VegetationType.Seaweed => new Color(60, 120, 80),   // Medium green
                    VegetationType.Coral => new Color(255, 100, 150),   // Pink/red
                    VegetationType.SeaGrass => new Color(80, 140, 100), // Light green
                    _ => Color.Green
                };
            }
            
            // Specific plant type colors
            switch (Type)
            {
                case VegetationType.TallGrass:
                    return Stage switch
                    {
                        GrowthStage.Seedling => new Color(180, 230, 180),
                        GrowthStage.Growing => new Color(100, 180, 80),
                        GrowthStage.Mature => new Color(80, 160, 60),
                        _ => Color.Green
                    };
                case VegetationType.BerryShrub:
                    if (HasBerries)
                        return new Color(180, 60, 80); // Reddish when berries present
                    return Stage switch
                    {
                        GrowthStage.Seedling => new Color(160, 200, 160),
                        GrowthStage.Growing => Color.Green,
                        GrowthStage.Mature => new Color(50, 130, 50),
                        _ => Color.Green
                    };
                case VegetationType.Flowers:
                    return Stage switch
                    {
                        GrowthStage.Seedling => new Color(200, 220, 200),
                        GrowthStage.Growing => new Color(220, 180, 200),
                        GrowthStage.Mature => new Color(255, 150, 200), // Pink flowers
                        _ => Color.Pink
                    };
            }
            
            // Land plants use growth-based colors
            return Stage switch
            {
                GrowthStage.Seedling => Color.Lerp(new Color(200, 255, 200), Color.LightGreen, GrowthProgress),
                GrowthStage.Growing => Color.Lerp(Color.LightGreen, Color.Green, GrowthProgress),
                GrowthStage.Mature => Color.Green,
                _ => Color.White
            };
        }
        
        /// <summary>
        /// Check if this is an underwater plant type
        /// </summary>
        public bool IsUnderwaterPlant()
        {
            return Type == VegetationType.Kelp || 
                   Type == VegetationType.Seaweed || 
                   Type == VegetationType.Coral || 
                   Type == VegetationType.SeaGrass;
        }
    }
}
