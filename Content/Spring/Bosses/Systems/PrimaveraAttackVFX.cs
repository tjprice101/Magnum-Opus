using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Spring.Bosses.Systems
{
    /// <summary>
    /// Primavera boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// Theme: Blooming, growth, rebirth, delicate petals.
    /// </summary>
    public static class PrimaveraAttackVFX
    {
        private static readonly Color SpringGreen = new Color(100, 200, 80);
        private static readonly Color BlossomPink = new Color(240, 160, 180);
        private static readonly Color SunshineYellow = new Color(240, 220, 80);
        private static readonly Color SproutGreen = new Color(60, 160, 60);

        #region Core Attack VFX

        /// <summary>PetalStorm: Swirling cherry blossom petals with pink and green sparks.</summary>
        public static void PetalStormTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 380f, 30, BlossomPink * 0.7f);
            Phase10BossVFX.AccelerandoSpiral(position, BlossomPink, 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void PetalStormImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(7f);
            CustomParticles.ExplosionBurst(position, BlossomPink, 10);
            Phase10BossVFX.CymbalCrashBurst(position, 0.7f);
            CustomParticles.HaloRing(position, SpringGreen, 0.5f, 15);
            CustomParticles.GenericMusicNotes(position, BlossomPink, 4, 45f);
        }

        /// <summary>BlossomBreeze: Gentle wind carrying petals that accelerates into razor gusts.</summary>
        public static void BlossomBreezeTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 450f, 30, SpringGreen * 0.5f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 450f, BlossomPink, 0.5f);
        }

        public static void BlossomBreezeTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, BlossomPink, 0.25f, 8);
            CustomParticles.GlowTrail(position, SpringGreen, 0.2f);
            if (Main.rand.NextBool(3))
                CustomParticles.GenericMusicNotes(position, SunshineYellow, 1, 25f);
        }

        /// <summary>SpringShower: Rain of golden pollen drops from above.</summary>
        public static void SpringShowerTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -200), SunshineYellow, 0.8f);
            TelegraphSystem.DangerZone(targetArea, 200f, 30, SunshineYellow * 0.3f);
        }

        public static void SpringShowerParticle(Vector2 position)
        {
            Color color = Color.Lerp(SunshineYellow, SpringGreen, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.25f, 6);
            if (Main.rand.NextBool(4))
                CustomParticles.GenericMusicNotes(position, BlossomPink, 1, 20f);
        }

        #endregion

        #region Phase 2B Attack VFX (70% HP)

        /// <summary>VernalVortex: Spiraling flower vortex pulling enemies inward.</summary>
        public static void VernalVortexTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 130f, 8, SpringGreen * 0.5f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { SpringGreen }, 0.7f);
        }

        public static void VernalVortexBurst(Vector2 center, int waveIndex)
        {
            CustomParticles.ExplosionBurst(center, Color.Lerp(SpringGreen, BlossomPink, waveIndex / 5f), 8 + waveIndex * 3);
            CustomParticles.GenericFlare(center, SunshineYellow, 0.5f, 15);
            Phase10BossVFX.NoteConstellationWarning(center, BlossomPink, 0.4f + waveIndex * 0.15f);
        }

        /// <summary>GrowthSurge: Unique healing VFX  Egreen restoration circle that mends the boss.</summary>
        public static void GrowthSurgeTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 100f, 6, SproutGreen * 0.6f);
            Phase10BossVFX.FermataHoldIndicator(center, SproutGreen, 0.5f);
        }

        public static void GrowthSurgeHealPulse(Vector2 center, float healProgress)
        {
            // Green restoration circle expanding outward
            float radius = 30f + healProgress * 80f;
            Color healColor = Color.Lerp(SproutGreen, SpringGreen, healProgress);
            CustomParticles.HaloRing(center, healColor, 0.3f + healProgress * 0.5f, 20);

            // Rising green sparkles indicate life restoration
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                CustomParticles.GenericFlare(center + offset, SproutGreen, 0.2f + healProgress * 0.2f, 15);
            }
            CustomParticles.GenericGlow(center, SpringGreen, 0.4f + healProgress * 0.3f, 10);
            Phase10BossVFX.PizzicatoPop(center, SpringGreen);
        }

        /// <summary>FloralBarrage: Cascading floral projectiles in expanding spiral arms.</summary>
        public static void FloralBarrageTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 550f, 30, BlossomPink * 0.7f);
            Phase10BossVFX.FortissimoFlashWarning(position, BlossomPink, 0.8f);
        }

        public static void FloralBarrageAfterimage(Vector2 position, int burstNumber)
        {
            float intensity = 0.3f + burstNumber * 0.12f;
            Color trailColor = Color.Lerp(BlossomPink, SunshineYellow, burstNumber / 5f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 10);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 15, 3);
        }

        #endregion

        #region Phase 2C Attack VFX (40% HP)

        /// <summary>BloomingJudgment: Massive radial flower explosion judgment attack.</summary>
        public static void BloomingJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 12, BlossomPink * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { BlossomPink }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, SpringGreen, 1.2f);
        }

        public static void BloomingJudgmentRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(16f);
            CustomParticles.GenericFlare(center, new Color(255, 240, 220), 1.2f, 20);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 2 == 0 ? BlossomPink : SpringGreen;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 55f, color, 0.4f, 15);
            }
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { BlossomPink, SpringGreen, SunshineYellow }, 1.3f);
            BossSignatureVFX.SpringBloomBurst(center, 1.5f);
        }

        /// <summary>RebornSpring: Devastating dive attack trailing flower petals and new growth.</summary>
        public static void RebornSpringTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 750f, 30, SproutGreen * 0.8f);
            TelegraphSystem.ImpactPoint(target, 60f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, SpringGreen, 0.8f);
        }

        public static void RebornSpringImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(20f);
            CustomParticles.ExplosionBurst(position, SpringGreen, 15);
            CustomParticles.GenericFlare(position, new Color(255, 255, 220), 1.5f, 25);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 40f,
                    Color.Lerp(BlossomPink, SpringGreen, i / 8f), 0.4f, 15);
            }
            BossSignatureVFX.SpringBloomBurst(position, 1.8f);
            Phase10BossVFX.TimpaniDrumrollImpact(position, SpringGreen, 1.4f);
        }

        /// <summary>AprilShowers: Ultimate spiral rain barrage finale.</summary>
        public static void AprilShowersTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 16, SunshineYellow);
            Phase10BossVFX.StaffLineConvergence(center, SunshineYellow, 1.2f);
            BossVFXOptimizer.WarningFlare(center, 1.0f);
        }

        public static void AprilShowersWave(Vector2 center, int waveIndex)
        {
            float angle = waveIndex * 0.15f;
            for (int arm = 0; arm < 5; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 5f;
                Vector2 pos = center + armAngle.ToRotationVector2() * 40f;
                Color color = Color.Lerp(BlossomPink, SpringGreen, arm / 5f);
                CustomParticles.GenericFlare(pos, color, 0.3f, 10);
            }
            CustomParticles.GenericMusicNotes(center, SunshineYellow, 3, 55f);
        }

        public static void AprilShowersFinale(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, new Color(255, 255, 220), 2f, 30);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color color = VFXIntegration.GetThemeColor("Spring", i / 20f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 100f, color, 0.8f, 25);
            }
            Phase10BossVFX.CodaFinale(center, BlossomPink, SpringGreen, 2f);
        }

        #endregion
    }
}
