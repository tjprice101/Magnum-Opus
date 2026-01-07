using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonanceEnergies
{
    public class FateResonantEnergy : ModItem
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
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Red;
            Item.scale = 0.5f;
        }

        public override void PostUpdate()
        {
            // Powerful crimson/pink destiny glow
            Lighting.AddLight(Item.Center, 0.8f, 0.3f, 0.5f);
            
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, -0.5f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            if (Main.rand.NextBool(20))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.RedTorch, 0f, 0f, 0, default, 0.9f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.3f;
            }
        }
    }
}
