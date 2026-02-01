using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;

namespace MagnumOpus.Content.Autumn.Materials
{
    /// <summary>
    /// Dormant Autumn Core - Found in Cavern/Underground chests, becomes active in Hardmode.
    /// Contains latent autumn energy waiting to wither.
    /// </summary>
    public class DormantAutumnCore : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A quiet slumber before the long winter'") { OverrideColor = new Color(200, 150, 80) });
        }

        public override void PostUpdate()
        {
            // Muted white/brown/orange - dormant autumn decay
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.035f) * 0.15f + 0.3f;
            Lighting.AddLight(Item.Center, 0.45f * pulse, 0.3f * pulse, 0.2f * pulse);

            if (Main.rand.NextBool(25))
            {
                // Falling leaf-like particles
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.AmberBolt, Main.rand.NextFloat(-0.3f, 0.3f), 0.2f, 130, default, 0.6f);
                dust.noGravity = false;
                dust.velocity *= 0.4f;
            }

            if (Main.rand.NextBool(40))
            {
                Dust decay = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.WoodFurniture, 0f, 0f, 150, new Color(139, 69, 19), 0.4f);
                decay.noGravity = true;
                decay.velocity *= 0.15f;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item (Hardmode progression)
            CreateRecipe()
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ItemID.Cobweb, 30)
                .AddIngredient<ResonantCrystalShard>(8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
