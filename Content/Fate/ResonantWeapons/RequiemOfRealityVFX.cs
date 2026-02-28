// ============================================================================
// DEPRECATED: Replaced by self-contained systems in RequiemOfReality/ folder.
// New VFX is distributed across Particles/, Primitives/, Shaders/, Projectiles/.
// ============================================================================
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
    /// VFX helper for Requiem of Reality — melee + music notes, 740 dmg.
    /// Swings release cosmic music notes that seek enemies.
    /// Every 4th swing spawns a spectral blade with combo attack.
    /// Palette: FatePalette.RealityRequiem
    /// (CosmicVoid -> FatePurple -> BrightCrimson -> DarkPink -> ConstellationSilver -> SupernovaWhite)
    /// </summary>
    public static class RequiemOfRealityVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: floating music notes orbit the player,
        /// glyphs drift in a rhythm pattern, star sparkles accompany,
        /// and a harmonic light pulse syncs to the rhythm.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Floating music notes orbiting the player
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = time * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                float orbitRadius = Main.rand.NextFloat(30f, 50f);
                Vector2 notePos = center + orbitAngle.ToRotationVector2() * orbitRadius;
                Color noteCol = FatePalette.PaletteLerp(FatePalette.RealityRequiem, Main.rand.NextFloat());
                var note = new GenericGlowParticle(notePos,
                    orbitAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 1.2f,
                    noteCol * 0.7f, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(note);
            }

            // Glyphs in rhythm pattern — oscillating with Math.Sin
            if (Main.rand.NextBool(12))
            {
                float oscillation = (float)Math.Sin(time * 0.08f) * 25f;
                Vector2 glyphPos = center + new Vector2(oscillation, -20f + Main.rand.NextFloat(-10f, 10f));
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.RealityRequiem, Main.rand.NextFloat(0.2f, 0.6f));
                CustomParticles.Glyph(glyphPos, glyphCol, 0.22f, -1);
            }

            // Star sparkle accompaniment
            if (Main.rand.NextBool(7))
            {
                Vector2 sparklePos = center + Main.rand.NextVector2Circular(35f, 35f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.ConstellationSilver : FatePalette.SupernovaWhite;
                var star = new GenericGlowParticle(sparklePos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.6f, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Harmonic light pulse synced to rhythm
            float rhythmPulse = (float)Math.Sin(time * 0.06f) * 0.5f + 0.5f;
            Color lightCol = Color.Lerp(FatePalette.FatePurple, FatePalette.BrightCrimson, rhythmPulse);
            float intensity = 0.3f + rhythmPulse * 0.15f;
            Lighting.AddLight(center, lightCol.ToVector3() * intensity);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// Standard 3-layer Fate item bloom for the Requiem of Reality weapon sprite.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.04f;
            FatePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // ===== SWING VFX =====

        /// <summary>
        /// Per-frame swing VFX: cosmic sparks, music notes scatter,
        /// sparkle accents along the swing arc.
        /// </summary>
        public static void SwingVFX(Vector2 swingPos, Player player)
        {
            if (Main.dedServ) return;

            // Cosmic sparks (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat());
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                var spark = new GlowSparkParticle(swingPos, sparkVel, sparkCol, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes scatter (1-in-4)
            if (Main.rand.NextBool(4))
                FateVFXLibrary.SpawnMusicNotes(swingPos, 1, 12f, 0.7f, 0.9f, 22);

            // Sparkle accents (1-in-5)
            if (Main.rand.NextBool(5))
            {
                Color accentCol = FatePalette.PaletteLerp(FatePalette.RealityRequiem, Main.rand.NextFloat());
                CustomParticles.GenericFlare(swingPos + Main.rand.NextVector2Circular(10f, 10f),
                    accentCol * 0.8f, 0.3f, 12);
            }

            Lighting.AddLight(swingPos, FatePalette.BrightCrimson.ToVector3() * 0.5f);
        }

        // ===== MUSIC NOTE SPAWN VFX =====

        /// <summary>
        /// One-shot VFX at the spawn position of a seeking music note projectile.
        /// Glyph accent, flare, and star sparkles mark the birth of each note.
        /// </summary>
        public static void MusicNoteSpawnVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            // Glyph accent at spawn
            Color glyphCol = FatePalette.PaletteLerp(FatePalette.RealityRequiem, Main.rand.NextFloat(0.3f, 0.7f));
            CustomParticles.Glyph(spawnPos, glyphCol, 0.25f, -1);

            // Central flare
            CustomParticles.GenericFlare(spawnPos, FatePalette.BrightCrimson, 0.4f, 14);

            // Star sparkles at note spawn
            FateVFXLibrary.SpawnStarSparkles(spawnPos, 3, 15f, 0.2f);

            Lighting.AddLight(spawnPos, FatePalette.DarkPink.ToVector3() * 0.5f);
        }

        // ===== MUSIC NOTE TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for seeking music note projectiles.
        /// GenericGlowParticle using the RealityRequiem palette with nebula wisps.
        /// </summary>
        public static void MusicNoteTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 trailDir = -velocity.SafeNormalize(Vector2.Zero);

            // Primary trail glow using RealityRequiem palette
            Color trailCol = FatePalette.PaletteLerp(FatePalette.RealityRequiem, Main.rand.NextFloat(0.2f, 0.8f));
            var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(4f, 4f),
                trailDir * 0.8f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                trailCol * 0.7f, 0.2f, 14, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Small nebula wisps (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color wispCol = Color.Lerp(FatePalette.FatePurple, FatePalette.DarkPink, Main.rand.NextFloat()) * 0.4f;
                var wisp = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(6f, 6f),
                    trailDir * 0.3f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    wispCol, 0.15f, 18, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            Lighting.AddLight(pos, FatePalette.FatePurple.ToVector3() * 0.3f);
        }

        // ===== MUSIC NOTE IMPACT VFX =====

        /// <summary>
        /// Impact VFX when a seeking music note hits an enemy.
        /// Uses FateVFXLibrary.ProjectileImpact at 0.6f intensity,
        /// cosmic flames, and electricity sparks via DrawCosmicLightning.
        /// </summary>
        public static void MusicNoteImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Shared projectile impact at moderate intensity
            FateVFXLibrary.ProjectileImpact(pos, 0.6f);

            // Cosmic flames burst outward
            for (int i = 0; i < 5; i++)
            {
                Vector2 flameVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                Color flameCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat());
                var flame = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(8f, 8f),
                    flameVel, flameCol * 0.7f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(flame);
            }

            // Electricity sparks via DrawCosmicLightning — short bursts
            for (int i = 0; i < 2; i++)
            {
                Vector2 sparkEnd = pos + Main.rand.NextVector2Circular(35f, 35f);
                FateVFXLibrary.DrawCosmicLightning(pos, sparkEnd, 6, 12f,
                    FatePalette.ConstellationSilver, FatePalette.SupernovaWhite);
            }

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.8f);
        }

        // ===== SPECTRAL BLADE COMBO SPAWN VFX =====

        /// <summary>
        /// Dramatic VFX when the 4th-swing spectral blade combo spawns.
        /// Glyph burst (8 glyphs), music notes (6), cosmic cloud burst,
        /// and a strong sound-cue light flash.
        /// </summary>
        public static void SpectralBladeComboSpawnVFX(Vector2 playerCenter)
        {
            if (Main.dedServ) return;

            // Dramatic glyph burst — 8 glyphs radiating outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 glyphPos = playerCenter + angle.ToRotationVector2() * 30f;
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.RealityRequiem, (float)i / 8f);
                CustomParticles.Glyph(glyphPos, glyphCol, 0.3f, -1);
            }

            // Music notes — 6 notes scattering outward
            FateVFXLibrary.SpawnMusicNotes(playerCenter, 6, 30f, 0.8f, 1.1f, 32);

            // Cosmic cloud burst — nebula energy eruption
            FateVFXLibrary.SpawnCosmicCloudBurst(playerCenter, 0.8f, 14);

            // Halo ring cascade
            FateVFXLibrary.SpawnGradientHaloRings(playerCenter, 4, 0.35f);

            // Central flare flash
            CustomParticles.GenericFlare(playerCenter, FatePalette.SupernovaWhite, 0.7f, 18);
            CustomParticles.GenericFlare(playerCenter, FatePalette.BrightCrimson, 0.55f, 16);

            // Bloom flash
            FateVFXLibrary.DrawBloom(playerCenter, 0.6f);

            // Strong sound-cue light
            Lighting.AddLight(playerCenter, FatePalette.SupernovaWhite.ToVector3() * 1.2f);
        }

        // ===== IMPACT VFX =====

        /// <summary>
        /// Full on-hit impact VFX for Requiem of Reality melee strikes.
        /// Combines FateVFXLibrary.MeleeImpact with lightning strike,
        /// music notes, glyphs, and 8 star particles for a cosmic crescendo.
        /// </summary>
        public static void ImpactVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Base melee impact via shared library
            FateVFXLibrary.MeleeImpact(hitPos, 0);

            // Lightning strike accent — short bolt from above
            Vector2 lightningStart = hitPos + new Vector2(Main.rand.NextFloat(-15f, 15f), -120f);
            FateVFXLibrary.DrawCosmicLightning(lightningStart, hitPos, 10, 25f,
                FatePalette.ConstellationSilver, FatePalette.SupernovaWhite);

            // Music notes burst at impact
            FateVFXLibrary.SpawnMusicNotes(hitPos, 3, 20f, 0.75f, 1.0f, 25);

            // Glyph accents around impact
            for (int i = 0; i < 3; i++)
            {
                Vector2 glyphPos = hitPos + Main.rand.NextVector2Circular(20f, 20f);
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.RealityRequiem, Main.rand.NextFloat());
                CustomParticles.Glyph(glyphPos, glyphCol, 0.22f, -1);
            }

            // 8 star particles radiating outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.ConstellationSilver : FatePalette.SupernovaWhite;
                var star = new GenericGlowParticle(hitPos, starVel, starCol * 0.8f, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            Lighting.AddLight(hitPos, FatePalette.BrightCrimson.ToVector3() * 0.9f);
        }
    }
}
*/