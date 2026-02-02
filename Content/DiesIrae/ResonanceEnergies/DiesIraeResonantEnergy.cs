using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.DiesIrae.ResonanceEnergies
{
    /// <summary>
    /// Dies Irae Resonant Energy - Rare crafting material for powerful Dies Irae items.
    /// Drops from the Dies Irae boss.
    /// Theme: Day of Wrath - concentrated infernal energy
    /// </summary>
    public class DiesIraeResonantEnergy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.scale = 0.5f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The wrath of judgment condensed to its purest form'") 
            { 
                OverrideColor = new Color(139, 0, 0) // Blood red
            });
        }

        public override void PostUpdate()
        {
            // Powerful infernal glow
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f;
            Lighting.AddLight(Item.Center, 1.0f * pulse, 0.35f * pulse, 0.15f * pulse);
            
            // Intense fire particles
            if (Main.rand.NextBool(8))
            {
                Dust fire = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, -0.8f, 150, default, 1.3f);
                fire.noGravity = true;
                fire.velocity *= 0.6f;
            }

            // Blood red embers
            if (Main.rand.NextBool(15))
            {
                Dust ember = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, 0f, 0, default, 1.0f);
                ember.noGravity = true;
                ember.velocity *= 0.4f;
            }
            
            // Hellfire sparks
            if (Main.rand.NextBool(25))
            {
                Dust spark = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.FlameBurst, Main.rand.NextFloat(-0.5f, 0.5f), -0.3f, 0, default, 0.8f);
                spark.noGravity = true;
            }
        }
    }
}
