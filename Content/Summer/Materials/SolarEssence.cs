using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Summer.Materials
{
    /// <summary>
    /// Solar Essence - Summer essence used in seasonal crafting.
    /// Orange sun burst with white center, drops from solar/fire enemies.
    /// </summary>
    public class SolarEssence : ModItem
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
            Item.rare = ItemRarityID.Yellow;
        }

        public override void PostUpdate()
        {
            // Orange sun burst with white center
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.25f + 0.8f;
            Lighting.AddLight(Item.Center, 0.9f * pulse, 0.6f * pulse, 0.2f * pulse);

            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.SolarFlare, Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-0.8f, 0.8f), 40, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            if (Main.rand.NextBool(18))
            {
                Dust flare = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, Main.rand.NextFloat(-1.5f, 1.5f), -1.5f, 60, default, 0.6f);
                flare.noGravity = true;
            }
        }
    }
}
