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
    /// Clair de Lune Resonant Energy - FINAL BOSS TIER crafting material
    /// Dropped by Clair de Lune boss and used to craft all Clair de Lune equipment.
    /// Theme: Temporal clockwork energy, crystallized time fragments
    /// </summary>
    public class ClairDeLuneResonantEnergy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 15); // Higher than Ode to Joy (12g)
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void PostUpdate()
        {
            // Temporal clockwork glow
            float pulse = 0.8f + 0.4f * (float)Math.Sin(Main.GameUpdateCount * 0.08f);
            
            // Dark gray with crimson core lighting
            Lighting.AddLight(Item.Center, ClairDeLuneColors.DarkGray.ToVector3() * 0.4f * pulse);
            Lighting.AddLight(Item.Center, ClairDeLuneColors.Crimson.ToVector3() * 0.3f * pulse);
            
            // Clockwork dust trail
            if (Main.rand.NextBool(4))
            {
                // Dark gray clockwork dust
                Dust gearDust = Dust.NewDustPerfect(
                    Item.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Silver,
                    Main.rand.NextVector2Circular(0.5f, 0.5f) - Vector2.UnitY * 0.3f,
                    100,
                    ClairDeLuneColors.DarkGray,
                    0.8f
                );
                gearDust.noGravity = true;
                gearDust.fadeIn = 1.2f;
            }
            
            // Crimson energy sparks
            if (Main.rand.NextBool(6))
            {
                Dust crimsonSpark = Dust.NewDustPerfect(
                    Item.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GemRuby,
                    Main.rand.NextVector2Circular(0.8f, 0.8f),
                    0,
                    ClairDeLuneColors.Crimson,
                    0.7f
                );
                crimsonSpark.noGravity = true;
            }
            
            // Crystal shimmer
            if (Main.rand.NextBool(8))
            {
                Dust crystal = Dust.NewDustPerfect(
                    Item.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.GemDiamond,
                    -Vector2.UnitY * 0.4f,
                    0,
                    ClairDeLuneColors.Crystal * 0.8f,
                    0.6f
                );
                crystal.noGravity = true;
            }
            
            // Brass accent sparks
            if (Main.rand.NextBool(10))
            {
                Dust brass = Dust.NewDustPerfect(
                    Item.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(0.6f, 0.6f),
                    0,
                    ClairDeLuneColors.Brass,
                    0.5f
                );
                brass.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Self-illuminating temporal glow
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            return Color.Lerp(ClairDeLuneColors.DarkGray, ClairDeLuneColors.Crystal, 0.3f) * pulse;
        }
    }
}
