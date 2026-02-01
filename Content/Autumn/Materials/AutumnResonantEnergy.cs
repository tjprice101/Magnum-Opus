using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Autumn.Materials
{
    /// <summary>
    /// Autumn Resonant Energy - Drops from Autunno, the Withering Maestro.
    /// White/brown/dark orange fade, used for high-tier autumn crafting.
    /// </summary>
    public class AutumnResonantEnergy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Lime;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The melancholy resonance of falling leaves'") { OverrideColor = new Color(200, 150, 80) });
        }

        public override void PostUpdate()
        {
            // White/brown/dark orange fade
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.75f;
            float colorShift = Main.GameUpdateCount * 0.015f;
            
            // Shifting between white, brown, and dark orange
            float r = 0.7f + (float)System.Math.Sin(colorShift) * 0.2f;
            float g = 0.45f + (float)System.Math.Sin(colorShift + 1.5f) * 0.15f;
            float b = 0.25f + (float)System.Math.Sin(colorShift + 3f) * 0.1f;
            
            Lighting.AddLight(Item.Center, r * pulse, g * pulse, b * pulse);

            if (Main.rand.NextBool(8))
            {
                // Falling leaf particles in spiral
                float angle = Main.GameUpdateCount * 0.05f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 6f;
                
                Dust dust = Dust.NewDustDirect(Item.Center + offset, 1, 1, DustID.AmberBolt, offset.X * 0.05f, 0.5f, 60, default, 0.8f);
                dust.noGravity = false;
                dust.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(15))
            {
                // White wisp rising
                Dust wisp = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Smoke, 0f, -0.7f, 120, Color.White, 0.6f);
                wisp.noGravity = true;
                wisp.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 0f, 0f, 0, default, 0.5f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.2f;
            }
        }
    }
}
