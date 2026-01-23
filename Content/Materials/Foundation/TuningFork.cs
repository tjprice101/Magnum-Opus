using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.Foundation
{
    /// <summary>
    /// Tuning Fork - Uncommon drop from cavern enemies.
    /// A metal fork that still vibrates with perfect pitch.
    /// </summary>
    public class TuningFork : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 15;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 15);
            Item.rare = ItemRarityID.Blue;
        }

        public override void PostUpdate()
        {
            // Vibrating resonance
            float vibration = (float)System.Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 0.3f;
            Lighting.AddLight(Item.Center, 0.3f * vibration, 0.35f * vibration, 0.4f * vibration);

            if (Main.rand.NextBool(25))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Silver, 0f, 0f, 80, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
    }
}
