using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.LaCampanella.ResonanceEnergies
{
    /// <summary>
    /// Remnant of the Bell's Harmony - Raw material dropped from La Campanella ore tiles.
    /// Can be refined into Resonant Cores at a furnace.
    /// The infernal essence of the bell's fiery song.
    /// </summary>
    public class RemnantOfTheBellsHarmony : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 55);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Chimes that pierce the veil of silence'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override void PostUpdate()
        {
            // Orange/gold fiery glow matching La Campanella's infernal theme
            Lighting.AddLight(Item.Center, 1.0f, 0.55f, 0.1f);
            
            // Orange fire particles
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, -0.8f, 150, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            // Black smoke wisps
            if (Main.rand.NextBool(20))
            {
                Dust smoke = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Smoke, 0f, -0.5f, 100, Color.Black, 0.7f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            // Occasional golden shimmer
            if (Main.rand.NextBool(30))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, 0f, 0, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Warm orange tint
            return new Color(255, 180, 100, 200);
        }
    }
}
