using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Bosses.Systems
{
    /// <summary>
    /// Eroica boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// </summary>
    public static class EroicaAttackVFX
    {
        private static readonly Color Gold = new Color(255, 200, 80);
        private static readonly Color Scarlet = new Color(200, 50, 50);
        private static readonly Color Pink = new Color(255, 150, 180);
        private static readonly Color White = new Color(255, 240, 220);

        #region Core Attack VFX

        /// <summary>SwordDash: Scarlet streak with embedded golden music notes.</summary>
        public static void SwordDashTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 400f, 30, Scarlet * 0.7f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 400f, Gold, 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void SwordDashTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, Gold, 0.3f, 8);
            if (Main.rand.NextBool(3))
                ThemedParticles.EroicaMusicNotes(position, 1, 30f);
            CustomParticles.EroicaTrail(position, velocity, 0.4f);
        }

        public static void SwordDashImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            CustomParticles.EroicaImpactBurst(position, 8);
            Phase10BossVFX.CymbalCrashBurst(position, 0.8f);
            CustomParticles.HaloRing(position, Scarlet, 0.5f, 15);
            ThemedParticles.EroicaMusicNotes(position, 4, 50f);
        }

        /// <summary>HeroicBarrage: Streams of golden orbs with scarlet sparkle accents.</summary>
        public static void HeroicBarrageTelegraph(Vector2 position)
        {
            TelegraphSystem.ConvergingRing(position, 100f, 6, Gold * 0.5f);
            Phase10BossVFX.AccelerandoSpiral(position, Scarlet, 0.5f);
        }

        public static void HeroicBarrageRelease(Vector2 position, int burstIndex)
        {
            float angle = burstIndex * 0.3f;
            Color color = Color.Lerp(Gold, Scarlet, (float)Math.Sin(angle) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.4f, 12);
            BossVFXOptimizer.OptimizedFlare(position, color, 0.3f, 15, 2);
        }

        /// <summary>GoldenRain: Projectiles descend like musical notation falling from a staff.</summary>
        public static void GoldenRainTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -200), Gold, 0.8f);
            TelegraphSystem.DangerZone(targetArea, 200f, 30, Gold * 0.3f);
        }

        public static void GoldenRainParticle(Vector2 position)
        {
            Color color = Color.Lerp(Gold, White, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.25f, 6);
            if (Main.rand.NextBool(4))
                ThemedParticles.EroicaMusicNotes(position, 1, 20f);
        }

        #endregion

        #region Phase 2B Attack VFX (70% HP)

        /// <summary>ValorCross: Cross-pattern projectile stream with golden impact halos.</summary>
        public static void ValorCrossTelegraph(Vector2 center)
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 * i;
                TelegraphSystem.ThreatLine(center, angle.ToRotationVector2(), 500f, 25, Gold * 0.6f);
            }
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { Scarlet }, 0.7f);
        }

        public static void ValorCrossImpact(Vector2 position)
        {
            CustomParticles.GlyphBurst(position, Gold, 6);
            CustomParticles.HaloRing(position, Gold, 0.6f, 18);
            Phase10BossVFX.ChordResolutionBloom(position, new[] { Gold }, 1.0f);
        }

        /// <summary>SakuraStorm: Cascading petals in spiral patterns.</summary>
        public static void SakuraStormBurst(Vector2 center, int waveIndex)
        {
            ThemedParticles.SakuraPetals(center, 8 + waveIndex * 4, 80f + waveIndex * 20f);
            CustomParticles.GenericFlare(center, Pink, 0.5f, 15);
            Phase10BossVFX.NoteConstellationWarning(center, Pink, 0.4f + waveIndex * 0.15f);
        }

        /// <summary>TriumphantCharge: Multi-dash with escalating golden afterimages.</summary>
        public static void TriumphantChargeTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, Gold * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, Gold, 0.9f);
        }

        public static void TriumphantChargeAfterimage(Vector2 position, int dashNumber)
        {
            float intensity = 0.3f + dashNumber * 0.15f;
            Color trailColor = Color.Lerp(Scarlet, Gold, dashNumber / 5f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 10);
            CustomParticles.EroicaTrail(position, Vector2.Zero, intensity);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 15, 3);
        }

        #endregion

        #region Phase 2C Attack VFX (40% HP)

        /// <summary>PhoenixDive: Massive diving attack with phoenix wing fire trail.</summary>
        public static void PhoenixDiveTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 800f, 30, new Color(255, 100, 40) * 0.8f);
            TelegraphSystem.ImpactPoint(target, 60f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, Scarlet, 0.8f);
        }

        public static void PhoenixDiveImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(20f);
            CustomParticles.ExplosionBurst(position, Gold, 15);
            CustomParticles.GenericFlare(position, White, 1.5f, 25);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 40f,
                    Color.Lerp(Scarlet, Gold, i / 8f), 0.4f, 15);
            }
            ThemedParticles.SakuraPetals(position, 30, 150f);
            Phase10BossVFX.TimpaniDrumrollImpact(position, Gold, 1.5f);
            BossSignatureVFX.EroicaPhoenixDive(position);
        }

        /// <summary>HeroesJudgment: Ultimate radial projectile pattern with heroic fanfare VFX.</summary>
        public static void HeroesJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 12, Gold * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { Gold }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, Scarlet, 1.2f);
        }

        public static void HeroesJudgmentRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(15f);
            CustomParticles.GenericFlare(center, White, 1.2f, 20);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 2 == 0 ? Gold : Scarlet;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, color, 0.4f, 15);
            }
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { Gold, Scarlet, White }, 1.5f);
            BossSignatureVFX.EroicaHeroesJudgment(center, 1, 1, 1.5f);
        }

        /// <summary>UltimateValor: Multi-phase spiral barrage finale.</summary>
        public static void UltimateValorTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 16, Gold);
            Phase10BossVFX.StaffLineConvergence(center, Gold, 1.2f);
            BossVFXOptimizer.WarningFlare(center, 1.0f);
        }

        public static void UltimateValorWave(Vector2 center, int waveIndex)
        {
            float angle = waveIndex * 0.15f;
            for (int arm = 0; arm < 5; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 5f;
                Vector2 pos = center + armAngle.ToRotationVector2() * 40f;
                Color color = Color.Lerp(Gold, Scarlet, arm / 5f);
                CustomParticles.GenericFlare(pos, color, 0.3f, 10);
            }
            ThemedParticles.SakuraPetals(center, 5, 60f);
        }

        public static void UltimateValorFinale(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, White, 2f, 30);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color color = VFXIntegration.GetThemeColor("Eroica", i / 20f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 100f, color, 0.8f, 25);
            }
            Phase10BossVFX.CodaFinale(center, Gold, Scarlet, 2f);
        }

        #endregion
    }
}
