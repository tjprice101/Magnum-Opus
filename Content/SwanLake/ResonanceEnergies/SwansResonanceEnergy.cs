using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonanceEnergies
{
    /// <summary>
    /// Swan's Resonance Energy - Rare energy drop from Swan Lake enemies.
    /// Used for crafting high-tier Swan Lake equipment.
    /// </summary>
    public class SwansResonanceEnergy : ModItem
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
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Cyan;
            Item.scale = 0.5f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The dying swan's final breath given form—tragic beauty that endures'") { OverrideColor = new Color(240, 240, 255) });
        }

        public override void PostUpdate()
        {
            // Elegant white/blue glow for swan lake theme
            Lighting.AddLight(Item.Center, 0.6f, 0.7f, 1.0f);
            
            // Feathery cloud particles
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Cloud, 0f, -0.5f, 150, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            // Icy sparkle
            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.2f;
            }
            
            // Occasional elegant white shimmer
            if (Main.rand.NextBool(40))
            {
                Dust shimmer = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.TintableDustLighted, 0f, -0.3f, 0, Color.White, 0.9f);
                shimmer.noGravity = true;
                shimmer.velocity *= 0.3f;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Bright white/blue ethereal glow
            return new Color(230, 240, 255, 180);
        }
    }
}
