using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.Eroica.SummonItems
{
    /// <summary>
    /// Score of Eroica - A mystical musical score used at the Grand Piano to summon Eroica, God of Valor.
    /// The score contains the notation for Beethoven's Eroica Symphony, imbued with magical power.
    /// </summary>
    public class ScoreOfEroica : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 34;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.consumable = false; // NOT consumable on use - consumed by piano
            Item.useStyle = ItemUseStyleID.None; // Cannot be used directly
        }

        public override void AddRecipes()
        {
            // Crafted from Eroica materials
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfEroicasTriumph>(), 10)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
