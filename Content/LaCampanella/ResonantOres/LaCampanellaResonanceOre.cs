using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.LaCampanella.ResonantOres
{
    /// <summary>
    /// The ore item that places the La Campanella Resonance Ore tile.
    /// </summary>
    public class LaCampanellaResonanceOre : ModItem
    {
        // Uses LaCampanellaResonanceOre.png

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<LaCampanellaResonanceOreTile>();
            Item.placeStyle = 0;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Ore that hums with the resonance of an ancient bell'") { OverrideColor = new Color(255, 140, 40) });
        }
    }
}
