using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonanceEnergies
{
    /// <summary>
    /// Remnant of Swan's Harmony - Raw ore drops from Swan Lake ore tiles.
    /// Can be refined into Resonant Cores at a furnace.
    /// </summary>
    public class RemnantOfSwansHarmony : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            // Has normal gravity - falls to ground when dropped
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A whisper of grace frozen in crystalline form'") { OverrideColor = new Color(240, 240, 255) });
        }

        public override void PostUpdate()
        {
            // Graceful white/blue glow matching Swan Lake theme
            Lighting.AddLight(Item.Center, 0.5f, 0.6f, 0.9f);
            
            // White feathery particles
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Cloud, 0f, -0.5f, 150, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Occasional icy shimmer
            if (Main.rand.NextBool(30))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, 0f, 0, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Slight blue/white tint
            return new Color(220, 235, 255, 200);
        }
    }
}
