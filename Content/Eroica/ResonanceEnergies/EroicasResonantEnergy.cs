using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.ResonanceEnergies
{
    public class EroicasResonantEnergy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
        }

        public override void PostUpdate()
        {
            // Heroic golden-orange glow effect when dropped in world
            Lighting.AddLight(Item.Center, 0.7f, 0.5f, 0.2f);
            
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.5f, 150, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.2f;
            }
        }
    }
}
