using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.Projectiles;

namespace MagnumOpus.Content.ClairDeLune.ResonantOres
{
    /// <summary>
    /// Clair de Lune Resonance Ore Item - FINAL BOSS TIER
    /// The ore item that places the Clair de Lune Resonance Ore tile.
    /// Post-Ode to Joy content - spawns in sky cloud pods after Ode to Joy boss is defeated.
    /// Theme: Temporal clockwork, crystallized time, crimson energy
    /// </summary>
    public class ClairDeLuneResonanceOre : ModItem
    {
        // Uses ClairDeLuneResonanceOreTile.png (shares texture)
        public override string Texture => "MagnumOpus/Content/ClairDeLune/ResonantOres/ClairDeLuneResonanceOreTile";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1); // Higher than other ores - FINAL BOSS tier
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<ClairDeLuneResonanceOreTile>();
            Item.placeStyle = 0;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Temporal ore crystallized from the shattered remnants of time itself'")
            {
                OverrideColor = ClairDeLuneColors.Crystal
            });
        }
        
        public override void PostUpdate()
        {
            // Temporal glow for dropped ore
            float pulse = 0.75f + 0.25f * (float)System.Math.Sin(Main.GameUpdateCount * 0.08f);
            Lighting.AddLight(Item.Center, ClairDeLuneColors.Crimson.ToVector3() * 0.3f * pulse);
        }
    }
}
