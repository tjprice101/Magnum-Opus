using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Cipher Nocturne channeled beam weapon.
    /// Handles hold-item ambient, item bloom, beam segment trails,
    /// beam endpoint unraveling, on-hit impacts, and snap-back
    /// implosion/explosion/impact effects.
    /// Call from CipherNocturne, RealityUnravelerBeam, and RealitySnapBack.
    /// </summary>
    public static class CipherNocturneVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: subtle staff aura with arcane void motes
        /// drifting near the staff tip while the weapon is held.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Subtle void motes drifting near staff tip
            if (Main.rand.NextBool(5))
            {
                Vector2 tipOffset = new Vector2(player.direction * 24f, -8f);
                Vector2 tipPos = center + tipOffset + Main.rand.NextVector2Circular(6f, 6f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat(0.2f, 0.5f));
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, Vector2.Zero, 0, col, 0.5f);
                d.noGravity = true;
            }

            // Rare green flame wisp near staff crystal
            if (Main.rand.NextBool(12))
            {
                Vector2 crystalOffset = new Vector2(player.direction * 26f, -10f);
                Vector2 crystalPos = center + crystalOffset + Main.rand.NextVector2Circular(4f, 4f);
                Dust g = Dust.NewDustPerfect(crystalPos, DustID.GreenTorch,
                    new Vector2(0f, -0.4f), 0, EnigmaPalette.GreenFlame * 0.6f, 0.4f);
                g.noGravity = true;
            }

            // Pulsing enigma glow
            float pulse = 0.25f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, EnigmaPalette.Purple.ToVector3() * pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Standard 3-layer Enigma item bloom for the Cipher Nocturne staff sprite.
        /// Call from PreDrawInWorld with additive SpriteBatch.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            EnigmaPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  BEAM SEGMENT VFX
        // =====================================================================

        /// <summary>
        /// Per-segment beam trail VFX: dense dual-color dust, contrasting sparkles,
        /// enigma shimmer particles, and periodic music notes along the beam body.
        /// Call each frame for every beam segment position.
        /// </summary>
        public static void BeamSegmentVFX(Vector2 pos, Vector2 direction, float beamIntensity)
        {
            if (Main.dedServ) return;

            Vector2 away = -direction.SafeNormalize(Vector2.Zero);

            // Dense void dust (2+ per frame) — purple-green alternating
            for (int i = 0; i < 2; i++)
            {
                float progress = Main.rand.NextFloat();
                int dustType = progress < 0.5f ? DustID.PurpleTorch : DustID.GreenTorch;
                Color dustCol = EnigmaPalette.GetEnigmaGradient(progress) * beamIntensity;
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, vel, progress < 0.3f ? 60 : 0, dustCol, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Contrasting sparkles (1-in-2)
            EnigmaVFXLibrary.SpawnContrastSparkle(pos, away);

            // Enigma shimmer (1-in-3)
            if (Main.rand.NextBool(3))
            {
                float hue = 0.28f + (Main.GameUpdateCount * 0.015f % 0.17f);
                Color shimmer = Main.hslToRgb(hue, 0.85f, 0.65f);
                var glow = new GenericGlowParticle(pos, away * 0.3f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    shimmer * 0.45f * beamIntensity, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6))
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            // Glyph accents along beam (1-in-8)
            if (Main.rand.NextBool(8))
                CustomParticles.Glyph(pos, EnigmaPalette.Purple * beamIntensity, 0.2f, -1);

            EnigmaVFXLibrary.AddPulsingLight(pos, beamIntensity * 0.45f);
        }

        // =====================================================================
        //  BEAM ENDPOINT VFX
        // =====================================================================

        /// <summary>
        /// Unraveling VFX at the beam endpoint: orbiting flares, void swirl motes,
        /// watching eyes, and reality-distortion sparkles where the beam terminates.
        /// </summary>
        public static void BeamEndpointVFX(Vector2 endPos, float beamIntensity)
        {
            if (Main.dedServ) return;

            // Dense endpoint dust (2+ per frame)
            for (int i = 0; i < 2; i++)
            {
                float progress = Main.rand.NextFloat();
                Color dustCol = EnigmaPalette.GetEnigmaGradient(progress) * beamIntensity;
                int dustType = progress < 0.5f ? DustID.PurpleTorch : DustID.GreenTorch;
                Dust d = Dust.NewDustPerfect(endPos + Main.rand.NextVector2Circular(10f, 10f),
                    dustType, Main.rand.NextVector2Circular(3f, 3f),
                    0, dustCol, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Contrasting green sparkle (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Dust green = Dust.NewDustPerfect(endPos + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.GreenTorch, Main.rand.NextVector2Circular(2f, 2f),
                    0, EnigmaPalette.GreenFlame * beamIntensity, 1.4f);
                green.noGravity = true;
            }

            // Orbiting unravel flares (every 6 frames)
            if (Main.GameUpdateCount % 6 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.15f;
                    float radius = 20f + MathF.Sin(Main.GameUpdateCount * 0.2f + i) * 15f;
                    Vector2 offset = angle.ToRotationVector2() * radius * beamIntensity;
                    Color flareCol = EnigmaPalette.GetEnigmaGradient((float)i / 4f) * beamIntensity;
                    CustomParticles.GenericFlare(endPos + offset, flareCol, 0.4f, 14);
                }
            }

            // Frequent endpoint flares (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Color flareColor = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * beamIntensity;
                CustomParticles.GenericFlare(endPos + Main.rand.NextVector2Circular(12f, 12f),
                    flareColor, 0.5f, 16);
            }

            // Watching eye (1-in-8)
            if (Main.rand.NextBool(8))
                CustomParticles.EnigmaEyeGaze(endPos + Main.rand.NextVector2Circular(20f, 20f),
                    EnigmaPalette.Purple * beamIntensity, 0.35f);

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6))
                EnigmaVFXLibrary.SpawnMusicNotes(endPos, 1, 10f, 0.7f, 0.9f, 25);

            // Enhanced bloom flare at endpoint (every 8 frames)
            if (Main.GameUpdateCount % 8 == 0)
                EnhancedParticles.BloomFlare(endPos, EnigmaPalette.GreenFlame * beamIntensity,
                    0.65f, 16, 4, 1.0f);

            Lighting.AddLight(endPos, EnigmaPalette.GetEnigmaGradient(0.7f).ToVector3() * beamIntensity * 0.8f);
        }

        // =====================================================================
        //  BEAM HIT VFX
        // =====================================================================

        /// <summary>
        /// On-hit impact VFX when the beam damages an enemy: flare burst,
        /// prismatic sparkle ring, glyph stack, and music notes at the impact point.
        /// </summary>
        public static void BeamHitVFX(Vector2 hitPos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Central flare
            CustomParticles.GenericFlare(hitPos, EnigmaPalette.GreenFlame, 0.5f, 12);

            // Halo ring at impact
            CustomParticles.HaloRing(hitPos, EnigmaPalette.Purple, 0.25f, 12);

            // Prismatic sparkle burst (1-in-3)
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                    Color sparkCol = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                    var spark = new GenericGlowParticle(targetCenter - new Vector2(0, 25f),
                        sparkVel, sparkCol, 0.3f, 18, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // Glyph stack at target
            CustomParticles.GlyphStack(hitPos, EnigmaPalette.Purple, 2, 0.25f);

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.7f, 0.9f, 22);

            // Watching eye at impact
            CustomParticles.EnigmaEyeImpact(hitPos, targetCenter, EnigmaPalette.GreenFlame, 0.4f);

            Lighting.AddLight(hitPos, EnigmaPalette.GreenFlame.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  SNAP-BACK IMPLOSION VFX
        // =====================================================================

        /// <summary>
        /// Implosion phase of the snap-back mechanic: inward-spiraling glow particles,
        /// contracting glyph ring, and void swirl converging on the center.
        /// Called during the first 40% of the snap-back projectile lifetime.
        /// </summary>
        public static void SnapBackImplosionVFX(Vector2 pos, float scale)
        {
            if (Main.dedServ) return;

            // Inward-spiraling glow particles (every 4 frames, 5 particles)
            if (Main.GameUpdateCount % 4 == 0)
            {
                float implodeRadius = 60f * scale;
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f + Main.GameUpdateCount * 0.2f;
                    Vector2 particlePos = pos + angle.ToRotationVector2() * implodeRadius;
                    Vector2 vel = (pos - particlePos).SafeNormalize(Vector2.Zero) * 6f;
                    Color col = EnigmaPalette.GetEnigmaGradient((float)i / 5f);
                    var glow = new GenericGlowParticle(particlePos, vel, col, 0.4f * scale, 14, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Central void pulse (every 6 frames)
            if (Main.GameUpdateCount % 6 == 0)
                CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.6f * scale, 12);

            // Music notes — reality snap echoes (1-in-5)
            if (Main.rand.NextBool(5))
            {
                Color noteCol = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f);
                ThemedParticles.MusicNote(pos, noteVel, noteCol, 0.35f * scale, 32);
            }

            Lighting.AddLight(pos, EnigmaPalette.Purple.ToVector3() * 0.7f * scale);
        }

        // =====================================================================
        //  SNAP-BACK EXPLOSION VFX
        // =====================================================================

        /// <summary>
        /// Explosion phase of the snap-back mechanic: outward-detonating flares,
        /// expanding glyph ring, radial dust burst, and cascading halos.
        /// Called during the last 60% of the snap-back projectile lifetime.
        /// </summary>
        public static void SnapBackExplosionVFX(Vector2 pos, float scale)
        {
            if (Main.dedServ) return;

            // Outward-detonating flares (every 4 frames, 6 particles)
            if (Main.GameUpdateCount % 4 == 0)
            {
                float explodeRadius = 50f * scale;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 particlePos = pos + angle.ToRotationVector2() * explodeRadius;
                    Color col = EnigmaPalette.GetEnigmaGradient((float)i / 6f);
                    CustomParticles.GenericFlare(particlePos, col, 0.45f * scale, 12);
                }
            }

            // Central green pulse (every 6 frames)
            if (Main.GameUpdateCount % 6 == 0)
                CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.65f * scale, 12);

            // Music notes (1-in-5)
            if (Main.rand.NextBool(5))
            {
                Color noteCol = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f);
                ThemedParticles.MusicNote(pos, noteVel, noteCol, 0.35f * scale, 32);
            }

            // Mystical flash at midpoint of explosion phase
            if (Main.GameUpdateCount % 15 == 0)
            {
                CustomParticles.GenericFlare(pos, EnigmaPalette.Purple, 0.75f * scale, 20);
                CustomParticles.HaloRing(pos, EnigmaPalette.GreenFlame, 0.45f * scale, 16);
            }

            Lighting.AddLight(pos, EnigmaPalette.GetEnigmaGradient(0.5f).ToVector3() * scale);
        }

        // =====================================================================
        //  SNAP-BACK IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Full snap-back impact VFX when the reality snap-back hits an enemy:
        /// central white flash, glyph burst, sparkle ring, eye burst,
        /// expanding halos, music note scatter, and radial dust burst.
        /// </summary>
        public static void SnapBackImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Central white + green flash
            EnigmaVFXLibrary.DrawBloom(pos, 0.8f);
            CustomParticles.GenericFlare(pos, Color.White, 0.8f, 18);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.65f, 20);

            // Offset flares in gradient ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 15f;
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 6f);
                CustomParticles.GenericFlare(pos + offset, col, 0.3f, 14);
            }

            // Glyph burst
            CustomParticles.GlyphBurst(pos, EnigmaPalette.GreenFlame, 8, 5f);

            // Halo + glyph circle
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.35f, 15);
            CustomParticles.GlyphCircle(pos, EnigmaPalette.Purple, count: 6, radius: 45f, rotationSpeed: 0.08f);

            // Eye burst
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 4, 4f);

            // Sparkle burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 8f);
                var spark = new GenericGlowParticle(pos, vel, col * 0.7f, 0.35f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Expanding halos
            for (int i = 0; i < 3; i++)
            {
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 3f);
                CustomParticles.HaloRing(pos, col, 0.3f + i * 0.12f, 14 + i * 3);
            }

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 4, 22f, 0.8f, 1.0f, 28);

            // Radial dust burst
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 14, 6f);

            Lighting.AddLight(pos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 1.0f);
        }
    }
}
