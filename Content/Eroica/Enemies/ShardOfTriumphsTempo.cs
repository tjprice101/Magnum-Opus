using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Shard of Triumph's Tempo - A crafting material dropped by Stolen Valor.
    /// Used to craft powerful Eroica equipment.
    /// </summary>
    public class ShardOfTriumphsTempo : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void PostUpdate()
        {
            // Golden/red glow effect when dropped
            Lighting.AddLight(Item.Center, 0.7f, 0.3f, 0.2f);

            // Occasional golden sparkle
            if (Main.rand.NextBool(15))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, 0f, 100, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.3f;
            }

            // Occasional red particle
            if (Main.rand.NextBool(20))
            {
                Dust red = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 0.6f);
                red.noGravity = true;
                red.velocity *= 0.2f;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Slightly self-illuminated
            return Color.Lerp(lightColor, Color.White, 0.3f);
        }
    }
}
