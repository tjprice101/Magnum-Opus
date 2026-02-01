using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.ResonanceEnergies
{
    public class MoonlightsResonantEnergy : ModItem
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
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.scale = 0.5f; // 50% smaller display size
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon's quiet melody, preserved'") { OverrideColor = new Color(140, 100, 200) });
        }

        public override void PostUpdate()
        {
            // Elegant glowing effect when dropped in world
            Lighting.AddLight(Item.Center, 0.5f, 0.3f, 0.7f);
            
            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -0.5f, 150, default, 1.0f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.2f;
            }
        }
    }
}
