using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;

namespace MagnumOpus.Content.Summer.Materials
{
    /// <summary>
    /// Dormant Summer Core - Found in Desert/Ocean chests, becomes active in Hardmode.
    /// Contains latent summer energy waiting to ignite.
    /// </summary>
    public class DormantSummerCore : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void PostUpdate()
        {
            // Faded orange/white - dormant summer heat
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.4f;
            Lighting.AddLight(Item.Center, 0.6f * pulse, 0.4f * pulse, 0.3f * pulse);

            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.OrangeTorch, 0f, -0.3f, 120, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.25f;
            }

            if (Main.rand.NextBool(35))
            {
                Dust spark = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, Main.rand.NextFloat(-0.5f, 0.5f), -0.5f, 100, default, 0.5f);
                spark.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item (Hardmode progression)
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.AncientCloth, 3)
                .AddIngredient<ResonantCrystalShard>(8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
