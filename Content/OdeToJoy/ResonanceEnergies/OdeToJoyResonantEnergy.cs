using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.OdeToJoy.ResonanceEnergies
{
    /// <summary>
    /// Ode to Joy Resonant Energy - Rare crafting material for powerful Ode to Joy items.
    /// Drops from the Ode to Joy boss.
    /// Theme: Joy and celebration - concentrated natural energy
    /// </summary>
    public class OdeToJoyResonantEnergy : ModItem
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
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.scale = 0.5f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The essence of pure, unbridled joy'") 
            { 
                OverrideColor = new Color(76, 175, 80) // Verdant green
            });
        }

        public override void PostUpdate()
        {
            // Joyous green glow
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f;
            Lighting.AddLight(Item.Center, 0.4f * pulse, 0.9f * pulse, 0.4f * pulse);
            
            // Nature particles
            if (Main.rand.NextBool(8))
            {
                Dust nature = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.JungleGrass, 0f, -0.8f, 150, default, 1.3f);
                nature.noGravity = true;
                nature.velocity *= 0.6f;
            }

            // Rose petal dust
            if (Main.rand.NextBool(15))
            {
                Dust petal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, 0f, 0f, 0, default, 1.0f);
                petal.noGravity = true;
                petal.velocity *= 0.4f;
            }
            
            // Golden sparkles
            if (Main.rand.NextBool(25))
            {
                Dust spark = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldCoin, Main.rand.NextFloat(-0.5f, 0.5f), -0.3f, 0, default, 0.8f);
                spark.noGravity = true;
            }
        }
    }
}
