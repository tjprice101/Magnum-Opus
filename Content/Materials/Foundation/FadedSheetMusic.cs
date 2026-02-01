using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.Foundation
{
    /// <summary>
    /// Faded Sheet Music - Rare drop from underground chests.
    /// Ancient musical scores with faded but still powerful notation.
    /// </summary>
    public class FadedSheetMusic : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 25);
            Item.rare = ItemRarityID.Green;
        }

        public override void PostUpdate()
        {
            // Aged paper with faint musical energy
            Lighting.AddLight(Item.Center, 0.2f, 0.18f, 0.15f);

            if (Main.rand.NextBool(40))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, -0.2f, 180, default, 0.5f);
                dust.noGravity = true;
                dust.velocity *= 0.15f;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Ancient scores with faded but still powerful notation'") { OverrideColor = new Color(200, 180, 150) });
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item
            CreateRecipe()
                .AddIngredient(ItemID.Book, 1)
                .AddIngredient(ItemID.MagicPowerPotion, 1)
                .AddIngredient<MinorMusicNote>(5)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}
