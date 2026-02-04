using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Projectiles;

namespace MagnumOpus.Content.ClairDeLune.ResonanceEnergies
{
    /// <summary>
    /// Remnant of Clair de Lune's Harmony - Dropped by the Clair de Lune boss
    /// Used to craft Resonant Core of Clair de Lune and other advanced materials.
    /// Theme: Shattered clockwork fragments, temporal echoes
    /// </summary>
    public class RemnantOfClairDeLunesHarmony : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1, silver: 50); // Slightly higher than Ode to Joy
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void PostUpdate()
        {
            // Subtle temporal flicker
            float flicker = 0.7f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.12f + Item.whoAmI);
            Lighting.AddLight(Item.Center, ClairDeLuneColors.Crimson.ToVector3() * 0.25f * flicker);
            
            // Occasional clockwork dust
            if (Main.rand.NextBool(12))
            {
                Dust remnant = Dust.NewDustPerfect(
                    Item.Center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Silver,
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    50,
                    ClairDeLuneColors.DarkGray,
                    0.5f
                );
                remnant.noGravity = true;
            }
            
            // Crimson temporal spark
            if (Main.rand.NextBool(18))
            {
                Dust spark = Dust.NewDustPerfect(
                    Item.Center,
                    DustID.GemRuby,
                    -Vector2.UnitY * 0.5f + Main.rand.NextVector2Circular(0.2f, 0.2f),
                    0,
                    ClairDeLuneColors.Crimson,
                    0.4f
                );
                spark.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.08f);
            return Color.Lerp(ClairDeLuneColors.DarkGray, ClairDeLuneColors.Crimson, 0.4f) * pulse;
        }
    }
}
