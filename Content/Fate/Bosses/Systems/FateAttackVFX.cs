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
    /// Fate boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// 
    /// The Warden of Melodies is ENDGAME  Eevery attack should feel
    /// like the cosmos itself is striking. Massive screen effects,
    /// cosmic grandeur, layered constellation VFX.
    /// </summary>
    public static class FateAttackVFX
    {
        private static readonly Color CosmicBlack = new Color(10, 5, 15);
        private static readonly Color DarkPink = new Color(180, 40, 80);
        private static readonly Color BrightCrimson = new Color(220, 40, 60);
        private static readonly Color CelestialWhite = new Color(230, 220, 255);

        #region Phase 1  EFated Prelude

        /// <summary>CosmicDash: Constellation trail with star-point sparkles.</summary>
        public static void CosmicDashTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 600f, 30, DarkPink * 0.7f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 600f, DarkPink, 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.6f);
        }

        public static void CosmicDashTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, DarkPink, 0.35f, 10);
            if (Main.rand.NextBool(3))
                ThemedParticles.FateMusicNotes(position, 1, 30f);
            CustomParticles.GlowTrail(position, DarkPink, 0.4f);
        }

        public static void CosmicDashImpact(Vector2 position)
        {
            TriggerCosmicFlash(8f);
            MagnumScreenEffects.AddScreenShake(12f);
            CustomParticles.FateImpactBurst(position, 10);
            Phase10BossVFX.CymbalCrashBurst(position, 1.0f);
            CustomParticles.HaloRing(position, DarkPink, 0.6f, 18);
            ThemedParticles.FateMusicNotes(position, 5, 60f);
            var bloom = new BloomParticle(position, Vector2.Zero, DarkPink, 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        /// <summary>StarfallBarrage: Streams of cosmic projectiles raining from constellations above.</summary>
        public static void StarfallBarrageTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -250), CelestialWhite, 0.8f);
            TelegraphSystem.DangerZone(targetArea, 250f, 40, DarkPink * 0.3f);
        }

        public static void StarfallBarrageParticle(Vector2 position)
        {
            Color color = Color.Lerp(DarkPink, CelestialWhite, Main.rand.NextFloat(0.4f));
            CustomParticles.GenericFlare(position, color, 0.3f, 8);
            if (Main.rand.NextBool(3))
                CustomParticles.Glyph(position, CelestialWhite * 0.5f, 0.2f, Main.rand.Next(1, 13));
        }

        /// <summary>GlyphCircle: Ancient glyphs orbiting then converging on target.</summary>
        public static void GlyphCircleTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 150f, 45, DarkPink * 0.6f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { CosmicBlack, DarkPink, BrightCrimson, CelestialWhite }, 0.5f);
            CustomParticles.GlyphCircle(center, DarkPink, 12, 120f, 0.03f);
        }

        public static void GlyphCircleRelease(Vector2 center, int burstIndex)
        {
            TriggerCosmicFlash(6f);
            float angle = burstIndex * 0.25f;
            Color color = Color.Lerp(DarkPink, CelestialWhite, (float)Math.Sin(angle) * 0.5f + 0.5f);
            CustomParticles.GlyphBurst(center, color, 6, 5f);
            CustomParticles.GenericFlare(center, CelestialWhite, 0.5f, 15);
            var bloom = new BloomParticle(center, Vector2.Zero, color, 0.5f, 18);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        /// <summary>DestinyChain: Linked cosmic chains that bind then explode.</summary>
        public static void DestinyChainTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 30f, 35, DarkPink * 0.5f);
            Phase10BossVFX.StaffLineLaser(start, end, DarkPink, 20f);
        }

        public static void DestinyChainImpact(Vector2 position)
        {
            TriggerCrimsonFlash(10f);
            MagnumScreenEffects.AddScreenShake(10f);
            CustomParticles.FateImpactBurst(position, 8);
            CustomParticles.HaloRing(position, BrightCrimson, 0.5f, 16);
            Phase10BossVFX.ChordResolutionBloom(position, new[] { DarkPink, BrightCrimson, CelestialWhite }, 0.8f);
            var bloom = new BloomParticle(position, Vector2.Zero, BrightCrimson, 0.5f, 20);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        #endregion

        #region Phase 2  ECosmic Awakening

        /// <summary>ConstellationStrike: Star-points connected into a constellation weapon that strikes.</summary>
        public static void ConstellationStrikeTelegraph(Vector2 center)
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 starPos = center + angle.ToRotationVector2() * 120f;
                CustomParticles.GenericFlare(starPos, CelestialWhite, 0.6f, 20);
                TelegraphSystem.ThreatLine(center, angle.ToRotationVector2(), 150f, 25, CelestialWhite * 0.5f);
            }
            Phase10BossVFX.NoteConstellationWarning(center, CelestialWhite, 0.7f);
        }

        public static void ConstellationStrikeImpact(Vector2 position)
        {
            TriggerCelestialFlash(12f);
            MagnumScreenEffects.AddScreenShake(15f);
            CustomParticles.GenericFlare(position, CelestialWhite, 1.2f, 22);
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Color color = i % 2 == 0 ? CelestialWhite : DarkPink;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 50f, color, 0.4f, 16);
            }
            BossSignatureVFX.FateConstellationStrike(
                new[] { position, position + new Vector2(60, -40), position + new Vector2(-50, -60) }, 1f, 1.2f);
        }

        /// <summary>TimeSlice: Reality-splitting chromatic aberration attack.</summary>
        public static void TimeSliceTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 50f, 30, BrightCrimson * 0.6f);
            Phase10BossVFX.TempoShiftDistortion(start, 120f, 180f, 100f);
            BossVFXOptimizer.WarningFlare(start, 0.8f);
        }

        public static void TimeSliceRelease(Vector2 start, Vector2 end)
        {
            TriggerCrimsonFlash(15f);
            MagnumScreenEffects.AddScreenShake(18f);
            BossSignatureVFX.FateTimeSlice(start, end, 1.5f);
            Phase10Integration.Fate.RealityTempoDistortion((start + end) / 2f, 1.2f);

            // Chromatic aberration burst along the slice
            Vector2 dir = (end - start).SafeNormalize(Vector2.UnitX);
            float length = (end - start).Length();
            for (int i = 0; i < 8; i++)
            {
                Vector2 pos = start + dir * (length * i / 8f);
                CustomParticles.GenericFlare(pos + new Vector2(-3, 0), new Color(255, 40, 60) * 0.5f, 0.4f, 10);
                CustomParticles.GenericFlare(pos + new Vector2(3, 0), new Color(60, 40, 255) * 0.5f, 0.4f, 10);
                CustomParticles.GenericFlare(pos, CelestialWhite, 0.6f, 12);
            }
        }

        /// <summary>UniversalJudgment: Ultimate radial cosmic judgment  Erings of celestial fire.</summary>
        public static void UniversalJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 60, CelestialWhite * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { CosmicBlack, DarkPink, BrightCrimson, CelestialWhite }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, CelestialWhite, 1.5f);
        }

        public static void UniversalJudgmentRelease(Vector2 center)
        {
            TriggerCelestialFlash(18f);
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, CelestialWhite, 2.0f, 28);
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = i % 3 == 0 ? CelestialWhite : (i % 3 == 1 ? DarkPink : BrightCrimson);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 80f, color, 0.5f, 18);
            }
            BossSignatureVFX.FateCosmicJudgment(center, 5, 5, 2.0f);
            Phase10Integration.Fate.CosmicJudgmentVFX(center, 1f);
        }

        #endregion

        #region True Form  ECosmic Finale

        /// <summary>CosmicVortex: Spiraling black hole that draws everything inward.</summary>
        public static void CosmicVortexTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 50, CosmicBlack);
            Phase10BossVFX.AccelerandoSpiral(center, DarkPink, 1.5f);
        }

        public static void CosmicVortexPulse(Vector2 center, int pulseIndex)
        {
            float progress = pulseIndex * 0.1f;
            Color color = Color.Lerp(DarkPink, BrightCrimson, progress);
            CustomParticles.FateCosmicBurst(center, 8 + pulseIndex * 2);
            CustomParticles.HaloRing(center, color, 0.8f - progress * 0.3f, 20);
            Phase10BossVFX.CrescendoRing(center, 50f + pulseIndex * 30f, 300f, color);
        }

        /// <summary>FinalMelody: The ultimate attack  Ea symphony of every cosmic element.</summary>
        public static void FinalMelodyTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 400f, 90, CelestialWhite);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { CosmicBlack, DarkPink, BrightCrimson, CelestialWhite }, 2.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, CelestialWhite, 2.0f);
            CustomParticles.GlyphCircle(center, CelestialWhite, 16, 150f, 0.04f);
        }

        public static void FinalMelodyWave(Vector2 center, int waveIndex)
        {
            float angle = waveIndex * 0.12f;
            for (int arm = 0; arm < 8; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 8f;
                Vector2 pos = center + armAngle.ToRotationVector2() * (40f + waveIndex * 10f);
                Color color = Color.Lerp(DarkPink, CelestialWhite, arm / 8f);
                CustomParticles.GenericFlare(pos, color, 0.4f, 12);
            }
            ThemedParticles.FateMusicNotes(center, 6, 80f);
        }

        public static void FinalMelodyFinale(Vector2 center)
        {
            TriggerSupernovaFlash(25f);
            MagnumScreenEffects.AddScreenShake(30f);
            CustomParticles.GenericFlare(center, CelestialWhite, 2.5f, 35);
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Color color = VFXIntegration.GetThemeColor("Fate", i / 24f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 140f, color, 1.0f, 30);
            }
            Phase10BossVFX.CodaFinale(center, CelestialWhite, CosmicBlack, 2.5f);
            Phase10BossVFX.CadenceFinisher(center, new[] { CosmicBlack, DarkPink, BrightCrimson, CelestialWhite }, 1f);

            // Supernova bloom ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 bloomVel = angle.ToRotationVector2() * 5f;
                Color bloomColor = Color.Lerp(DarkPink, CelestialWhite, i / 16f);
                var bloom = new BloomParticle(center, bloomVel, bloomColor, 0.6f, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        #endregion
    }
}
