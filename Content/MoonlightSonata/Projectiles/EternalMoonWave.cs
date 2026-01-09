using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Purple energy wave projectile fired by the Eternal Moon sword.
    /// Uses particles and visual effects instead of a texture.
    /// </summary>
    public class EternalMoonWave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible base texture

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255; // Fully transparent - we use dust for visuals
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Create purple energy wave effect with particles
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Enhanced trail using ThemedParticles
            ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
            
            // Musical note trail - floating notes shed from the wave
            ThemedParticles.MoonlightMusicTrail(Projectile.Center, Projectile.velocity);
            
            // Main wave particles (reduced count - ThemedParticles handles additional visuals)
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-15, 15), Main.rand.NextFloat(-15, 15));
                Vector2 dustPos = Projectile.Center + offset;
                
                // Dark purple core
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.1f;
                dust.fadeIn = 1.2f;
            }
            
            // Trailing particles
            for (int i = 0; i < 1; i++)
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity * Main.rand.NextFloat(0.5f, 1f);
                Dust trail = Dust.NewDustDirect(trailPos, 1, 1, DustID.PurpleCrystalShard, 0f, 0f, 100, default, 1.2f);
                trail.noGravity = true;
                trail.velocity *= 0.3f;
            }

            // Wave pulsing effect
            Projectile.scale = 1f + (float)System.Math.Sin(Projectile.timeLeft * 0.3f) * 0.1f;
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);
            
            // Enhanced hit effect with ThemedParticles
            ThemedParticles.MoonlightSparks(target.Center, target.velocity);
            ThemedParticles.MoonlightSparkles(target.Center, 6, 25f);
            
            // Musical notes on hit
            ThemedParticles.MoonlightMusicNotes(target.Center, 3, 20f);
        }

        public override void OnKill(int timeLeft)
        {
            // Enhanced dissipation with ThemedParticles
            ThemedParticles.MoonlightBloomBurst(Projectile.Center, 0.6f);
            
            // Musical death burst
            ThemedParticles.MoonlightMusicalImpact(Projectile.Center, 0.5f, false);
            
            // Dissipation effect (reduced count)
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleTorch, velocity.X, velocity.Y, 100, default, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw the projectile sprite, only use particles
            return false;
        }
    }
}
