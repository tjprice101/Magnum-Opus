using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.OdeToJoy.ResonanceEnergies
{
    /// <summary>
    /// Shard of Ode to Joy's Tempo - A crystallized fragment of jubilant harmony.
    /// Drops from the Ode to Joy boss.
    /// Used in crafting high-tier Ode to Joy weapons and accessories.
    /// </summary>
    public class ShardOfOdeToJoysTempo : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A crystallized echo of universal brotherhood'") { OverrideColor = new Color(255, 200, 50) });
        }

        public override void PostUpdate()
        {
            // Warm golden pulsing glow
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f;
            Lighting.AddLight(Item.Center, 0.9f * pulse, 0.7f * pulse, 0.2f * pulse);

            // Jubilant golden sparkle
            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.5f, 120, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            // Warm amber particles
            if (Main.rand.NextBool(20))
            {
                Dust amber = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.AmberBolt, 0f, 0f, 0, default, 0.8f);
                amber.noGravity = true;
                amber.velocity = Main.rand.NextVector2Circular(1.2f, 1.2f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 220, 120, 220);
        }
    }
}
