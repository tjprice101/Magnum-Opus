using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Winter.Materials
{
    /// <summary>
    /// Frost Essence - Winter essence used in seasonal crafting.
    /// White snowflake with light blue glow, drops from ice/frost enemies.
    /// </summary>
    public class FrostEssence : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 15;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chill of eternal slumber'") { OverrideColor = new Color(150, 200, 255) });
        }

        public override void PostUpdate()
        {
            // White snowflake with light blue glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.65f;
            Lighting.AddLight(Item.Center, 0.55f * pulse, 0.65f * pulse, 0.85f * pulse);

            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.4f, 0.4f), 60, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.35f;
            }

            if (Main.rand.NextBool(20))
            {
                // Tiny ice crystal
                Dust crystal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Ice, 0f, 0f, 40, default, 0.6f);
                crystal.noGravity = true;
                crystal.velocity *= 0.15f;
            }
        }
    }
}
