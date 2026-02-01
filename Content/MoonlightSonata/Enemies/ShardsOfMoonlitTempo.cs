using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Shards of Moonlit Tempo - A crafting material dropped by Lunus and Waning Deer.
    /// Used to craft powerful Moonlight Sonata equipment.
    /// </summary>
    public class ShardsOfMoonlitTempo : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 45;
            Item.height = 45;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Material", "'Crystallized fragments of a lunar melody'")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(140, 100, 200)
            });
        }
    }
}
