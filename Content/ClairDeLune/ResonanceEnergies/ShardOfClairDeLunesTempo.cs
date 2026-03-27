using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.ClairDeLune.ResonanceEnergies
{
    /// <summary>
    /// Shard of Clair de Lune's Tempo - A crystallized fragment of moonlit reverie.
    /// Drops from the Clair de Lune boss.
    /// Used in crafting high-tier Clair de Lune weapons and accessories.
    /// </summary>
    public class ShardOfClairDeLunesTempo : ModItem
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
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A shard of moonlit silence, still humming its lullaby'") { OverrideColor = new Color(150, 200, 255) });
        }

        public override void PostUpdate()
        {
            // Soft blue moonlit pulsing glow
            float pulse = 0.8f + (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f;
            Lighting.AddLight(Item.Center, 0.5f * pulse, 0.7f * pulse, 0.9f * pulse);

            // Pearl-white soft particles
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, -0.4f, 130, default, 0.9f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Night mist blue drifting particles
            if (Main.rand.NextBool(22))
            {
                Dust mist = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.BlueFairy, 0f, 0f, 0, default, 0.7f);
                mist.noGravity = true;
                mist.velocity = Main.rand.NextVector2Circular(1.0f, 1.0f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(180, 220, 255, 220);
        }
    }
}
