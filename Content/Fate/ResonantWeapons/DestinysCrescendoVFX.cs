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
    /// VFX helper for Destiny's Crescendo — cosmic deity summoner weapon (400 dmg).
    /// Summons a cosmic deity minion that rapidly slashes and fires cosmic light beams.
    /// Handles hold-item ambient, item bloom, summon burst, deity ambient/slash/beam effects.
    /// Call from DestinysCrescendo, CosmicDeityMinion, and DeityBeamProjectile.
    /// </summary>
    public static class DestinysCrescendoVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: 6-point star formation, orbiting deity essence glyphs,
        /// cosmic cloud aura, and divine radiance light pulse.
        /// The summoner channels the crescendo of destiny through their staff.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 6-point star formation — divine constellation around the wielder
            if (Main.rand.NextBool(7))
            {
                for (int i = 0; i < 6; i++)
                {
                    float starAngle = time * 0.025f + MathHelper.TwoPi * i / 6f;
                    float starRadius = 45f + MathF.Sin(time * 0.03f + i * 0.5f) * 8f;
                    Vector2 starPos = center + starAngle.ToRotationVector2() * starRadius;
                    Color starCol = FatePalette.PaletteLerp(FatePalette.DestinyCrescendo, (float)i / 6f);
                    var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(0.3f, 0.3f),
                        starCol * 0.4f, 0.14f, 16, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
            }

            // Orbiting deity essence glyphs — 2 glyphs at opposite positions
            if (Main.rand.NextBool(10))
            {
                for (int i = 0; i < 2; i++)
                {
                    float glyphAngle = time * 0.04f + MathHelper.Pi * i;
                    Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * 35f;
                    Color glyphCol = FatePalette.GetCosmicGradient(0.3f + i * 0.4f);
                    CustomParticles.Glyph(glyphPos, glyphCol * 0.5f, 0.2f, -1);
                }
            }

            // Cosmic cloud aura — nebula wisps around the summoner
            if (Main.rand.NextBool(15))
                FateVFXLibrary.SpawnCosmicCloudTrail(center + Main.rand.NextVector2Circular(25f, 25f),
                    Vector2.Zero, 0.3f);

            // Star sparkle motes
            if (Main.rand.NextBool(9))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Color sparkCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var spark = new GenericGlowParticle(sparkPos, Main.rand.NextVector2Circular(0.4f, 0.4f),
                    sparkCol * 0.35f, 0.12f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes — destiny's melody builds
            if (Main.rand.NextBool(12))
                FateVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.65f, 0.85f, 25);

            // Divine radiance light pulse
            float pulse = 0.25f + MathF.Sin(time * 0.06f) * 0.1f;
            Lighting.AddLight(center, FatePalette.DarkPink.ToVector3() * pulse);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// Standard 3-layer Fate item bloom for the summoning staff sprite.
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

        // ===== SUMMON VFX =====

        /// <summary>
        /// One-shot VFX when a cosmic deity minion is summoned.
        /// Supernova explosion, glyph burst, 12-star spark ring,
        /// and constellation burst — the crescendo reaches its peak.
        /// </summary>
        public static void SummonVFX(Vector2 summonPos)
        {
            if (Main.dedServ) return;

            // Supernova explosion as the deity manifests
            FateVFXLibrary.SupernovaExplosion(summonPos, 0.8f);

            // Glyph burst — the summoning seal explodes outward
            FateVFXLibrary.SpawnGlyphBurst(summonPos, 10, 5f);

            // 12 star sparks in a ring — celestial herald formation
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkPos = summonPos + angle.ToRotationVector2() * 40f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color sparkCol = FatePalette.PaletteLerp(FatePalette.DestinyCrescendo, (float)i / 12f);
                var spark = new GlowSparkParticle(sparkPos, sparkVel,
                    sparkCol * 0.8f, 0.25f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Constellation burst — the deity's star map forms
            FateVFXLibrary.SpawnConstellationBurst(summonPos, 8, 65f, 0.9f);

            // Gradient halo rings — summoning resonance
            FateVFXLibrary.SpawnGradientHaloRings(summonPos, 5, 0.35f);

            // Music notes cascade — the crescendo peaks
            FateVFXLibrary.SpawnMusicNotes(summonPos, 5, 30f, 0.8f, 1.1f, 30);

            // Radial dust burst — cosmic energy coalescing
            FateVFXLibrary.SpawnRadialDustBurst(summonPos, 14, 5f);

            Lighting.AddLight(summonPos, FatePalette.StarGold.ToVector3() * 1.0f);
        }

        // ===== DEITY AMBIENT VFX =====

        /// <summary>
        /// Per-frame ambient VFX for the cosmic deity minion.
        /// Subtle glow particles orbiting the deity, star sparkles, and pulsing divine light.
        /// The deity radiates celestial authority.
        /// </summary>
        public static void DeityAmbientVFX(Vector2 deityPos)
        {
            if (Main.dedServ) return;

            // Orbiting glow particles — celestial aura
            if (Main.GameUpdateCount % 6 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.06f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 20f + MathF.Sin(Main.GameUpdateCount * 0.08f + i) * 6f;
                    Vector2 orbitPos = deityPos + angle.ToRotationVector2() * radius;
                    Color orbitCol = FatePalette.PaletteLerp(FatePalette.DestinyCrescendo, (float)i / 3f);
                    var glow = new GenericGlowParticle(orbitPos, Main.rand.NextVector2Circular(0.3f, 0.3f),
                        orbitCol * 0.5f, 0.18f, 14, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Star sparkles — twinkling divine presence
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkPos = deityPos + Main.rand.NextVector2Circular(18f, 18f);
                Color sparkCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var spark = new GenericGlowParticle(sparkPos, Main.rand.NextVector2Circular(0.4f, 0.4f),
                    sparkCol * 0.4f, 0.12f, 12, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Cosmic dust — soft divine aura
            if (Main.rand.NextBool(4))
            {
                Color dustCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(deityPos + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.PinkTorch, Main.rand.NextVector2Circular(1f, 1f), 0, dustCol, 0.9f);
                d.noGravity = true;
            }

            // Periodic glyph aura — the deity's sigil
            if (Main.GameUpdateCount % 20 == 0)
                CustomParticles.GlyphCircle(deityPos, FatePalette.DarkPink * 0.5f,
                    count: 3, radius: 25f, rotationSpeed: 0.05f);

            // Music notes — the deity hums the crescendo (1-in-8)
            if (Main.rand.NextBool(8))
                FateVFXLibrary.SpawnMusicNotes(deityPos, 1, 12f, 0.65f, 0.8f, 20);

            // Pulsing divine light
            float pulse = 0.3f + MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.1f;
            Lighting.AddLight(deityPos, FatePalette.BrightCrimson.ToVector3() * pulse);
        }

        // ===== DEITY SLASH VFX =====

        /// <summary>
        /// VFX when the cosmic deity performs a rapid melee slash.
        /// Directional spark burst, music notes, glyph accent, and cosmic dust.
        /// Each slash is a measure in the cosmic crescendo.
        /// </summary>
        public static void DeitySlashVFX(Vector2 slashPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            Vector2 normDir = direction.SafeNormalize(Vector2.UnitX);

            // Directional spark burst — cosmic energy arcing from the slash
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = normDir.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 6f);
                Color sparkCol = FatePalette.PaletteLerp(FatePalette.DestinyCrescendo,
                    Main.rand.NextFloat(0.2f, 0.8f));
                var spark = new GenericGlowParticle(slashPos, sparkVel,
                    sparkCol * 0.7f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Fate dual-color dust
            FateVFXLibrary.SpawnFateSwingDust(slashPos, -normDir);

            // Music notes — the slash strikes a chord
            FateVFXLibrary.SpawnMusicNotes(slashPos, 1, 10f, 0.7f, 0.9f, 20);

            // Glyph accent at slash point
            if (Main.rand.NextBool(3))
                FateVFXLibrary.SpawnGlyphAccent(slashPos, 0.2f);

            // Cosmic dust burst in slash direction
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = normDir.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 4f);
                Color dustCol = FatePalette.GetGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(slashPos, DustID.PinkTorch, dustVel, 0, dustCol, 1.1f);
                d.noGravity = true;
            }

            // Subtle halo at slash
            CustomParticles.HaloRing(slashPos, FatePalette.BrightCrimson * 0.5f, 0.2f, 10);

            Lighting.AddLight(slashPos, FatePalette.BrightCrimson.ToVector3() * 0.5f);
        }

        // ===== DEITY BEAM VFX =====

        /// <summary>
        /// Cosmic light beam VFX fired by the deity minion.
        /// Uses DrawCosmicLightning for the beam body with DarkPink primary
        /// and StarGold secondary, plus beam particles along the line.
        /// The beam is destiny's judgment rendered in light.
        /// </summary>
        public static void DeityBeamVFX(Vector2 start, Vector2 end)
        {
            if (Main.dedServ) return;

            // Cosmic lightning beam — the deity channels cosmic judgment
            FateVFXLibrary.DrawCosmicLightning(start, end, 14, 35f,
                FatePalette.DarkPink, FatePalette.StarGold);

            // Beam particles along the line — energy flowing through the beam
            float beamDist = Vector2.Distance(start, end);
            int particleCount = (int)(beamDist / 12f);

            for (int i = 0; i < particleCount; i++)
            {
                float progress = (float)i / particleCount;
                Vector2 particlePos = Vector2.Lerp(start, end, progress);
                particlePos += Main.rand.NextVector2Circular(6f, 6f);

                if (Main.rand.NextBool(3))
                {
                    Color beamCol = FatePalette.PaletteLerp(FatePalette.DestinyCrescendo, progress);
                    var glow = new GenericGlowParticle(particlePos,
                        Main.rand.NextVector2Circular(1f, 1f),
                        beamCol * 0.5f, 0.15f, 12, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Endpoint flares — bright impact/emission points
            CustomParticles.GenericFlare(start, FatePalette.DarkPink, 0.4f, 12);
            CustomParticles.GenericFlare(end, FatePalette.StarGold, 0.5f, 14);

            // Star sparkles at beam endpoint
            FateVFXLibrary.SpawnStarSparkles(end, 3, 15f, 0.2f);

            // Music note at beam origin
            if (Main.rand.NextBool(4))
                FateVFXLibrary.SpawnMusicNotes(start, 1, 8f, 0.7f, 0.85f, 18);

            Lighting.AddLight(end, FatePalette.StarGold.ToVector3() * 0.7f);
        }

        // ===== DEITY DEATH VFX =====

        /// <summary>
        /// VFX when the cosmic deity despawns or is unsummoned.
        /// Graceful dissolution into star particles and cosmic dust.
        /// </summary>
        public static void DeityDespawnVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Star particle dissolution
            FateVFXLibrary.SpawnStarSparkles(pos, 8, 30f, 0.25f);

            // Glyph circle — the seal fades
            FateVFXLibrary.SpawnGlyphCircle(pos, 4, 30f, 0.04f);

            // Gradient halos — fading resonance
            FateVFXLibrary.SpawnGradientHaloRings(pos, 3, 0.25f);

            // Music notes — the crescendo's final echoes
            FateVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.7f, 0.95f, 28);

            // Cosmic cloud dissolution
            FateVFXLibrary.SpawnCosmicCloudBurst(pos, 0.5f, 8);

            Lighting.AddLight(pos, FatePalette.DarkPink.ToVector3() * 0.6f);
        }

        // ===== TRAIL RENDERING FUNCTIONS =====

        /// <summary>
        /// Trail color function for deity beam projectiles.
        /// Uses DestinyCrescendo palette with additive-safe output.
        /// </summary>
        public static Color CrescendoTrailColor(float completionRatio)
        {
            Color c = FatePalette.PaletteLerp(FatePalette.DestinyCrescendo,
                0.2f + completionRatio * 0.6f);
            float fade = 1f - MathF.Pow(completionRatio, 1.4f);
            return (c * fade) with { A = 0 };
        }

        /// <summary>
        /// Trail width function for deity beam projectiles.
        /// Moderate width with elegant taper.
        /// </summary>
        public static float CrescendoTrailWidth(float completionRatio)
            => FateVFXLibrary.CosmicBeamWidth(completionRatio, 12f);
    }
}
