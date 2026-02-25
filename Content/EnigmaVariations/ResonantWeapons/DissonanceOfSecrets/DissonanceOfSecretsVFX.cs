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
    /// VFX helper for the Dissonance of Secrets magic staff weapon.
    /// Handles hold-item ambient, world item bloom, cast flash,
    /// mystery orb trail/ambient/aura, riddlebolt VFX,
    /// and massive cascade explosion effects.
    /// Call from DissonanceOfSecrets, RiddleCascadeOrb, and Riddlebolt.
    /// </summary>
    public static class DissonanceOfSecretsVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: orbiting mini-orbs, watching eyes,
        /// glyph accents, green flame wisps, and music notes.
        /// Call from HoldItem().
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 3-point orbiting mini mystery orbs (cycling)
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = time * 0.04f + MathHelper.TwoPi * i / 3f;
                    float radius = 35f + MathF.Sin(time * 0.06f + i * 1.2f) * 8f;
                    Vector2 orbPos = center + angle.ToRotationVector2() * radius;
                    float progress = (i / 3f + time * 0.01f) % 1f;

                    Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.DissonanceOrb, progress);
                    var orb = new GenericGlowParticle(orbPos, Vector2.Zero, col, 0.22f, 14, true);
                    MagnumParticleHandler.SpawnParticle(orb);
                }
            }

            // Watching eye particles
            if (Main.rand.NextBool(25))
            {
                float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = center + eyeAngle.ToRotationVector2() * Main.rand.NextFloat(28f, 50f);
                EnigmaVFXLibrary.SpawnGazingEye(eyePos, center, 0.35f);
            }

            // Glyph accents
            if (Main.rand.NextBool(30))
            {
                Vector2 glyphOffset = Main.rand.NextVector2Circular(40f, 40f);
                EnigmaVFXLibrary.SpawnGlyphAccent(center + glyphOffset, 0.25f);
            }

            // Green flame wisps rising upward
            if (Main.rand.NextBool(18))
            {
                Vector2 flamePos = center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 flameVel = new Vector2(0, -1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                var flame = new GenericGlowParticle(flamePos, flameVel,
                    EnigmaPalette.GreenFlame * 0.6f, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(flame);
            }

            // Periodic music notes
            if (Main.rand.NextBool(35))
                EnigmaVFXLibrary.SpawnMusicNotes(center, 1, 35f, 0.7f, 0.9f, 30);

            // Pulsing arcane light with color shift
            EnigmaVFXLibrary.AddPulsingLight(center, time, 0.4f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// 4-layer bloom for DissonanceOfSecrets when lying in the world.
        /// Uses the enhanced bloom with purple-green color shift.
        /// Call from PreDrawInWorld.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            EnigmaPalette.DrawItemBloomEnhanced(sb, tex, pos, origin, rotation, scale, pulse, time);
        }

        // =====================================================================
        //  CAST VFX
        // =====================================================================

        /// <summary>
        /// On-cast VFX when the orb is fired: central flare, glyph circle,
        /// sparkle burst, and music notes.
        /// Call from Shoot().
        /// </summary>
        public static void CastVFX(Vector2 castPos)
        {
            if (Main.dedServ) return;

            // Central bloom flare
            CustomParticles.GenericFlare(castPos, EnigmaPalette.GreenFlame, 0.6f, 18);
            CustomParticles.GenericFlare(castPos, EnigmaPalette.WhiteGreenFlash, 0.35f, 12);

            // Glyph circle at cast point
            EnigmaVFXLibrary.SpawnGlyphCircle(castPos, 4, 30f, 0.06f);

            // Sparkle burst outward
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.DissonanceOrb, (float)i / 6f);
                var spark = new GenericGlowParticle(castPos, vel, col * 0.7f, 0.25f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes
            EnigmaVFXLibrary.SpawnMusicNotes(castPos, 4, 20f, 0.8f, 1.0f, 25);

            // Halo ring
            CustomParticles.HaloRing(castPos, EnigmaPalette.Purple, 0.25f, 12);

            Lighting.AddLight(castPos, EnigmaPalette.GreenFlame.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  ORB TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame mystery orb trail VFX: dense dual-color dust, sparkles,
        /// enigma shimmer, flares, and music notes.
        /// Call from RiddleCascadeOrb.AI() every frame.
        /// </summary>
        public static void OrbTrailVFX(Vector2 pos, Vector2 velocity, float currentScale)
        {
            if (Main.dedServ) return;

            // Dense dual-color void dust (2 per frame)
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(10f, 10f) * currentScale;

                // Purple torch dust
                Dust dustPurple = Dust.NewDustPerfect(pos + dustOffset, DustID.PurpleTorch,
                    -velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f),
                    0, EnigmaPalette.Purple, 1.3f);
                dustPurple.noGravity = true;
                dustPurple.fadeIn = 1.4f;

                // Green flame contrast dust
                Dust dustGreen = Dust.NewDustPerfect(pos + dustOffset * 0.5f, DustID.CursedTorch,
                    -velocity * 0.2f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    0, EnigmaPalette.GreenFlame, 1.1f);
                dustGreen.noGravity = true;
                dustGreen.fadeIn = 1.3f;
            }

            // Contrasting sparkle particles (1 in 2)
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(15f, 15f) * currentScale;
                var sparkle = new SparkleParticle(pos + sparkleOffset,
                    -velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    EnigmaPalette.GreenFlame, 0.45f * currentScale, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Enigma shimmer trails with hue cycling (1 in 3)
            if (Main.rand.NextBool(3))
            {
                Color shimmerColor = EnigmaPalette.GetShimmer((float)Main.timeForVisualEffects);
                var shimmer = new GenericGlowParticle(pos, -velocity * 0.15f,
                    shimmerColor, 0.35f * currentScale, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            // Pearlescent void flare (1 in 4)
            if (Main.rand.NextBool(4))
            {
                float shift = MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(EnigmaPalette.Purple, EnigmaPalette.GreenFlame, shift) * 0.8f;
                CustomParticles.GenericFlare(pos, pearlColor, 0.4f * currentScale, 16);
            }

            // Frequent arcane flares (1 in 2)
            if (Main.rand.NextBool(2))
            {
                float growthProgress = MathHelper.Clamp(currentScale / 2.5f, 0f, 1f);
                Vector2 flareOffset = Main.rand.NextVector2Circular(8f, 8f) * currentScale;
                Color flareCol = EnigmaPalette.GetEnigmaGradient(growthProgress);
                CustomParticles.GenericFlare(pos + flareOffset, flareCol, 0.35f * currentScale, 14);
            }

            // Music notes orbiting the growing orb (1 in 6)
            if (Main.rand.NextBool(6))
            {
                Vector2 noteOffset = Main.rand.NextVector2Circular(30f, 30f) * currentScale;
                EnigmaVFXLibrary.SpawnMusicNotes(pos + noteOffset, 1, 10f,
                    0.8f * currentScale, 0.95f * currentScale, 35);
            }

            // Pulsing mystery light
            float pulse = MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(pos, EnigmaPalette.Purple.ToVector3() * 0.7f * currentScale * pulse);
        }

        // =====================================================================
        //  ORB AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Periodic ambient orb VFX: orbiting sparkle constellation,
        /// rotating glyph circle, core pulsing, and void swirl.
        /// Call from RiddleCascadeOrb.AI() on timed intervals.
        /// </summary>
        public static void OrbAmbientVFX(Vector2 pos, float currentScale, float growthProgress)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Orbiting sparkle constellation (every 15 frames)
            if (Main.GameUpdateCount % 15 == 0)
            {
                int sparkleCount = 2 + (int)(growthProgress * 2);
                float baseAngle = time * 0.03f;

                for (int i = 0; i < sparkleCount; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / sparkleCount;
                    float radius = 45f * currentScale;
                    Vector2 sparklePos = pos + angle.ToRotationVector2() * radius;
                    Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.DissonanceOrb, growthProgress);
                    CustomParticles.GenericFlare(sparklePos, col, 0.4f * currentScale, 16);
                }
            }

            // Rotating glyph circle (every 25 frames)
            if (Main.GameUpdateCount % 25 == 0)
            {
                int glyphCount = 3 + (int)(growthProgress * 2);
                EnigmaVFXLibrary.SpawnGlyphCircle(pos, glyphCount, 35f * currentScale, 0.04f);
            }

            // Core pulsing flare (every 12 frames)
            if (Main.GameUpdateCount % 12 == 0)
            {
                float corePulse = 0.55f + MathF.Sin(time * 0.12f) * 0.15f;
                Color coreCol = EnigmaPalette.GetEnigmaGradient(growthProgress);
                CustomParticles.GenericFlare(pos, coreCol, corePulse * currentScale * 0.55f, 14);
            }

            // Void swirl inward (every 12 frames)
            if (Main.GameUpdateCount % 12 == 0)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 60f * currentScale + Main.rand.NextFloat(30f);
                Vector2 particlePos = pos + angle.ToRotationVector2() * radius;
                Vector2 vel = (pos - particlePos).SafeNormalize(Vector2.Zero) * 3f;

                Color swirlCol = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * 0.65f;
                var glow = new GenericGlowParticle(particlePos, vel,
                    swirlCol, 0.28f * currentScale, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        // =====================================================================
        //  ORB AURA DAMAGE VFX
        // =====================================================================

        /// <summary>
        /// Aura damage indicator on an enemy within the orb's damage aura.
        /// Flare at enemy plus occasional fractal lightning from orb to target.
        /// Call from DealAuraDamage() per affected enemy.
        /// </summary>
        public static void OrbAuraDamageVFX(Vector2 orbPos, Vector2 targetPos)
        {
            if (Main.dedServ) return;

            // Flare at affected enemy
            CustomParticles.GenericFlare(targetPos, EnigmaPalette.Purple * 0.5f, 0.25f, 8);

            // Occasional fractal lightning from orb to enemy
            if (Main.rand.NextBool(4))
            {
                MagnumVFX.DrawFractalLightning(orbPos, targetPos,
                    EnigmaPalette.GreenFlame * 0.4f, 6, 15f, 2, 0.2f);
            }

            Lighting.AddLight(targetPos, EnigmaPalette.Purple.ToVector3() * 0.3f);
        }

        // =====================================================================
        //  RIDDLEBOLT RELEASE VFX
        // =====================================================================

        /// <summary>
        /// Riddlebolt spawn flare when the orb releases a homing projectile.
        /// Central flare, halo ring, and sparkle targeting line.
        /// Call from ReleaseRiddlebolt().
        /// </summary>
        public static void RiddleboltReleaseVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Central spawn flare
            CustomParticles.GenericFlare(spawnPos, EnigmaPalette.GreenFlame, 0.5f, 14);
            CustomParticles.GenericFlare(spawnPos, EnigmaPalette.WhiteGreenFlash, 0.3f, 10);

            // Halo ring
            CustomParticles.HaloRing(spawnPos, EnigmaPalette.Purple * 0.6f, 0.25f, 10);

            // Sparkle burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 4f);
                var spark = new GenericGlowParticle(spawnPos, vel, col * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music note
            if (Main.rand.NextBool(2))
                EnigmaVFXLibrary.SpawnMusicNotes(spawnPos, 1, 10f, 0.7f, 0.85f, 20);

            Lighting.AddLight(spawnPos, EnigmaPalette.GreenFlame.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  RIDDLEBOLT TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame riddlebolt trail VFX: dual-color dust, enigma shimmer,
        /// periodic flares, and music note whispers.
        /// Call from Riddlebolt.AI() every frame.
        /// </summary>
        public static void RiddleboltTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Dual-color void dust
            EnigmaVFXLibrary.SpawnEnigmaSwingDust(pos, -velocity.SafeNormalize(Vector2.Zero));

            // Green flame contrast sparkle
            EnigmaVFXLibrary.SpawnContrastSparkle(pos, -velocity.SafeNormalize(Vector2.Zero));

            // Enigma mystery shimmer trail (1 in 3)
            if (Main.rand.NextBool(3))
            {
                Color shimmerColor = EnigmaPalette.GetMysteryShimmer((float)Main.timeForVisualEffects);
                var glow = new GenericGlowParticle(pos, -velocity * 0.08f,
                    shimmerColor * 0.5f, 0.18f, 10, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Periodic flare (every 5 frames)
            if (Main.GameUpdateCount % 5 == 0)
            {
                Color trailColor = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(pos, trailColor * 0.6f, 0.25f, 12);
            }

            // Music note whisper (1 in 6)
            if (Main.rand.NextBool(6))
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 20);

            EnigmaVFXLibrary.AddPulsingLight(pos, (float)Main.timeForVisualEffects, 0.35f);
        }

        // =====================================================================
        //  RIDDLEBOLT IMPACT VFX
        // =====================================================================

        /// <summary>
        /// On-hit riddlebolt impact VFX: bloom flare, offset flares,
        /// glyph impact, watching eye, halo ring, and music notes.
        /// Call from Riddlebolt.OnHitNPC().
        /// </summary>
        public static void RiddleboltImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Central impact flare
            CustomParticles.GenericFlare(pos, Color.White, 0.6f, 16);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.5f, 18);

            // Offset flares in ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 20f;
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.DissonanceOrb, (float)i / 6f);
                CustomParticles.GenericFlare(pos + offset, col, 0.35f, 14);
            }

            // Glyph impact
            CustomParticles.GlyphImpact(pos, EnigmaPalette.Purple, EnigmaPalette.GreenFlame, 0.4f);

            // Watching eye at impact
            CustomParticles.EnigmaEyeImpact(pos, targetCenter, EnigmaPalette.GreenFlame, 0.5f);

            // Halo ring
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.3f, 12);

            // Glyph circle formation
            EnigmaVFXLibrary.SpawnGlyphCircle(pos, 6, 45f, 0.06f);

            // Music notes burst
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 3, 15f, 0.8f, 1.0f, 25);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  CASCADE EXPLOSION VFX
        // =====================================================================

        /// <summary>
        /// Massive cascade explosion VFX when the mystery orb dies or reaches max size.
        /// Multi-layered glyph circles, fractal explosion rings, particle storm,
        /// watching eye burst, grand sparkle formation, and music note scatter.
        /// Call from TriggerCascadeExplosion() or OnKill().
        /// </summary>
        public static void CascadeExplosionVFX(Vector2 pos, float currentScale)
        {
            if (Main.dedServ) return;

            // Central themed flash with enhanced bloom
            EnigmaVFXLibrary.DrawBloom(pos, 1.5f * currentScale);
            CustomParticles.GenericFlare(pos, Color.White, 1.2f, 25);
            CustomParticles.GenericFlare(pos, EnigmaPalette.ArcaneFlash, 1.0f * currentScale, 30);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.8f * currentScale, 28);

            // Multiple glyph circles at different radii — signature cascade effect
            for (int circle = 0; circle < 4; circle++)
            {
                float radius = (40f + circle * 50f) * currentScale;
                int glyphCount = 6 + circle * 2;
                float rotSpeed = (circle % 2 == 0 ? 1f : -1f) * (0.05f - circle * 0.01f);
                Color circleCol = EnigmaPalette.PaletteLerp(
                    EnigmaPalette.DissonanceOrb, (float)circle / 4f);
                CustomParticles.GlyphCircle(pos, circleCol, count: glyphCount,
                    radius: radius, rotationSpeed: rotSpeed);
            }

            // Glyph tower at center
            CustomParticles.GlyphTower(pos, EnigmaPalette.Purple, layers: 5,
                baseScale: 0.6f * currentScale);

            // Massive glyph burst
            EnigmaVFXLibrary.SpawnGlyphBurst(pos, 16, 8f);

            // Multi-layered fractal explosion rings
            for (int layer = 0; layer < 4; layer++)
            {
                int points = 10 + layer * 4;
                float radius = (50f + layer * 45f) * currentScale;

                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.2f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color col = EnigmaPalette.PaletteLerp(
                        EnigmaPalette.DissonanceOrb, (float)i / points);
                    CustomParticles.GenericFlare(pos + offset, col,
                        0.75f * currentScale - layer * 0.1f, 25);
                }
            }

            // Particle explosion outward (30 glowing orbs)
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                Color col = EnigmaPalette.GetEnigmaGradient((float)i / 30f);
                var glow = new GenericGlowParticle(pos, vel, col,
                    0.5f * currentScale, Main.rand.Next(30, 50), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Watching eyes burst from explosion
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 8, 5f);
            CustomParticles.EnigmaEyeFormation(pos, EnigmaPalette.EyeGreen,
                6, 80f * currentScale);

            // Grand sparkle formation rings
            for (int ring = 0; ring < 3; ring++)
            {
                int pointsInRing = 8 + ring * 4;
                float ringRadius = (100f + ring * 40f) * currentScale;

                for (int i = 0; i < pointsInRing; i++)
                {
                    float starAngle = MathHelper.TwoPi * i / pointsInRing + ring * 0.2f;
                    Vector2 starVel = starAngle.ToRotationVector2() * (4f + ring * 1.5f);
                    float t = (ring * pointsInRing + i) / (float)(3 * pointsInRing);
                    Color starCol = EnigmaPalette.GetEnigmaGradient(t);
                    var star = new GenericGlowParticle(pos, starVel,
                        starCol, 0.5f - ring * 0.1f, 25, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
            }

            // Cascading gradient halo rings
            EnigmaVFXLibrary.SpawnGradientHaloRings(pos, 7, 0.35f * currentScale);

            // Void swirl collapse
            EnigmaVFXLibrary.SpawnVoidSwirl(pos, 10, 80f * currentScale);

            // Dense radial dust burst
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 20, 8f);

            // Music notes explode outward
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 8, 80f * currentScale, 0.8f, 1.2f, 40);
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 12, 40f, 0.9f, 1.1f, 35);

            // Screen shake
            MagnumScreenEffects.AddScreenShake(8f);

            Lighting.AddLight(pos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 1.5f * currentScale);
        }
    }
}
