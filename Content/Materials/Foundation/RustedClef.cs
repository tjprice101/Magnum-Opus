using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Materials.Foundation
{
    /// <summary>
    /// Rusted Clef - Rare find in ice biome chests.
    /// A treble clef of rusted metal, frozen in time.
    /// </summary>
    public class RustedClef : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Frozen in time, yet its melody endures'") { OverrideColor = new Color(150, 200, 255) });
        }

        public override void PostUpdate()
        {
            // Icy rust shimmer
            Lighting.AddLight(Item.Center, 0.2f, 0.25f, 0.35f);

            if (Main.rand.NextBool(30))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.IceTorch, 0f, 0f, 120, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }

            if (Main.rand.NextBool(50))
            {
                Dust rust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Copper, 0f, 0f, 150, default, 0.4f);
                rust.noGravity = true;
                rust.velocity *= 0.1f;
            }
        }

        public override void AddRecipes()
        {
            // Alternative recipe for chest-only item
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 5)
                .AddIngredient(ItemID.IceBlock, 20)
                .AddIngredient<MinorMusicNote>(3)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 5)
                .AddIngredient(ItemID.IceBlock, 20)
                .AddIngredient<MinorMusicNote>(3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
