using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Spring.Materials
{
    /// <summary>
    /// Blossom Essence - Spring essence used in seasonal crafting.
    /// Pink petal glow with white center, drops from spring-themed enemies.
    /// </summary>
    public class BlossomEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 15;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
        }

        public override void PostUpdate()
        {
            // Pink petal glow with white center
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.7f;
            Lighting.AddLight(Item.Center, 0.7f * pulse, 0.45f * pulse, 0.55f * pulse);

            if (Main.rand.NextBool(12))
            {
                Color petalColor = Main.rand.NextBool(3) ? Color.White : new Color(255, 183, 197);
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkFairy, Main.rand.NextFloat(-0.5f, 0.5f), -0.6f, 60, petalColor, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, 0f, 0f, 0, default, 0.5f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.2f;
            }
        }
    }
}
