using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Summer.Materials
{
    /// <summary>
    /// Solstice Bar - Summer-themed crafting bar.
    /// Radiant orange with white sheen, forged from Embers of Intensity and Solar Essence.
    /// </summary>
    public class SolsticeBar : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = -1;
        }

        public override void PostUpdate()
        {
            // Radiant orange with white sheen
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.7f;
            Lighting.AddLight(Item.Center, 0.8f * pulse, 0.5f * pulse, 0.2f * pulse);

            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.SolarFlare, 0f, -0.5f, 60, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.35f;
            }

            if (Main.rand.NextBool(30))
            {
                Dust spark = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, Main.rand.NextFloat(-1f, 1f), -1f, 80, default, 0.5f);
                spark.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ModContent.ItemType<EmberOfIntensity>(), 3)
                .AddIngredient(ModContent.ItemType<SolarEssence>(), 1)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}
