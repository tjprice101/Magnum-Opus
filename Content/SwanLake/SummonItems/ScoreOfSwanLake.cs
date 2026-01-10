using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;

namespace MagnumOpus.Content.SwanLake.SummonItems
{
    /// <summary>
    /// Score of Swan Lake - A mystical monochromatic musical score used at the Grand Piano 
    /// to summon Swan Lake, The Monochromatic Fractal.
    /// The score contains the notation for Tchaikovsky's Swan Lake, imbued with duality magic.
    /// </summary>
    public class ScoreOfSwanLake : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 13;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 34;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.consumable = false; // NOT consumable on use - consumed by piano
            Item.useStyle = ItemUseStyleID.None; // Cannot be used directly
        }

        public override void AddRecipes()
        {
            // Crafted from Swan Lake materials
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfSwansHarmony>(), 10)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfSwanLake>(), 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
