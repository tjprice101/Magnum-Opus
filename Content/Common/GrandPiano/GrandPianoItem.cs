using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Common.GrandPiano
{
    /// <summary>
    /// The Grand Piano item - allows players to pick up and relocate the mystical Grand Piano.
    /// Used to summon the musical bosses of Magnum Opus by placing Score items.
    /// </summary>
    public class GrandPianoItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 32;
            Item.maxStack = 1; // Only one can exist
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ItemRarityID.Purple; // Endgame rarity
            Item.createTile = ModContent.TileType<GrandPianoTile>();
        }
    }
}
