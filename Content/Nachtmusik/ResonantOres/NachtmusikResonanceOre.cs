using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Nachtmusik.ResonantOres
{
    /// <summary>
    /// The ore item that places the Nachtmusik Resonance Ore tile.
    /// Post-Fate content - spawns in Underground after Fate is defeated.
    /// </summary>
    public class NachtmusikResonanceOre : ModItem
    {
        // Uses NachtmusikResonanceOreTile.png (shares texture)
        public override string Texture => "MagnumOpus/Content/Nachtmusik/ResonantOres/NachtmusikResonanceOreTile";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 75);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<NachtmusikResonanceOreTile>();
            Item.placeStyle = 0;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Celestial ore imbued with the radiance of the night sky'")
            {
                OverrideColor = new Color(123, 104, 238)
            });
        }
    }
}
