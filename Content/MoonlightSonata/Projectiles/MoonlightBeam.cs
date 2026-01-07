using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Moonlight beam projectile - heavy duty, bounces off surfaces, moves fast.
    /// Dark purple center with light purple gradient and sparkles.
    /// </summary>
    public class MoonlightBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible base

        private int bounceCount = 0;
        private const int MaxBounces = 5;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 8; // Can hit many enemies
            Projectile.timeLeft = 180; // Lasts longer for bouncing
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 3; // Very fast movement
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounceCount++;
            
            if (bounceCount >= MaxBounces)
            {
                // Explode on final bounce
                for (int i = 0; i < 15; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleTorch, velocity.X, velocity.Y, 100, default, 1.8f);
                    dust.noGravity = true;
                }
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
                return true; // Kill projectile
            }

            // Bounce off walls
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }

            // Bounce effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PinkFairy, 
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.2f);
                dust.noGravity = true;
            }

            return false; // Don't kill projectile
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Dark purple core (center) - bigger and brighter
            Dust core = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.PurpleTorch, 0f, 0f, 50, default, 2.2f);
            core.noGravity = true;
            core.velocity = Vector2.Zero;
            core.fadeIn = 1.8f;
            
            // Light purple outer glow (gradient effect)
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                Dust glow = Dust.NewDustDirect(Projectile.Center + offset, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                glow.noGravity = true;
                glow.velocity = Projectile.velocity * 0.02f;
            }
            
            // Sparkle effect - more frequent
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Dust sparkle = Dust.NewDustDirect(sparklePos, 1, 1, DustID.PinkFairy, 0f, 0f, 200, default, 1f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                sparkle.fadeIn = 0.6f;
            }
            
            // Heavy trail particles
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(5f, 15f);
                Dust trail = Dust.NewDustDirect(trailPos, 1, 1, DustID.PurpleCrystalShard, 0f, 0f, 150, default, 1.3f);
                trail.noGravity = true;
                trail.velocity *= 0.1f;
            }
            
            // Lighting - brighter purple glow
            Lighting.AddLight(Projectile.Center, 0.6f, 0.25f, 0.9f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);
            
            // Sparkly hit effect - bigger
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PinkFairy, velocity.X, velocity.Y, 100, default, 1.5f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Burst of sparkles on death
            for (int i = 0; i < 18; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.PinkFairy;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, velocity.X, velocity.Y, 100, default, 1.6f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false; // Don't draw sprite, only particles
        }
    }
}
