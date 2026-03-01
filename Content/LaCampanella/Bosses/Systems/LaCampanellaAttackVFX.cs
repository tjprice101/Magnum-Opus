using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.LaCampanella.Bosses.Systems
{
    /// <summary>
    /// La Campanella boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// Theme: Black smoke, infernal orange, gold highlights, bell chimes.
    /// </summary>
    public static class LaCampanellaAttackVFX
    {
        private static readonly Color InfernalOrange = new Color(255, 140, 40);
        private static readonly Color SmokeBlack = new Color(30, 20, 15);
        private static readonly Color BellGold = new Color(220, 180, 80);
        private static readonly Color FlameWhite = new Color(255, 230, 200);

        #region Phase 1 Attack VFX

        /// <summary>BellSlam: Massive downward slam with fire shockwave.</summary>
        public static void BellSlamTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, InfernalOrange * 0.7f);
            TelegraphSystem.ImpactPoint(target, 80f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, InfernalOrange, 0.7f);
        }

        public static void BellSlamImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(15f);
            CustomParticles.LaCampanellaImpactBurst(position, 12);
            CustomParticles.LaCampanellaBellChime(position, 10);
            Phase10BossVFX.TimpaniDrumrollImpact(position, InfernalOrange, 1.2f);
            ThemedParticles.LaCampanellaImpact(position, 1.0f);
            CustomParticles.HaloRing(position, BellGold, 0.8f, 20);
            BossSignatureVFX.LaCampanellaBellToll(position, 1);
        }

        /// <summary>TollWave: Radial sound wave ring expanding outward.</summary>
        public static void TollWaveTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 120f, 8, InfernalOrange * 0.5f);
            Phase10BossVFX.FortissimoFlashWarning(center, BellGold, 0.7f);
        }

        public static void TollWaveRelease(Vector2 center, int waveIndex)
        {
            float ringScale = 0.5f + waveIndex * 0.2f;
            Color ringColor = Color.Lerp(InfernalOrange, BellGold, (float)Math.Sin(waveIndex * 0.4f) * 0.5f + 0.5f);
            CustomParticles.HaloRing(center, ringColor, ringScale, 18);
            ThemedParticles.LaCampanellaShockwave(center, 0.8f + waveIndex * 0.15f);
            BossVFXOptimizer.OptimizedFlare(center, InfernalOrange, 0.4f, 12);
        }

        /// <summary>EmberShower: Raining embers from above.</summary>
        public static void EmberShowerTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -250), InfernalOrange, 0.7f);
            TelegraphSystem.DangerZone(targetArea, 250f, 30, InfernalOrange * 0.3f);
        }

        public static void EmberShowerParticle(Vector2 position)
        {
            Color color = Color.Lerp(InfernalOrange, FlameWhite, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.3f, 8);
            ThemedParticles.LaCampanellaSparks(position, Vector2.UnitY, 2, 3f);
            if (Main.rand.NextBool(4))
                CustomParticles.LaCampanellaMusicNotes(position, 1, 20f);
        }

        /// <summary>FireWallSweep: Horizontal wall of fire sweeping across the arena.</summary>
        public static void FireWallSweepTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 40f, 45, InfernalOrange * 0.6f);
            Phase10BossVFX.AccelerandoSpiral(start, InfernalOrange, 0.6f);
        }

        public static void FireWallSweepTrail(Vector2 position, Vector2 velocity)
        {
            ThemedParticles.LaCampanellaTrail(position, velocity);
            CustomParticles.GenericFlare(position, InfernalOrange, 0.35f, 10);
            BossVFXOptimizer.OptimizedHalo(position, SmokeBlack, 0.3f, 12);
        }

        #endregion

        #region Phase 2 Attack VFX

        /// <summary>ChimeRings: Concentric rings of bell projectiles.</summary>
        public static void ChimeRingsTelegraph(Vector2 center)
        {
            for (int i = 0; i < 3; i++)
            {
                float radius = 80f + i * 60f;
                TelegraphSystem.ConvergingRing(center, radius, 6, BellGold * 0.5f);
            }
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { InfernalOrange, BellGold, FlameWhite }, 0.6f);
        }

        public static void ChimeRingsRelease(Vector2 center, int ringIndex)
        {
            CustomParticles.LaCampanellaBellChime(center, 8 + ringIndex * 3);
            CustomParticles.HaloRing(center, BellGold, 0.5f + ringIndex * 0.15f, 16);
            Phase10BossVFX.NoteConstellationWarning(center, InfernalOrange, 0.5f + ringIndex * 0.15f);
        }

        /// <summary>InfernoCircle: Ring of fire closing in on the player.</summary>
        public static void InfernoCircleTelegraph(Vector2 center, float radius)
        {
            TelegraphSystem.DangerZone(center, radius, 60, InfernalOrange * 0.4f);
            Phase10BossVFX.CrescendoDangerRings(center, InfernalOrange, 0.9f, 4);
            BossVFXOptimizer.WarningFlare(center, 0.7f);
        }

        public static void InfernoCircleRelease(Vector2 center, float radius)
        {
            int count = 16;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, InfernalOrange, 0.4f, 12);
                ThemedParticles.LaCampanellaSparks(pos, -angle.ToRotationVector2(), 3, 5f);
            }
            ThemedParticles.LaCampanellaShockwave(center, 1.2f);
        }

        /// <summary>RhythmicToll: Multi-hit bell pattern with escalating intensity.</summary>
        public static void RhythmicTollTelegraph(Vector2 position)
        {
            Phase10BossVFX.MetronomeTickWarning(position, BellGold, 3, 6);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void RhythmicTollStrike(Vector2 position, int tollNumber)
        {
            float intensity = 0.4f + tollNumber * 0.15f;
            CustomParticles.LaCampanellaImpactBurst(position, 6 + tollNumber * 2);
            CustomParticles.HaloRing(position, Color.Lerp(BellGold, InfernalOrange, tollNumber / 6f), intensity, 15);
            BossSignatureVFX.LaCampanellaBellToll(position, tollNumber, intensity);
        }

        #endregion

        #region Phase 3 Attack VFX (Enraged)

        /// <summary>InfernalJudgment: Ultimate radial fire pattern with black smoke walls.</summary>
        public static void InfernalJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 12, InfernalOrange * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { InfernalOrange, BellGold, FlameWhite }, 0.9f);
            Phase10BossVFX.FortissimoFlashWarning(center, InfernalOrange, 1.3f);
        }

        public static void InfernalJudgmentRelease(Vector2 center, int wave, int totalWaves)
        {
            MagnumScreenEffects.AddScreenShake(18f);
            BossSignatureVFX.LaCampanellaInfernalJudgment(center, wave, totalWaves, 1.2f);
            CustomParticles.GenericFlare(center, FlameWhite, 1.5f, 25);
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Color color = i % 2 == 0 ? InfernalOrange : BellGold;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 50f, color, 0.5f, 18);
            }
        }

        /// <summary>BellLaserGrid: Criss-crossing laser beams from bell positions.</summary>
        public static void BellLaserGridTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 30f, 40, BellGold * 0.5f);
            BossVFXOptimizer.WarningLine(start, (end - start).SafeNormalize(Vector2.UnitX),
                (end - start).Length(), 12, WarningType.Danger);
        }

        public static void BellLaserGridBeam(Vector2 start, Vector2 end)
        {
            Phase10BossVFX.StaffLineLaser(start, end, InfernalOrange, 35f);
            ThemedParticles.LaCampanellaSparkles(start, 4, 20f);
        }

        /// <summary>TripleSlam: Three consecutive bell slams with escalating power.</summary>
        public static void TripleSlamImpact(Vector2 position, int slamIndex)
        {
            float intensity = 0.6f + slamIndex * 0.3f;
            MagnumScreenEffects.AddScreenShake(10f + slamIndex * 5f);
            CustomParticles.LaCampanellaImpactBurst(position, 8 + slamIndex * 4);
            CustomParticles.LaCampanellaBellChime(position, 6 + slamIndex * 3);
            ThemedParticles.LaCampanellaImpact(position, intensity);
            Phase10BossVFX.TimpaniDrumrollImpact(position, InfernalOrange, intensity);
        }

        /// <summary>InfernalTorrent: Stream of fire projectiles in spiral pattern.</summary>
        public static void InfernalTorrentTelegraph(Vector2 position)
        {
            Phase10BossVFX.AccelerandoSpiral(position, InfernalOrange, 0.8f, 16);
            TelegraphSystem.ConvergingRing(position, 150f, 8, InfernalOrange * 0.6f);
        }

        public static void InfernalTorrentRelease(Vector2 position, int burstIndex)
        {
            float angle = burstIndex * 0.25f;
            Color color = Color.Lerp(InfernalOrange, FlameWhite, (float)Math.Sin(angle) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.5f, 14);
            ThemedParticles.LaCampanellaSparks(position, angle.ToRotationVector2(), 4, 6f);
        }

        /// <summary>InfernoCage: Enclosing cage of fire pillars.</summary>
        public static void InfernoCageTelegraph(Vector2 center, float radius)
        {
            int pillarCount = 8;
            for (int i = 0; i < pillarCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pillarCount;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                TelegraphSystem.ThreatLine(pos, Vector2.UnitY, 300f, 30, InfernalOrange * 0.5f);
            }
            Phase10BossVFX.CrescendoDangerRings(center, SmokeBlack, 0.6f);
        }

        public static void InfernoCageRelease(Vector2 center, float radius)
        {
            int pillarCount = 8;
            for (int i = 0; i < pillarCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pillarCount;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, InfernalOrange, 0.6f, 15);
                ThemedParticles.LaCampanellaBloomBurst(pos, 0.8f);
            }
            MagnumScreenEffects.AddScreenShake(10f);
        }

        /// <summary>ResonantShock: Waves of sonic force emanating from the bell.</summary>
        public static void ResonantShockTelegraph(Vector2 center)
        {
            Phase10BossVFX.FortissimoFlashWarning(center, BellGold, 0.8f);
            BossVFXOptimizer.DangerZoneRing(center, 150f, 12);
        }

        public static void ResonantShockRelease(Vector2 center, float radius)
        {
            CustomParticles.HaloRing(center, BellGold, radius / 150f, 20);
            CustomParticles.HaloRing(center, InfernalOrange, radius / 200f, 22);
            Phase10BossVFX.SforzandoSpike(center, InfernalOrange, 1.0f);
            ThemedParticles.LaCampanellaShockwave(center, radius / 100f);
        }

        /// <summary>GrandFinale: The ultimate attack  Emassive bell descent with full infernal eruption.</summary>
        public static void GrandFinaleTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 16, InfernalOrange);
            Phase10BossVFX.StaffLineConvergence(center, BellGold, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, FlameWhite, 1.5f);
            BossVFXOptimizer.WarningFlare(center, 1.2f);
        }

        public static void GrandFinaleRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, FlameWhite, 2.0f, 30);
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = VFXIntegration.GetThemeColor("LaCampanella", i / 16f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 120f, color, 0.9f, 25);
            }
            Phase10BossVFX.CodaFinale(center, InfernalOrange, BellGold, 2.0f);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { InfernalOrange, BellGold, FlameWhite }, 1.8f);
            BossSignatureVFX.LaCampanellaInfernalJudgment(center, 5, 5, 2.0f);
        }

        #endregion
    }
}
