namespace TimelessTales.Vegetation
{
    /// <summary>
    /// Represents the growth stage of a plant
    /// </summary>
    public enum GrowthStage
    {
        Seedling = 0,   // Just planted/sprouted
        Growing = 1,    // Actively growing
        Mature = 2      // Fully grown, harvestable
    }
    
    /// <summary>
    /// Types of vegetation that can grow
    /// </summary>
    public enum VegetationType
    {
        // Land vegetation
        Grass,
        TallGrass,
        Shrub,
        BerryShrub,
        Flowers,
        Wheat,
        Carrot,
        Flax,
        
        // Underwater vegetation
        Kelp,           // Tall swaying kelp
        Seaweed,        // Short seaweed
        Coral,          // Colorful coral
        SeaGrass        // Underwater grass
    }
}
