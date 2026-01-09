using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.MoonlightSonata.ResonantWeapons
{
    public class IncisorOfMoonlight : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // Stronger than Zenith (190 damage)
            Item.damage = 280; // Balanced: Premium melee ~1400 DPS with projectile
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 12; // Fast swing
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<MoonlightWaveProjectile>();
            Item.shootSpeed = 12f;
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 10)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 25)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Purple sparkles when swinging
            if (Main.rand.NextBool(2))
            {
                // Main purple dust
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.PurpleTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, default, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            if (Main.rand.NextBool(3))
            {
                // Blue-ish purple accents
                Dust dust2 = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.PurpleCrystalShard, 0f, 0f, 100, default, 0.9f);
                dust2.noGravity = true;
                dust2.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }

            if (Main.rand.NextBool(4))
            {
                // Occasional bright sparkle
                Dust sparkle = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 
                    DustID.Enchanted_Pink, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity *= 0.5f;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn 3 projectiles in a 120 degree cone
            float coneAngle = MathHelper.ToRadians(120f);
            float halfCone = coneAngle / 2f;
            
            // Middle projectile (straight ahead)
            Projectile.NewProjectile(source, position, velocity, type, damage / 2, knockback, player.whoAmI);
            
            // Left projectile (-60 degrees)
            Vector2 leftVelocity = velocity.RotatedBy(-halfCone);
            Projectile.NewProjectile(source, position, leftVelocity, type, damage / 2, knockback, player.whoAmI);
            
            // Right projectile (+60 degrees)
            Vector2 rightVelocity = velocity.RotatedBy(halfCone);
            Projectile.NewProjectile(source, position, rightVelocity, type, damage / 2, knockback, player.whoAmI);
            
            // Burst of purple particles at swing point
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = velocity.RotatedByRandom(MathHelper.ToRadians(60)) * Main.rand.NextFloat(0.3f, 0.8f);
                Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.PurpleTorch, dustVel.X, dustVel.Y, 100, default, 1.2f);
                dust.noGravity = true;
            }

            return false; // We already created the projectiles
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Extra sparkle burst on hit
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, 
                    DustID.PurpleTorch, 0f, 0f, 150, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
        }
    }
}
