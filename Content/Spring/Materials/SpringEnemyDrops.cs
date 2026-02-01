using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Spring.Materials
{
    /// <summary>
    /// Petal of Rebirth - Primary bar material for Spring.
    /// Drops from Plantera's Tentacles (8%) and Jungle Hardmode enemies (3%).
    /// </summary>
    public class PetalOfRebirth : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A fragment of nature's eternal cycle'") { OverrideColor = new Color(255, 180, 200) });
        }

        public override void PostUpdate()
        {
            // Spring rebirth glow - pink and green
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.06f) * 0.2f + 0.5f;
            Lighting.AddLight(Item.Center, 0.6f * pulse, 0.45f * pulse, 0.5f * pulse);

            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkFairy, 0f, -0.4f, 80, new Color(255, 183, 197), 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
    }

    /// <summary>
    /// Vernal Dust - Spring accessory crafting material.
    /// Drops from Jungle Hardmode enemies (5%).
    /// </summary>
    public class VernalDust : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 50;
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 25);
            Item.rare = ItemRarityID.Pink;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Pollen of the awakening season'") { OverrideColor = new Color(255, 180, 200) });
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.3f, 0.35f, 0.25f);

            if (Main.rand.NextBool(25))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.JungleSpore, 0f, -0.2f, 100, default, 0.5f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// Rainbow Petal - Rare spring material from Rainbow Slime.
    /// Drops from Rainbow Slime (10%).
    /// </summary>
    public class RainbowPetal : ModItem
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
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A rare gift from the prismatic slime'") { OverrideColor = new Color(255, 180, 200) });
        }

        public override void PostUpdate()
        {
            float hue = (Main.GameUpdateCount * 0.02f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 1f, 0.6f);
            Lighting.AddLight(Item.Center, rainbow.ToVector3() * 0.6f);

            if (Main.rand.NextBool(15))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.RainbowMk2, 0f, -0.5f, 60, rainbow, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }
        }
    }
}
