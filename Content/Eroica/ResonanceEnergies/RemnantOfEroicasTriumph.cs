using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.ResonanceEnergies
{
    /// <summary>
    /// Raw ore drops from Eroica minions and ore tiles.
    /// Can be refined into Resonant Cores at a furnace.
    /// </summary>
    public class RemnantOfEroicasTriumph : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            // Has normal gravity - falls to ground when dropped
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void PostUpdate()
        {
            // Pink/rose glow matching Eroica's cherry blossom theme
            Lighting.AddLight(Item.Center, 0.9f, 0.4f, 0.5f);
            
            // Pink sparkle particles
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, 0f, -0.5f, 150, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Occasional cherry blossom-like shimmer
            if (Main.rand.NextBool(30))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkFairy, 0f, 0f, 0, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Slight pink tint
            return new Color(255, 200, 220, 200);
        }
    }
}
