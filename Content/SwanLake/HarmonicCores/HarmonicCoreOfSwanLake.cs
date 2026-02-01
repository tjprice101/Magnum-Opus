using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.SwanLake.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Swan Lake - Tier 3
    /// Unique Effect: Feathered Grace - dodge chance + orbiting feathers deal damage
    /// </summary>
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
            Item.scale = 1.25f;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 3 Harmonic Core]")
            {
                OverrideColor = new Color(220, 240, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Color(220, 240, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+8% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◁EFeathered Grace")
            {
                OverrideColor = new Color(200, 220, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  12% chance to gracefully dodge attacks")
            {
                OverrideColor = new Color(220, 235, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  Prismatic feathers orbit you and damage nearby enemies")
            {
                OverrideColor = new Color(220, 235, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Dance with the elegance of the dying swan'")
            {
                OverrideColor = new Color(160, 180, 200)
            });
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.4f, 0.5f, 0.7f);
            
            if (Main.GameUpdateCount % 40 == 0)
            {
                // Rainbow shimmer
                float hue = (Main.GameUpdateCount * 0.01f) % 1f;
                Color shimmerColor = Main.hslToRgb(hue, 0.7f, 0.85f);
                CustomParticles.GenericFlare(Item.Center, shimmerColor * 0.6f, 0.35f, 25);
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
                .AddIngredient<ShardOfTheFeatheredTempo>(10)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
}
