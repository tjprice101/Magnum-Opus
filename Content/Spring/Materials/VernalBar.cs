using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Spring.Materials
{
    /// <summary>
    /// Vernal Bar - Spring-themed crafting bar.
    /// Polished white-pink with blue tint, forged from Petals of Rebirth and Blossom Essence.
    /// </summary>
    public class VernalBar : ModItem
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
            Item.createTile = -1; // No tile placement for now
        }

        public override void PostUpdate()
        {
            // Polished white-pink with blue tint glow
            float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.6f;
            Lighting.AddLight(Item.Center, 0.6f * pulse, 0.5f * pulse, 0.55f * pulse);

            if (Main.rand.NextBool(20))
            {
                Color petalColor = Main.rand.NextBool() ? new Color(255, 183, 197) : new Color(173, 216, 230);
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PinkFairy, 0f, -0.4f, 80, petalColor, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 3)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 1)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}
