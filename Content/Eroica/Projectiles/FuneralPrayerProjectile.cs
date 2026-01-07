using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Funeral Prayer projectile - large flaming bolt with red and black particles.
    /// </summary>
    public class FuneralPrayerProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible - particle-based

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.alpha = 255;
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            // Intense lighting
            Lighting.AddLight(Projectile.Center, 0.8f, 0.2f, 0.1f);

            // Large red flames
            for (int i = 0; i < 3; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.0f);
                flame.noGravity = true;
                flame.velocity = Projectile.velocity * 0.3f;
            }

            // Black smoke particles
            if (Main.rand.NextBool(2))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.5f);
                smoke.noGravity = true;
                smoke.velocity *= 0.5f;
            }

            // Ember particles
            if (Main.rand.NextBool(3))
            {
                Dust ember = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, 0f, 100, default, 1.2f);
                ember.noGravity = true;
                ember.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnKill(int timeLeft)
        {
            // Large explosion on death
            for (int i = 0; i < 30; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                flame.noGravity = true;
                flame.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            for (int i = 0; i < 20; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 2.0f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }
    }
}
