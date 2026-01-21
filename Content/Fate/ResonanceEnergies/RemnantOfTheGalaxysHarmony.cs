using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.ResonanceEnergies
{
    /// <summary>
    /// Raw ore drops from Fate ore tiles.
    /// Can be refined into Resonant Cores at a furnace.
    /// </summary>
    public class RemnantOfTheGalaxysHarmony : ModItem
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
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void PostUpdate()
        {
            // Cosmic pink/magenta glow matching Fate's celestial theme
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f;
            Lighting.AddLight(Item.Center, 0.7f * pulse, 0.2f * pulse, 0.5f * pulse);
            
            // Pink cosmic particles
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, -0.5f, 150, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Occasional crimson shimmer
            if (Main.rand.NextBool(30))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, 0f, 0, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Cosmic pink tint
            return new Color(255, 150, 200, 200);
        }
    }
}
