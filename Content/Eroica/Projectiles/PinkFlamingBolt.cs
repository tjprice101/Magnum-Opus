using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Pink flaming bolt shot by Movement III.
    /// Fast moving, straight trajectory. Particle-only (no sprite).
    /// </summary>
    public class PinkFlamingBolt : ModProjectile
    {
        // Use a blank/invisible texture - this projectile is purely particle-based
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180; // 3 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255; // Fully invisible base sprite
            Projectile.light = 0.6f;
        }

        public override void AI()
        {
            // Pink flame lighting
            Lighting.AddLight(Projectile.Center, 1f, 0.3f, 0.5f);

            // Rotation
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Core pink flame particles - these ARE the projectile visually
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, 
                    DustID.PinkTorch, 0f, 0f, 100, default, 2.2f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            // Pink flame trail particles
            for (int i = 0; i < 2; i++)
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                trail.noGravity = true;
                trail.velocity = -Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Bright sparkle core
            if (Main.rand.NextBool(2))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PinkFairy, 0f, 0f, 0, default, 1.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Fire accent particles
            if (Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Torch, 0f, 0f, 100, new Color(255, 100, 150), 1.5f);
                fire.noGravity = true;
                fire.velocity = -Projectile.velocity * 0.15f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Burst of pink flames on impact
            for (int i = 0; i < 25; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PinkTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            // Fire burst
            for (int i = 0; i < 15; i++)
            {
                Dust fire = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Torch, 0f, 0f, 100, new Color(255, 100, 150), 1.5f);
                fire.noGravity = true;
                fire.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }

            // Sparkle burst
            for (int i = 0; i < 10; i++)
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PinkFairy, 0f, 0f, 0, default, 1.5f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw any sprite - particles only
            return false;
        }
    }
}
