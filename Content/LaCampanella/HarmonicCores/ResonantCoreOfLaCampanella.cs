using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Common;

namespace MagnumOpus.Content.LaCampanella.HarmonicCores
{
    /// <summary>
    /// Resonant Core of La Campanella - refined crystal used for crafting powerful La Campanella equipment.
    /// Crafted from Remnant of the Bell's Harmony at a Moonlight Furnace.
    /// </summary>
    public class ResonantCoreOfLaCampanella : ModItem
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
            Item.value = Item.sellPrice(silver: 90);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfTheBellsHarmony>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Orange/gold bell-like glow when dropped
            Lighting.AddLight(Item.Center, 0.9f, 0.6f, 0.2f);
            
            // Orange flame particles
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, 0f, 100, default, 0.9f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }

            // Occasional gold shimmer
            if (Main.rand.NextBool(35))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Warm orange tint
            return new Color(255, 180, 100, 200);
        }
    }
}
