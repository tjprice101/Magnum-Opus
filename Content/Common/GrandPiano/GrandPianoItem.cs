using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MagnumOpus.Content.Common.GrandPiano
{
    /// <summary>
    /// The Grand Piano item - allows players to pick up and relocate the mystical Grand Piano.
    /// Used to summon the musical bosses of Magnum Opus by placing Score items.
    /// </summary>
    public class GrandPianoItem : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Piano;

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
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "The mystical Grand Piano of Magnum Opus"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Place Score items on it to summon musical bosses"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The instrument through which the symphony of creation is conducted'")
            {
                OverrideColor = new Color(200, 180, 255)
            });
        }
    }
}
