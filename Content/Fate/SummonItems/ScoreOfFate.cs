using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.Fate.SummonItems
{
    /// <summary>
    /// Score of Fate - A mystical cosmic musical score used at the Grand Piano to summon Fate, Warden of Melodies.
    /// The score contains the notation for Beethoven's Symphony No. 5 "Fate", imbued with celestial destiny.
    /// </summary>
    public class ScoreOfFate : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 15;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 34;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.consumable = false; // NOT consumable on use - consumed by piano
            Item.useStyle = ItemUseStyleID.None; // Cannot be used directly
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Usage", "Place at a Grand Piano to invoke the Warden of Melodies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Da-da-da-DUM—four notes that echo across eternity, heralding the arrival\nof cosmic destiny itself, the conductor of all that was and will be'") 
            { 
                OverrideColor = new Color(200, 80, 120) 
            });
        }

        public override void AddRecipes()
        {
            // Crafted from Fate materials
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfTheGalaxysHarmony>(), 10)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
