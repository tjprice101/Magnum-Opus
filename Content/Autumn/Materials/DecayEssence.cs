using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Autumn.Materials
{
    /// <summary>
    /// Decay Essence - Autumn essence used in seasonal crafting.
    /// Dark orange glow with white wisps, drops from eclipse/undead enemies.
    /// </summary>
    public class DecayEssence : ModItem
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
            Item.rare = ItemRarityID.Orange;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Beauty found in the withering'") { OverrideColor = new Color(200, 150, 80) });
        }

        public override void PostUpdate()
        {
            // Dark orange glow with white wisps
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.6f;
            Lighting.AddLight(Item.Center, 0.65f * pulse, 0.35f * pulse, 0.15f * pulse);

            if (Main.rand.NextBool(15))
            {
                // Falling leaf-like particle
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.AmberBolt, Main.rand.NextFloat(-0.4f, 0.4f), 0.4f, 80, default, 0.7f);
                dust.noGravity = false;
                dust.velocity *= 0.5f;
            }

            if (Main.rand.NextBool(25))
            {
                // White wisp
                Dust wisp = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Smoke, 0f, -0.5f, 150, Color.White, 0.5f);
                wisp.noGravity = true;
                wisp.velocity *= 0.3f;
            }
        }
    }
}
