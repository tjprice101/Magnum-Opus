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
    /// L'Estate boss attack VFX choreography across all 4 phases.
    /// Vivaldi's Summer: Scorching Stillness -> Gathering Storm -> Full Tempest -> Solar Eclipse.
    /// Every attack telegraph, impact, and phase transition effect lives here.
    /// </summary>
    public static class LEstateAttackVFX
    {
        // Vivaldi's Summer palette
        private static readonly Color SunGold = new Color(255, 200, 50);
        private static readonly Color BlazingOrange = new Color(255, 140, 40);
        private static readonly Color WhiteHot = new Color(255, 250, 240);
        private static readonly Color DeepAmber = new Color(180, 100, 20);
        private static readonly Color HeatRed = new Color(220, 60, 30);
        private static readonly Color StormAmber = new Color(150, 90, 30);

        #region Phase 1 — Scorching Stillness (100-60% HP)

        public static void SolarFlareTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 400f, 30, SunGold * 0.7f);
            Phase10BossVFX.AccelerandoSpiral(position, BlazingOrange, 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void SolarFlareImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            LEstateSky.TriggerSolarFlash(4f);
            CustomParticles.ExplosionBurst(position, SunGold, 10);
            Phase10BossVFX.CymbalCrashBurst(position, 0.7f);
            CustomParticles.HaloRing(position, BlazingOrange, 0.5f, 15);
            CustomParticles.GenericMusicNotes(position, SunGold, 3, 40f);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero, SunGold, 0.4f, 10));
        }

        public static void HeatWaveTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 500f, 30, BlazingOrange * 0.6f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 500f, DeepAmber, 0.5f);
        }

        public static void HeatWaveTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, BlazingOrange, 0.3f, 8);
            CustomParticles.GlowTrail(position, DeepAmber, 0.25f);
            if (Main.rand.NextBool(3))
                CustomParticles.GenericMusicNotes(position, SunGold, 1, 25f);
        }

        public static void SunshowerBombardTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -220), SunGold, 0.8f);
            TelegraphSystem.DangerZone(targetArea, 220f, 30, SunGold * 0.3f);
        }

        public static void SunshowerBombardParticle(Vector2 position)
        {
            Color color = Color.Lerp(SunGold, WhiteHot, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.25f, 6);
            if (Main.rand.NextBool(5))
                CustomParticles.GenericMusicNotes(position, BlazingOrange, 1, 18f);
        }

        #endregion

        #region Phase 2 — Gathering Storm (60-30% HP)

        public static void ScorchingDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, HeatRed * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, SunGold, 0.8f);
        }

        public static void ScorchingDashAfterimage(Vector2 position, int dashNumber)
        {
            float intensity = 0.3f + dashNumber * 0.12f;
            Color trailColor = Color.Lerp(BlazingOrange, HeatRed, dashNumber / 5f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 8);
            CustomParticles.GlowTrail(position, trailColor, intensity * 0.7f);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 12, 3);
        }

        public static void ZenithBeamTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 30f, 30, WhiteHot * 0.6f);
            Phase10BossVFX.ChordBuildupSpiral(start, new[] { SunGold }, 0.7f);
            BossVFXOptimizer.WarningFlare(start, 0.6f);
        }

        public static void ZenithBeamImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(12f);
            LEstateSky.TriggerScorchFlash(6f);
            CustomParticles.GenericFlare(position, WhiteHot, 0.8f, 18);
            CustomParticles.HaloRing(position, SunGold, 0.6f, 18);
            Phase10BossVFX.SforzandoSpike(position, SunGold, 0.8f);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero, WhiteHot, 0.4f, 12));
        }

        public static void InfernoRingTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 180f, 10, HeatRed * 0.5f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { HeatRed }, 0.6f);
        }

        public static void InfernoRingBurst(Vector2 center, int waveIndex)
        {
            LEstateSky.TriggerScorchFlash(2f + waveIndex * 0.4f);
            CustomParticles.ExplosionBurst(center, Color.Lerp(BlazingOrange, HeatRed, waveIndex / 5f), 8 + waveIndex * 2);
            CustomParticles.GenericFlare(center, SunGold, 0.5f, 12);
            Phase10BossVFX.NoteConstellationWarning(center, BlazingOrange, 0.4f + waveIndex * 0.12f);
        }

        #endregion

        #region Phase 3 — Full Tempest (30-0% HP)

        public static void SummerSolsticeTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 220f, 14, SunGold * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { SunGold }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, HeatRed, 1.2f);
        }

        public static void SummerSolsticeRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(18f);
            LEstateSky.TriggerZenithFlash(10f);
            CustomParticles.GenericFlare(center, WhiteHot, 1.2f, 22);

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 2 == 0 ? SunGold : HeatRed;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, color, 0.4f, 14);
            }

            Phase10BossVFX.TuttiFullEnsemble(center, new[] { SunGold, BlazingOrange, HeatRed }, 1.3f);
            BossSignatureVFX.SummerHeatWave(center, 1.3f);

            // Bloom cascade ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel,
                    Color.Lerp(SunGold, HeatRed, i / 8f), 0.4f, 16));
            }
        }

        public static void SolarStormTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 800f, 30, HeatRed * 0.8f);
            TelegraphSystem.ImpactPoint(target, 70f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, HeatRed, 0.8f);
        }

        public static void SolarStormImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(20f);
            LEstateSky.TriggerScorchFlash(12f);
            CustomParticles.ExplosionBurst(position, SunGold, 15);
            CustomParticles.GenericFlare(position, WhiteHot, 1.5f, 22);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 45f,
                    Color.Lerp(BlazingOrange, SunGold, i / 8f), 0.4f, 13);
            }

            BossSignatureVFX.SummerHeatWave(position, 1.5f);
            Phase10BossVFX.TimpaniDrumrollImpact(position, SunGold, 1.2f);

            // Bloom cascade
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 4.5f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel,
                    Color.Lerp(BlazingOrange, SunGold, i / 6f), 0.4f, 14));
            }
        }

        public static void SupernovaTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 280f, 18, WhiteHot);
            Phase10BossVFX.StaffLineConvergence(center, SunGold, 1.2f);
            BossVFXOptimizer.WarningFlare(center, 1.0f);
        }

        public static void SupernovaWave(Vector2 center, int waveIndex)
        {
            float angle = waveIndex * 0.12f;
            for (int arm = 0; arm < 6; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 6f;
                Vector2 pos = center + armAngle.ToRotationVector2() * 45f;
                Color color = Color.Lerp(SunGold, HeatRed, arm / 6f);
                CustomParticles.GenericFlare(pos, color, 0.35f, 8);
            }
            LEstateSky.TriggerSolarFlash(1.5f + waveIndex * 0.25f);
            CustomParticles.GenericMusicNotes(center, WhiteHot, 2, 50f);
        }

        #endregion

        #region Phase Transitions

        public static void PhaseTransitionVFX(Vector2 center, int fromPhase, int toPhase)
        {
            // Escalating intensity per phase transition
            float intensity = toPhase * 0.5f;

            for (int i = 0; i < 8 + toPhase * 2; i++)
            {
                float progress = i / (float)(8 + toPhase * 2);
                Color haloColor = Color.Lerp(SunGold, HeatRed, progress);
                CustomParticles.HaloRing(center, haloColor, 0.4f + i * 0.12f, 15 + i * 2);
            }

            CustomParticles.GenericFlare(center, WhiteHot, 1.5f + intensity, 30 + toPhase * 5);
            CustomParticles.GenericFlare(center, SunGold, 1.2f + intensity * 0.5f, 25 + toPhase * 3);
            MagnumScreenEffects.AddScreenShake(8f + toPhase * 4f);

            // Phase-specific flash
            switch (toPhase)
            {
                case 2:
                    LEstateSky.TriggerSolarFlash(6f);
                    Phase10BossVFX.CrescendoDangerRings(center, BlazingOrange, 0.8f);
                    break;
                case 3:
                    LEstateSky.TriggerZenithFlash(10f);
                    Phase10BossVFX.TuttiFullEnsemble(center, new[] { SunGold, BlazingOrange, HeatRed }, 1.0f);
                    break;
            }
        }

        public static void EnrageTransitionVFX(Vector2 center)
        {
            // Eclipse onset: dramatic darkening burst
            MagnumScreenEffects.AddScreenShake(25f);
            LEstateSky.TriggerEclipseFlash(15f);

            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = Color.Lerp(WhiteHot, SunGold, i / 16f);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 80f, color, 0.6f, 20);
                Vector2 vel = angle.ToRotationVector2() * 5f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, color, 0.5f, 20));
            }

            CustomParticles.GenericFlare(center, WhiteHot, 1.2f, 40);
            Phase10BossVFX.CodaFinale(center, WhiteHot, SunGold, 1.5f);
        }

        #endregion

        #region Death Sequence

        public static void DeathEscalation(Vector2 center, int deathTimer)
        {
            float intensity = MathHelper.Clamp(deathTimer / 100f, 0f, 1f);

            // Escalating sky flashes every 20 frames
            if (deathTimer % 20 == 0 && deathTimer > 0)
                LEstateSky.TriggerSolarFlash(3f + intensity * 10f);

            if (deathTimer % 4 == 0)
            {
                int burstCount = (int)(8 + intensity * 14);
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + deathTimer * 0.05f;
                    Vector2 offset = angle.ToRotationVector2() * (35f + intensity * 50f);
                    Color flareColor = Color.Lerp(SunGold, HeatRed, (float)i / 6f);
                    CustomParticles.GenericFlare(center + offset, flareColor, 0.4f + intensity * 0.35f, 12);
                }
            }

            MagnumScreenEffects.AddScreenShake(intensity * 5f);
        }

        public static void DeathFinale(Vector2 center)
        {
            LEstateSky.TriggerSupernovaFlash(20f);
            MagnumScreenEffects.AddScreenShake(25f);

            CustomParticles.GenericFlare(center, WhiteHot, 1.2f, 45);
            CustomParticles.GenericFlare(center, SunGold, 1.2f, 40);
            CustomParticles.GenericFlare(center, BlazingOrange, 1.2f, 35);

            for (int i = 0; i < 16; i++)
            {
                Color ringColor = Color.Lerp(SunGold, HeatRed, i / 16f);
                CustomParticles.HaloRing(center, ringColor, 0.4f + i * 0.15f, 20 + i * 2);
            }

            // Supernova bloom ring
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 vel = angle.ToRotationVector2() * 5.5f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel,
                    Color.Lerp(SunGold, HeatRed, i / 14f), 0.6f, 22));
            }

            // Ascending sparkles
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, WhiteHot, 0.35f, 28));
            }

            Phase10BossVFX.CodaFinale(center, SunGold, HeatRed, 2.0f);
        }

        #endregion
    }
}
