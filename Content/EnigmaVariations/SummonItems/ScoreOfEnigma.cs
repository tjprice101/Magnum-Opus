using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.EnigmaVariations.SummonItems
{
    /// <summary>
    /// Score of Enigma - A mystical musical score used at the Grand Piano to summon Enigma, The Hollow Mystery.
    /// The score contains the notation for Elgar's Enigma Variations, imbued with arcane mystery.
    /// </summary>
    public class ScoreOfEnigma : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 14;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 34;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.consumable = false; // NOT consumable on use - consumed by piano
            Item.useStyle = ItemUseStyleID.None; // Cannot be used directly
        }

        public override void AddRecipes()
        {
            // Crafted from Enigma materials
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfMysteries>(), 10)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEnigma>(), 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
