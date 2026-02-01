using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.ResonantOres
{
    /// <summary>
    /// The ore item that places the Fate Resonance Ore tile.
    /// </summary>
    public class FateResonanceOre : ModItem
    {
        // Uses FateResonanceOre.png

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<FateResonanceOreTile>();
            Item.placeStyle = 0;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Crystallized destiny, humming with the inevitability of the cosmos'")
            {
                OverrideColor = new Color(180, 40, 80)
            });
        }
    }
}
