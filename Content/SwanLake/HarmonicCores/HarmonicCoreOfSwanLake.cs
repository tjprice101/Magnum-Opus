using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.SwanLake.HarmonicCores
{
    public class HarmonicCoreOfSwanLake : ModItem
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
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 3 Harmonic Core]")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(220, 240, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(220, 240, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ClassBonus", "All Classes: +8% Damage")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "ChromaticHeader", "◆ CHROMATIC (Offensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 150, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticBuff", "  Dying Swan: +9% damage, lower HP = more crit")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet", "  Up to +35% damage and +22% crit at low HP")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DiatonicHeader", "◇ DIATONIC (Defensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(150, 150, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicBuff", "  Swan's Grace: +10 DEF, +10% DR, +12% speed")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet", "  Build up to 25% dodge while moving")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet2", "  Heal while standing still")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
        }

        public override void PostUpdate()
        {
            // Graceful white/blue glow
            Lighting.AddLight(Item.Center, 0.4f, 0.5f, 0.7f);
            
            // Pearlescent halo effect for item in world
            if (Main.GameUpdateCount % 40 == 0)
            {
                CustomParticles.SwanLakeHalo(Item.Center, 0.4f);
            }
            
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Cloud, 0f, -0.5f, 100, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfSwanLake>(25)
                .AddIngredient<SwansResonanceEnergy>(25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
}
