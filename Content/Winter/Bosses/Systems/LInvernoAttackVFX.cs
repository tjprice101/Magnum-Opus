using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Winter.Bosses.Systems
{
    /// <summary>
    /// L'Inverno boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// Theme: Frozen silence, ice crystals, blizzard, absolute zero.
    /// </summary>
    public static class LInvernoAttackVFX
    {
        private static readonly Color FrostBlue = new Color(120, 180, 240);
        private static readonly Color IceWhite = new Color(220, 235, 255);
        private static readonly Color DeepIndigo = new Color(40, 50, 100);
        private static readonly Color CrystalSilver = new Color(190, 200, 220);

        #region Core Attack VFX

        /// <summary>IcicleStorm: Shattering ice shards raining with crystalline bursts.</summary>
        public static void IcicleStormTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 380f, 30, FrostBlue * 0.7f);
            Phase10BossVFX.AccelerandoSpiral(position, CrystalSilver, 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void IcicleStormImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            CustomParticles.ExplosionBurst(position, FrostBlue, 10);
            Phase10BossVFX.CymbalCrashBurst(position, 0.7f);
            CustomParticles.HaloRing(position, IceWhite, 0.5f, 15);
            CustomParticles.GenericMusicNotes(position, FrostBlue, 4, 45f);
        }

        /// <summary>FrostBreath: Freezing exhalation cone of crystallized air.</summary>
        public static void FrostBreathTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.SectorCone(position, direction, MathHelper.PiOver4, 400f, 30, FrostBlue * 0.5f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 400f, IceWhite, 0.5f);
        }

        public static void FrostBreathTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, IceWhite, 0.3f, 8);
            CustomParticles.GlowTrail(position, FrostBlue, 0.25f);
            if (Main.rand.NextBool(3))
                CustomParticles.GenericMusicNotes(position, CrystalSilver, 1, 25f);
        }

        /// <summary>CrystalBarrage: Volley of precision ice crystal projectiles.</summary>
        public static void CrystalBarrageTelegraph(Vector2 position)
        {
            TelegraphSystem.ConvergingRing(position, 100f, 6, CrystalSilver * 0.5f);
            Phase10BossVFX.StaccatoMultiBurst(position, FrostBlue, 4, 25f);
        }

        public static void CrystalBarrageParticle(Vector2 position)
        {
            Color color = Color.Lerp(FrostBlue, IceWhite, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.25f, 6);
            if (Main.rand.NextBool(4))
                CustomParticles.GenericMusicNotes(position, CrystalSilver, 1, 20f);
        }

        #endregion

        #region Phase 2B Attack VFX (70% HP)

        /// <summary>GlacialCharge: Armored ice charge leaving frozen ground in its wake.</summary>
        public static void GlacialChargeTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, DeepIndigo * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, FrostBlue, 0.9f);
        }

        public static void GlacialChargeAfterimage(Vector2 position, int dashNumber)
        {
            float intensity = 0.3f + dashNumber * 0.15f;
            Color trailColor = Color.Lerp(FrostBlue, IceWhite, dashNumber / 5f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 10);
            CustomParticles.GlowTrail(position, trailColor, intensity * 0.7f);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 15, 3);
        }

        /// <summary>BlizzardVortex: Swirling blizzard cyclone pulling enemies in.</summary>
        public static void BlizzardVortexTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 160f, 10, FrostBlue * 0.5f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepIndigo }, 0.8f);
        }

        public static void BlizzardVortexBurst(Vector2 center, int waveIndex)
        {
            CustomParticles.ExplosionBurst(center, Color.Lerp(FrostBlue, IceWhite, waveIndex / 5f), 8 + waveIndex * 3);
            CustomParticles.GenericFlare(center, CrystalSilver, 0.5f, 15);
            Phase10BossVFX.NoteConstellationWarning(center, FrostBlue, 0.4f + waveIndex * 0.15f);
        }

        /// <summary>FreezeRay: Concentrated absolute zero beam that crystallizes on contact.</summary>
        public static void FreezeRayTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 25f, 30, IceWhite * 0.6f);
            Phase10BossVFX.ChordBuildupSpiral(start, new[] { FrostBlue }, 0.7f);
            BossVFXOptimizer.WarningFlare(start, 0.6f);
        }

        public static void FreezeRayImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(12f);
            CustomParticles.GenericFlare(position, IceWhite, 0.9f, 20);
            CustomParticles.HaloRing(position, FrostBlue, 0.7f, 20);
            Phase10BossVFX.SforzandoSpike(position, CrystalSilver, 0.9f);
        }

        #endregion

        #region Phase 2C Attack VFX (40% HP)

        /// <summary>WintersJudgment: Massive radial ice explosion with crystalline shatter.</summary>
        public static void WintersJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 12, FrostBlue * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { FrostBlue }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, IceWhite, 1.2f);
        }

        public static void WintersJudgmentRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(18f);
            CustomParticles.GenericFlare(center, IceWhite, 1.3f, 22);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 2 == 0 ? FrostBlue : CrystalSilver;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 55f, color, 0.4f, 15);
            }
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { FrostBlue, IceWhite, CrystalSilver }, 1.4f);
            BossSignatureVFX.WinterFrostBurst(center, 1.5f);
        }

        /// <summary>AbsoluteZero: Devastating area freeze that crystallizes everything.</summary>
        public static void AbsoluteZeroTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 750f, 30, DeepIndigo * 0.8f);
            TelegraphSystem.ImpactPoint(target, 65f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, DeepIndigo, 0.8f);
        }

        public static void AbsoluteZeroImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(22f);
            CustomParticles.ExplosionBurst(position, IceWhite, 15);
            CustomParticles.GenericFlare(position, new Color(240, 248, 255), 1.5f, 25);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 45f,
                    Color.Lerp(FrostBlue, IceWhite, i / 8f), 0.4f, 15);
            }
            BossSignatureVFX.WinterFrostBurst(position, 1.8f);
            Phase10BossVFX.TimpaniDrumrollImpact(position, FrostBlue, 1.4f);
        }

        /// <summary>EternalFrost: Ultimate blizzard spiral barrage finale.</summary>
        public static void EternalFrostTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 260f, 16, IceWhite);
            Phase10BossVFX.StaffLineConvergence(center, FrostBlue, 1.3f);
            BossVFXOptimizer.WarningFlare(center, 1.0f);
        }

        public static void EternalFrostWave(Vector2 center, int waveIndex)
        {
            float angle = waveIndex * 0.14f;
            for (int arm = 0; arm < 6; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 6f;
                Vector2 pos = center + armAngle.ToRotationVector2() * 42f;
                Color color = Color.Lerp(FrostBlue, DeepIndigo, arm / 6f);
                CustomParticles.GenericFlare(pos, color, 0.3f, 10);
            }
            CustomParticles.GenericMusicNotes(center, CrystalSilver, 3, 60f);
        }

        public static void EternalFrostFinale(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(28f);
            CustomParticles.GenericFlare(center, IceWhite, 2.2f, 30);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color color = VFXIntegration.GetThemeColor("Winter", i / 20f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 110f, color, 0.9f, 28);
            }
            Phase10BossVFX.CodaFinale(center, FrostBlue, DeepIndigo, 2.2f);
        }

        #endregion
    }
}
