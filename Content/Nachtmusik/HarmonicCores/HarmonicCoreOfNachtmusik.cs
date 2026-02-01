using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Nachtmusik - Tier 7 (Post-Fate)
    /// Unique Effect: Queen's Serenade - Periodically releases a nocturnal melody that heals the player
    /// and damages nearby enemies based on your damage dealt in the last few seconds.
    /// The serenade also grants a brief period of increased critical strike chance.
    /// </summary>
    public class HarmonicCoreOfNachtmusik : ModItem
    {
        // Nachtmusik theme colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.scale = 1.3f;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 100);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 7 Harmonic Core - Nocturnal]")
            {
                OverrideColor = Violet
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = Violet
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+18% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "CritBonus", "+8% Critical Strike Chance")
            {
                OverrideColor = new Color(200, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◁EQueen's Serenade")
            {
                OverrideColor = Gold
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  Every 8 seconds, releases a nocturnal melody")
            {
                OverrideColor = Violet
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  The melody heals 5% of damage dealt in the last 5 seconds")
            {
                OverrideColor = Violet
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect3", "  Nearby enemies take 15% of that damage as star damage")
            {
                OverrideColor = Violet
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect4", "  Grants +12% crit chance for 3 seconds after the serenade")
            {
                OverrideColor = new Color(255, 230, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The Queen of Radiance's melody echoes through the night'")
            {
                OverrideColor = new Color(150, 130, 180)
            });
        }

        public override void PostUpdate()
        {
            // Powerful nocturnal celestial glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.8f;
            float goldPulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f + 1.5f) * 0.15f;
            
            Lighting.AddLight(Item.Center, 
                (0.35f + goldPulse * 0.4f) * pulse, 
                (0.25f + goldPulse * 0.35f) * pulse, 
                0.65f * pulse);
            
            // Purple cosmic flare particles
            if (Main.GameUpdateCount % 18 == 0)
            {
                CustomParticles.GenericFlare(Item.Center, Violet * 0.7f, 0.5f, 28);
                CustomParticles.GenericFlare(Item.Center, Gold * 0.5f, 0.4f, 22);
            }
            
            // Deep purple cosmic dust
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -0.9f, 80, default, 1.6f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
            
            // Golden starlight
            if (Main.rand.NextBool(15))
            {
                Dust gold = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.6f, 0, default, 1.2f);
                gold.noGravity = true;
                gold.velocity *= 0.4f;
            }
            
            // Star white twinkle
            if (Main.rand.NextBool(25))
            {
                Dust star = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.WhiteTorch, 0f, 0f, 0, default, 0.8f);
                star.noGravity = true;
                star.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 25)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<RemnantOfNachtmusiksHarmony>(), 50)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
}
