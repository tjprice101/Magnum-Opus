using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.EnigmaVariations.ResonanceEnergies
{
    /// <summary>
    /// Shard of the Mystery's Tempo - A crystallized fragment of enigmatic rhythm.
    /// Drops from Enigma minibosses.
    /// Used in crafting high-tier Enigma weapons and accessories.
    /// </summary>
    public class ShardOfTheMysterysTempo : ModItem
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
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time itself questions its own rhythm'") { OverrideColor = new Color(140, 60, 200) });
        }

        public override void PostUpdate()
        {
            // Mysterious pulsing glow - purple to green
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f;
            Lighting.AddLight(Item.Center, 0.4f * pulse, 0.2f * pulse, 0.6f * pulse);
            
            // Purple mystery particles
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -0.8f, 120, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            // Eerie green flame effect
            if (Main.rand.NextBool(15))
            {
                Dust green = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GreenTorch, 0f, -0.5f, 0, default, 0.9f);
                green.noGravity = true;
                green.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }

            // Void wisp effect
            if (Main.rand.NextBool(25))
            {
                Dust wisp = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Shadowflame, 0f, 0f, 0, default, 0.7f);
                wisp.noGravity = true;
                wisp.velocity = Main.rand.NextVector2Circular(1.2f, 1.2f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Slight purple glow effect
            return new Color(200, 180, 255, 200);
        }
    }
}
