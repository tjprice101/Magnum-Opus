using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Nachtmusik.ResonanceEnergies
{
    /// <summary>
    /// Shard of Nachtmusik's Tempo - Rare crafting material from Nachtmusik boss.
    /// Used for crafting the most powerful Nachtmusik items.
    /// Equivalent to Fate's ShardOfFatesTempo.
    /// </summary>
    public class ShardOfNachtmusiksTempo : ModItem
    {
        // Placeholder texture until custom art is ready
        public override string Texture => "Terraria/Images/Item_" + ItemID.FragmentVortex;
        
        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 15);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Material", "'A crystallized fragment of the Queen's celestial rhythm'") 
            { 
                OverrideColor = Color.Lerp(DeepPurple, Gold, 0.5f) 
            });
        }

        public override void PostUpdate()
        {
            // Ambient glow
            Lighting.AddLight(Item.Center, Violet.ToVector3() * 0.4f);
            
            // Occasional star sparkle
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height,
                    DustID.PurpleTorch, 0f, -0.5f, 100, default, 0.8f);
                dust.noGravity = true;
            }
            
            if (Main.rand.NextBool(30))
            {
                Dust gold = Dust.NewDustDirect(Item.position, Item.width, Item.height,
                    DustID.GoldFlame, 0f, -0.3f, 100, default, 0.6f);
                gold.noGravity = true;
            }
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            // Slight glow effect
            return Color.Lerp(lightColor, Color.White, 0.3f);
        }
    }
}
