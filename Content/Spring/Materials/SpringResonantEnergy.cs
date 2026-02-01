using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Spring.Materials
{
    /// <summary>
    /// Spring Resonant Energy - Drops from Primavera, Herald of Bloom.
    /// White/pink/light blue swirl, used for high-tier spring crafting.
    /// </summary>
    public class SpringResonantEnergy : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Pure resonance of rebirth and rejuvenation'") { OverrideColor = new Color(255, 180, 200) });
        }

        public override void PostUpdate()
        {
            // White/pink/light blue swirling energy
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.07f) * 0.2f + 0.85f;
            float colorShift = Main.GameUpdateCount * 0.02f;
            
            // Shifting between white, pink, and light blue
            float r = 0.85f + (float)System.Math.Sin(colorShift) * 0.15f;
            float g = 0.65f + (float)System.Math.Sin(colorShift + 2f) * 0.2f;
            float b = 0.75f + (float)System.Math.Sin(colorShift + 4f) * 0.2f;
            
            Lighting.AddLight(Item.Center, r * pulse, g * pulse, b * pulse);

            if (Main.rand.NextBool(8))
            {
                // Swirling particles
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 8f;
                Color[] colors = { Color.White, new Color(255, 183, 197), new Color(173, 216, 230) };
                Color particleColor = colors[Main.rand.Next(colors.Length)];
                
                Dust dust = Dust.NewDustDirect(Item.Center + offset, 1, 1, DustID.PinkFairy, -offset.X * 0.1f, -offset.Y * 0.1f, 40, particleColor, 0.9f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            if (Main.rand.NextBool(15))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 0f, 0f, 0, default, 0.6f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.2f;
            }
        }
    }
}
