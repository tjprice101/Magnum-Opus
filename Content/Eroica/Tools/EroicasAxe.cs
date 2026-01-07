using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.Tools
{
    /// <summary>
    /// Eroica's Axe - a powerful axe crafted from Eroica materials.
    /// Higher tier than Moonlight's Axe.
    /// </summary>
    public class EroicasAxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 91; // Higher than Moonlight
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 8;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 15);
            Item.rare = ModContent.RarityType<EroicaRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Axe power - stronger than Moonlight (225%)
            Item.axe = 250;
            
            // Enable reforging
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 15)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Microsoft.Xna.Framework.Rectangle hitbox)
        {
            // Scarlet red and black particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.RedTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 1.3f;
            }

            if (Main.rand.NextBool(4))
            {
                Dust dust2 = Dust.NewDustDirect(new Microsoft.Xna.Framework.Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, 0f, 100, default, 0.9f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }
    }
}
