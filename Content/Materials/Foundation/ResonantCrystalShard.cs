using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.Foundation
{
    /// <summary>
    /// Resonant Crystal Shard - Found in underground caverns (ore veins).
    /// Base crafting ingredient for pre-hardmode musical items.
    /// </summary>
    public class ResonantCrystalShard : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.Blue;
        }

        public override void PostUpdate()
        {
            // Multi-colored musical glow when dropped in world
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.8f;
            Lighting.AddLight(Item.Center, 0.4f * pulse, 0.3f * pulse, 0.5f * pulse);

            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleCrystalShard, 0f, -0.3f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item
            CreateRecipe(3)
                .AddIngredient(ItemID.Amethyst, 1)
                .AddIngredient(ItemID.StoneBlock, 10)
                .AddTile(TileID.WorkBenches)
                .Register();

            CreateRecipe(3)
                .AddIngredient(ItemID.Topaz, 1)
                .AddIngredient(ItemID.StoneBlock, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
