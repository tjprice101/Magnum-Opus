using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Winter.Materials
{
    /// <summary>
    /// Shard of Stillness - Primary bar material for Winter.
    /// Drops from Ice Queen (15%) and Hardmode Ice enemies (2%).
    /// </summary>
    public class ShardOfStillness : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void PostUpdate()
        {
            // Frozen stillness glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.03f) * 0.1f + 0.5f;
            Lighting.AddLight(Item.Center, 0.45f * pulse, 0.5f * pulse, 0.65f * pulse);

            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, -0.3f, 80, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.25f;
            }
        }
    }

    /// <summary>
    /// Frozen Core - Winter material from Ice Golem.
    /// Drops from Ice Golem (20%).
    /// </summary>
    public class FrozenCore : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void PostUpdate()
        {
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.7f;
            Lighting.AddLight(Item.Center, 0.5f * pulse, 0.6f * pulse, 0.9f * pulse);

            if (Main.rand.NextBool(10))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 8f;
                Dust dust = Dust.NewDustDirect(Item.Center + offset, 1, 1, DustID.IceTorch, -offset.X * 0.1f, -offset.Y * 0.1f, 60, default, 0.8f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Icicle Coronet - Winter material from Ice Queen.
    /// Drops from Ice Queen (10%).
    /// </summary>
    public class IcicleCoronet : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void PostUpdate()
        {
            float shimmer = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.65f;
            Lighting.AddLight(Item.Center, 0.55f * shimmer, 0.65f * shimmer, 0.85f * shimmer);

            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Ice, 0f, 0f, 50, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }
    }

    /// <summary>
    /// Permafrost Shard - Common winter material from Ice enemies.
    /// Drops from any Hardmode Ice enemy (3%).
    /// </summary>
    public class PermafrostShard : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 50;
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 15);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.2f, 0.25f, 0.35f);

            if (Main.rand.NextBool(30))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Ice, 0f, 0f, 100, default, 0.4f);
                dust.noGravity = true;
            }
        }
    }
}
