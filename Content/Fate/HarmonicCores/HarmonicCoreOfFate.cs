using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Fate - Tier 6 (Ultimate)
    /// Unique Effect: Cosmic Destiny - hits leave cosmic marks that explode for massive bonus damage
    /// </summary>
    public class HarmonicCoreOfFate : ModItem
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
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ItemRarityID.Red;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 6 Harmonic Core - Ultimate]")
            {
                OverrideColor = new Color(255, 100, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Color(255, 100, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+15% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◆ Cosmic Destiny")
            {
                OverrideColor = new Color(255, 100, 120)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  Every hit leaves a cosmic mark on enemies")
            {
                OverrideColor = new Color(255, 150, 160)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  After a brief delay, marks explode in cosmic flares")
            {
                OverrideColor = new Color(255, 150, 160)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect3", "  Cosmic flares deal 40% of the original hit as bonus damage")
            {
                OverrideColor = new Color(255, 150, 160)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The strongest Harmonic Core, forged by destiny itself'")
            {
                OverrideColor = new Color(180, 120, 140)
            });
        }

        public override void PostUpdate()
        {
            // Powerful cosmic glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(Item.Center, 0.7f * pulse, 0.3f * pulse, 0.5f * pulse);
            
            // Cosmic flare particles
            if (Main.GameUpdateCount % 20 == 0)
            {
                CustomParticles.GenericFlare(Item.Center, new Color(255, 100, 120) * 0.6f, 0.45f, 25);
                CustomParticles.GenericFlare(Item.Center, new Color(200, 80, 120) * 0.4f, 0.35f, 20);
            }
            
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, -0.8f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.6f;
            }
            
            if (Main.rand.NextBool(25))
            {
                Dust dust2 = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.RedTorch, 0f, 0f, 100, default, 1.0f);
                dust2.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.ResonantCoreOfFate>(), 25)
                .AddIngredient<FateResonantEnergy>(25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
}
