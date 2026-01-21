using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.ResonanceEnergies
{
    /// <summary>
    /// Shard of Fate's Tempo - A crystallized fragment of cosmic rhythm.
    /// Drops from the Fate boss and Herald of Fate.
    /// Used in crafting high-tier Fate weapons and accessories.
    /// </summary>
    public class ShardOfFatesTempo : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void PostUpdate()
        {
            // Cosmic pulsing glow - dark pink to crimson
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f;
            Lighting.AddLight(Item.Center, 0.8f * pulse, 0.25f * pulse, 0.4f * pulse);
            
            // Dark pink cosmic particles
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, -0.8f, 120, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            // Star sparkle effect
            if (Main.rand.NextBool(20))
            {
                Dust star = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkFairy, 0f, 0f, 0, default, 0.9f);
                star.noGravity = true;
                star.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
            }

            // Occasional crimson pulse
            if (Main.rand.NextBool(25))
            {
                Dust crimson = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, -0.3f, 0, default, 0.7f);
                crimson.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Cosmic dark pink/crimson tint - matches Fate theme
            return new Color(255, 120, 180, 220);
        }
    }
}
