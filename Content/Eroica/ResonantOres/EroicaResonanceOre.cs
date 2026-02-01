using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.ResonantOres
{
    /// <summary>
    /// The ore item that places the Eroica Resonance Ore tile.
    /// </summary>
    public class EroicaResonanceOre : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<EroicaResonanceOreTile>();
            Item.rare = ModContent.RarityType<EroicaRarity>();
            Item.value = Item.sellPrice(silver: 50);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Material", "'Ore forged in the flames of triumph'") { OverrideColor = new Color(200, 50, 50) });
        }
    }
}
