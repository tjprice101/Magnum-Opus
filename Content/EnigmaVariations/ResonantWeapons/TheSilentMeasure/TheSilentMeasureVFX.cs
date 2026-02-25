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
    /// VFX helper for The Silent Measure ranged gun weapon.
    /// Handles hold-item ambient with charge indicator, muzzle flash (normal + paradox),
    /// bullet trails, bullet-split into seekers, homing seeker trails,
    /// "?" shaped explosions, seeker impacts, paradox bolt trails, and
    /// paradox bolt impacts with chain lightning visuals.
    /// Call from TheSilentMeasure, QuestionSeekerBolt, HomingQuestionSeeker,
    /// and ParadoxPiercingBolt.
    /// </summary>
    public static class TheSilentMeasureVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: subtle precision aura near barrel,
        /// charge indicator intensifies as the 5th (paradox) shot approaches.
        /// </summary>
        public static void HoldItemVFX(Player player, int shotCounter)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Subtle ambient muzzle aura — measured, minimal
            if (Main.rand.NextBool(8))
            {
                Vector2 muzzleOffset = new Vector2(player.direction * 25f, -5f);
                Vector2 muzzlePos = center + muzzleOffset + Main.rand.NextVector2Circular(4f, 4f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat(0.2f, 0.5f));
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.PurpleTorch, Vector2.Zero, 0, col, 0.45f);
                d.noGravity = true;
            }

            // Charge indicator near barrel — intensifies approaching 5th shot
            if (shotCounter >= 3 && Main.rand.NextBool(15 - shotCounter * 2))
            {
                Vector2 barrelPos = center + new Vector2(player.direction * 30f, -5f);
                float chargeProgress = (shotCounter - 3) / 2f;
                Color chargeColor = Color.Lerp(EnigmaPalette.Purple, EnigmaPalette.GreenFlame, chargeProgress);
                var charge = new GlowSparkParticle(
                    barrelPos + Main.rand.NextVector2Circular(6f, 6f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    chargeColor, 0.12f + chargeProgress * 0.08f, 10);
                MagnumParticleHandler.SpawnParticle(charge);
            }

            // Precise watching eye — the measure observes its target
            if (Main.rand.NextBool(35))
            {
                Vector2 eyePos = center + new Vector2(player.direction * 35f, -8f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.4f, 0.2f, Main.MouseWorld);
            }

            // Subtle pulsing light — surgical precision
            float pulse = MathF.Sin(time * 0.07f) * 0.1f + 0.9f;
            Lighting.AddLight(center, EnigmaPalette.Purple.ToVector3() * pulse * 0.25f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Standard Enigma 3-layer bloom for the weapon sprite when drawn in-world.
        /// Uses EnigmaPalette.DrawItemBloom for consistency across all Enigma items.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.08f;
            EnigmaPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  NORMAL SHOT MUZZLE FLASH
        // =====================================================================

        /// <summary>
        /// Precise, surgical muzzle flash for standard shots.
        /// Tight dust cone with restrained sparkle burst — every shot is deliberate.
        /// </summary>
        public static void NormalMuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Focused flare — precise, not flashy
            CustomParticles.GenericFlare(muzzlePos, EnigmaPalette.UnresolvedTension, 0.3f, 9);

            // Tight dual-color dust cone
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = direction.RotatedByRandom(0.2f) * Main.rand.NextFloat(3f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.GreenTorch;
                Color col = dustType == DustID.PurpleTorch ? EnigmaPalette.Purple : EnigmaPalette.GreenFlame;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, dustVel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Subtle halo
            CustomParticles.HaloRing(muzzlePos, EnigmaPalette.GlyphPurple * 0.5f, 0.18f, 8);

            // Occasional music note — the silence whispers
            if (Main.rand.NextBool(3))
                EnigmaVFXLibrary.SpawnMusicNotes(muzzlePos, 1, 10f, 0.7f, 0.85f, 20);

            Lighting.AddLight(muzzlePos, EnigmaPalette.Purple.ToVector3() * 0.4f);
        }

        // =====================================================================
        //  PARADOX SHOT MUZZLE FLASH (every 5th shot)
        // =====================================================================

        /// <summary>
        /// Enhanced muzzle flash for the paradox piercing bolt (every 5th shot).
        /// Multi-layer bloom, glyph burst, eye reveal — the silence breaks.
        /// </summary>
        public static void ParadoxMuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Intense double-flare — white core with green bloom
            CustomParticles.GenericFlare(muzzlePos, Color.White, 0.7f, 15);
            CustomParticles.GenericFlare(muzzlePos, EnigmaPalette.GreenFlame, 0.55f, 18);

            // Expanding halo
            CustomParticles.HaloRing(muzzlePos, EnigmaPalette.UnresolvedTension, 0.35f, 12);

            // Glyph burst at muzzle — the paradox manifests
            EnigmaVFXLibrary.SpawnGlyphBurst(muzzlePos, 4, 3f);

            // Enhanced directional sparkle burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.SilentMeasure, (float)i / 6f);
                var spark = new GenericGlowParticle(muzzlePos, sparkVel, col * 0.8f, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Watching eye — the measure reveals itself
            CustomParticles.EnigmaEyeGaze(muzzlePos, EnigmaPalette.EyeGreen * 0.7f, 0.3f);

            // Music notes scatter
            EnigmaVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 12f, 0.8f, 1.0f, 22);

            Lighting.AddLight(muzzlePos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  BULLET TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame normal bullet trail: dual-color dust, enigma shimmer,
        /// contrast sparkles, periodic music notes.
        /// Measured density — precise, not overwhelming.
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Dual-color void dust trail
            EnigmaVFXLibrary.SpawnEnigmaSwingDust(pos, awayDir);
            EnigmaVFXLibrary.SpawnContrastSparkle(pos, awayDir);

            // Enigma shimmer hue cycling
            if (Main.rand.NextBool(3))
            {
                Color shimmer = EnigmaPalette.GetShimmer((float)Main.timeForVisualEffects);
                var glow = new GenericGlowParticle(pos, awayDir * 0.5f,
                    shimmer * 0.5f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Glyph trail accent
            if (Main.rand.NextBool(8))
                CustomParticles.GlyphTrail(pos, velocity, EnigmaPalette.GlyphPurple * 0.7f, 0.25f);

            // Music note trail — the silent measure whispers
            if (Main.rand.NextBool(6))
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 20);

            EnigmaVFXLibrary.AddPulsingLight(pos, (float)Main.timeForVisualEffects, 0.4f);
        }

        // =====================================================================
        //  BULLET SPLIT VFX
        // =====================================================================

        /// <summary>
        /// Visual burst when a bullet splits into 3 homing seekers on first hit.
        /// The measured silence fractures into three seeking questions.
        /// </summary>
        public static void BulletSplitVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Split flash — the bullet shatters
            CustomParticles.GenericFlare(pos, EnigmaPalette.EyeGreen, 0.5f, 14);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.3f, 12);

            // Three directional glyph accents — one per seeker
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 glyphPos = pos + angle.ToRotationVector2() * 12f;
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.SilentMeasure, (float)i / 3f);
                CustomParticles.Glyph(glyphPos, col, 0.2f);
            }

            // Radial sparkle burst — the arrow fragments into light
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 8f);
                var spark = new GenericGlowParticle(pos, vel, col * 0.7f, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Watching eye at split point
            CustomParticles.EnigmaEyeGaze(pos, EnigmaPalette.EyeGreen * 0.6f, 0.3f);

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 2, 10f, 0.7f, 0.9f, 22);

            Lighting.AddLight(pos, EnigmaPalette.EyeGreen.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  SEEKER TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame trail VFX for homing seeker projectiles.
        /// Lighter than main bullet — seekers are ghostly, searching.
        /// </summary>
        public static void SeekerTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Ghostly enigma glow trail
            if (Main.rand.NextBool(3))
            {
                Color trailCol = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat(0.3f, 0.7f)) * 0.5f;
                var trail = new GenericGlowParticle(pos, awayDir * 1f,
                    trailCol, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Green torch dust — seeker spark
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.GreenTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f), 0, default, 0.8f);
                d.noGravity = true;
            }

            // Periodic glyph accent — the question seeks
            if (Main.rand.NextBool(10))
                CustomParticles.Glyph(pos, EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()), 0.15f);

            // Music note trail — faint melody of seeking
            if (Main.rand.NextBool(8))
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.65f, 0.8f, 18);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.25f);
        }

        // =====================================================================
        //  "?" SHAPED PARTICLE EXPLOSION
        // =====================================================================

        /// <summary>
        /// Creates a "?" shaped particle explosion — the signature Silent Measure
        /// impact visual. The dot, curve, and radial burst form a question mark
        /// from enigma gradient particles.
        /// </summary>
        public static void QuestionMarkExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Dot of the "?" — bright green core
            CustomParticles.GenericFlare(
                pos + new Vector2(0, 28f), EnigmaPalette.EyeGreen, 0.7f, 22);

            // Curve of the "?" — gradient particles tracing the question mark
            for (int i = 0; i < 14; i++)
            {
                float t = (float)i / 14f;
                float curveAngle = MathHelper.Pi * 1.4f * t - MathHelper.Pi * 0.55f;
                float curveRadius = 22f - t * 10f;
                float yOffset = -12f - t * 35f;

                Vector2 curvePos = pos + new Vector2(
                    MathF.Cos(curveAngle) * curveRadius, yOffset);
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.SilentMeasure, t);
                CustomParticles.GenericFlare(curvePos, col, 0.45f - t * 0.12f, 20);
            }

            // Radial accent burst surrounding the "?"
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 10f);
                CustomParticles.GenericFlare(pos + offset, col, 0.45f, 18);
            }

            // Halo ring + glyph circle — the question is posed
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.5f, 20);
            CustomParticles.GlyphCircle(pos, EnigmaPalette.GreenFlame,
                count: 6, radius: 40f, rotationSpeed: 0.05f);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  SEEKER IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Homing seeker on-hit impact: bloom flash, glyph burst, watching eye,
        /// and radial dust burst. Lighter than the main impact.
        /// </summary>
        public static void SeekerImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Standard enigma impact (scaled for seeker — comboStep 0)
            EnigmaVFXLibrary.MeleeImpact(pos, 0);

            // Glyph burst at impact
            EnigmaVFXLibrary.SpawnGlyphBurst(pos, 3, 3f);

            // Cascading sparkle shower
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 sparkPos = pos + angle.ToRotationVector2() * 15f;
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.SilentMeasure, (float)i / 4f);
                CustomParticles.GenericFlare(sparkPos, col, 0.35f, 14);
            }

            // Watching eye at impact — the seeker found its answer
            CustomParticles.EnigmaEyeImpact(pos, targetCenter, EnigmaPalette.EyeGreen, 0.35f);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  PARADOX BOLT TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Enhanced per-frame trail for the paradox piercing bolt (every 5th shot).
        /// Denser particles, glyph trail, intensified enigma effects.
        /// The paradox bolt is the silence made manifest — heavier than normal bullets.
        /// </summary>
        public static void ParadoxBoltTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Enhanced dense dual-color dust (3 per frame)
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(4f, 4f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.PurpleTorch,
                    awayDir * Main.rand.NextFloat(1f, 2f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, col, 1.4f);
                d.noGravity = true;
            }

            // Green flame contrast sparkle
            EnigmaVFXLibrary.SpawnContrastSparkle(pos, awayDir);

            // Enigma shimmer — intensified
            if (Main.rand.NextBool(2))
            {
                Color shimmer = EnigmaPalette.GetShimmer((float)Main.timeForVisualEffects);
                var glow = new GenericGlowParticle(pos, awayDir * 0.8f,
                    shimmer * 0.7f, 0.3f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Glyph trail — the paradox leaves runes in its wake
            if (Main.rand.NextBool(4))
                CustomParticles.GlyphTrail(pos, velocity, EnigmaPalette.Purple, 0.3f);

            // Music notes — the silence is breaking
            if (Main.rand.NextBool(5))
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.8f, 0.95f, 22);

            // Periodic flare pulses
            if (Main.rand.NextBool(3))
            {
                Color flareCol = EnigmaPalette.PaletteLerp(
                    EnigmaPalette.SilentMeasure, Main.rand.NextFloat());
                CustomParticles.GenericFlare(pos, flareCol * 0.6f, 0.3f, 12);
            }

            EnigmaVFXLibrary.AddPulsingLight(pos, (float)Main.timeForVisualEffects, 0.55f);
        }

        // =====================================================================
        //  PARADOX BOLT IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Paradox bolt on-hit impact with chain lightning visuals.
        /// The ultimate impact: white-core flash, offset flares, eye burst,
        /// glyph explosion, convergent sparkle ring, and chain spark indicators.
        /// </summary>
        public static void ParadoxBoltImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // White-core double flash — the paradox detonates
            CustomParticles.GenericFlare(pos, Color.White, 0.7f, 18);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.6f, 20);

            // Offset gradient flares — radial revelation ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 20f;
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.SilentMeasure, (float)i / 8f);
                CustomParticles.GenericFlare(pos + offset, col, 0.35f, 14);
            }

            // Halo ring + glyph burst — the answer explodes outward
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.4f, 15);
            EnigmaVFXLibrary.SpawnGlyphBurst(pos, 8, 5f);

            // Eye burst — many eyes witness the paradox
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 4, 4f);

            // Watching eye gazing at the target
            CustomParticles.EnigmaEyeImpact(pos, targetCenter, EnigmaPalette.GreenFlame, 0.45f);

            // Convergent sparkle ring — reality bends inward
            for (int i = 0; i < 8; i++)
            {
                float ringAngle = MathHelper.TwoPi * i / 8f;
                Vector2 ringPos = pos + ringAngle.ToRotationVector2() * 45f;
                Vector2 convergeVel = (pos - ringPos).SafeNormalize(Vector2.Zero) * 3f;
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 8f);
                var converge = new GenericGlowParticle(ringPos, convergeVel,
                    col * 0.7f, 0.35f, 20, true);
                MagnumParticleHandler.SpawnParticle(converge);
            }

            // Expanding cascading halos
            for (int i = 0; i < 3; i++)
            {
                Color haloCol = EnigmaPalette.GetRevelationGradient((float)i / 3f);
                CustomParticles.HaloRing(pos, haloCol, 0.25f + i * 0.12f, 14 + i * 3);
            }

            // Music notes burst
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 4, 20f, 0.8f, 1.0f, 28);

            // Radial dust burst
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 14, 6f);

            // Chain lightning spark indicators at impact point
            CustomParticles.GenericFlare(pos, EnigmaPalette.RiddleShimmer, 0.35f, 12);

            Lighting.AddLight(pos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  TRAIL RENDERING FUNCTIONS
        // =====================================================================

        /// <summary>
        /// Trail color function for precision bullet trails.
        /// Uses the SilentMeasure palette with additive-safe {A=0} output.
        /// </summary>
        public static Color PrecisionTrailColor(float completionRatio)
        {
            Color c = EnigmaPalette.PaletteLerp(EnigmaPalette.SilentMeasure,
                0.3f + completionRatio * 0.5f);
            float fade = 1f - completionRatio * 0.85f;
            return (c * fade) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for precision bullet trails.
        /// Thin, surgical taper — deliberate silence.
        /// </summary>
        public static float PrecisionTrailWidth(float completionRatio)
            => EnigmaVFXLibrary.PrecisionTrailWidth(completionRatio, 5f);
    }
}
