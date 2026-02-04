using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.OdeToJoy.ResonanceEnergies
{
    /// <summary>
    /// Remnant of Ode to Joy's Bloom - Raw crafting material dropped by Ode to Joy boss
    /// Can be refined into Resonant Core of Ode to Joy
    /// Theme: Joy and celebration - blossom essence
    /// </summary>
    public class RemnantOfOdeToJoysBloom : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A fragment of nature's endless celebration'") 
            { 
                OverrideColor = new Color(255, 182, 193) // Rose pink
            });
        }

        public override void PostUpdate()
        {
            // Soft green/pink glow
            float pulse = 0.75f + (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f;
            Lighting.AddLight(Item.Center, 0.35f * pulse, 0.7f * pulse, 0.4f * pulse);
            
            // Petal particles
            if (Main.rand.NextBool(15))
            {
                Dust petal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, 0f, 0f, 100, default, 0.8f);
                petal.noGravity = true;
                petal.velocity *= 0.3f;
            }
            
            // Green nature dust
            if (Main.rand.NextBool(20))
            {
                Dust nature = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.JungleGrass, 0f, 0f, 100, default, 0.7f);
                nature.noGravity = true;
                nature.velocity *= 0.2f;
            }
        }
    }
}
