// DEPRECATED: Replaced by FractalOfTheStars/ folder self-contained system
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
    /// VFX helper for Fractal of the Stars — true melee weapon (850 dmg).
    /// On hit, spawns 3 spectral blades that orbit the target and fire prismatic beams.
    /// Handles hold-item ambient, item bloom, swing effects, impact burst,
    /// spectral blade trail, and spectral blade beam effects.
    /// Call from FractalOfTheStars, FractalSpectralBlade, and FractalPrismaticBeam.
    /// </summary>
    public static class FractalOfTheStarsVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: triple glyph orbit at 120 degrees apart,
        /// spectral blade echo particles, star particles, and cosmic pulse light.
        /// The fractal reveals infinite recursion in every star.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Triple glyph orbit — 3 glyphs at 120 degrees apart, radius 50
            if (Main.rand.NextBool(7))
            {
                for (int i = 0; i < 3; i++)
                {
                    float glyphAngle = time * 0.035f + MathHelper.TwoPi * i / 3f;
                    Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * 50f;
                    Color glyphCol = FatePalette.PaletteLerp(FatePalette.StarFractal, (float)i / 3f);
                    CustomParticles.Glyph(glyphPos, glyphCol * 0.45f, 0.2f, -1);
                }
            }

            // Spectral blade echo particles — ghostly blade fragments
            if (Main.rand.NextBool(10))
            {
                Vector2 echoPos = center + Main.rand.NextVector2Circular(35f, 35f);
                Color echoCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat(0.2f, 0.7f)) * 0.35f;
                var echo = new GenericGlowParticle(echoPos,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    echoCol, 0.16f, 16, true);
                MagnumParticleHandler.SpawnParticle(echo);
            }

            // Star particles — twinkling cosmic motes
            if (Main.rand.NextBool(8))
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(starPos,
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    starCol * 0.35f, 0.12f, 14, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Music notes — the fractal's recursive melody
            if (Main.rand.NextBool(12))
                FateVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.65f, 0.85f, 25);

            // Cosmic pulse light
            float pulse = 0.25f + MathF.Sin(time * 0.055f) * 0.1f;
            Lighting.AddLight(center, FatePalette.FateCyan.ToVector3() * pulse);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// Standard 3-layer Fate item bloom for the weapon sprite.
        /// Uses DrawItemBloom for consistent cosmic glow across all Fate items.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb,
            Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.06f;
            FatePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // ===== SWING VFX =====

        /// <summary>
        /// Per-frame VFX during melee swing. Cosmic sparks using palette gradient,
        /// occasional glyph accent, star particles, and fate swing dust.
        /// Every swing fractures starlight into prismatic shards.
        /// </summary>
        public static void SwingVFX(Vector2 swingPos, Player player)
        {
            if (Main.dedServ) return;

            Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);

            // Cosmic sparks along swing arc (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat());
                Vector2 sparkVel = -direction * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f);
                var spark = new GenericGlowParticle(swingPos + Main.rand.NextVector2Circular(6f, 6f),
                    sparkVel, sparkCol * 0.6f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Glyph accent along swing (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.StarFractal, Main.rand.NextFloat());
                CustomParticles.Glyph(swingPos, glyphCol * 0.5f, 0.18f, -1);
            }

            // Star dust particles (1-in-4)
            if (Main.rand.NextBool(4))
                FateVFXLibrary.SpawnStarSparkles(swingPos, 1, 10f, 0.15f);

            // Fate dual-color swing dust
            FateVFXLibrary.SpawnFateSwingDust(swingPos, -direction);

            // Contrast sparkle
            FateVFXLibrary.SpawnContrastSparkle(swingPos, -direction);

            // Music notes (periodic)
            if (Main.GameUpdateCount % 7 == 0)
                FateVFXLibrary.SpawnMusicNotes(swingPos, 1, 10f, 0.7f, 0.9f, 20);

            Lighting.AddLight(swingPos, FatePalette.FateCyan.ToVector3() * 0.45f);
        }

        // ===== IMPACT VFX =====

        /// <summary>
        /// On-hit melee impact VFX. Supernova explosion at 0.6 scale, glyph burst,
        /// music notes, 10 star particles in a ring, and strong cosmic lighting.
        /// The fractal shatters on impact — every fragment a universe.
        /// </summary>
        public static void ImpactVFX(Vector2 hitPos, int damageDone)
        {
            if (Main.dedServ) return;

            // Supernova explosion scaled to 0.6 — powerful but not screen-filling
            FateVFXLibrary.SupernovaExplosion(hitPos, 0.6f);

            // Glyph burst — fractal runes exploding outward
            FateVFXLibrary.SpawnGlyphBurst(hitPos, 8, 5f);

            // Music notes scatter — the fractal's melody shatters
            FateVFXLibrary.SpawnMusicNotes(hitPos, 4, 25f, 0.8f, 1.0f, 28);

            // 10 star particles in an expanding ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color starCol = FatePalette.PaletteLerp(FatePalette.StarFractal, (float)i / 10f);
                var star = new GenericGlowParticle(hitPos, vel,
                    starCol * 0.7f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Central flare — white-hot fractal core
            CustomParticles.GenericFlare(hitPos, FatePalette.WhiteCelestial, 0.6f, 16);
            CustomParticles.GenericFlare(hitPos, FatePalette.FateCyan, 0.5f, 18);

            // Halo rings — fractal resonance
            FateVFXLibrary.SpawnGradientHaloRings(hitPos, 4, 0.3f);

            // Dust burst
            FateVFXLibrary.SpawnRadialDustBurst(hitPos, 12, 6f);

            // Strong cosmic lighting
            Lighting.AddLight(hitPos, FatePalette.StarGold.ToVector3() * 1.2f);
        }

        // ===== SPECTRAL BLADE TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for orbiting spectral blades.
        /// GenericGlowParticle trail using StarFractal palette interpolation,
        /// star particle accents, and subtle cosmic dust.
        /// Each blade is a recursive echo of fractal starlight.
        /// </summary>
        public static void SpectralBladeTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 awayDir = -velocity.SafeNormalize(Vector2.Zero);

            // Primary glow trail — interpolated through StarFractal palette
            if (Main.rand.NextBool(2))
            {
                float trailT = (Main.GameUpdateCount * 0.02f) % 1f;
                Color trailCol = FatePalette.PaletteLerp(FatePalette.StarFractal, trailT);
                var trail = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(4f, 4f),
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    trailCol * 0.55f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Star particle accents — twinkling blade trail
            if (Main.rand.NextBool(5))
            {
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(6f, 6f),
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    starCol * 0.45f, 0.12f, 12, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Cosmic dust trail
            if (Main.rand.NextBool(3))
            {
                Color dustCol = FatePalette.GetGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f), 0, dustCol, 0.9f);
                d.noGravity = true;
            }

            // Glyph trail accent (1-in-8)
            if (Main.rand.NextBool(8))
                FateVFXLibrary.SpawnGlyphAccent(pos, 0.15f);

            // Music note (1-in-10)
            if (Main.rand.NextBool(10))
                FateVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.6f, 0.8f, 16);

            Lighting.AddLight(pos, FatePalette.FateCyan.ToVector3() * 0.3f);
        }

        // ===== SPECTRAL BLADE BEAM VFX =====

        /// <summary>
        /// Prismatic beam VFX fired from spectral blades toward enemies.
        /// Beam particles along the ray using palette interpolation,
        /// scaling intensity with growthProgress as the beam charges up.
        /// Each beam is a fractal of infinite starlight.
        /// </summary>
        public static void SpectralBladeBeamVFX(Vector2 start, Vector2 end, float growthProgress)
        {
            if (Main.dedServ) return;

            float beamDist = Vector2.Distance(start, end);
            int particleCount = (int)(beamDist / 10f);
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);

            // Beam particles along the ray — prismatic fractal energy
            for (int i = 0; i < particleCount; i++)
            {
                float progress = (float)i / particleCount;
                Vector2 particlePos = Vector2.Lerp(start, end, progress);
                particlePos += Main.rand.NextVector2Circular(4f * growthProgress, 4f * growthProgress);

                // Scale particle count with growth — more particles as beam strengthens
                if (Main.rand.NextBool(2))
                {
                    Color beamCol = FatePalette.PaletteLerp(FatePalette.StarFractal, progress);
                    float particleScale = 0.12f + growthProgress * 0.1f;
                    var glow = new GenericGlowParticle(particlePos,
                        direction.RotatedByRandom(0.3f) * 0.5f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        beamCol * 0.5f * growthProgress, particleScale, 10, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Beam dust — prismatic fragments along the ray
            for (int i = 0; i < 2; i++)
            {
                float dustProgress = Main.rand.NextFloat();
                Vector2 dustPos = Vector2.Lerp(start, end, dustProgress);
                Color dustCol = FatePalette.PaletteLerp(FatePalette.StarFractal, dustProgress);
                Dust d = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.PinkTorch, Main.rand.NextVector2Circular(1f, 1f),
                    0, dustCol, 0.8f * growthProgress);
                d.noGravity = true;
            }

            // Origin flare — beam emission point
            if (growthProgress > 0.3f)
                CustomParticles.GenericFlare(start, FatePalette.FateCyan * growthProgress, 0.3f, 10);

            // Endpoint flare as beam reaches full power
            if (growthProgress > 0.6f)
            {
                CustomParticles.GenericFlare(end, FatePalette.StarGold * growthProgress, 0.35f, 12);
                FateVFXLibrary.SpawnStarSparkles(end, 1, 8f, 0.1f);
            }

            // Star sparkles along beam (1-in-4)
            if (Main.rand.NextBool(4) && growthProgress > 0.4f)
            {
                float sparkProgress = Main.rand.NextFloat();
                Vector2 sparkPos = Vector2.Lerp(start, end, sparkProgress);
                var spark = new GenericGlowParticle(sparkPos + Main.rand.NextVector2Circular(6f, 6f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    FatePalette.WhiteCelestial * 0.4f * growthProgress, 0.1f, 10, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(end, FatePalette.FateCyan.ToVector3() * 0.5f * growthProgress);
        }

        // ===== SPECTRAL BLADE SPAWN VFX =====

        /// <summary>
        /// One-shot VFX when a spectral blade spawns after a melee hit.
        /// Flash of prismatic energy and glyph seal formation.
        /// </summary>
        public static void SpectralBladeSpawnVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Prismatic flare
            CustomParticles.GenericFlare(spawnPos, FatePalette.FateCyan, 0.45f, 14);
            CustomParticles.HaloRing(spawnPos, FatePalette.NebulaPurple * 0.6f, 0.25f, 12);

            // Star spark ring — 3 sparkles forming the blade's initial position
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3f);
                Color sparkCol = FatePalette.PaletteLerp(FatePalette.StarFractal, (float)i / 3f);
                var spark = new GlowSparkParticle(spawnPos, sparkVel,
                    sparkCol * 0.7f, 0.2f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music note
            FateVFXLibrary.SpawnMusicNotes(spawnPos, 1, 8f, 0.7f, 0.85f, 18);

            Lighting.AddLight(spawnPos, FatePalette.FateCyan.ToVector3() * 0.6f);
        }

        // ===== TRAIL RENDERING FUNCTIONS =====

        /// <summary>
        /// Trail color function for spectral blade projectile trails.
        /// Uses StarFractal palette with additive-safe output.
        /// </summary>
        public static Color FractalTrailColor(float completionRatio)
        {
            Color c = FatePalette.PaletteLerp(FatePalette.StarFractal,
                0.15f + completionRatio * 0.7f);
            float fade = 1f - MathF.Pow(completionRatio, 1.2f);
            return (c * fade) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for spectral blade trails.
        /// Standard fate trail with moderate width.
        /// </summary>
        public static float FractalTrailWidth(float completionRatio)
            => FateVFXLibrary.FateTrailWidth(completionRatio, 14f);
    }
}
*/