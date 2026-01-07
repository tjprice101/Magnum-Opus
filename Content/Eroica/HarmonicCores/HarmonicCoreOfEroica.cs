using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Eroica - Equippable core that provides combat-focused bonuses.
    /// Can be upgraded using Eroica's Resonant Energy.
    /// </summary>
    public class HarmonicCoreOfEroica : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.scale = 1.25f; // Display 25% larger
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.maxStack = 1;
            
            // Not directly usable - must be placed in Harmonic Core slot
            Item.useStyle = ItemUseStyleID.None;
            Item.UseSound = null;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 25)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 2 Harmonic Core]")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core slot (HC button)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "BaseBonus", "Base Effect: +5 Defense, +20 Max Life")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ClassTrees", "Features 4 class trees: Melee, Ranged, Magic, Summon")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "UpgradeInfo", "Upgrade with Eroica's Resonant Energy")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 150, 180)
            });
        }
    }
}
