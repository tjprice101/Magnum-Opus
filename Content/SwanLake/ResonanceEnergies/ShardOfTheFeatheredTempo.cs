using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.SwanLake.ResonanceEnergies
{
    /// <summary>
    /// Shard of the Feathered Tempo - A crafting material dropped by Swan Lake enemies.
    /// Used to craft powerful Swan Lake equipment.
    /// Evokes the graceful, dualistic nature of Tchaikovsky's Swan Lake ballet.
    /// </summary>
    public class ShardOfTheFeatheredTempo : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void PostUpdate()
        {
            // Elegant black-white shifting glow (Swan duality)
            float time = Main.GameUpdateCount * 0.025f;
            float duality = (float)System.Math.Sin(time) * 0.5f + 0.5f;
            float brightness = 0.6f + duality * 0.3f;
            Lighting.AddLight(Item.Center, brightness, brightness, brightness * 1.05f);

            // White feather sparkle (Odette - White Swan)
            if (Main.rand.NextBool(10))
            {
                Dust white = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.SilverCoin, 0f, -0.3f, 100, default, 0.9f);
                white.noGravity = true;
                white.velocity *= 0.4f;
            }

            // Dark shimmer (Odile - Black Swan)
            if (Main.rand.NextBool(15))
            {
                Dust dark = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Smoke, 0f, 0f, 150, Color.DarkGray, 0.7f);
                dark.noGravity = true;
                dark.velocity *= 0.25f;
            }

            // Rainbow iridescent feather shimmer
            if (Main.rand.NextBool(25))
            {
                int dustType = Main.rand.Next(4) switch
                {
                    0 => DustID.PinkTorch,
                    1 => DustID.IceTorch,
                    2 => DustID.PurpleTorch,
                    _ => DustID.WhiteTorch
                };
                Dust rainbow = Dust.NewDustDirect(Item.position, Item.width, Item.height, dustType, 0f, -0.5f, 100, default, 0.5f);
                rainbow.noGravity = true;
                rainbow.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Elegant pearlescent self-illumination with subtle shift
            float time = Main.GameUpdateCount * 0.02f;
            float shift = (float)System.Math.Sin(time) * 0.2f + 0.8f;
            byte gray = (byte)(200 * shift);
            return new Color(gray, gray, (byte)(gray * 1.05f), 255);
        }
    }
}
