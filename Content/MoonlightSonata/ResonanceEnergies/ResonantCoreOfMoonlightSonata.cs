using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.ResonanceEnergies
{
    /// <summary>
    /// Resonant Core of Moonlight Sonata - refined crystal used for crafting powerful Moonlight equipment.
    /// Crafted from 5 Remnants of Moonlight's Harmony at a Moonlight Furnace.
    /// </summary>
    public class ResonantCoreOfMoonlightSonata : ModItem
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
            Item.value = Item.sellPrice(silver: 75);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A fragment of lunar tranquility'") { OverrideColor = new Color(140, 100, 200) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfMoonlightsHarmony>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Brighter glow when dropped
            Lighting.AddLight(Item.Center, 0.4f, 0.2f, 0.6f);
            
            if (Main.rand.NextBool(25))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleCrystalShard, 0f, 0f, 100, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }
    }
}
