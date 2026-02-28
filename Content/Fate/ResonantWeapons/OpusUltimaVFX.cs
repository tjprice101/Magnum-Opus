// DEPRECATED: Replaced by OpusUltima/ folder self-contained system
/*
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// VFX helper for Opus Ultima — melee + projectile weapon (720 dmg).
    /// Fires a cosmic energy ball that explodes into 5 seeker balls.
    /// Handles hold-item ambient, item bloom, swing effects, melee impact,
    /// energy ball trail/explosion, and seeker ball trail effects.
    /// Call from OpusUltima, OpusUltimaSwing, CosmicEnergyBall, and CosmicSeekerBall.
    /// </summary>
    public static class OpusUltimaVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: orbiting energy spheres at 180 degrees apart,
        /// star aura, occasional glyph, and ambient cosmic light.
        /// The magnum opus gathers the cosmos in the wielder's grasp.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Orbiting energy spheres — 2 spheres at 180 degrees, cosmic gradient
            if (Main.rand.NextBool(10))
            {
                for (int i = 0; i < 2; i++)
                {
                    float sphereAngle = time * 0.04f + MathHelper.Pi * i;
                    float sphereRadius = 30f + MathF.Sin(time * 0.03f + i * 2f) * 6f;
                    Vector2 spherePos = center + sphereAngle.ToRotationVector2() * sphereRadius;
                    Color sphereCol = FatePalette.GetCosmicGradient(0.3f + i * 0.4f);
                    var sphere = new GenericGlowParticle(spherePos,
                        Main.rand.NextVector2Circular(0.3f, 0.3f),
                        sphereCol * 0.5f, 0.2f, 16, true);
                    MagnumParticleHandler.SpawnParticle(sphere);
                }
            }

            // Star aura — twinkling cosmic motes
            if (Main.rand.NextBool(7))
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(35f, 35f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(starPos,
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    starCol * 0.35f, 0.12f, 14, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Occasional glyph — fate's signature rune
            if (Main.rand.NextBool(15))
            {
                Vector2 glyphPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color glyphCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat(0.2f, 0.7f));
                CustomParticles.Glyph(glyphPos, glyphCol * 0.4f, 0.18f, -1);
            }

            // Music notes — the opus whispers
            if (Main.rand.NextBool(12))
                FateVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.65f, 0.85f, 25);

            // Ambient cosmic light
            float pulse = 0.2f + MathF.Sin(time * 0.055f) * 0.1f;
            Lighting.AddLight(center, FatePalette.BrightCrimson.ToVector3() * pulse);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// Enhanced 4-layer PreDrawInWorld bloom with crimson-gold color shift.
        /// Uses DrawItemBloomEnhanced with time shift for dynamic cosmic aura.
        /// The opus radiates transcendent celestial energy.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb,
            Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = Main.GameUpdateCount * 0.03f;
            float pulse = 1f + MathF.Sin(time * 1.2f) * 0.07f;

            // Switch to additive blending for bloom layers
            FateVFXLibrary.BeginFateAdditive(sb);

            // Enhanced bloom with color shift
            FatePalette.DrawItemBloomEnhanced(sb, tex, pos, origin, rotation, scale, pulse, time);

            // Restore standard blending
            FateVFXLibrary.EndFateAdditive(sb);
        }

        // ===== SWING VFX =====

        /// <summary>
        /// Per-frame VFX during melee swing. Cosmic sparks, star particles,
        /// glyph trails, and fate swing dust. Every swing is a verse in the opus.
        /// </summary>
        public static void SwingVFX(Vector2 swingPos, Player player)
        {
            if (Main.dedServ) return;

            Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);

            // Cosmic sparks (1-in-2) — palette-colored energy fragments
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = FatePalette.PaletteLerp(FatePalette.OpusUltimaPalette,
                    Main.rand.NextFloat());
                Vector2 sparkVel = -direction * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f);
                var spark = new GenericGlowParticle(swingPos + Main.rand.NextVector2Circular(6f, 6f),
                    sparkVel, sparkCol * 0.6f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Star particles (1-in-4) — twinkling cosmic dust from the blade
            if (Main.rand.NextBool(4))
                FateVFXLibrary.SpawnStarSparkles(swingPos, 1, 10f, 0.15f);

            // Glyph trails (1-in-6) — fate runes trailing the blade
            if (Main.rand.NextBool(6))
            {
                Color glyphCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat());
                CustomParticles.Glyph(swingPos, glyphCol * 0.45f, 0.18f, -1);
            }

            // Fate dual-color swing dust
            FateVFXLibrary.SpawnFateSwingDust(swingPos, -direction);

            // Contrast sparkle
            FateVFXLibrary.SpawnContrastSparkle(swingPos, -direction);

            // Music notes (periodic)
            if (Main.GameUpdateCount % 7 == 0)
                FateVFXLibrary.SpawnMusicNotes(swingPos, 1, 10f, 0.7f, 0.9f, 20);

            Lighting.AddLight(swingPos, FatePalette.BrightCrimson.ToVector3() * 0.5f);
        }

        // ===== IMPACT VFX =====

        /// <summary>
        /// On-hit melee impact VFX. Shared melee impact base, glyph burst,
        /// 6 star particles expanding outward, and cosmic lighting.
        /// The opus strikes a resonant chord in reality.
        /// </summary>
        public static void ImpactVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Shared Fate melee impact as base
            FateVFXLibrary.MeleeImpact(hitPos, 0);

            // Glyph burst — fate runes shattering outward
            FateVFXLibrary.SpawnGlyphBurst(hitPos, 6, 4f);

            // 6 star particles expanding outward in palette gradient
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color starCol = FatePalette.PaletteLerp(FatePalette.OpusUltimaPalette, (float)i / 6f);
                var star = new GenericGlowParticle(hitPos, vel,
                    starCol * 0.7f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Weapon-colored flare — bright crimson impact core
            CustomParticles.GenericFlare(hitPos, FatePalette.BrightCrimson, 0.5f, 14);

            // Impact dust burst
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Color dustCol = FatePalette.GetGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitPos, DustID.PinkTorch, dustVel, 0, dustCol, 1.1f);
                d.noGravity = true;
            }

            // Music notes
            FateVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.8f, 1.0f, 24);

            // Cosmic lighting
            Lighting.AddLight(hitPos, FatePalette.BrightCrimson.ToVector3() * 0.8f);
        }

        // ===== ENERGY BALL TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for the cosmic energy ball projectile.
        /// Multi-layer cloud trail using OpusUltimaPalette, star sparkle accents,
        /// and strong cosmic cloud trail via library.
        /// The energy ball is a condensed universe hurtling forward.
        /// </summary>
        public static void EnergyBallTrailVFX(Vector2 pos, Vector2 velocity, float scale)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Multi-layer cloud trail — 4 layers using OpusUltimaPalette
            for (int layer = 0; layer < 4; layer++)
            {
                float layerProgress = (float)layer / 4f;
                Color cloudCol = FatePalette.PaletteLerp(FatePalette.OpusUltimaPalette, layerProgress) * 0.5f;
                float particleScale = (0.2f + layer * 0.08f) * scale;

                Vector2 offset = Main.rand.NextVector2Circular(6f * scale, 6f * scale);
                Vector2 cloudVel = awayDir * (0.3f + layer * 0.15f) + Main.rand.NextVector2Circular(1f, 1f);

                var cloud = new GenericGlowParticle(pos + offset, cloudVel,
                    cloudCol, particleScale, 20, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }

            // Star sparkle accents — twinkling motes in the energy ball's wake
            if (Main.rand.NextBool(3))
            {
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(10f * scale, 10f * scale),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.5f, 0.15f * scale, 14, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Strong cosmic cloud trail via library
            if (Main.rand.NextBool(2))
                FateVFXLibrary.SpawnCosmicCloudTrail(pos, velocity, 0.5f * scale);

            // Dense cosmic dust trail
            for (int i = 0; i < 2; i++)
            {
                Color dustCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f * scale, 5f * scale),
                    DustID.PinkTorch,
                    awayDir * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustCol, 1.2f * scale);
                d.noGravity = true;
            }

            // Glyph accent (1-in-6)
            if (Main.rand.NextBool(6))
                FateVFXLibrary.SpawnGlyphAccent(pos, 0.2f * scale);

            // Music note (1-in-5)
            if (Main.rand.NextBool(5))
                FateVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.9f, 18);

            // Bright cosmic light
            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.6f * scale);
        }

        // ===== ENERGY BALL EXPLOSION VFX =====

        /// <summary>
        /// Explosion VFX when the cosmic energy ball detonates and spawns 5 seekers.
        /// Supernova explosion at 1.0 scale, additional glyph circle,
        /// and music note explosion. The opus reaches its climax.
        /// </summary>
        public static void EnergyBallExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Full-scale supernova explosion — the opus detonates
            FateVFXLibrary.SupernovaExplosion(pos, 1.0f);

            // Additional glyph circle — the opus seal expands
            FateVFXLibrary.SpawnGlyphCircle(pos, 10, 55f, 0.07f);

            // Glyph burst — runes exploding outward
            FateVFXLibrary.SpawnGlyphBurst(pos, 14, 7f);

            // Music note explosion — the final chord detonates
            FateVFXLibrary.SpawnMusicNotes(pos, 8, 45f, 0.85f, 1.2f, 35);

            // Central cosmic flash — brilliant dual flare
            CustomParticles.GenericFlare(pos, FatePalette.SupernovaWhite, 0.8f, 20);
            CustomParticles.GenericFlare(pos, FatePalette.BrightCrimson, 0.65f, 22);

            // 5 directional flares — one for each seeker
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 flarePos = pos + angle.ToRotationVector2() * 25f;
                Color flareCol = FatePalette.PaletteLerp(FatePalette.OpusUltimaPalette, (float)i / 5f);
                CustomParticles.GenericFlare(flarePos, flareCol, 0.45f, 16);
            }

            // Expanding gradient ring — cosmic shockwave
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                Color ringCol = FatePalette.GetCosmicGradient((float)i / 12f);
                var ring = new GenericGlowParticle(pos, vel,
                    ringCol * 0.6f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Cascading halo rings — resonance waves
            FateVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.35f);

            // Constellation burst — the final star map
            FateVFXLibrary.SpawnConstellationBurst(pos, 7, 65f, 1.0f);

            // Cosmic cloud burst — nebula shockwave
            FateVFXLibrary.SpawnCosmicCloudBurst(pos, 0.9f, 18);

            Lighting.AddLight(pos, FatePalette.SupernovaWhite.ToVector3() * 1.8f);
        }

        // ===== SEEKER BALL TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for seeker balls spawned from energy ball explosion.
        /// Smaller trail with BrightCrimson to StarGold glow particles and sparks.
        /// Each seeker carries a fragment of the opus's purpose.
        /// </summary>
        public static void SeekerBallTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Primary glow trail — BrightCrimson to StarGold gradient
            if (Main.rand.NextBool(2))
            {
                float gradientT = (Main.GameUpdateCount * 0.025f) % 1f;
                Color trailCol = Color.Lerp(FatePalette.BrightCrimson, FatePalette.StarGold, gradientT);
                var trail = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(3f, 3f),
                    awayDir * Main.rand.NextFloat(0.4f, 1.2f) + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    trailCol * 0.5f, 0.16f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Spark particles — energetic seeker exhaust
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkVel = awayDir * Main.rand.NextFloat(1f, 2f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                Color sparkCol = Color.Lerp(FatePalette.BrightCrimson, FatePalette.StarGold, Main.rand.NextFloat());
                var spark = new GlowSparkParticle(pos, sparkVel,
                    sparkCol * 0.55f, 0.1f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Cosmic dust
            if (Main.rand.NextBool(3))
            {
                Color dustCol = FatePalette.GetGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f), 0, dustCol, 0.8f);
                d.noGravity = true;
            }

            // Star particle accent (1-in-6)
            if (Main.rand.NextBool(6))
            {
                var star = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    FatePalette.StarGold * 0.4f, 0.1f, 10, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Music note (1-in-8)
            if (Main.rand.NextBool(8))
                FateVFXLibrary.SpawnMusicNotes(pos, 1, 5f, 0.6f, 0.8f, 14);

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.3f);
        }

        // ===== SEEKER IMPACT VFX =====

        /// <summary>
        /// On-hit VFX when a seeker ball strikes an enemy.
        /// Smaller explosion with melee impact base and gradient dust burst.
        /// </summary>
        public static void SeekerImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Melee impact as base
            FateVFXLibrary.MeleeImpact(pos, 0);

            // Gradient flare — crimson to gold
            CustomParticles.GenericFlare(pos, FatePalette.BrightCrimson, 0.4f, 12);
            CustomParticles.HaloRing(pos, FatePalette.StarGold * 0.5f, 0.22f, 10);

            // Dust burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Color dustCol = FatePalette.GetGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch, dustVel, 0, dustCol, 1.0f);
                d.noGravity = true;
            }

            // Star sparkles
            FateVFXLibrary.SpawnStarSparkles(pos, 2, 12f, 0.15f);

            // Music note
            FateVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 18);

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.6f);
        }

        // ===== TRAIL RENDERING FUNCTIONS =====

        /// <summary>
        /// Trail color function for energy ball projectile trails.
        /// Uses OpusUltimaPalette with additive-safe output.
        /// </summary>
        public static Color OpusTrailColor(float completionRatio)
        {
            Color c = FatePalette.PaletteLerp(FatePalette.OpusUltimaPalette,
                0.2f + completionRatio * 0.6f);
            float fade = 1f - MathF.Pow(completionRatio, 1.3f);
            return (c * fade) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for energy ball trails.
        /// Wide cosmic trail for the main projectile.
        /// </summary>
        public static float OpusTrailWidth(float completionRatio)
            => FateVFXLibrary.CosmicTrailWidth(completionRatio, 16f);

        /// <summary>
        /// Trail color function for seeker ball trails.
        /// BrightCrimson to StarGold gradient with additive-safe output.
        /// </summary>
        public static Color SeekerTrailColor(float completionRatio)
        {
            Color c = Color.Lerp(FatePalette.BrightCrimson, FatePalette.StarGold, completionRatio);
            float fade = 1f - MathF.Pow(completionRatio, 1.5f);
            return (c * fade) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for seeker ball trails.
        /// Narrower than main energy ball for smaller seekers.
        /// </summary>
        public static float SeekerTrailWidth(float completionRatio)
            => FateVFXLibrary.CosmicBeamWidth(completionRatio, 8f);
    }
}
*/
