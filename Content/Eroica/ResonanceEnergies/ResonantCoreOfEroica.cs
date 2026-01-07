using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.ResonanceEnergies
{
    /// <summary>
    /// Resonant Core of Eroica - refined crystal used for crafting powerful Eroica equipment.
    /// Crafted from 5 Remnants of Eroica's Triumph at a Moonlight Furnace.
    /// </summary>
    public class ResonantCoreOfEroica : ModItem
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
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfEroicasTriumph>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Pink/rose heroic glow when dropped
            Lighting.AddLight(Item.Center, 0.9f, 0.4f, 0.5f);
            
            // Pink crystal particles
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }

            // Occasional shimmer
            if (Main.rand.NextBool(35))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkFairy, 0f, 0f, 0, default, 0.6f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Slight pink tint
            return new Color(255, 200, 220, 200);
        }
    }
}
