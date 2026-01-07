using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Triumphant Fractal - Magic staff that fires three fractal projectiles with massive explosions.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// </summary>
    public class TriumphantFractal : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 280; // Higher than MoonlightsCalling (225)
            Item.DamageType = DamageClass.Magic;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TriumphantFractalProjectile>();
            Item.shootSpeed = 14f;
            Item.mana = 20;
            Item.noMelee = true;            Item.maxStack = 1;        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire 3 fractals in a tight spread
            int numberOfProjectiles = 3;
            float spreadAngle = MathHelper.ToRadians(15);

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float angle = spreadAngle * ((float)i / (numberOfProjectiles - 1) - 0.5f);
                Vector2 perturbedVelocity = velocity.RotatedBy(angle);

                Projectile.NewProjectile(source, position, perturbedVelocity, type, damage, knockback, player.whoAmI);
            }

            // Spawn dramatic red and black particles at cast location
            for (int i = 0; i < 15; i++)
            {
                Dust cast = Dust.NewDustDirect(position, 20, 20,
                    DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                cast.noGravity = true;
                cast.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust smoke = Dust.NewDustDirect(position, 20, 20,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.2f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 28)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 22)
                .AddIngredient(ItemID.LunarBar, 16)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
