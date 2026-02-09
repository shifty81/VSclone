using Microsoft.Xna.Framework;
using TimelessTales.Vegetation;
using TimelessTales.World;
using Xunit;

namespace TimelessTales.Tests
{
    public class VegetationPlacementTests
    {
        [Fact]
        public void Plant_TallGrass_HasWindSway()
        {
            // Arrange & Act
            var plant = new Plant(Vector3.Zero, VegetationType.TallGrass);

            // Assert
            Assert.True(plant.WindSwayAmplitude > 0);
            Assert.True(plant.WindSwayFrequency > 0);
        }

        [Fact]
        public void Plant_TallGrass_HasLargerSwayThanShortGrass()
        {
            // Arrange
            var shortGrass = new Plant(Vector3.Zero, VegetationType.Grass);
            var tallGrass = new Plant(Vector3.Zero, VegetationType.TallGrass);

            // Assert
            Assert.True(tallGrass.WindSwayAmplitude > shortGrass.WindSwayAmplitude);
        }

        [Fact]
        public void Plant_BerryShrub_ProducesBerries()
        {
            // Arrange
            var berryShrub = new Plant(Vector3.Zero, VegetationType.BerryShrub, GrowthStage.Mature);

            // Assert
            Assert.True(berryShrub.HasBerries);
            Assert.True(berryShrub.BerryCount > 0);
        }

        [Fact]
        public void Plant_BerryShrub_CanBeHarvested()
        {
            // Arrange
            var berryShrub = new Plant(Vector3.Zero, VegetationType.BerryShrub, GrowthStage.Mature);

            // Act
            int harvested = berryShrub.HarvestBerries();

            // Assert
            Assert.True(harvested > 0);
            Assert.False(berryShrub.HasBerries);
            Assert.Equal(0, berryShrub.BerryCount);
        }

        [Fact]
        public void Plant_BerryShrub_RegrowsAfterHarvest()
        {
            // Arrange
            var berryShrub = new Plant(Vector3.Zero, VegetationType.BerryShrub, GrowthStage.Mature);
            berryShrub.HarvestBerries();
            Assert.Equal(0, berryShrub.BerryCount);

            // Act - simulate berry regrowth (180 seconds per berry)
            for (int i = 0; i < 200; i++)
            {
                berryShrub.Update(1.0f); // 200 seconds
            }

            // Assert - at least 1 berry should have regrown
            Assert.True(berryShrub.BerryCount > 0);
        }

        [Fact]
        public void Plant_BerryShrub_SeedlingHasNoBerries()
        {
            // Arrange
            var berryShrub = new Plant(Vector3.Zero, VegetationType.BerryShrub, GrowthStage.Seedling);

            // Assert
            Assert.False(berryShrub.HasBerries);
            Assert.Equal(0, berryShrub.BerryCount);
        }

        [Fact]
        public void Plant_BerryShrub_ProducesBerriesOnMaturing()
        {
            // Arrange
            var berryShrub = new Plant(Vector3.Zero, VegetationType.BerryShrub, GrowthStage.Growing);

            // Act - force to mature
            berryShrub.SetStage(GrowthStage.Mature);

            // Assert
            Assert.True(berryShrub.HasBerries);
            Assert.True(berryShrub.BerryCount > 0);
        }

        [Fact]
        public void Plant_Flowers_HasUniqueColor()
        {
            // Arrange
            var flower = new Plant(Vector3.Zero, VegetationType.Flowers, GrowthStage.Mature);

            // Act
            var color = flower.GetColorTint();

            // Assert - flowers should be pinkish
            Assert.True(color.R > color.G); // More red than green
        }

        [Fact]
        public void Plant_TallGrass_HasUniqueColor()
        {
            // Arrange
            var tallGrass = new Plant(Vector3.Zero, VegetationType.TallGrass, GrowthStage.Mature);

            // Act
            var color = tallGrass.GetColorTint();

            // Assert - dark green
            Assert.True(color.G > 0);
        }

        [Fact]
        public void BiomeDensity_DesertHasLowGrass()
        {
            // Act
            float desertGrass = VegetationManager.GetBiomeDensityMultiplier(BiomeType.Desert, VegetationType.Grass);
            float temperateGrass = VegetationManager.GetBiomeDensityMultiplier(BiomeType.Temperate, VegetationType.Grass);

            // Assert
            Assert.True(desertGrass < temperateGrass);
        }

        [Fact]
        public void BiomeDensity_TropicalHasHighGrass()
        {
            // Act
            float tropicalGrass = VegetationManager.GetBiomeDensityMultiplier(BiomeType.Tropical, VegetationType.Grass);

            // Assert
            Assert.True(tropicalGrass > 1.0f);
        }

        [Fact]
        public void BiomeDensity_BorealHasBerries()
        {
            // Act
            float borealBerries = VegetationManager.GetBiomeDensityMultiplier(BiomeType.Boreal, VegetationType.BerryShrub);

            // Assert - boreal forests are known for berry bushes
            Assert.True(borealBerries >= 1.0f);
        }

        [Fact]
        public void Plant_NonBerryShrub_CannotHarvestBerries()
        {
            // Arrange
            var shrub = new Plant(Vector3.Zero, VegetationType.Shrub, GrowthStage.Mature);

            // Act
            int harvested = shrub.HarvestBerries();

            // Assert
            Assert.Equal(0, harvested);
            Assert.False(shrub.HasBerries);
        }

        [Fact]
        public void Plant_UnderwaterPlant_HasSwayParameters()
        {
            // Arrange
            var kelp = new Plant(Vector3.Zero, VegetationType.Kelp, GrowthStage.Mature);

            // Assert
            Assert.True(kelp.WindSwayAmplitude > 0);
            Assert.True(kelp.IsUnderwaterPlant());
        }

        [Fact]
        public void VegetationManager_PlantCountTracking()
        {
            // Arrange
            var worldManager = new WorldManager(42);
            var vegManager = worldManager.VegetationManager;

            // Act
            vegManager.PlacePlant(new Vector3(0, 70, 0), VegetationType.Grass);
            vegManager.PlacePlant(new Vector3(1, 70, 0), VegetationType.Grass);
            vegManager.PlacePlant(new Vector3(2, 70, 0), VegetationType.Shrub);

            // Assert
            Assert.Equal(3, vegManager.GetPlantCount());
            Assert.Equal(2, vegManager.GetPlantCount(VegetationType.Grass));
            Assert.Equal(1, vegManager.GetPlantCount(VegetationType.Shrub));
            Assert.Equal(0, vegManager.GetPlantCount(VegetationType.BerryShrub));
        }
    }
}
