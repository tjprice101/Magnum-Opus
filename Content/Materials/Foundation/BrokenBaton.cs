using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.Foundation
{
    /// <summary>
    /// Broken Baton - Rare drop from dungeon chests.
    /// A snapped conductor's baton that still resonates with commanding power.
    /// </summary>
    public class BrokenBaton : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 8;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Orange;
        }

        public override void PostUpdate()
        {
            // Residual conductor's energy
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.25f;
            Lighting.AddLight(Item.Center, 0.3f * pulse, 0.25f * pulse, 0.2f * pulse);

            if (Main.rand.NextBool(35))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldCoin, 0f, 0f, 100, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.25f;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item
            CreateRecipe()
                .AddIngredient(ItemID.Bone, 10)
                .AddIngredient(ItemID.GoldBar, 2)
                .AddIngredient<MinorMusicNote>(3)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.Bone, 10)
                .AddIngredient(ItemID.PlatinumBar, 2)
                .AddIngredient<MinorMusicNote>(3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
