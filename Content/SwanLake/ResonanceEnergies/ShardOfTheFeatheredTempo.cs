using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonanceEnergies
{
    /// <summary>
    /// Shard of the Feathered Tempo - Raw material dropped from Swan Lake enemies.
    /// Can be refined into Resonant Cores at a furnace.
    /// </summary>
    public class ShardOfTheFeatheredTempo : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void PostUpdate()
        {
            // Graceful white/blue glow matching Swan Lake theme
            Lighting.AddLight(Item.Center, 0.5f, 0.6f, 0.9f);
            
            // Feathery white particles
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Cloud, 0f, -0.5f, 150, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Occasional icy blue shimmer
            if (Main.rand.NextBool(30))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, 0f, 0, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Slight blue/white tint
            return new Color(220, 230, 255, 200);
        }
    }
}
