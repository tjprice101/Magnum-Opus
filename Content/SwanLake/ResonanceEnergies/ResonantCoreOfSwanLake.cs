using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.SwanLake.ResonanceEnergies
{
    /// <summary>
    /// Resonant Core of Swan Lake - refined crystal used for crafting powerful Swan Lake equipment.
    /// Crafted from Swan's Resonance Energy at a Moonlight Furnace.
    /// </summary>
    public class ResonantCoreOfSwanLake : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfSwansHarmony>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Graceful blue/white glow when dropped
            Lighting.AddLight(Item.Center, 0.5f, 0.7f, 0.9f);
            
            // Icy crystal particles
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }

            // Occasional feathery shimmer
            if (Main.rand.NextBool(35))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Cloud, 0f, 0f, 0, default, 0.6f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Slight blue/white tint
            return new Color(220, 235, 255, 200);
        }
    }
}
