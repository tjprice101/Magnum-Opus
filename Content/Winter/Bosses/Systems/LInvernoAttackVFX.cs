using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Content.Winter.Bosses.Systems.LInvernoSkySystem;

namespace MagnumOpus.Content.Winter.Bosses.Systems
{
    /// <summary>
    /// Phase-thematic L'Inverno boss attack VFX choreography.
    /// Phase 1: Geometric/hexagonal crystalline patterns.
    /// Phase 2: Cracking frost, oppressive cold, heavier impacts.
    /// Phase 3: Storm-emerging attacks, void-emerging projectiles.
    /// Enrage: Silence-shattering ice-burst rings.
    /// </summary>
    public static class LInvernoAttackVFX
    {
        // Updated palette
        private static readonly Color IceBlue = new Color(168, 216, 234);
        private static readonly Color FrostWhite = new Color(232, 244, 248);
        private static readonly Color DeepGlacialBlue = new Color(27, 79, 114);
        private static readonly Color CrystalCyan = new Color(0, 229, 255);
        private static readonly Color BlizzardWhite = new Color(240, 248, 255);
        private static readonly Color GlacialPurple = new Color(123, 104, 174);
        private static readonly Color PaleSilverBlue = new Color(190, 210, 230);
        private static readonly Color AbsoluteZeroBlue = new Color(200, 230, 255);

        /// <summary>Helper to get phase-appropriate accent color.</summary>
        private static Color GetPhaseAccent(int phase)
        {
            switch (phase)
            {
                case 1: return PaleSilverBlue;
                case 2: return IceBlue;
                case 3: return CrystalCyan;
                default: return AbsoluteZeroBlue;
            }
        }

        #region Core Attack VFX (Phase 1 primary — geometric, crystalline)

        public static void IcicleStormTelegraph(Vector2 position, Vector2 direction)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Color telegraphColor = phase <= 2 ? IceBlue : CrystalCyan;

            TelegraphSystem.ThreatLine(position, direction, 380f, 30, telegraphColor * 0.7f);

            // Phase 1: Clean hexagonal convergence pattern
            if (phase == 1)
            {
                Phase10BossVFX.AccelerandoSpiral(position, PaleSilverBlue, 0.5f);
            }
            // Phase 2+: More aggressive warning
            else
            {
                Phase10BossVFX.AccelerandoSpiral(position, GetPhaseAccent(phase), 0.6f + (phase - 1) * 0.15f);
                BossVFXOptimizer.WarningFlare(position, 0.4f + phase * 0.1f);
            }
        }

