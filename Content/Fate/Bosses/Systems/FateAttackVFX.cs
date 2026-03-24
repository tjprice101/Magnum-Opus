using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Content.Fate.Bosses.Systems.FateSkySystem;

namespace MagnumOpus.Content.Fate.Bosses.Systems
{
    /// <summary>
    /// Fate boss attack choreography -- per-attack VFX sequences.
    ///
    /// Design principles for the Warden of Melodies:
    ///   Phase 1 (Awakening, >70% HP): Dark cosmic crimson palette with
    ///       boss-fragment echoes -- fleeting references to the 10 themes.
    ///   Phase 2 (Convergence, 40-70% HP): Dual-theme fusion attacks --
    ///       two theme colors merge in each telegraph + impact.
    ///   Phase 3 (Singularity, <40% HP): Inward singularity pull then
    ///       outward launch -- black hole accretion VFX on every attack.
    ///   All impacts produce chromatic aberration ripples via RGB-split flares.
    /// </summary>
    public static class FateAttackVFX
    {
        // 10 theme colors matching FateBossShaderSystem
        private static readonly Color[] ThemeColors = new Color[]
        {
            new Color(200, 120, 180),  // Spring
            new Color(255, 200, 50),   // Eroica
            new Color(80, 200, 100),   // Enigma
            new Color(140, 100, 200),  // Moonlight Sonata
            new Color(240, 240, 255),  // Swan Lake
            new Color(255, 140, 40),   // La Campanella
            new Color(200, 50, 30),    // Dies Irae
            new Color(150, 200, 255),  // Clair de Lune
            new Color(100, 120, 200),  // Nachtmusik
            new Color(255, 200, 50),   // Ode to Joy
        };

        // --- Shared VFX Helpers ---

        /// <summary>
        /// Chromatic aberration impact ripple -- draws RGB-split flares at impact point.
        /// Every Fate attack impact should call this for the signature "reality fracture" feel.
        /// </summary>
        private static void ChromaticImpactRipple(Vector2 position, float intensity = 1f)
        {
            float offset = 4f * intensity;
            float scale = 0.4f * intensity;
            int lifetime = (int)(12 * intensity);

            // Red channel offset left
            CustomParticles.GenericFlare(position + new Vector2(-offset, 0), new Color(255, 40, 60) * 0.5f, scale, lifetime);
            // Blue channel offset right
            CustomParticles.GenericFlare(position + new Vector2(offset, 0), new Color(60, 40, 255) * 0.5f, scale, lifetime);
            // White core
            CustomParticles.GenericFlare(position, FatePalette.WhiteCelestial * 0.7f, scale * 1.2f, lifetime + 4);
        }

        /// <summary>
        /// Spawns a boss-fragment echo -- a fleeting particle in one of the 10 theme colors.
        /// Used in Phase 1 to hint at the cosmic power the Warden commands.
        /// </summary>
        private static void SpawnFragmentEcho(Vector2 position, float spreadRadius)
        {
            int themeIdx = Main.rand.Next(10);
            Color echoColor = Color.Lerp(ThemeColors[themeIdx], FatePalette.BrightCrimson, 0.35f);
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            Vector2 pos = position + angle.ToRotationVector2() * Main.rand.NextFloat(spreadRadius);
            CustomParticles.GenericFlare(pos, echoColor, 0.35f, 14);
            if (Main.rand.NextBool(3))
                CustomParticles.Glyph(pos, ThemeColors[themeIdx] * 0.5f, 0.25f, Main.rand.Next(1, 13));
        }

        /// <summary>
        /// Dual-theme fusion burst -- spawns particles blending two theme colors.
        /// Used in Phase 2+ to show the Warden merging the themes together.
        /// </summary>
        private static void DualThemeBurst(Vector2 position, int count, float spreadRadius)
        {
            int themeA = Main.rand.Next(10);
            int themeB = (themeA + 3 + Main.rand.Next(4)) % 10;
            for (int i = 0; i < count; i++)
            {
                float blend = (float)i / count;
                Color fusionColor = Color.Lerp(ThemeColors[themeA], ThemeColors[themeB], blend);
                fusionColor = Color.Lerp(fusionColor, FatePalette.BrightCrimson, 0.3f);
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(0.2f);
                Vector2 pos = position + angle.ToRotationVector2() * Main.rand.NextFloat(spreadRadius);
                CustomParticles.GenericFlare(pos, fusionColor, 0.4f, 16);
            }
        }

