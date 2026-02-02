using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.DiesIrae.ResonanceEnergies
{
    /// <summary>
    /// Remnant of Dies Irae's Wrath - Raw material dropped from Dies Irae ore.
    /// Can be refined into Resonant Cores at a furnace.
    /// Theme: Day of Wrath - fragments of infernal judgment
    /// </summary>
    public class RemnantOfDiesIraesWrath : ModItem
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
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Echoes of wrath that never fade'") 
            { 
                OverrideColor = new Color(139, 0, 0) // Blood red
            });
        }

        public override void PostUpdate()
        {
            // Infernal ember glow
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f;
            Lighting.AddLight(Item.Center, 0.9f * pulse, 0.25f * pulse, 0.1f * pulse);
            
            // Fire particles
            if (Main.rand.NextBool(10))
            {
                Dust fire = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, -0.7f, 150, default, 1.1f);
                fire.noGravity = true;
                fire.velocity *= 0.4f;
            }

            // Blood red embers
            if (Main.rand.NextBool(25))
            {
                Dust ember = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, 0f, 0, default, 0.9f);
                ember.noGravity = true;
                ember.velocity = Main.rand.NextVector2Circular(0.8f, 0.8f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Infernal red-orange tint
            return new Color(255, 120, 80, 220);
        }
    }
}
