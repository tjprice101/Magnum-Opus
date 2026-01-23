using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Winter.Materials
{
    /// <summary>
    /// Winter Resonant Energy - Drops from L'Inverno, the Silent Finale.
    /// White/light blue crystalline brilliance, used for high-tier winter crafting.
    /// </summary>
    public class WinterResonantEnergy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Lime;
        }

        public override void PostUpdate()
        {
            // White/light blue crystalline brilliance
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.8f;
            float shimmer = (float)System.Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 0.9f;
            
            Lighting.AddLight(Item.Center, 0.65f * pulse * shimmer, 0.75f * pulse * shimmer, 1f * pulse * shimmer);

            if (Main.rand.NextBool(7))
            {
                // Crystalline frost particles orbiting
                float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 10f;
                
                Dust dust = Dust.NewDustDirect(Item.Center + offset, 1, 1, DustID.IceTorch, -offset.X * 0.08f, -offset.Y * 0.08f, 30, default, 0.9f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(12))
            {
                // Ice crystal sparkle
                Dust crystal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Ice, Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.3f, 0.3f), 40, default, 0.7f);
                crystal.noGravity = true;
                crystal.velocity *= 0.2f;
            }

            if (Main.rand.NextBool(20))
            {
                // White brilliance
                Dust brilliance = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 0f, 0f, 0, Color.LightCyan, 0.6f);
                brilliance.noGravity = true;
                brilliance.velocity *= 0.15f;
            }
        }
    }
}
