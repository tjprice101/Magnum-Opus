using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace MagnumOpus.Content.MoonlightSonata.ResonantOres
{
    /// <summary>
    /// The ore item that places the Moonlit Resonance Ore tile.
    /// </summary>
    public class MoonlitResonanceOre : ModItem
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
            Item.createTile = ModContent.TileType<MoonlitResonanceOreTile>();
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Material", "'Ore infused with lunar resonance'")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(140, 100, 200)
            });
        }
    }
}
