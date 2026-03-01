using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Summer.Bosses.Systems
{
    /// <summary>
    /// L'Estate boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// Theme: Solar power, scorching heat, zenith, blazing.
    /// </summary>
    public static class LEstateAttackVFX
    {
        private static readonly Color SolarGold = new Color(255, 200, 50);
        private static readonly Color ScorchOrange = new Color(240, 130, 30);
        private static readonly Color ZenithWhite = new Color(255, 250, 230);
        private static readonly Color HeatRed = new Color(220, 60, 30);

        #region Core Attack VFX

        /// <summary>SolarFlare: Erupting solar prominence with intense golden fire.</summary>
        public static void SolarFlareTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 400f, 30, SolarGold * 0.7f);
            Phase10BossVFX.AccelerandoSpiral(position, ScorchOrange, 0.7f);
            BossVFXOptimizer.WarningFlare(position, 0.6f);
        }

        public static void SolarFlareImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(10f);
            CustomParticles.ExplosionBurst(position, SolarGold, 12);
            Phase10BossVFX.CymbalCrashBurst(position, 0.9f);
            CustomParticles.HaloRing(position, ScorchOrange, 0.6f, 18);
            CustomParticles.GenericMusicNotes(position, SolarGold, 4, 50f);
        }

        /// <summary>HeatWave: Rippling distortion wave of scorching air.</summary>
        public static void HeatWaveTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 500f, 30, ScorchOrange * 0.6f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 500f, HeatRed, 0.6f);
        }

        public static void HeatWaveTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, ScorchOrange, 0.35f, 8);
            CustomParticles.GlowTrail(position, HeatRed, 0.3f);
            if (Main.rand.NextBool(3))
                CustomParticles.GenericMusicNotes(position, SolarGold, 1, 30f);
        }

        /// <summary>SunshowerBombard: Golden rain of solar projectiles from above.</summary>
        public static void SunshowerBombardTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -220), SolarGold, 0.9f);
            TelegraphSystem.DangerZone(targetArea, 220f, 30, SolarGold * 0.3f);
        }

        public static void SunshowerBombardParticle(Vector2 position)
        {
            Color color = Color.Lerp(SolarGold, ZenithWhite, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.3f, 6);
            if (Main.rand.NextBool(4))
                CustomParticles.GenericMusicNotes(position, ScorchOrange, 1, 20f);
        }

        #endregion

        #region Phase 2B Attack VFX (70% HP)

        /// <summary>ScorchingDash: Blazing high-speed dash leaving flame trails.</summary>
        public static void ScorchingDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, HeatRed * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, SolarGold, 0.9f);
        }

        public static void ScorchingDashAfterimage(Vector2 position, int dashNumber)
        {
            float intensity = 0.35f + dashNumber * 0.15f;
            Color trailColor = Color.Lerp(ScorchOrange, HeatRed, dashNumber / 5f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 10);
            CustomParticles.GlowTrail(position, trailColor, intensity * 0.8f);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 15, 3);
        }

        /// <summary>ZenithBeam: Concentrated solar beam burning through the arena.</summary>
        public static void ZenithBeamTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 30f, 30, ZenithWhite * 0.6f);
            Phase10BossVFX.ChordBuildupSpiral(start, new[] { SolarGold }, 0.8f);
            BossVFXOptimizer.WarningFlare(start, 0.7f);
        }

        public static void ZenithBeamImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(14f);
            CustomParticles.GenericFlare(position, ZenithWhite, 1.0f, 20);
            CustomParticles.HaloRing(position, SolarGold, 0.7f, 20);
            Phase10BossVFX.SforzandoSpike(position, SolarGold, 1.0f);
        }

        /// <summary>InfernoRing: Expanding ring of solar fire constricting inward.</summary>
        public static void InfernoRingTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 180f, 10, HeatRed * 0.5f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { HeatRed }, 0.7f);
        }

        public static void InfernoRingBurst(Vector2 center, int waveIndex)
        {
            CustomParticles.ExplosionBurst(center, Color.Lerp(ScorchOrange, HeatRed, waveIndex / 5f), 10 + waveIndex * 3);
            CustomParticles.GenericFlare(center, SolarGold, 0.6f, 15);
            Phase10BossVFX.NoteConstellationWarning(center, ScorchOrange, 0.5f + waveIndex * 0.15f);
        }

        #endregion

        #region Phase 2C Attack VFX (40% HP)

        /// <summary>SummerSolstice: Massive radial solar explosion at peak power.</summary>
        public static void SummerSolsticeTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 220f, 14, SolarGold * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { SolarGold }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, HeatRed, 1.3f);
        }

        public static void SummerSolsticeRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(20f);
            CustomParticles.GenericFlare(center, ZenithWhite, 1.5f, 25);
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Color color = i % 2 == 0 ? SolarGold : HeatRed;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 65f, color, 0.5f, 15);
            }
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { SolarGold, ScorchOrange, HeatRed }, 1.5f);
            BossSignatureVFX.SummerHeatWave(center, 1.5f);
        }

        /// <summary>SolarStorm: Devastating barrage of solar eruptions.</summary>
        public static void SolarStormTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 800f, 30, HeatRed * 0.8f);
            TelegraphSystem.ImpactPoint(target, 70f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, HeatRed, 0.9f);
        }

        public static void SolarStormImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(22f);
            CustomParticles.ExplosionBurst(position, SolarGold, 18);
            CustomParticles.GenericFlare(position, ZenithWhite, 1.8f, 25);
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 50f,
                    Color.Lerp(ScorchOrange, SolarGold, i / 10f), 0.5f, 15);
            }
            BossSignatureVFX.SummerHeatWave(position, 1.8f);
            Phase10BossVFX.TimpaniDrumrollImpact(position, SolarGold, 1.5f);
        }

        /// <summary>Supernova: Ultimate cataclysmic solar explosion finale.</summary>
        public static void SupernovaTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 280f, 18, ZenithWhite);
            Phase10BossVFX.StaffLineConvergence(center, SolarGold, 1.4f);
            BossVFXOptimizer.WarningFlare(center, 1.2f);
        }

        public static void SupernovaWave(Vector2 center, int waveIndex)
        {
            float angle = waveIndex * 0.12f;
            for (int arm = 0; arm < 6; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 6f;
                Vector2 pos = center + armAngle.ToRotationVector2() * 45f;
                Color color = Color.Lerp(SolarGold, HeatRed, arm / 6f);
                CustomParticles.GenericFlare(pos, color, 0.35f, 10);
            }
            CustomParticles.GenericMusicNotes(center, SolarGold, 4, 65f);
        }

        public static void SupernovaFinale(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(30f);
            CustomParticles.GenericFlare(center, ZenithWhite, 2.5f, 35);
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Color color = VFXIntegration.GetThemeColor("Summer", i / 24f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 120f, color, 1.0f, 30);
            }
            Phase10BossVFX.CodaFinale(center, SolarGold, HeatRed, 2.5f);
        }

        #endregion
    }
}
