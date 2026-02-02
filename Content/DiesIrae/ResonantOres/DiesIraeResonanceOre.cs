using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MagnumOpus.Common;

namespace MagnumOpus.Content.DiesIrae.ResonantOres
{
    /// <summary>
    /// Dies Irae Resonance Ore - The ore item that places the Dies Irae Resonance Ore tile.
    /// Found in the Underworld after defeating the Fate boss.
    /// Theme: Day of Wrath - infernal judgment, hellfire, damnation
    /// </summary>
    public class DiesIraeResonanceOre : ModItem
    {
        // Uses DiesIraeResonanceOreTile.png (shares texture with tile)
        public override string Texture => "MagnumOpus/Content/DiesIrae/ResonantOres/DiesIraeResonanceOreTile";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<DiesIraeResonanceOreTile>();
            Item.placeStyle = 0;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forged in the fires of final judgment, awaiting the condemned'")
            {
                OverrideColor = new Color(139, 0, 0) // Blood red
            });
        }
    }
}
