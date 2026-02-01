using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.Foundation
{
    /// <summary>
    /// Old Metronome - Uncommon find in surface chests.
    /// A wooden triangle metronome that still keeps perfect time.
    /// </summary>
    public class OldMetronome : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 10;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 18;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.Blue;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Even after all these years, it still keeps perfect time'") { OverrideColor = new Color(180, 140, 100) });
        }

        public override void PostUpdate()
        {
            // Rhythmic pulsing glow synchronized with internal tick
            float tick = (Main.GameUpdateCount % 60) / 60f;
            float intensity = tick < 0.5f ? tick * 2f : (1f - tick) * 2f;
            Lighting.AddLight(Item.Center, 0.25f * intensity, 0.2f * intensity, 0.15f * intensity);

            if (Main.rand.NextBool(45) && intensity > 0.8f)
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.WoodFurniture, 0f, -0.3f, 100, default, 0.5f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 20)
                .AddIngredient(ItemID.IronBar, 3)
                .AddIngredient<MinorMusicNote>(3)
                .AddTile(TileID.WorkBenches)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.Wood, 20)
                .AddIngredient(ItemID.LeadBar, 3)
                .AddIngredient<MinorMusicNote>(3)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
