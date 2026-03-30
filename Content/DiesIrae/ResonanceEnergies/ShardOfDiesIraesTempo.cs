using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.DiesIrae.ResonanceEnergies
{
    /// <summary>
    /// Shard of Dies Irae's Tempo - Rare crafting material from Dies Irae boss.
    /// Used for crafting the most powerful Dies Irae items.
    /// Equivalent to Nachtmusik's ShardOfNachtmusiksTempo.
    /// </summary>
    public class ShardOfDiesIraesTempo : ModItem
    {
        // Dies Irae theme colors - Blood Red, Charred Black, Ember Orange
        private static readonly Color BloodRed = new Color(200, 50, 30);
        private static readonly Color CharredBlack = new Color(40, 20, 15);
        private static readonly Color EmberOrange = new Color(255, 120, 40);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Material", "'A burning fragment torn from the rhythm of judgment'") 
            { 
                OverrideColor = new Color(200, 50, 30) // Blood Red
            });
        }

        public override void PostUpdate()
        {
            // Ambient ember glow
            Lighting.AddLight(Item.Center, BloodRed.ToVector3() * 0.4f);
            
            // Occasional ember particle
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height,
                    DustID.Torch, 0f, -0.5f, 100, default, 0.8f);
                dust.noGravity = true;
            }
            
            // Occasional crimson sparkle
            if (Main.rand.NextBool(30))
            {
                Dust ember = Dust.NewDustDirect(Item.position, Item.width, Item.height,
                    DustID.CrimsonTorch, 0f, -0.3f, 100, default, 0.6f);
                ember.noGravity = true;
            }
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            // Slight ember glow effect
            return Color.Lerp(lightColor, Color.White, 0.3f);
        }
    }
}
