using TimelessTales.Entities;
using TimelessTales.Blocks;
using Xunit;

namespace TimelessTales.Tests
{
    public class MaterialPouchTests
    {
        [Fact]
        public void MaterialPouch_InitializesEmpty()
        {
            var pouch = new MaterialPouch();
            
            Assert.Equal(0f, pouch.GetCurrentWeight());
            Assert.Equal(10000f, pouch.GetMaxCapacity());
            Assert.Equal(10000f, pouch.GetRemainingCapacity());
            Assert.Equal(0f, pouch.GetFillPercentage());
        }
        
        [Fact]
        public void MaterialPouch_AddMaterial_IncreasesWeight()
        {
            var pouch = new MaterialPouch();
            
            bool added = pouch.AddMaterial(MaterialType.StoneBits, 100f);
            
            Assert.True(added);
            Assert.Equal(100f, pouch.GetCurrentWeight());
            Assert.Equal(100f, pouch.GetMaterialAmount(MaterialType.StoneBits));
        }
        
        [Fact]
        public void MaterialPouch_AddMultipleMaterials_TracksEach()
        {
            var pouch = new MaterialPouch();
            
            pouch.AddMaterial(MaterialType.StoneBits, 50f);
            pouch.AddMaterial(MaterialType.WoodFibers, 30f);
            pouch.AddMaterial(MaterialType.CopperNuggets, 20f);
            
            Assert.Equal(100f, pouch.GetCurrentWeight());
            Assert.Equal(50f, pouch.GetMaterialAmount(MaterialType.StoneBits));
            Assert.Equal(30f, pouch.GetMaterialAmount(MaterialType.WoodFibers));
            Assert.Equal(20f, pouch.GetMaterialAmount(MaterialType.CopperNuggets));
        }
        
        [Fact]
        public void MaterialPouch_AddSameMaterial_Stacks()
        {
            var pouch = new MaterialPouch();
            
            pouch.AddMaterial(MaterialType.StoneBits, 50f);
            pouch.AddMaterial(MaterialType.StoneBits, 30f);
            
            Assert.Equal(80f, pouch.GetCurrentWeight());
            Assert.Equal(80f, pouch.GetMaterialAmount(MaterialType.StoneBits));
        }
        
        [Fact]
        public void MaterialPouch_RemoveMaterial_DecreasesWeight()
        {
            var pouch = new MaterialPouch();
            pouch.AddMaterial(MaterialType.StoneBits, 100f);
            
            bool removed = pouch.RemoveMaterial(MaterialType.StoneBits, 40f);
            
            Assert.True(removed);
            Assert.Equal(60f, pouch.GetCurrentWeight());
            Assert.Equal(60f, pouch.GetMaterialAmount(MaterialType.StoneBits));
        }
        
        [Fact]
        public void MaterialPouch_RemoveMaterial_InsufficientAmount_ReturnsFalse()
        {
            var pouch = new MaterialPouch();
            pouch.AddMaterial(MaterialType.StoneBits, 50f);
            
            bool removed = pouch.RemoveMaterial(MaterialType.StoneBits, 100f);
            
            Assert.False(removed);
            Assert.Equal(50f, pouch.GetMaterialAmount(MaterialType.StoneBits));
        }
        
        [Fact]
        public void MaterialPouch_RejectsMaterialWhenFull()
        {
            var pouch = new MaterialPouch();
            
            // Fill to capacity
            bool added1 = pouch.AddMaterial(MaterialType.StoneBits, 10000f);
            // Try to add more
            bool added2 = pouch.AddMaterial(MaterialType.WoodFibers, 1f);
            
            Assert.True(added1);
            Assert.False(added2);
            Assert.Equal(10000f, pouch.GetCurrentWeight());
            Assert.Equal(1.0f, pouch.GetFillPercentage());
        }
        
        [Fact]
        public void MaterialPouch_GetFillPercentage_CalculatesCorrectly()
        {
            var pouch = new MaterialPouch();
            
            pouch.AddMaterial(MaterialType.StoneBits, 2500f);
            Assert.Equal(0.25f, pouch.GetFillPercentage(), 3);
            
            pouch.AddMaterial(MaterialType.WoodFibers, 2500f);
            Assert.Equal(0.50f, pouch.GetFillPercentage(), 3);
        }
        
        [Fact]
        public void MaterialDropTable_ReturnsCorrectDrop_ForStone()
        {
            var drop = MaterialDropTable.GetDrop(BlockType.Stone);
            
            Assert.NotNull(drop);
            Assert.Equal(MaterialType.StoneBits, drop.Value.material);
            Assert.Equal(10f, drop.Value.amount);
        }
        
        [Fact]
        public void MaterialDropTable_ReturnsCorrectDrop_ForOres()
        {
            var copperDrop = MaterialDropTable.GetDrop(BlockType.CopperOre);
            var ironDrop = MaterialDropTable.GetDrop(BlockType.IronOre);
            
            Assert.NotNull(copperDrop);
            Assert.Equal(MaterialType.CopperNuggets, copperDrop.Value.material);
            
            Assert.NotNull(ironDrop);
            Assert.Equal(MaterialType.IronNuggets, ironDrop.Value.material);
        }
        
        [Fact]
        public void MaterialDropTable_CalculateDropAmount_IncreaseWithHardness()
        {
            float baseAmount = 10f;
            float lowHardness = 0.5f;
            float highHardness = 3.0f;
            
            float lowDrop = MaterialDropTable.CalculateDropAmount(baseAmount, lowHardness);
            float highDrop = MaterialDropTable.CalculateDropAmount(baseAmount, highHardness);
            
            Assert.True(highDrop > lowDrop);
            Assert.Equal(11f, lowDrop, 2); // 10 * (1 + 0.5 * 0.2) = 11
            Assert.Equal(16f, highDrop, 2); // 10 * (1 + 3.0 * 0.2) = 16
        }
        
        [Fact]
        public void MaterialDropTable_ReturnsNull_ForAir()
        {
            var drop = MaterialDropTable.GetDrop(BlockType.Air);
            
            Assert.Null(drop);
        }
        
        [Fact]
        public void MaterialPouch_GetAllMaterials_ReturnsAllItems()
        {
            var pouch = new MaterialPouch();
            pouch.AddMaterial(MaterialType.StoneBits, 50f);
            pouch.AddMaterial(MaterialType.WoodFibers, 30f);
            
            var materials = pouch.GetAllMaterials();
            
            Assert.Equal(2, materials.Count);
            Assert.Equal(50f, materials[MaterialType.StoneBits]);
            Assert.Equal(30f, materials[MaterialType.WoodFibers]);
        }
    }
}
