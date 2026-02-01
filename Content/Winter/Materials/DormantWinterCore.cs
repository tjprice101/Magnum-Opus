using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;

namespace MagnumOpus.Content.Winter.Materials
{
    /// <summary>
    /// Dormant Winter Core - Found in Ice biome chests, becomes active in Hardmode.
    /// Contains latent winter energy waiting to freeze.
    /// </summary>
    public class DormantWinterCore : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Frozen potential waiting for the spring'") { OverrideColor = new Color(150, 200, 255) });
        }

        public override void PostUpdate()
        {
            // White/pale light blue - dormant winter frost
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.03f) * 0.1f + 0.35f;
            Lighting.AddLight(Item.Center, 0.4f * pulse, 0.45f * pulse, 0.55f * pulse);

            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, -0.2f, 100, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.25f;
            }

            if (Main.rand.NextBool(35))
            {
                // Tiny ice crystal sparkle
                Dust crystal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Ice, 0f, 0f, 80, default, 0.5f);
                crystal.noGravity = true;
                crystal.velocity *= 0.1f;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item (Hardmode progression)
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.IceBlock, 50)
                .AddIngredient<ResonantCrystalShard>(8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
