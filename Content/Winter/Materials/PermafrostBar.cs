using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Winter.Materials
{
    /// <summary>
    /// Permafrost Bar - Winter-themed crafting bar.
    /// Frosted white with light blue sheen, forged from Shards of Stillness and Frost Essence.
    /// </summary>
    public class PermafrostBar : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Forged in the heart of the endless frost'") { OverrideColor = new Color(150, 200, 255) });
        }

        public override void PostUpdate()
        {
            // Frosted white with light blue sheen
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.1f + 0.5f;
            Lighting.AddLight(Item.Center, 0.5f * pulse, 0.55f * pulse, 0.7f * pulse);

            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, -0.3f, 80, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.25f;
            }

            if (Main.rand.NextBool(30))
            {
                Dust crystal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Ice, 0f, 0f, 60, default, 0.5f);
                crystal.noGravity = true;
                crystal.velocity *= 0.1f;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ModContent.ItemType<ShardOfStillness>(), 3)
                .AddIngredient(ModContent.ItemType<FrostEssence>(), 1)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}
