using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.EnigmaVariations.ResonanceEnergies
{
    /// <summary>
    /// Resonant Core of Enigma - refined crystal used for crafting powerful Enigma equipment.
    /// Crafted from 5 Remnants of Mysteries at a Moonlight Furnace.
    /// </summary>
    public class ResonantCoreOfEnigma : ModItem
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
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfMysteries>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Purple/green mysterious glow when dropped
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f;
            Lighting.AddLight(Item.Center, 0.4f * pulse, 0.2f * pulse, 0.6f * pulse);
            
            // Purple mystery particles
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -0.5f, 100, default, 0.9f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Green flame accent
            if (Main.rand.NextBool(25))
            {
                Dust green = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GreenTorch, 0f, -0.3f, 0, default, 0.7f);
                green.noGravity = true;
                green.velocity *= 0.2f;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Slight purple glow effect
            return new Color(200, 180, 255, 200);
        }
    }
}
