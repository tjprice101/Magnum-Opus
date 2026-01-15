using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Enigma - Tier 5
    /// Unique Effect: Mystery Shield + Prismatic Flares - reflect projectiles and shoot prismatic flares
    /// </summary>
    public class HarmonicCoreOfEnigma : ModItem
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
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ItemRarityID.Lime;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 5 Harmonic Core]")
            {
                OverrideColor = new Color(150, 255, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Color(150, 255, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+12% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◆ Prismatic Flares")
            {
                OverrideColor = new Color(150, 255, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  15% chance to deflect incoming damage")
            {
                OverrideColor = new Color(180, 255, 220)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  Periodically fires prismatic flares at nearby enemies")
            {
                OverrideColor = new Color(180, 255, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The answer to every question is another mystery'")
            {
                OverrideColor = new Color(100, 180, 140)
            });
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.2f, 0.6f, 0.3f);
            
            // Prismatic pulse
            if (Main.GameUpdateCount % 25 == 0)
            {
                float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                Color pulseColor = Main.hslToRgb(hue, 1f, 0.7f);
                CustomParticles.GenericFlare(Item.Center, pulseColor * 0.6f, 0.4f, 20);
            }
            
            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GreenTorch, 0f, -0.5f, 100, default, 1.3f);
                dust.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfEnigma>(25)
                .AddIngredient<EnigmaResonantEnergy>(25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
    
    public class ResonantCoreOfEnigma : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 99;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ItemRarityID.Lime;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.2f, 0.5f, 0.3f);
        }
    }
}
