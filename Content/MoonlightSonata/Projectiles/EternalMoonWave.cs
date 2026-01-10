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
            
            // Enhanced trail using ThemedParticles - more frequent
            ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
            
            // Musical note trail - floating notes shed from the wave
            ThemedParticles.MoonlightMusicTrail(Projectile.Center, Projectile.velocity);
            
            // NEW: Vivid flare trail
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GenericFlare(Projectile.Center, new Color(180, 120, 255), 0.3f, 15);
            }
            
            // Main wave particles - enhanced with more vivid colors
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-18, 18), Main.rand.NextFloat(-18, 18));
                Vector2 dustPos = Projectile.Center + offset;
                
                // Bright purple core
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.PurpleTorch, 0f, 0f, 80, default, 2.2f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.1f;
                dust.fadeIn = 1.3f;
            }
            
            // NEW: Pink accent particles
            if (Main.rand.NextBool(2))
            {
                Dust pink = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 1, 1, 
                    DustID.Enchanted_Pink, 0f, 0f, 0, default, 1.0f);
                pink.noGravity = true;
                pink.velocity = Projectile.velocity * 0.05f;
            }
            
            // Trailing particles - enhanced
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity * Main.rand.NextFloat(0.5f, 1f);
                Dust trail = Dust.NewDustDirect(trailPos, 1, 1, DustID.PurpleCrystalShard, 0f, 0f, 100, default, 1.4f);
                trail.noGravity = true;
                trail.velocity *= 0.3f;
            }

            // Wave pulsing effect
            Projectile.scale = 1f + (float)System.Math.Sin(Projectile.timeLeft * 0.3f) * 0.12f;
            
            // Enhanced Lighting
            Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0.9f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);
            
            // ENHANCED: Vivid hit effects with flares
            CustomParticles.GenericFlare(target.Center, new Color(200, 120, 255), 0.6f, 20);
            
            // Enhanced hit effect with ThemedParticles
            ThemedParticles.MoonlightSparks(target.Center, target.velocity);
            ThemedParticles.MoonlightSparkles(target.Center, 10, 30f);
            
            // Musical notes on hit
            ThemedParticles.MoonlightMusicNotes(target.Center, 5, 25f);
        }

        public override void OnKill(int timeLeft)
        {
            // ENHANCED: Vivid dissipation with flare
            CustomParticles.GenericFlare(Projectile.Center, new Color(180, 140, 255), 0.5f, 18);
            
            // Enhanced dissipation with ThemedParticles
            ThemedParticles.MoonlightBloomBurst(Projectile.Center, 0.8f);
            
            // Musical death burst
            ThemedParticles.MoonlightMusicalImpact(Projectile.Center, 0.6f, false);
            
            // Dissipation effect (enhanced)
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleTorch, velocity.X, velocity.Y, 100, default, 1.7f);
                dust.noGravity = true;
                dust.fadeIn = 0.9f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw the projectile sprite, only use particles
            return false;
        }
    }
}
