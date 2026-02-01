using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;

namespace MagnumOpus.Content.Spring.Materials
{
    /// <summary>
    /// Dormant Spring Core - Found in Forest/Jungle chests, becomes active in Hardmode.
    /// Contains latent spring energy waiting to bloom.
    /// </summary>
    public class DormantSpringCore : ModItem
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
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Latent energy waiting to awaken with the thaw'") { OverrideColor = new Color(255, 180, 200) });
        }

        public override void PostUpdate()
        {
            // Pale white with pink/light blue hints - dormant spring energy
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * 0.15f + 0.35f;
            Lighting.AddLight(Item.Center, 0.5f * pulse, 0.4f * pulse, 0.45f * pulse);

            if (Main.rand.NextBool(25))
            {
                Color petalColor = Main.rand.NextBool() ? new Color(255, 183, 197) : new Color(173, 216, 230);
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkTorch, 0f, -0.4f, 100, petalColor, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item (Hardmode progression)
            CreateRecipe()
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.JungleSpores, 10)
                .AddIngredient<ResonantCrystalShard>(8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
