using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.EnigmaVariations.ResonantOres
{
    /// <summary>
    /// The ore item that places the Enigma Resonance Ore tile.
    /// </summary>
    public class EnigmaResonanceOre : ModItem
    {
        // Uses EnigmaResonanceOre.png

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<EnigmaResonanceOreTile>();
            Item.placeStyle = 0;
        }
    }
}