        /// <summary>
        /// Singularity inward pull + outward launch VFX.
        /// Phase 3 signature: particles rush inward to center, pause, then explode outward.
        /// </summary>
        private static void SingularityPulse(Vector2 center, float radius, int particleCount)
        {
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 spawnPos = center + angle.ToRotationVector2() * radius;
                Vector2 inwardVel = (center - spawnPos).SafeNormalize(Vector2.Zero) * 4f;
                Color color = Color.Lerp(FatePalette.DarkPink, FatePalette.WhiteCelestial, (float)i / particleCount);
                var spark = new SparkleParticle(spawnPos, inwardVel, color * 0.8f, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            // Delayed outward bloom (spawned immediately but visually builds the sequence)
            var bloom = new BloomParticle(center, Vector2.Zero, FatePalette.BrightCrimson, 0.5f, 22);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        #region Phase 1 -- Fated Prelude (>70% HP)

        // --- CosmicDash: Teleport + streaking constellation trail ---

        public static void CosmicDashTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 600f, 30, FatePalette.DarkPink * 0.7f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 600f, FatePalette.DarkPink, 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.6f);
            // Fragment echo at dash origin
            SpawnFragmentEcho(position, 40f);
        }

        public static void CosmicDashTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, FatePalette.DarkPink, 0.35f, 10);
            CustomParticles.GlowTrail(position, FatePalette.DarkPink, 0.4f);
            if (Main.rand.NextBool(3))
                SpawnFragmentEcho(position, 25f);
        }

        public static void CosmicDashImpact(Vector2 position)
        {
            TriggerCosmicFlash(8f);
            MagnumScreenEffects.AddScreenShake(12f);
            ChromaticImpactRipple(position, 1.2f);
            CustomParticles.FateImpactBurst(position, 10);
            Phase10BossVFX.CymbalCrashBurst(position, 1.0f);
            CustomParticles.HaloRing(position, FatePalette.DarkPink, 0.6f, 18);
            ThemedParticles.FateMusicNotes(position, 5, 60f);
            var bloom = new BloomParticle(position, Vector2.Zero, FatePalette.DarkPink, 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        // --- StarfallBarrage: Raining cosmic projectiles from above ---

        public static void StarfallBarrageTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -250), FatePalette.WhiteCelestial, 0.8f);
            TelegraphSystem.DangerZone(targetArea, 250f, 40, FatePalette.DarkPink * 0.3f);
            // Scattered fragment echoes falling from above
            for (int i = 0; i < 3; i++)
                SpawnFragmentEcho(targetArea + new Vector2(Main.rand.NextFloat(-120, 120), -200), 30f);
        }

        public static void StarfallBarrageParticle(Vector2 position)
        {
            Color color = Color.Lerp(FatePalette.DarkPink, FatePalette.WhiteCelestial, Main.rand.NextFloat(0.4f));
            CustomParticles.GenericFlare(position, color, 0.3f, 8);
            ChromaticImpactRipple(position, 0.5f);
            if (Main.rand.NextBool(3))
                CustomParticles.Glyph(position, FatePalette.WhiteCelestial * 0.5f, 0.2f, Main.rand.Next(1, 13));
        }

        // --- GlyphCircle: Orbiting glyphs converging on target ---

        public static void GlyphCircleTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 150f, 45, FatePalette.DarkPink * 0.6f);
            Phase10BossVFX.ChordBuildupSpiral(center,
                new[] { FatePalette.CosmicVoid, FatePalette.DarkPink, FatePalette.BrightCrimson, FatePalette.WhiteCelestial }, 0.5f);
            CustomParticles.GlyphCircle(center, FatePalette.DarkPink, 12, 120f, 0.03f);
        }

        public static void GlyphCircleRelease(Vector2 center, int burstIndex)
        {
            TriggerCosmicFlash(6f);
            float angle = burstIndex * 0.25f;
            Color color = Color.Lerp(FatePalette.DarkPink, FatePalette.WhiteCelestial, (float)Math.Sin(angle) * 0.5f + 0.5f);
            CustomParticles.GlyphBurst(center, color, 6, 5f);
            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 0.5f, 15);
            ChromaticImpactRipple(center, 0.8f);
            var bloom = new BloomParticle(center, Vector2.Zero, color, 0.5f, 18);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        // --- DestinyChain: Linked cosmic chains that bind then explode ---

        public static void DestinyChainTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 30f, 35, FatePalette.DarkPink * 0.5f);
            Phase10BossVFX.StaffLineLaser(start, end, FatePalette.DarkPink, 20f);
            SpawnFragmentEcho(start, 30f);
            SpawnFragmentEcho(end, 30f);
        }

        public static void DestinyChainImpact(Vector2 position)
        {
            TriggerCrimsonFlash(10f);
            MagnumScreenEffects.AddScreenShake(10f);
            ChromaticImpactRipple(position, 1.0f);
            CustomParticles.FateImpactBurst(position, 8);
            CustomParticles.HaloRing(position, FatePalette.BrightCrimson, 0.5f, 16);
            Phase10BossVFX.ChordResolutionBloom(position,
                new[] { FatePalette.DarkPink, FatePalette.BrightCrimson, FatePalette.WhiteCelestial }, 0.8f);
            var bloom = new BloomParticle(position, Vector2.Zero, FatePalette.BrightCrimson, 0.5f, 20);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        #endregion

        #region Phase 2 -- Cosmic Convergence (40-70% HP)

        // --- ConstellationStrike: Star-points fusing two theme palettes ---

        public static void ConstellationStrikeTelegraph(Vector2 center)
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 starPos = center + angle.ToRotationVector2() * 120f;
                // Alternate between two random theme colors
                int themeA = (i * 3) % 10;
                int themeB = (i * 3 + 5) % 10;
                Color starColor = Color.Lerp(ThemeColors[themeA], ThemeColors[themeB], 0.5f);
                CustomParticles.GenericFlare(starPos, starColor, 0.6f, 20);
                TelegraphSystem.ThreatLine(center, angle.ToRotationVector2(), 150f, 25, starColor * 0.5f);
            }
            Phase10BossVFX.NoteConstellationWarning(center, FatePalette.WhiteCelestial, 0.7f);
        }

        public static void ConstellationStrikeImpact(Vector2 position)
        {
            TriggerCelestialFlash(12f);
            MagnumScreenEffects.AddScreenShake(15f);
            ChromaticImpactRipple(position, 1.5f);
            CustomParticles.GenericFlare(position, FatePalette.WhiteCelestial, 1.2f, 22);
            // Dual-theme fusion rings
            DualThemeBurst(position, 10, 60f);
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Color color = i % 2 == 0 ? FatePalette.WhiteCelestial : FatePalette.DarkPink;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 50f, color, 0.4f, 16);
            }
            BossSignatureVFX.FateConstellationStrike(
                new[] { position, position + new Vector2(60, -40), position + new Vector2(-50, -60) }, 1f, 1.2f);
        }

        // --- TimeSlice: Reality-cutting chromatic attack ---

        public static void TimeSliceTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 50f, 30, FatePalette.BrightCrimson * 0.6f);
            Phase10BossVFX.TempoShiftDistortion(start, 120f, 180f, 100f);
            BossVFXOptimizer.WarningFlare(start, 0.8f);
            // Dual-theme particles along slice path
            DualThemeBurst((start + end) / 2f, 6, (end - start).Length() * 0.3f);
        }

        public static void TimeSliceRelease(Vector2 start, Vector2 end)
        {
            TriggerCrimsonFlash(15f);
            MagnumScreenEffects.AddScreenShake(18f);
            BossSignatureVFX.FateTimeSlice(start, end, 1.5f);
            Phase10Integration.Fate.RealityTempoDistortion((start + end) / 2f, 1.2f);

            // Full chromatic aberration along the slice line
            Vector2 dir = (end - start).SafeNormalize(Vector2.UnitX);
            float length = (end - start).Length();
            for (int i = 0; i < 8; i++)
            {
                Vector2 pos = start + dir * (length * i / 8f);
                ChromaticImpactRipple(pos, 1.0f);
            }
        }

        // --- UniversalJudgment: Radial cosmic judgment rings ---

        public static void UniversalJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 60, FatePalette.WhiteCelestial * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center,
                new[] { FatePalette.CosmicVoid, FatePalette.DarkPink, FatePalette.BrightCrimson, FatePalette.WhiteCelestial }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, FatePalette.WhiteCelestial, 1.5f);
            // Dual-theme converging fragments
            DualThemeBurst(center, 8, 200f);
        }

        public static void UniversalJudgmentRelease(Vector2 center)
        {
            TriggerCelestialFlash(18f);
            MagnumScreenEffects.AddScreenShake(25f);
            ChromaticImpactRipple(center, 2.0f);
            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 1.2f, 28);
            // Theme-color ring: each ring segment a different theme
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = ThemeColors[i % 10];
                color = Color.Lerp(color, FatePalette.WhiteCelestial, 0.3f);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 80f, color, 0.5f, 18);
            }
            BossSignatureVFX.FateCosmicJudgment(center, 5, 5, 2.0f);
            Phase10Integration.Fate.CosmicJudgmentVFX(center, 1f);
        }

        #endregion

        #region Phase 3 -- Singularity (20-40% HP) & Cosmic Wrath (<20%)

        // --- CosmicVortex: Black hole that draws matter inward then detonates ---

        public static void CosmicVortexTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 50, FatePalette.CosmicVoid);
            Phase10BossVFX.AccelerandoSpiral(center, FatePalette.DarkPink, 1.5f);
            // Singularity pull preview
            SingularityPulse(center, 200f, 8);
        }

        public static void CosmicVortexPulse(Vector2 center, int pulseIndex)
        {
            float progress = pulseIndex * 0.1f;
            Color color = Color.Lerp(FatePalette.DarkPink, FatePalette.BrightCrimson, progress);
            CustomParticles.FateCosmicBurst(center, 8 + pulseIndex * 2);
            CustomParticles.HaloRing(center, color, 0.8f - progress * 0.3f, 20);
            Phase10BossVFX.CrescendoRing(center, 50f + pulseIndex * 30f, 300f, color);
            // Progressive singularity pull -- grows with each pulse
            SingularityPulse(center, 120f + pulseIndex * 25f, 6 + pulseIndex * 2);
            ChromaticImpactRipple(center, 0.6f + progress * 0.5f);
        }

        // --- FinalMelody: The grand finale -- all 10 themes converge and detonate ---

        public static void FinalMelodyTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 400f, 90, FatePalette.WhiteCelestial);
            Phase10BossVFX.TuttiFullEnsemble(center,
                new[] { FatePalette.CosmicVoid, FatePalette.DarkPink, FatePalette.BrightCrimson, FatePalette.WhiteCelestial }, 2.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, FatePalette.WhiteCelestial, 2.0f);
            CustomParticles.GlyphCircle(center, FatePalette.WhiteCelestial, 16, 150f, 0.04f);
            // All 10 theme fragments converge
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 pos = center + angle.ToRotationVector2() * 300f;
                CustomParticles.GenericFlare(pos, ThemeColors[i], 0.5f, 30);
                CustomParticles.Glyph(pos, ThemeColors[i] * 0.6f, 0.35f, i + 1);
            }
        }

        public static void FinalMelodyWave(Vector2 center, int waveIndex)
        {
            float angle = waveIndex * 0.12f;
            for (int arm = 0; arm < 8; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 8f;
                Vector2 pos = center + armAngle.ToRotationVector2() * (40f + waveIndex * 10f);
                // Each arm uses a different theme color cycling through with wave
                Color color = ThemeColors[(arm + waveIndex) % 10];
                color = Color.Lerp(color, FatePalette.WhiteCelestial, 0.3f);
                CustomParticles.GenericFlare(pos, color, 0.4f, 12);
            }
            ThemedParticles.FateMusicNotes(center, 6, 80f);
            // Singularity inward pull every other wave
            if (waveIndex % 2 == 0)
                SingularityPulse(center, 100f + waveIndex * 15f, 6);
        }

        public static void FinalMelodyFinale(Vector2 center)
        {
            TriggerSupernovaFlash(25f);
            TriggerRealityPunch();
            MagnumScreenEffects.AddScreenShake(30f);
            ChromaticImpactRipple(center, 3.0f);
            CustomParticles.GenericFlare(center, FatePalette.WhiteCelestial, 1.2f, 35);

            // 10-theme supernova ring -- each segment a different conquered theme
            for (int i = 0; i < 24; i++)
            {
                float ringAngle = MathHelper.TwoPi * i / 24f;
                Color color = ThemeColors[i % 10];
                color = Color.Lerp(color, FatePalette.WhiteCelestial, 0.2f);
                CustomParticles.GenericFlare(center + ringAngle.ToRotationVector2() * 140f, color, 1.0f, 30);
            }
            Phase10BossVFX.CodaFinale(center, FatePalette.WhiteCelestial, FatePalette.CosmicVoid, 2.5f);
            Phase10BossVFX.CadenceFinisher(center,
                new[] { FatePalette.CosmicVoid, FatePalette.DarkPink, FatePalette.BrightCrimson, FatePalette.WhiteCelestial }, 1f);

            // Supernova bloom ring with theme gradient
            for (int i = 0; i < 16; i++)
            {
                float bloomAngle = MathHelper.TwoPi * i / 16f;
                Vector2 bloomVel = bloomAngle.ToRotationVector2() * 5f;
                Color bloomColor = ThemeColors[i % 10];
                bloomColor = Color.Lerp(bloomColor, FatePalette.WhiteCelestial, 0.3f);
                var bloom = new BloomParticle(center, bloomVel, bloomColor, 0.6f, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Final singularity collapse outward
            SingularityPulse(center, 250f, 20);
        }

        #endregion
    }
}
