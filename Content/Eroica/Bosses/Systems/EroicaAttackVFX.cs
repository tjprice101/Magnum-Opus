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
    /// Calamity-tier Eroica boss attack choreography system.
    /// Each attack has multi-layered VFX: telegraph → execution → impact,
    /// with sky flash integration, bloom particle cascades, and musical motifs.
    /// Every impact is a layered composition—never a single particle call.
    /// </summary>
    public static class EroicaAttackVFX
    {
        private static readonly Color Gold = new Color(255, 200, 80);
        private static readonly Color Scarlet = new Color(200, 50, 50);
        private static readonly Color Pink = new Color(255, 150, 180);
        private static readonly Color White = new Color(255, 240, 220);
        private static readonly Color EmberOrange = new Color(255, 140, 30);

        #region Core Attack VFX

        /// <summary>SwordDash: Scarlet streak with embedded golden music notes and converging bloom.</summary>
        public static void SwordDashTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 400f, 30, Scarlet * 0.7f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 400f, Gold, 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);

            // Bloom converging particles along the threat line
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = position + direction * (80f * i);
                Vector2 vel = direction * -2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                var bloom = new BloomParticle(pos, vel, Color.Lerp(Gold, Scarlet, i / 5f), 0.2f + i * 0.05f, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        public static void SwordDashTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, Gold, 0.3f, 8);
            if (Main.rand.NextBool(3))
                ThemedParticles.EroicaMusicNotes(position, 1, 30f);
            CustomParticles.EroicaTrail(position, velocity, 0.4f);

            // Ember sparks flying off the dash path
            if (Main.rand.NextBool(2))
            {
                Vector2 perpVel = new Vector2(-velocity.Y, velocity.X).SafeNormalize(Vector2.UnitX);
                Vector2 sparkVel = perpVel * Main.rand.NextFloat(-3f, 3f) + Vector2.UnitY * Main.rand.NextFloat(-1f, 0.5f);
                var spark = new GlowSparkParticle(position + Main.rand.NextVector2Circular(8f, 8f), sparkVel, EmberOrange, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(15, 30));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public static void SwordDashImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            EroicaSkySystem.TriggerScarletFlash(0.4f);

            // Layer 1: Central burst
            CustomParticles.EroicaImpactBurst(position, 8);
            Phase10BossVFX.CymbalCrashBurst(position, 0.8f);

            // Layer 2: Expanding halo ring
            CustomParticles.HaloRing(position, Scarlet, 0.5f, 15);

            // Layer 3: Music notes streaming outward
            ThemedParticles.EroicaMusicNotes(position, 4, 50f);

            // Layer 4: Radial bloom particle shower
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color col = Color.Lerp(Gold, Scarlet, Main.rand.NextFloat());
                var bloom = new BloomParticle(position, vel, col, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(20, 40));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        /// <summary>HeroicBarrage: Streams of golden orbs with scarlet sparkle accents and bloom cascades.</summary>
        public static void HeroicBarrageTelegraph(Vector2 position)
        {
            TelegraphSystem.ConvergingRing(position, 100f, 6, Gold * 0.5f);
            Phase10BossVFX.AccelerandoSpiral(position, Scarlet, 0.5f);

            // Orbiting sparkle particles around the telegraph ring
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.GlobalTimeWrappedHourly * 2f + MathHelper.TwoPi * i / 3f;
                Vector2 orbPos = position + angle.ToRotationVector2() * 60f;
                var sparkle = new SparkleParticle(orbPos, Main.rand.NextVector2Circular(0.5f, 0.5f), Gold, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public static void HeroicBarrageRelease(Vector2 position, int burstIndex)
        {
            float angle = burstIndex * 0.3f;
            Color color = Color.Lerp(Gold, Scarlet, (float)Math.Sin(angle) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.4f, 12);
            BossVFXOptimizer.OptimizedFlare(position, color, 0.3f, 15, 2);

            // Each burst spawns escalating bloom particles
            int particleCount = Math.Min(burstIndex + 1, 4);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                var bloom = new BloomParticle(position, vel, color, 0.2f + burstIndex * 0.03f, Main.rand.Next(15, 30));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Golden flash on every 3rd burst
            if (burstIndex % 3 == 0)
                EroicaSkySystem.TriggerGoldenFlash(0.3f);
        }

        /// <summary>GoldenRain: Projectiles descend like musical notation falling from a staff.</summary>
        public static void GoldenRainTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -200), Gold, 0.8f);
            TelegraphSystem.DangerZone(targetArea, 200f, 30, Gold * 0.3f);

            // Descending sparkle curtain in the danger zone
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = targetArea + new Vector2(Main.rand.NextFloat(-120f, 120f), -200f + Main.rand.NextFloat(-30f, 30f));
                var sparkle = new SparkleParticle(pos, Vector2.UnitY * Main.rand.NextFloat(1f, 2.5f), Gold, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(40, 70));
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public static void GoldenRainParticle(Vector2 position)
        {
            Color color = Color.Lerp(Gold, White, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.25f, 6);
            if (Main.rand.NextBool(4))
                ThemedParticles.EroicaMusicNotes(position, 1, 20f);

            // Small trailing sparkle
            if (Main.rand.NextBool(3))
            {
                var spark = new GlowSparkParticle(position, Vector2.UnitY * Main.rand.NextFloat(0.5f, 1.5f), Gold, 0.12f, Main.rand.Next(10, 20));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        #endregion

        #region Phase 2B Attack VFX (70% HP)

        /// <summary>ValorCross: Cross-pattern projectile stream with golden impact halos and radial bloom.</summary>
        public static void ValorCrossTelegraph(Vector2 center)
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 * i;
                TelegraphSystem.ThreatLine(center, angle.ToRotationVector2(), 500f, 25, Gold * 0.6f);
            }
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { Scarlet }, 0.7f);

            // Pulsing bloom at the cross center
            var bloom = new BloomParticle(center, Vector2.Zero, Gold, 0.6f, 0.3f, 25);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        public static void ValorCrossImpact(Vector2 position)
        {
            CustomParticles.GlyphBurst(position, Gold, 6);
            CustomParticles.HaloRing(position, Gold, 0.6f, 18);
            Phase10BossVFX.ChordResolutionBloom(position, new[] { Gold }, 1.0f);

            // Sky flash and radial bloom particles
            EroicaSkySystem.TriggerGoldenFlash(0.4f);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 4f);
                var bloom = new BloomParticle(position, vel, Color.Lerp(Gold, White, Main.rand.NextFloat(0.3f)), 0.25f, Main.rand.Next(20, 40));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        /// <summary>SakuraStorm: Cascading petals in spiral patterns with pink bloom hazes.</summary>
        public static void SakuraStormBurst(Vector2 center, int waveIndex)
        {
            ThemedParticles.SakuraPetals(center, 8 + waveIndex * 4, 80f + waveIndex * 20f);
            CustomParticles.GenericFlare(center, Pink, 0.5f, 15);
            Phase10BossVFX.NoteConstellationWarning(center, Pink, 0.4f + waveIndex * 0.15f);

            // Pink bloom haze expanding with each wave
            for (int i = 0; i < waveIndex + 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color col = Color.Lerp(Pink, White, Main.rand.NextFloat(0.4f));
                var bloom = new BloomParticle(center + Main.rand.NextVector2Circular(40f, 40f), vel, col, 0.3f + waveIndex * 0.05f, Main.rand.Next(30, 50));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Sky flash on wave 0 (the initial burst)
            if (waveIndex == 0)
                EroicaSkySystem.TriggerAttackFlash(0.35f, Pink);
        }

        /// <summary>TriumphantCharge: Multi-dash with escalating golden afterimages and bloom trails.</summary>
        public static void TriumphantChargeTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, Gold * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, Gold, 0.9f);

            // Bloom build-up at charge origin
            var bloom = new BloomParticle(position, Vector2.Zero, Gold, 0.5f, 0.2f, 30);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        public static void TriumphantChargeAfterimage(Vector2 position, int dashNumber)
        {
            float intensity = 0.3f + dashNumber * 0.15f;
            Color trailColor = Color.Lerp(Scarlet, Gold, dashNumber / 5f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 10);
            CustomParticles.EroicaTrail(position, Vector2.Zero, intensity);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 15, 3);

            // Escalating bloom particles per dash
            int bloomCount = Math.Min(dashNumber + 2, 6);
            for (int i = 0; i < bloomCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                var bloom = new BloomParticle(position, vel, trailColor, 0.15f + dashNumber * 0.04f, Main.rand.Next(15, 30));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Sky flash on later dashes
            if (dashNumber >= 3)
                EroicaSkySystem.TriggerGoldenFlash(0.2f + dashNumber * 0.1f);
        }

        #endregion

        #region Phase 2C Attack VFX (40% HP)

        /// <summary>PhoenixDive: Massive diving attack with phoenix wing fire trail, multi-layered impact.</summary>
        public static void PhoenixDiveTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 800f, 30, new Color(255, 100, 40) * 0.8f);
            TelegraphSystem.ImpactPoint(target, 60f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, Scarlet, 0.8f);

            // Rising ember particles along the dive path
            for (int i = 0; i < 6; i++)
            {
                float t = i / 6f;
                Vector2 pathPos = Vector2.Lerp(position, target, t);
                Vector2 vel = -Vector2.UnitY * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 0.5f);
                var spark = new GlowSparkParticle(pathPos, vel, Color.Lerp(EmberOrange, Gold, t), 0.2f + t * 0.15f, Main.rand.Next(25, 45));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public static void PhoenixDiveImpact(Vector2 position)
        {
            // Layer 1: Screen effects (most impactful attack)
            MagnumScreenEffects.AddScreenShake(20f);
            EroicaSkySystem.TriggerAttackFlash(0.8f, new Color(255, 200, 100));

            // Layer 2: Central explosion burst
            CustomParticles.ExplosionBurst(position, Gold, 15);
            CustomParticles.GenericFlare(position, White, 1.5f, 25);

            // Layer 3: Radial halo rings
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 40f,
                    Color.Lerp(Scarlet, Gold, i / 8f), 0.4f, 15);
            }

            // Layer 4: Sakura petal cascade
            ThemedParticles.SakuraPetals(position, 30, 150f);

            // Layer 5: Musical signature
            Phase10BossVFX.TimpaniDrumrollImpact(position, Gold, 1.5f);
            BossSignatureVFX.EroicaPhoenixDive(position);

            // Layer 6: Bloom particle shower (outward explosion)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                Color col = Color.Lerp(Gold, EmberOrange, Main.rand.NextFloat());
                var bloom = new BloomParticle(position, vel, col, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(25, 50));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Layer 7: Ascending ember sparks (rising heat)
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-8f, -3f));
                var spark = new GlowSparkParticle(position + Main.rand.NextVector2Circular(30f, 30f), vel, EmberOrange, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(30, 60));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        /// <summary>HeroesJudgment: Ultimate radial projectile pattern with heroic fanfare VFX and full bloom composition.</summary>
        public static void HeroesJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 12, Gold * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { Gold }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, Scarlet, 1.2f);

            // Central bloom build-up
            var bloom = new BloomParticle(center, Vector2.Zero, White, 0.8f, 0.3f, 30);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        public static void HeroesJudgmentRelease(Vector2 center)
        {
            // Layer 1: Screen effects
            MagnumScreenEffects.AddScreenShake(15f);
            EroicaSkySystem.TriggerGoldenFlash(0.7f);

            // Layer 2: Central flare
            CustomParticles.GenericFlare(center, White, 1.2f, 20);

            // Layer 3: Alternating gold/scarlet halo ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 2 == 0 ? Gold : Scarlet;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, color, 0.4f, 15);
            }

            // Layer 4: Full musical ensemble burst
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { Gold, Scarlet, White }, 1.5f);
            BossSignatureVFX.EroicaHeroesJudgment(center, 1, 1, 1.5f);

            // Layer 5: Dense bloom particle cascade
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 6f);
                Color col = i % 3 == 0 ? White : (i % 3 == 1 ? Gold : Scarlet);
                var bloom = new BloomParticle(center, vel, col, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(20, 45));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Layer 6: Music notes streaming outward
            ThemedParticles.EroicaMusicNotes(center, 6, 80f);
        }

        /// <summary>UltimateValor: Multi-phase spiral barrage finale — the hero's symphony.</summary>
        public static void UltimateValorTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 16, Gold);
            Phase10BossVFX.StaffLineConvergence(center, Gold, 1.2f);
            BossVFXOptimizer.WarningFlare(center, 1.0f);

            // Dramatic sky flash for the ultimate attack telegraph
            EroicaSkySystem.TriggerGoldenFlash(0.5f);
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

                // Bloom streaks along each arm
                if (Main.rand.NextBool(2))
                {
                    Vector2 vel = armAngle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                    var bloom = new BloomParticle(pos, vel, color, 0.15f, Main.rand.Next(15, 25));
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }
            ThemedParticles.SakuraPetals(center, 5, 60f);

            // Escalating sky flash every 4th wave
            if (waveIndex % 4 == 0)
                EroicaSkySystem.TriggerGoldenFlash(0.2f + waveIndex * 0.02f);
        }

        public static void UltimateValorFinale(Vector2 center)
        {
            // Layer 1: Maximum screen impact
            MagnumScreenEffects.AddScreenShake(25f);
            EroicaSkySystem.TriggerAttackFlash(1f, White);

            // Layer 2: Massive central flare
            CustomParticles.GenericFlare(center, White, 2f, 30);

            // Layer 3: Radial theme-colored bloom ring
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color color = VFXIntegration.GetThemeColor("Eroica", i / 20f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 100f, color, 0.8f, 25);
            }

            // Layer 4: Musical finale
            Phase10BossVFX.CodaFinale(center, Gold, Scarlet, 2f);

            // Layer 5: Dense radial bloom shower
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                Color col = Color.Lerp(Gold, White, Main.rand.NextFloat());
                var bloom = new BloomParticle(center, vel, col, Main.rand.NextFloat(0.3f, 0.8f), Main.rand.Next(30, 60));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Layer 6: Sakura cascade
            ThemedParticles.SakuraPetals(center, 40, 200f);

            // Layer 7: Rising ember column
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-10f, -4f));
                var spark = new GlowSparkParticle(center + Main.rand.NextVector2Circular(40f, 20f), vel, EmberOrange, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(40, 70));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        #endregion
    }
}
