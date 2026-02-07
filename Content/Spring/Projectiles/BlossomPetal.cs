using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Spring.Projectiles
{
    /// <summary>
    /// BlossomPetal - Cherry blossom petal projectile from Blossom's Edge
    /// TRUE_VFX_STANDARDS: Layered spinning flares, dense dust, orbiting music notes, hslToRgb color oscillation
    /// </summary>
    public class BlossomPetal : ModProjectile
    {
        // Spring color palette - pink hues (0.92-0.98 on hue wheel)
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(180, 230, 180);
        
        // Hue range for color oscillation (pink range)
        private const float HueMin = 0.92f;
        private const float HueMax = 0.98f;

        public override string Texture => "MagnumOpus/Assets/Particles/BarrageOfGlintsAndSparkles";

        public override void SetStaticDefaults()
        {
            // Enable trail rendering
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.alpha = 50;
            Projectile.light = 0.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Gentle floating motion with curved path (Ark of the Cosmos style)
            Projectile.velocity.Y += 0.03f;
            Projectile.velocity.X *= 0.99f;
            
            // Sine-wave swaying for curved trail
            float waveOffset = (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.12f;
            Projectile.velocity.X += waveOffset;
            
            // Gentle spin
            Projectile.rotation += Projectile.velocity.X * 0.05f;

            // ═══════════════════════════════════════════════════════════════
            // DENSE DUST TRAIL - 2+ particles per frame! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 dustVel = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                // COLOR OSCILLATION with Main.hslToRgb
                float hue = HueMin + ((Main.GameUpdateCount * 0.02f + i * 0.1f) % 1f) * (HueMax - HueMin);
                Color oscillatingColor = Main.hslToRgb(hue, 0.85f, 0.8f);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PinkFairy, dustVel, 100, oscillatingColor, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }

            // ═══════════════════════════════════════════════════════════════
            // CONTRASTING SPARKLES - 1-in-2 for visual pop
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Dust contrast = Dust.NewDustPerfect(sparklePos, DustID.WhiteTorch, 
                    -Projectile.velocity * 0.15f, 0, SpringWhite, 1.2f);
                contrast.noGravity = true;
            }

            // ═══════════════════════════════════════════════════════════════
            // FREQUENT FLARES littering the air - 1-in-2
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(8f, 8f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.4f, 15);
            }

            // ═══════════════════════════════════════════════════════════════
            // ORBITING MUSIC NOTES - Locked to projectile! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            float orbitAngle = Main.GameUpdateCount * 0.08f;
            float orbitRadius = 12f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 4f;
            
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * orbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    
                    // Note velocity matches projectile + slight outward drift
                    Vector2 noteVel = Projectile.velocity * 0.7f + noteAngle.ToRotationVector2() * 0.3f;
                    
                    // VISIBLE SCALE (0.75f+) with shimmer
                    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i) * 0.1f;
                    ThemedParticles.MusicNote(notePos, noteVel, SpringPink * 0.9f, 0.75f * shimmer, 35);
                    
                    // Sparkle companion for bloom effect
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, SpringWhite * 0.6f, 0.25f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // GLOW PARTICLE TRAIL for shimmer
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(3))
            {
                Color trailColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.7f;
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f, 
                    trailColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // ═══════════════════════════════════════════════════════════════
            // DYNAMIC PARTICLE EFFECTS - Pulsing glow and twinkling sparks
            // ═══════════════════════════════════════════════════════════════
            if (Main.GameUpdateCount % 6 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, SpringPink, SpringWhite, 0.26f, 18, 0.14f, 0.2f);
            }
            if (Main.rand.NextBool(5))
            {
                TwinklingSparks(Projectile.Center, SpringWhite, 2, 12f, 0.2f, 22);
            }

            // Pulsing light - BRIGHT (1.0f+ intensity)
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * pulse);

            // Fade out near end of life
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 30f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system for Spring theme with cherry blossom effects
            float alpha = 1f - Projectile.alpha / 255f;
            ProceduralProjectileVFX.DrawSpringProjectile(Main.spriteBatch, Projectile, alpha);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // ═══════════════════════════════════════════════════════════════
            // GLIMMER CASCADE - NOT a puff! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Central glimmer - multiple layered spinning flares
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.3f + layer * 0.12f;
                float layerAlpha = 0.8f - layer * 0.15f;
                Color layerColor = Color.Lerp(SpringWhite, SpringPink, layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 18 - layer * 2);
            }
            
            // Expanding glow rings
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(SpringPink, SpringGreen, ring / 3f);
                CustomParticles.HaloRing(Projectile.Center, ringColor * 0.7f, 0.3f + ring * 0.1f, 14 + ring * 3);
            }
            
            // Radial sparkle burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + (float)i / 10f * (HueMax - HueMin);
                Color sparkleColor = Main.hslToRgb(hue, 0.85f, 0.8f);
                
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, sparkleColor, 0.4f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dust explosion for density
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Color dustColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 100, dustColor, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
            
            // Music note finale - burst outward
            ThemedParticles.MusicNoteBurst(Projectile.Center, SpringPink * 0.9f, 6, 4f);
            
            // Bright lighting flash
            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 1.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ═══════════════════════════════════════════════════════════════
            // GLIMMER IMPACT - Layered cascade (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Central flash cascade
            CustomParticles.GenericFlare(target.Center, Color.White, 0.7f, 20);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.6f, 18);
            CustomParticles.GenericFlare(target.Center, SpringGreen, 0.45f, 16);
            
            // Music notes BURST outward with gradient
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float hue = HueMin + (float)i / 6f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 0.9f, 0.8f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.8f, 35);
            }
            
            // Halo ring expansion
            CustomParticles.HaloRing(target.Center, SpringPink * 0.6f, 0.45f, 16);
            CustomParticles.HaloRing(target.Center, SpringWhite * 0.4f, 0.3f, 14);
            
            // Sparkle burst ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, 
                    Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.9f, 0.4f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dust burst for density
            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.PinkFairy, burstVel, 80, SpringPink, 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 1.1f;
            }
            
            // === DYNAMIC PARTICLE EFFECTS - Spring impact ===
            SpringImpact(target.Center, 1f);
            SpiralBurst(target.Center, SpringPink, SpringGreen, 6, 0.15f, 3.5f, 0.32f, 24);
            
            // Bright impact lighting
            Lighting.AddLight(target.Center, SpringPink.ToVector3() * 1.2f);
        }
    }
}
