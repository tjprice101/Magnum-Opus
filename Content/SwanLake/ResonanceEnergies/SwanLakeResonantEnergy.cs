using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonanceEnergies
{
    public class SwanLakeResonantEnergy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Cyan;
            Item.scale = 0.5f;
        }

        public override void PostUpdate()
        {
            // Graceful white/blue glow for swan lake theme
            Lighting.AddLight(Item.Center, 0.5f, 0.6f, 0.8f);
            
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Cloud, 0f, -0.5f, 150, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Ice, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.2f;
            }
        }
    }
}
