using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// ADVANCED VFX INTEGRATION - Enhanced hooks for bosses, weapons, and projectiles
    /// 
    /// This system provides:
    /// - Boss attack VFX with screen distortions and advanced trails
    /// - Weapon swing VFX with dynamic particle trails
    /// - Projectile VFX with themed visual effects
    /// - Death/impact explosions with full shader integration
    /// </summary>
    public static class AdvancedVFXIntegration
    {
        #region Boss Attack Integration

        /// <summary>
        /// Enhanced boss attack windup with screen distortion buildup
        /// </summary>
        public static void BossAttackWindup(string theme, Vector2 position, float progress, float scale = 1f)
        {
            // Get theme colors
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Converging particles that scale with progress
            int particleCount = (int)(4 + progress * 8);
            float radius = 120f * (1f - progress * 0.6f) * scale;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.05f;
                Vector2 pos = position + angle.ToRotationVector2() * radius;
                Color particleColor = Color.Lerp(primary, secondary, (float)i / particleCount);
                
                CustomParticles.GenericFlare(pos, particleColor, 0.25f + progress * 0.25f, 12);
            }

            // Screen distortion buildup (subtle until near completion)
            if (progress > 0.6f)
            {
                float distortionIntensity = (progress - 0.6f) / 0.4f * 0.3f * scale;
                ScreenDistortionManager.TriggerThemeEffect(theme, position, distortionIntensity, 5);
            }

            // Central glow that intensifies
            CustomParticles.GenericFlare(position, Color.Lerp(primary, Color.White, progress * 0.5f), 0.3f + progress * 0.4f, 8);

            // Music notes orbiting during charge
            if (Main.rand.NextBool(6) && progress > 0.3f)
            {
                float noteAngle = Main.GameUpdateCount * 0.08f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = noteAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 notePos = position + angle.ToRotationVector2() * (30f + progress * 20f);
                    ThemedParticles.MusicNote(notePos, angle.ToRotationVector2() * 2f, primary, 0.7f + progress * 0.3f, 25);
                }
            }
        }

        /// <summary>
        /// Enhanced boss attack release with full VFX burst
        /// </summary>
        public static void BossAttackRelease(string theme, Vector2 position, float scale = 1f)
        {
            // Get theme palette
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Central flash cascade
            CustomParticles.GenericFlare(position, Color.White, 1.2f * scale, 20);
            CustomParticles.GenericFlare(position, primary, 0.9f * scale, 18);
            CustomParticles.GenericFlare(position, secondary, 0.6f * scale, 16);

            // Expanding halo rings with gradient
            for (int i = 0; i < 5; i++)
            {
                Color ringColor = Color.Lerp(primary, secondary, i / 5f);
                float ringScale = (0.3f + i * 0.15f) * scale;
                CustomParticles.HaloRing(position, ringColor, ringScale, 18 + i * 3);
            }

            // Radial particle burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f) * scale;
                Color burstColor = Color.Lerp(primary, secondary, (float)i / 12f);
                
                var glow = new GenericGlowParticle(position, vel, burstColor * 0.8f, 0.4f * scale, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Theme-specific screen effect
            ScreenDistortionManager.TriggerThemeEffect(theme, position, scale * 0.5f, 25);

            // Music note explosion
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                ThemedParticles.MusicNote(position, noteVel, primary, 0.8f, 30);
            }
        }

        /// <summary>
        /// Enhanced boss dash with trail creation
        /// </summary>
        public static int BossDashStart(string theme, Vector2 startPosition, float width = 30f)
        {
            // Create trail for the dash
            int trailId = AdvancedTrailSystem.CreateThemeTrail(theme, width);
            
            // Departure VFX
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            
            CustomParticles.GenericFlare(startPosition, primary, 0.7f, 15);
            CustomParticles.HaloRing(startPosition, primary, 0.4f, 12);
            
            return trailId;
        }

        /// <summary>
        /// Update boss dash trail
        /// </summary>
        public static void BossDashUpdate(int trailId, Vector2 position, float rotation)
        {
            AdvancedTrailSystem.UpdateTrail(trailId, position, rotation);
        }

        /// <summary>
        /// End boss dash with impact effect
        /// </summary>
        public static void BossDashEnd(string theme, int trailId, Vector2 endPosition, float scale = 1f)
        {
            AdvancedTrailSystem.EndTrail(trailId);
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            
            // Impact burst
            CustomParticles.GenericFlare(endPosition, Color.White, 0.8f * scale, 18);
            CustomParticles.GenericFlare(endPosition, primary, 0.6f * scale, 15);
            
            for (int i = 0; i < 3; i++)
            {
                CustomParticles.HaloRing(endPosition, Color.Lerp(primary, secondary, i / 3f), 0.25f + i * 0.1f, 12 + i * 2);
            }
        }

        /// <summary>
        /// Enhanced boss phase transition with spectacular VFX
        /// </summary>
        public static void BossPhaseTransition(string theme, Vector2 position, float scale = 1.2f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Massive shockwave sequence
            for (int phase = 0; phase < 3; phase++)
            {
                for (int i = 0; i < 4; i++)
                {
                    float ringScale = scale * (0.3f + i * 0.2f + phase * 0.3f);
                    Color ringColor = Color.Lerp(primary, secondary, (phase * 4 + i) / 12f);
                    int delay = phase * 8 + i * 3;
                    CustomParticles.HaloRing(position, ringColor, ringScale, 20 + delay);
                }
            }

            // Central nova
            CustomParticles.GenericFlare(position, Color.White, 1.5f * scale, 25);
            CustomParticles.GenericFlare(position, primary, 1.2f * scale, 22);
            CustomParticles.GenericFlare(position, secondary, 0.9f * scale, 20);

            // Particle explosion
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 15f);
                Color particleColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(position, vel, particleColor * 0.9f, 0.5f * scale, 30, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Screen distortion
            ScreenDistortionManager.TriggerThemeEffect(theme, position, scale * 0.6f, 40);

            // Music note finale
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                ThemedParticles.MusicNote(position, noteVel, primary, 0.9f, 35);
            }

            // Screen shake
            MagnumScreenEffects.AddScreenShake(scale * 12f);
        }

        /// <summary>
        /// Enhanced boss death explosion with multi-phase spectacle
        /// </summary>
        public static void BossDeathExplosion(string theme, Vector2 position, float scale = 2f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Phase 1: Initial flash
            CustomParticles.GenericFlare(position, Color.White, 2f * scale, 30);
            CustomParticles.GenericFlare(position, primary, 1.6f * scale, 27);
            CustomParticles.GenericFlare(position, secondary, 1.2f * scale, 25);

            // Phase 2: Massive shockwave sequence
            for (int ring = 0; ring < 8; ring++)
            {
                float ringScale = scale * (0.4f + ring * 0.25f);
                Color ringColor = Color.Lerp(primary, secondary, ring / 8f);
                int lifetime = 22 + ring * 4;
                CustomParticles.HaloRing(position, ringColor, ringScale, lifetime);
            }

            // Phase 3: Radial flare pattern
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 flarePos = position + angle.ToRotationVector2() * 80f * scale;
                Color flareColor = Color.Lerp(primary, secondary, (float)i / 16f);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.6f * scale, 25);
            }

            // Phase 4: Massive particle explosion
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 20f);
                Color particleColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(position, vel, particleColor * 0.9f, 0.6f * scale, 40, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Phase 5: Music note cascade
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color noteColor = Color.Lerp(primary, secondary, (float)i / 20f);
                ThemedParticles.MusicNote(position, noteVel, noteColor, 0.9f + Main.rand.NextFloat(0.2f), 45);
            }

            // Major screen distortion
            ScreenDistortionManager.TriggerThemeEffect(theme, position, scale * 0.7f, 50);

            // Screen shake
            MagnumScreenEffects.AddScreenShake(scale * 18f);
        }

        #endregion

        #region Weapon VFX Integration

        /// <summary>
        /// Enhanced melee swing VFX with sword arc trails
        /// </summary>
        public static void MeleeSwingVFX(string theme, Player player, Rectangle hitbox, float swingProgress)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Dense particle trail during swing
            if (swingProgress > 0.2f && swingProgress < 0.8f)
            {
                Vector2 center = hitbox.Center.ToVector2();
                
                // Main trail particles
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = center + Main.rand.NextVector2Circular(hitbox.Width / 3f, hitbox.Height / 3f);
                    Vector2 vel = player.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f);
                    Color trailColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                    
                    Dust d = Dust.NewDustPerfect(pos, DustID.MagicMirror, vel, 0, trailColor, 1.4f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // Sparkle accents
                if (Main.rand.NextBool(2))
                {
                    var sparkle = new SparkleParticle(center + Main.rand.NextVector2Circular(15f, 15f),
                        Main.rand.NextVector2Circular(2f, 2f), Color.White * 0.8f, 0.35f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }

                // Flare trail
                if (Main.rand.NextBool(3))
                {
                    CustomParticles.GenericFlare(center + Main.rand.NextVector2Circular(10f, 10f), primary, 0.35f, 12);
                }

                // Music notes in swing
                if (Main.rand.NextBool(8))
                {
                    Vector2 notePos = center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 noteVel = player.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f);
                    ThemedParticles.MusicNote(notePos, noteVel, primary * 0.9f, 0.75f, 30);
                }
            }
        }

        /// <summary>
        /// Enhanced melee impact VFX
        /// </summary>
        public static void MeleeImpactVFX(string theme, Vector2 position, float scale = 1f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Impact flash
            CustomParticles.GenericFlare(position, Color.White, 0.8f * scale, 15);
            CustomParticles.GenericFlare(position, primary, 0.6f * scale, 12);

            // Expanding rings
            for (int i = 0; i < 3; i++)
            {
                CustomParticles.HaloRing(position, Color.Lerp(primary, secondary, i / 3f), (0.2f + i * 0.1f) * scale, 12 + i * 2);
            }

            // Spark burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(primary, secondary, (float)i / 8f);
                
                var sparkle = new SparkleParticle(position, sparkVel, sparkColor, 0.3f * scale, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Music note
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.MusicNote(position, Main.rand.NextVector2Circular(2f, 2f), primary, 0.7f, 25);
            }
        }

        /// <summary>
        /// Enhanced ranged muzzle flash VFX
        /// </summary>
        public static void RangedMuzzleFlashVFX(string theme, Vector2 position, Vector2 direction, float scale = 1f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Central flash
            CustomParticles.GenericFlare(position, Color.White, 0.7f * scale, 10);
            CustomParticles.GenericFlare(position, primary, 0.5f * scale, 8);

            // Directional particles
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 8f) + Main.rand.NextVector2Circular(2f, 2f);
                Color particleColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(position, vel, particleColor * 0.7f, 0.3f * scale, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Small halo
            CustomParticles.HaloRing(position, primary * 0.8f, 0.2f * scale, 8);
        }

        /// <summary>
        /// Enhanced magic cast VFX with glyph circles
        /// </summary>
        public static void MagicCastVFX(string theme, Vector2 position, float chargeProgress, float scale = 1f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Orbiting glyph particles
            float orbitAngle = Main.GameUpdateCount * 0.06f;
            int glyphCount = 3 + (int)(chargeProgress * 3);
            
            for (int i = 0; i < glyphCount; i++)
            {
                float angle = orbitAngle + MathHelper.TwoPi * i / glyphCount;
                float radius = 25f + chargeProgress * 20f;
                Vector2 glyphPos = position + angle.ToRotationVector2() * radius * scale;
                
                CustomParticles.Glyph(glyphPos, Color.Lerp(primary, secondary, (float)i / glyphCount), 0.3f + chargeProgress * 0.2f, -1);
            }

            // Central glow
            CustomParticles.GenericFlare(position, primary, (0.3f + chargeProgress * 0.4f) * scale, 8);

            // Sparkle ring
            if (Main.rand.NextBool(4))
            {
                float sparkleAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 sparklePos = position + sparkleAngle.ToRotationVector2() * (15f + chargeProgress * 10f) * scale;
                var sparkle = new SparkleParticle(sparklePos, Vector2.Zero, secondary, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        #endregion

        #region Projectile VFX Integration

        /// <summary>
        /// Attach advanced trail to projectile
        /// </summary>
        public static void AttachProjectileTrail(Projectile projectile, string theme, float width = 20f)
        {
            projectile.AttachThemeTrail(theme, width);
        }

        /// <summary>
        /// Enhanced projectile trail VFX (call in AI)
        /// </summary>
        public static void ProjectileTrailVFX(string theme, Projectile projectile)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Dense dust trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 dustVel = -projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                Dust d = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, dustVel, 0, primary, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Contrasting sparkles
            if (Main.rand.NextBool(2))
            {
                Dust contrast = Dust.NewDustPerfect(projectile.Center, DustID.WhiteTorch, -projectile.velocity * 0.1f, 0, Color.White, 1.0f);
                contrast.noGravity = true;
            }

            // Flares littering air
            if (Main.rand.NextBool(2))
            {
                Vector2 flarePos = projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(flarePos, primary, 0.35f, 12);
            }

            // Glow particles
            if (Main.rand.NextBool(3))
            {
                Color trailColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(projectile.Center, -projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Music notes in trail
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = Main.GameUpdateCount * 0.08f;
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 notePos = projectile.Center + noteAngle.ToRotationVector2() * 12f;
                    ThemedParticles.MusicNote(notePos, projectile.velocity * 0.7f, primary, 0.7f, 25);
                }
            }
        }

        /// <summary>
        /// Enhanced projectile death VFX
        /// </summary>
        public static void ProjectileDeathVFX(string theme, Vector2 position, float scale = 1f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Layered flares
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = (0.3f + layer * 0.15f) * scale;
                Color layerColor = Color.Lerp(Color.White, primary, layer / 4f);
                CustomParticles.GenericFlare(position, layerColor * (0.8f - layer * 0.15f), layerScale, 18 - layer * 2);
            }

            // Expanding rings
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(primary, secondary, ring / 3f);
                CustomParticles.HaloRing(position, ringColor, (0.25f + ring * 0.1f) * scale, 14 + ring * 3);
            }

            // Radial sparkle burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color sparkleColor = Color.Lerp(primary, Color.White, (float)i / 10f);
                
                var sparkle = new SparkleParticle(position, sparkleVel, sparkleColor, 0.35f * scale, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Dust explosion
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(position, DustID.MagicMirror, dustVel, 0, primary, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Music note finale
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                ThemedParticles.MusicNote(position, noteVel, primary, 0.75f, 30);
            }
        }

        #endregion
    }
}
