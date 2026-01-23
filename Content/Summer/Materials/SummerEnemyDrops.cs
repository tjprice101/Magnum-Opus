using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Summer.Materials
{
    /// <summary>
    /// Ember of Intensity - Primary bar material for Summer.
    /// Drops from Solar Pillar enemies (5%) and Lava enemies (3%).
    /// </summary>
    public class EmberOfIntensity : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
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
            // Intense burning glow
            float flicker = Main.rand.NextFloat(0.85f, 1.15f);
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 0.7f;
            Lighting.AddLight(Item.Center, 0.9f * pulse * flicker, 0.5f * pulse * flicker, 0.15f * pulse * flicker);

            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.SolarFlare, Main.rand.NextFloat(-1f, 1f), -1f, 50, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            if (Main.rand.NextBool(20))
            {
                Dust spark = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, Main.rand.NextFloat(-1.5f, 1.5f), -2f, 80, default, 0.5f);
                spark.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Sunfire Core - Summer accessory material from Mothron.
    /// Drops from Mothron (15%).
    /// </summary>
    public class SunfireCore : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void PostUpdate()
        {
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f + 0.9f;
            Lighting.AddLight(Item.Center, 1f * pulse, 0.7f * pulse, 0.2f * pulse);

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.SolarFlare, Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-0.8f, 0.8f), 30, default, 1f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Heat Scale - Summer material from Hell enemies.
    /// Drops from Lava Bat, Fire Imp, and Hell enemies (6%).
    /// </summary>
    public class HeatScale : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 30;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Orange;
        }

        public override void PostUpdate()
        {
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 0.4f;
            Lighting.AddLight(Item.Center, 0.7f * pulse, 0.35f * pulse, 0.1f * pulse);

            if (Main.rand.NextBool(25))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, -0.3f, 100, default, 0.5f);
                dust.noGravity = true;
            }
        }
    }
}
