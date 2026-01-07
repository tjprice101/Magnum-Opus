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
    /// Sakura's Blossom - Melee weapon that creates spectral copies seeking enemies.
    /// Rainbow rarity, higher tier than Moonlight weapons.
    /// </summary>
    public class SakurasBlossom : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 320; // Higher than EternalMoon (275)
            Item.DamageType = DamageClass.Melee;
            Item.width = 70;
            Item.height = 70;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<SakurasBlossomSpectral>();
            Item.shootSpeed = 10f;
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Get cursor direction
            Vector2 cursorDirection = velocity;
            cursorDirection.Normalize();

            // Create 3 spectral swords in a 100 degree cone towards cursor
            float spreadAngle = MathHelper.ToRadians(100f); // 100 degree total spread
            float startAngle = -spreadAngle / 2f; // Start at -50 degrees from center

            for (int i = 0; i < 3; i++)
            {
                // Evenly distribute the 3 swords across the 100 degree cone
                float angle = startAngle + (spreadAngle / 2f) * i; // -50°, 0°, +50°
                Vector2 spectralVelocity = cursorDirection.RotatedBy(angle) * 15f;

                Projectile.NewProjectile(source, player.Center, spectralVelocity,
                    ModContent.ProjectileType<SakurasBlossomSpectral>(), damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Intense scarlet and black particles
            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.RedTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.8f);
                flame.noGravity = true;
                flame.velocity *= 2f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.3f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Create massive scarlet and black explosion on hit
            for (int i = 0; i < 30; i++)
            {
                Dust explosion = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            for (int i = 0; i < 20; i++)
            {
                Dust smoke = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 2.0f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 30)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 25)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
