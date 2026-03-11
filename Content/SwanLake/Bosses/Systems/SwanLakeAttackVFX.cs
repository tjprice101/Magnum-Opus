using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

using static MagnumOpus.Content.SwanLake.Bosses.Systems.SwanLakeSkySystem;

namespace MagnumOpus.Content.SwanLake.Bosses.Systems
{
    /// <summary>
    /// Swan Lake boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// Mood-aware: Graceful, Tempest, DyingSwan colors shift accordingly.
    /// </summary>
    public static class SwanLakeAttackVFX
    {
        private static readonly Color PureWhite = new Color(240, 240, 255);
        private static readonly Color JetBlack = new Color(15, 15, 20);
        private static readonly Color PearlPink = new Color(255, 230, 240);
        private static readonly Color PearlBlue = new Color(230, 240, 255);

        private static Color GetPrismatic(float offset = 0f)
        {
            float hue = ((float)Main.timeForVisualEffects * 0.005f + offset) % 1f;
            return Main.hslToRgb(hue, 0.6f, 0.8f);
        }

        #region Phase 1  EGraceful Mood

        /// <summary>FeatherCascade: Waves of feathers descending gracefully.</summary>
        public static void FeatherCascadeTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -200), PureWhite, 0.6f);
            TelegraphSystem.DangerZone(targetArea, 200f, 30, PureWhite * 0.3f);
        }

        public static void FeatherCascadeParticle(Vector2 position)
        {
            CustomParticles.SwanFeatherDrift(position, Main.rand.NextBool() ? PureWhite : PearlPink, 0.35f);
            if (Main.rand.NextBool(3))
                CustomParticles.PrismaticSparkle(position, GetPrismatic(), 0.25f);
        }

        /// <summary>PrismaticSparkleRing: Ring of rainbow sparkles expanding outward.</summary>
        public static void PrismaticSparkleRingTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 100f, 6, PureWhite * 0.5f);
            Phase10BossVFX.NoteConstellationWarning(center, PureWhite, 0.5f);
        }

        public static void PrismaticSparkleRingRelease(Vector2 center, int ringIndex)
        {
            int count = 12 + ringIndex * 4;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * (60f + ringIndex * 30f);
                CustomParticles.PrismaticSparkle(pos, GetPrismatic(i / (float)count), 0.35f);
            }
            CustomParticles.HaloRing(center, PureWhite, 0.5f + ringIndex * 0.15f, 18);
        }

        /// <summary>DualSwanArcSlashes: Two simultaneous arc attacks (black + white).</summary>
        public static void DualSwanArcSlashesTelegraph(Vector2 position, Vector2 dir1, Vector2 dir2)
        {
            TelegraphSystem.ThreatLine(position, dir1, 350f, 30, PureWhite * 0.6f);
            TelegraphSystem.ThreatLine(position, dir2, 350f, 30, JetBlack * 0.6f);
            Phase10BossVFX.CounterpointDuality(position, PureWhite, JetBlack);
        }

        public static void DualSwanArcSlashesImpact(Vector2 position, bool isWhite)
        {
            Color color = isWhite ? PureWhite : JetBlack;
            TriggerWhiteFlash(8f);
            CustomParticles.SwanFeatherBurst(position, 8, 0.4f);
            CustomParticles.HaloRing(position, color, 0.5f, 16);
            ThemedParticles.SwanLakeSparks(position, Vector2.UnitX, 6, 5f);
            BossSignatureVFX.SwanLakeGracefulStrike(position, Vector2.UnitX, 0.8f);
            var bloom = new BloomParticle(position, Vector2.Zero, color, 0.5f, 12);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        /// <summary>GracefulDash: Elegant gliding charge with feather trail.</summary>
        public static void GracefulDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 500f, 30, PureWhite * 0.6f);
            Phase10BossVFX.GlissandoSlideWarning(position, target, PureWhite, 0.5f);
        }

        public static void GracefulDashTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.SwanFeatherTrail(position, velocity, 0.3f);
            if (Main.rand.NextBool(3))
                CustomParticles.PrismaticSparkle(position, GetPrismatic(), 0.2f);
        }

        #endregion

        #region Phase 2  ETempest Mood

        /// <summary>LightningFractalStorm: Fractal lightning pattern across the arena.</summary>
        public static void LightningFractalStormTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 150f, 10, PureWhite * 0.7f);
            Phase10BossVFX.CrescendoDangerRings(center, PureWhite, 0.9f, 5);
            Phase10BossVFX.FortissimoFlashWarning(center, PureWhite, 1.0f);
        }

        public static void LightningFractalStormBolt(Vector2 start, Vector2 end)
        {
            Phase10BossVFX.StaffLineLaser(start, end, PureWhite, 25f);
            CustomParticles.PrismaticSparkleBurst(start, PureWhite, 6);
        }

        /// <summary>TempestDash: Violent charge with prismatic distortion.</summary>
        public static void TempestDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, GetPrismatic() * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, PureWhite, 0.9f);
        }

        public static void TempestDashImpact(Vector2 position)
        {
            TriggerPrismaticFlash(10f);
            MagnumScreenEffects.AddScreenShake(12f);
            CustomParticles.SwanFeatherExplosion(position, 15, 0.5f);
            ThemedParticles.SwanLakeRainbowExplosion(position, 1.0f);
            CustomParticles.HaloRing(position, PureWhite, 0.7f, 18);
            CustomParticles.HaloRing(position, JetBlack, 0.5f, 20);
            // Bloom burst at impact
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                var bloom = new BloomParticle(position, vel, GetPrismatic(i / 6f), 0.4f, 15);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        /// <summary>PrismaticBarrage: Streams of rainbow projectiles.</summary>
        public static void PrismaticBarrageTelegraph(Vector2 position)
        {
            Phase10BossVFX.AccelerandoSpiral(position, PureWhite, 0.7f, 12);
            TelegraphSystem.ConvergingRing(position, 120f, 8, GetPrismatic() * 0.5f);
        }

        public static void PrismaticBarrageRelease(Vector2 position, int burstIndex)
        {
            Color color = GetPrismatic(burstIndex * 0.1f);
            CustomParticles.GenericFlare(position, color, 0.4f, 14);
            CustomParticles.PrismaticSparkle(position, color, 0.3f);
            BossVFXOptimizer.OptimizedFlare(position, PureWhite, 0.3f, 10);
        }

        /// <summary>FeatherStorm: Massive burst of black and white feathers.</summary>
        public static void FeatherStormRelease(Vector2 center)
        {
            TriggerWhiteFlash(8f);
            MagnumScreenEffects.AddScreenShake(8f);
            CustomParticles.SwanFeatherExplosion(center, 20, 0.45f);
            CustomParticles.SwanFeatherDuality(center, 10, 0.4f);
            ThemedParticles.SwanLakeShockwave(center, 1.0f);
            var bloom = new BloomParticle(center, Vector2.Zero, PureWhite, 0.6f, 18);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        /// <summary>MirrorDance: Mirrored attack patterns from twin positions.</summary>
        public static void MirrorDanceTelegraph(Vector2 pos1, Vector2 pos2)
        {
            TelegraphSystem.ThreatLine(pos1, (pos2 - pos1).SafeNormalize(Vector2.UnitX), (pos2 - pos1).Length(), 30, PureWhite * 0.5f);
            Phase10BossVFX.CounterpointDuality(Vector2.Lerp(pos1, pos2, 0.5f), PureWhite, JetBlack);
        }

        public static void MirrorDanceImpact(Vector2 position)
        {
            CustomParticles.SwanFeatherBurst(position, 10, 0.35f);
            ThemedParticles.SwanLakeImpact(position, 0.8f);
        }

        #endregion

        #region Phase 3  EDying Swan Mood

        /// <summary>MonochromaticApocalypse: The ultimate rotating beam of pure destruction.</summary>
        public static void MonochromaticApocalypseTelegraph(Vector2 center)
        {
            TriggerMonochromeFlash(15f);
            TelegraphSystem.ConvergingRing(center, 250f, 16, PureWhite);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { PureWhite, JetBlack, GetPrismatic() }, 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(center, PureWhite, 1.5f);
            BossVFXOptimizer.WarningFlare(center, 1.2f);
        }

        public static void MonochromaticApocalypseBeam(Vector2 origin, float rotation, float length)
        {
            BossSignatureVFX.SwanLakeFractalLaser(origin, rotation, length, 1.5f);
            Phase10Integration.SwanLake.MonochromaticApocalypseVFX(origin, rotation, 1.2f);
        }

        /// <summary>DyingSwanLament: Fading feather bursts that slow and dissipate.</summary>
        public static void DyingSwanLamentRelease(Vector2 center)
        {
            TriggerMonochromeFlash(8f);
            CustomParticles.SwanFeatherExplosion(center, 12, 0.35f);
            ThemedParticles.SwanLakeSparkles(center, 10, 50f);
            Phase10BossVFX.DiminuendoFade(center, PureWhite, 0.5f);
            // Ascending sparkle wisps — elegance in decay
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1.5f));
                var sparkle = new SparkleParticle(center + Main.rand.NextVector2Circular(30f, 30f), vel, PureWhite, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        /// <summary>FinalSerenade: The death attack  Eall-encompassing prismatic explosion.</summary>
        public static void FinalSerenadeTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 20, PureWhite);
            Phase10BossVFX.StaffLineConvergence(center, PureWhite, 1.2f);
        }

        public static void FinalSerenadeRelease(Vector2 center)
        {
            TriggerDeathFlash(20f);
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, PureWhite, 2.0f, 30);
            ThemedParticles.SwanLakeRainbowExplosion(center, 2.0f);
            CustomParticles.PrismaticSparkleRainbow(center, 20);
            // Massive bloom supernova ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                var bloom = new BloomParticle(center, vel, GetPrismatic(i / 16f), 0.7f, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color color = GetPrismatic(i / 20f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 100f, color, 0.8f, 25);
                CustomParticles.SwanFeatherDrift(center + angle.ToRotationVector2() * 80f, color, 0.4f);
            }
            Phase10BossVFX.CodaFinale(center, PureWhite, JetBlack, 2.0f);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { PureWhite, JetBlack, GetPrismatic() }, 2.0f);
            BossSignatureVFX.SwanLakeSerenade(center, 5, 5, 2.0f);
        }

        /// <summary>GhostSwanDash: Fading spectral dash with monochrome trail.</summary>
        public static void GhostSwanDashTrail(Vector2 position, Vector2 velocity)
        {
            Color fadedWhite = PureWhite * 0.5f;
            CustomParticles.SwanFeatherTrail(position, velocity, 0.25f);
            CustomParticles.GenericGlow(position, fadedWhite, 0.2f, 15);
        }

        /// <summary>ShatteredReflection: Fragments of the fractal boss fly outward.</summary>
        public static void ShatteredReflectionBurst(Vector2 center, int fragmentCount)
        {
            TriggerPrismaticFlash(12f);
            for (int i = 0; i < fragmentCount; i++)
            {
                float angle = MathHelper.TwoPi * i / fragmentCount;
                Vector2 pos = center + angle.ToRotationVector2() * 50f;
                Color color = i % 2 == 0 ? PureWhite : JetBlack;
                CustomParticles.GenericFlare(pos, color, 0.5f, 18);
                CustomParticles.SwanFeatherBurst(pos, 3, 0.3f);
                var bloom = new BloomParticle(pos, angle.ToRotationVector2() * 2f, color, 0.35f, 15);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            CustomParticles.PrismaticSparkleBurst(center, PureWhite, 12);
        }

        #endregion
    }
}
