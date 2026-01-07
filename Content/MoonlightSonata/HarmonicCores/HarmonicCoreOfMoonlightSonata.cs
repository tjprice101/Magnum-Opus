using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Moonlight Sonata - Equippable core that provides magic-focused bonuses.
    /// Can be upgraded using Moonlight's Resonant Energy.
    /// </summary>
    public class HarmonicCoreOfMoonlightSonata : ModItem
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
            Item.value = Item.sellPrice(gold: 15);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.maxStack = 1;
            
            // Not directly usable - must be placed in Harmonic Core slot
            Item.useStyle = ItemUseStyleID.None;
            Item.UseSound = null;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 25)
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 25)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 1 Harmonic Core]")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(180, 150, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core slot (HC button)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(180, 150, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "BaseBonus", "Base Effect: +20 Max Mana, +1 Mana Regen")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(150, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ClassTrees", "Features 4 class trees: Melee, Ranged, Magic, Summon")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "UpgradeInfo", "Upgrade with Moonlight's Resonant Energy")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 180, 255)
            });
        }
    }
}
