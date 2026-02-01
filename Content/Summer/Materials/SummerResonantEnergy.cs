using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Summer.Materials
{
    /// <summary>
    /// Summer Resonant Energy - Drops from L'Estate, Lord of the Zenith.
    /// Blazing orange/white radiance, used for high-tier summer crafting.
    /// </summary>
    public class SummerResonantEnergy : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The burning passion of the longest day'") { OverrideColor = new Color(255, 140, 50) });
        }

        public override void PostUpdate()
        {
            // Blazing orange/white radiance
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) * 0.25f + 0.9f;
            float flicker = Main.rand.NextFloat(0.9f, 1.1f);
            
            Lighting.AddLight(Item.Center, 1f * pulse * flicker, 0.65f * pulse * flicker, 0.25f * pulse * flicker);

            if (Main.rand.NextBool(6))
            {
                // Solar flame particles
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 velocity = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * Main.rand.NextFloat(0.5f, 1.5f);
                
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.SolarFlare, velocity.X, velocity.Y, 30, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.6f;
            }

            if (Main.rand.NextBool(10))
            {
                // White-hot core sparkle
                Dust core = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, Main.rand.NextFloat(-2f, 2f), -2f, 50, Color.White, 0.7f);
                core.noGravity = true;
            }

            if (Main.rand.NextBool(20))
            {
                Dust flare = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 0f, 0f, 0, default, 0.7f);
                flare.noGravity = true;
                flare.velocity *= 0.3f;
            }
        }
    }
}
