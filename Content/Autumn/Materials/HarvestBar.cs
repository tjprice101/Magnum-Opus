using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Autumn.Materials
{
    /// <summary>
    /// Harvest Bar - Autumn-themed crafting bar.
    /// Polished white-brown with orange tint, forged from Leaves of Ending and Decay Essence.
    /// </summary>
    public class HarvestBar : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = -1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forged from autumn's final bounty'") { OverrideColor = new Color(200, 150, 80) });
        }

        public override void PostUpdate()
        {
            // Polished white-brown with orange tint
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.55f;
            Lighting.AddLight(Item.Center, 0.6f * pulse, 0.4f * pulse, 0.25f * pulse);

            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.AmberBolt, Main.rand.NextFloat(-0.3f, 0.3f), 0.3f, 100, default, 0.6f);
                dust.noGravity = false;
                dust.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(35))
            {
                Dust decay = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Copper, 0f, 0f, 120, default, 0.4f);
                decay.noGravity = true;
                decay.velocity *= 0.15f;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 3)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 1)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}
