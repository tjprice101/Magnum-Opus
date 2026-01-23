using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.LaCampanella.Bosses;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.LaCampanella.SummonItems
{
    /// <summary>
    /// Score of La Campanella - A mystical musical score used at the Grand Piano to summon La Campanella, Chime of Life.
    /// The score contains the notation for Liszt's La Campanella, imbued with infernal fire.
    /// </summary>
    public class ScoreOfLaCampanella : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 11;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 34;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.consumable = false; // NOT consumable on use - consumed by piano
            Item.useStyle = ItemUseStyleID.None; // Cannot be used directly
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Usage", "Place at a Grand Piano to invoke the Chime of Life"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each note burns with the fury of an infernal bell—play this score,\nand summon forth flames that have sung since the dawn of creation'") 
            { 
                OverrideColor = new Color(255, 140, 40) 
            });
        }

        public override void AddRecipes()
        {
            // Crafted from La Campanella materials
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfTheInfernalBell>(), 10)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
