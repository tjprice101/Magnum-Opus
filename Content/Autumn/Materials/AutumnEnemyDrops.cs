using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Autumn.Materials
{
    /// <summary>
    /// Leaf of Ending - Primary bar material for Autumn.
    /// Drops from Pumpking (12%) and Eclipse enemies (3%).
    /// </summary>
    public class LeafOfEnding : ModItem
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
            // Autumn decay glow - brown and dark orange
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.45f;
            Lighting.AddLight(Item.Center, 0.6f * pulse, 0.35f * pulse, 0.15f * pulse);

            if (Main.rand.NextBool(18))
            {
                // Falling leaf motion
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.AmberBolt, Main.rand.NextFloat(-0.3f, 0.3f), 0.4f, 100, default, 0.6f);
                dust.noGravity = false;
                dust.velocity *= 0.5f;
            }
        }
    }

    /// <summary>
    /// Twilight Wing Fragment - Autumn material from Mothron.
    /// Drops from Mothron (10%).
    /// </summary>
    public class TwilightWingFragment : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 15;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void PostUpdate()
        {
            // Twilight purple-orange shimmer
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.5f;
            float shift = (float)System.Math.Sin(Main.GameUpdateCount * 0.03f);
            Lighting.AddLight(Item.Center, (0.5f + shift * 0.2f) * pulse, 0.3f * pulse, (0.5f - shift * 0.2f) * pulse);

            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Shadowflame, 0f, -0.3f, 80, default, 0.6f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Death's Note - Autumn material from Reaper.
    /// Drops from Reaper (8%).
    /// </summary>
    public class DeathsNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void PostUpdate()
        {
            // Eerie death glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.2f + 0.4f;
            Lighting.AddLight(Item.Center, 0.3f * pulse, 0.2f * pulse, 0.35f * pulse);

            if (Main.rand.NextBool(25))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Wraith, 0f, -0.4f, 120, default, 0.5f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Decay Fragment - Common autumn material from Eclipse enemies.
    /// Drops from any Eclipse enemy (4%).
    /// </summary>
    public class DecayFragment : ModItem
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
            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.Orange;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.25f, 0.15f, 0.1f);

            if (Main.rand.NextBool(30))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Smoke, 0f, 0f, 150, new Color(139, 69, 19), 0.4f);
                dust.noGravity = true;
            }
        }
    }
}
