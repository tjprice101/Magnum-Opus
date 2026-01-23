using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.Foundation
{
    /// <summary>
    /// Minor Music Note - Common drop from surface enemies.
    /// Simple crafting ingredient for musical items.
    /// </summary>
    public class MinorMusicNote : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 50;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(copper: 50);
            Item.rare = ItemRarityID.White;
        }

        public override void PostUpdate()
        {
            // Gentle musical glow
            Lighting.AddLight(Item.Center, 0.25f, 0.25f, 0.3f);

            if (Main.rand.NextBool(30))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.MagicMirror, 0f, 0f, 150, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }
    }
}