        public static void IcicleStormImpact(Vector2 position)
        {
            int phase = LInvernoSky.GetVFXPhase();
            float shakeBase = 6f + phase * 2f;
            MagnumScreenEffects.AddScreenShake(shakeBase);

            if (phase >= 3)
                TriggerBlizzardFlash(4f + phase);
            else
                TriggerFrostFlash(3f + phase * 1.5f);

            // Layered burst — more layers at higher phases
            CustomParticles.ExplosionBurst(position, GetPhaseAccent(phase), 8 + phase * 3);
            Phase10BossVFX.CymbalCrashBurst(position, 0.5f + phase * 0.15f);
            CustomParticles.HaloRing(position, FrostWhite, 0.4f + phase * 0.1f, 12 + phase * 3);
            CustomParticles.GenericMusicNotes(position, IceBlue, 3 + phase, 40f + phase * 5f);

            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero,
                GetPhaseAccent(phase), 0.35f + phase * 0.08f, 10 + phase * 2));

            // Phase 3+: Additional storm debris
            if (phase >= 3)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                    MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                        position + vel * 5f, vel, BlizzardWhite, 0.2f, 16));
                }
            }
        }

        public static void FrostBreathTelegraph(Vector2 position, Vector2 direction)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Color coneColor = phase <= 2 ? IceBlue : DeepGlacialBlue;

            TelegraphSystem.SectorCone(position, direction, MathHelper.PiOver4, 400f, 30, coneColor * 0.5f);

            if (phase == 1)
                Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 400f, PaleSilverBlue, 0.4f);
            else
                Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 400f, GetPhaseAccent(phase), 0.5f + phase * 0.1f);
        }

        public static void FrostBreathTrail(Vector2 position, Vector2 velocity)
        {
            int phase = LInvernoSky.GetVFXPhase();
            CustomParticles.GenericFlare(position, FrostWhite, 0.25f + phase * 0.05f, 6 + phase * 2);
            CustomParticles.GlowTrail(position, GetPhaseAccent(phase), 0.2f + phase * 0.05f);

            if (Main.rand.NextBool(4 - Math.Min(phase, 3)))
                CustomParticles.GenericMusicNotes(position, PaleSilverBlue, 1, 20f + phase * 5f);
        }

        public static void CrystalBarrageTelegraph(Vector2 position)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Color ringColor = phase <= 2 ? PaleSilverBlue : CrystalCyan;

            TelegraphSystem.ConvergingRing(position, 100f, 6, ringColor * 0.5f);
            Phase10BossVFX.StaccatoMultiBurst(position, GetPhaseAccent(phase), 3 + phase, 20f + phase * 5f);
        }

        public static void CrystalBarrageParticle(Vector2 position)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Color color = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat(0.3f));
            if (phase >= 3) color = Color.Lerp(CrystalCyan, BlizzardWhite, Main.rand.NextFloat(0.4f));

            CustomParticles.GenericFlare(position, color, 0.2f + phase * 0.05f, 5 + phase);
            if (Main.rand.NextBool(5 - Math.Min(phase, 3)))
                CustomParticles.GenericMusicNotes(position, PaleSilverBlue, 1, 18f + phase * 3f);
        }

        #endregion

        #region Phase 2 Attack VFX (70% HP — cracking, oppressive)

        public static void GlacialChargeTelegraph(Vector2 position, Vector2 target)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);

            Color lineColor = phase == 2 ? DeepGlacialBlue : (phase >= 3 ? new Color(15, 40, 80) : DeepGlacialBlue);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, lineColor * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, GetPhaseAccent(phase), 0.8f + phase * 0.1f);
        }

        public static void GlacialChargeAfterimage(Vector2 position, int dashNumber)
        {
            int phase = LInvernoSky.GetVFXPhase();
            float intensity = 0.3f + dashNumber * 0.15f;

            Color trailColor;
            switch (phase)
            {
                case 2: trailColor = Color.Lerp(IceBlue, FrostWhite, dashNumber / 5f); break;
                case 3: trailColor = Color.Lerp(CrystalCyan, BlizzardWhite, dashNumber / 5f); break;
                default: trailColor = Color.Lerp(AbsoluteZeroBlue, Color.White, dashNumber / 5f); break;
            }

            CustomParticles.GenericFlare(position, trailColor, intensity, 8 + phase * 2);
            CustomParticles.GlowTrail(position, trailColor, intensity * 0.7f);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 12 + phase * 3, 3);

            // Phase 3+: Shattered frost debris
            if (phase >= 3 && Main.rand.NextBool(2))
            {
                Vector2 debrisVel = Main.rand.NextVector2Circular(2f, 2f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    position, debrisVel, BlizzardWhite, 0.15f, 12));
            }
        }

        public static void BlizzardVortexTelegraph(Vector2 center)
        {
            int phase = LInvernoSky.GetVFXPhase();
            TelegraphSystem.ConvergingRing(center, 160f, 10, GetPhaseAccent(phase) * 0.5f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepGlacialBlue }, 0.7f + phase * 0.1f);
        }

        public static void BlizzardVortexBurst(Vector2 center, int waveIndex)
        {
            int phase = LInvernoSky.GetVFXPhase();
            float flashIntensity = 2f + waveIndex * 0.5f + (phase - 2) * 0.5f;
            TriggerBlizzardFlash(flashIntensity);

            Color burstColor = Color.Lerp(GetPhaseAccent(phase), FrostWhite, waveIndex / 5f);
            CustomParticles.ExplosionBurst(center, burstColor, 8 + waveIndex * 3 + phase * 2);
            CustomParticles.GenericFlare(center, PaleSilverBlue, 0.4f + waveIndex * 0.1f, 12 + phase * 3);
            Phase10BossVFX.NoteConstellationWarning(center, GetPhaseAccent(phase), 0.4f + waveIndex * 0.15f);

            // Storm bloom ring
            for (int i = 0; i < 4 + phase; i++)
            {
                float angle = MathHelper.TwoPi * i / (4 + phase);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (2f + waveIndex);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel,
                    burstColor, 0.3f, 12 + waveIndex * 2));
            }
        }

        public static void FreezeRayTelegraph(Vector2 start, Vector2 end)
        {
            int phase = LInvernoSky.GetVFXPhase();
            TelegraphSystem.LaserPath(start, end, 25f, 30, FrostWhite * 0.6f);
            Phase10BossVFX.ChordBuildupSpiral(start, new[] { GetPhaseAccent(phase) }, 0.6f + phase * 0.1f);
            BossVFXOptimizer.WarningFlare(start, 0.5f + phase * 0.1f);
        }

        public static void FreezeRayImpact(Vector2 position)
        {
            int phase = LInvernoSky.GetVFXPhase();
            MagnumScreenEffects.AddScreenShake(10f + phase * 2f);
            TriggerFrostFlash(6f + phase * 2f);

            CustomParticles.GenericFlare(position, FrostWhite, 0.7f + phase * 0.15f, 16 + phase * 4);
            CustomParticles.HaloRing(position, GetPhaseAccent(phase), 0.5f + phase * 0.1f, 16 + phase * 4);
            Phase10BossVFX.SforzandoSpike(position, PaleSilverBlue, 0.7f + phase * 0.15f);

            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero,
                FrostWhite, 0.35f + phase * 0.08f, 12 + phase * 2));

            // Phase 3+: Frost ground crack sparkles
            if (phase >= 3)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 crackPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (25f + i * 8f);
                    MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                        crackPos, Vector2.Zero, CrystalCyan, 0.18f, 20));
                }
            }
        }

        #endregion

        #region Phase 3 Attack VFX (40% HP — storm-emerging, overwhelming)

        public static void WintersJudgmentTelegraph(Vector2 center)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Color ringColor = phase >= 3 ? CrystalCyan : IceBlue;

            TelegraphSystem.ConvergingRing(center, 200f, 12, ringColor * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { GetPhaseAccent(phase) }, 0.9f + phase * 0.15f);
            Phase10BossVFX.FortissimoFlashWarning(center, FrostWhite, 1.0f + phase * 0.2f);
        }

        public static void WintersJudgmentRelease(Vector2 center)
        {
            int phase = LInvernoSky.GetVFXPhase();
            MagnumScreenEffects.AddScreenShake(16f + phase * 3f);
            TriggerCrystalFlash(10f + phase * 2f);

            CustomParticles.GenericFlare(center, FrostWhite, 1.1f + phase * 0.2f, 18 + phase * 4);

            // Expanding ring of phase-colored halos
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 2 == 0 ? GetPhaseAccent(phase) : PaleSilverBlue;
                Vector2 ringPos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 55f;
                CustomParticles.HaloRing(ringPos, color, 0.35f + phase * 0.05f, 12 + phase * 3);
            }

            Phase10BossVFX.TuttiFullEnsemble(center,
                new[] { GetPhaseAccent(phase), FrostWhite, PaleSilverBlue }, 1.2f + phase * 0.2f);
            BossSignatureVFX.WinterFrostBurst(center, 1.3f + phase * 0.2f);

            // Bloom cascade ring
            for (int i = 0; i < 8 + phase * 2; i++)
            {
                float angle = MathHelper.TwoPi * i / (8 + phase * 2);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3.5f + phase);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel,
                    Color.Lerp(GetPhaseAccent(phase), FrostWhite, (float)i / (8 + phase * 2)), 0.45f + phase * 0.05f, 16 + phase * 2));
            }
        }

        public static void AbsoluteZeroTelegraph(Vector2 position, Vector2 target)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);

            Color lineColor = phase >= 3 ? new Color(15, 30, 60) : DeepGlacialBlue;
            TelegraphSystem.ThreatLine(position, dir, 750f, 30, lineColor * 0.8f);
            TelegraphSystem.ImpactPoint(target, 65f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, DeepGlacialBlue, 0.7f + phase * 0.1f);
        }

        public static void AbsoluteZeroImpact(Vector2 position)
        {
            int phase = LInvernoSky.GetVFXPhase();
            MagnumScreenEffects.AddScreenShake(20f + phase * 3f);
            TriggerBlizzardFlash(12f + phase * 3f);

            CustomParticles.ExplosionBurst(position, FrostWhite, 12 + phase * 3);
            CustomParticles.GenericFlare(position, BlizzardWhite, 1.3f + phase * 0.2f, 20 + phase * 5);

            // Expanding halo rings
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 ringPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 45f;
                CustomParticles.HaloRing(ringPos,
                    Color.Lerp(GetPhaseAccent(phase), FrostWhite, i / 8f), 0.35f + phase * 0.05f, 12 + phase * 3);
            }

            BossSignatureVFX.WinterFrostBurst(position, 1.6f + phase * 0.2f);
            Phase10BossVFX.TimpaniDrumrollImpact(position, GetPhaseAccent(phase), 1.2f + phase * 0.2f);

            // Bloom cascade ring — silence-shattering in Phase 4
            int bloomCount = phase >= 4 ? 16 : 8 + phase * 2;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (4f + phase);
                Color bloomColor = phase >= 4
                    ? Color.Lerp(AbsoluteZeroBlue, Color.White, (float)i / bloomCount)
                    : Color.Lerp(GetPhaseAccent(phase), FrostWhite, (float)i / bloomCount);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel,
                    bloomColor, 0.4f + phase * 0.08f, 14 + phase * 2));
            }

            // Phase 4: Frozen silence ring — sparkles that hang motionless
            if (phase >= 4)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 frozenPos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (60f + i * 8f);
                    MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                        frozenPos, Vector2.Zero, AbsoluteZeroBlue, 0.25f, 30));
                }
            }
        }

        public static void EternalFrostTelegraph(Vector2 center)
        {
            int phase = LInvernoSky.GetVFXPhase();
            Color ringColor = phase >= 3 ? BlizzardWhite : FrostWhite;

            TelegraphSystem.ConvergingRing(center, 260f, 16, ringColor);
            Phase10BossVFX.StaffLineConvergence(center, GetPhaseAccent(phase), 1.1f + phase * 0.2f);
            BossVFXOptimizer.WarningFlare(center, 0.8f + phase * 0.15f);
        }

        public static void EternalFrostWave(Vector2 center, int waveIndex)
        {
            int phase = LInvernoSky.GetVFXPhase();
            float angle = waveIndex * 0.14f;
            int armCount = 5 + (phase >= 3 ? 2 : 0);

            for (int arm = 0; arm < armCount; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / armCount;
                Vector2 pos = center + new Vector2((float)Math.Cos(armAngle), (float)Math.Sin(armAngle)) * 40f;
                Color color = Color.Lerp(GetPhaseAccent(phase), PaleSilverBlue, (float)arm / armCount);
                CustomParticles.GenericFlare(pos, color, 0.3f + phase * 0.05f, 8 + phase * 2);
            }

            float flashStr = 2f + waveIndex * 0.3f + (phase - 2) * 0.3f;
            TriggerFrostFlash(flashStr);
            CustomParticles.GenericMusicNotes(center, FrostWhite, 2 + phase, 50f + phase * 5f);
        }

        public static void EternalFrostFinale(Vector2 center)
        {
            int phase = LInvernoSky.GetVFXPhase();
            MagnumScreenEffects.AddScreenShake(25f + phase * 4f);
            TriggerAbsoluteZeroFlash(16f + phase * 3f);

            CustomParticles.GenericFlare(center, BlizzardWhite, 2.2f + phase * 0.3f, 30 + phase * 5);

            // Expanding ring of theme-colored flares
            for (int i = 0; i < 20; i++)
            {
                float flareAngle = MathHelper.TwoPi * i / 20f;
                Color color = VFXIntegration.GetThemeColor("Winter", (float)i / 20f);
                Vector2 flarePos = center + new Vector2((float)Math.Cos(flareAngle), (float)Math.Sin(flareAngle)) * 100f;
                CustomParticles.GenericFlare(flarePos, color, 0.8f + phase * 0.1f, 24 + phase * 4);
            }

            Phase10BossVFX.CodaFinale(center, GetPhaseAccent(phase), FrostWhite, 2.0f + phase * 0.2f);

            // Frost supernova bloom ring
            int supernovaCount = 12 + phase * 2;
            for (int i = 0; i < supernovaCount; i++)
            {
                float snAngle = MathHelper.TwoPi * i / supernovaCount;
                Vector2 vel = new Vector2((float)Math.Cos(snAngle), (float)Math.Sin(snAngle)) * (5f + phase);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel,
                    Color.Lerp(GetPhaseAccent(phase), FrostWhite, (float)i / supernovaCount), 0.6f + phase * 0.08f, 22 + phase * 2));
            }

            // Ascending ice sparkles
            for (int i = 0; i < 8 + phase * 3; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 4.5f));
                Color sparkColor = phase >= 4 ? AbsoluteZeroBlue : FrostWhite;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, sparkColor, 0.3f + phase * 0.05f, 26 + phase * 2));
            }
        }

        #endregion
    }
}