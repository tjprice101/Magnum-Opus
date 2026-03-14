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
    /// Movement-aware Eroica boss attack VFX — Beethoven's Third Symphony in combat.
    /// Each attack adapts its visual character to the current movement:
    /// M1 (Call to Arms): Bold, heroic, golden.
    /// M2 (Funeral March): Heavy, somber, crimson smoke and falling petals.
    /// M3 (Scherzo): Electric, staccato, rapid sparks and flickering.
    /// M4 (Apotheosis): Maximum intensity, white-hot, screen-commanding.
    /// </summary>
    public static class EroicaAttackVFX
    {
        // Core palette
        private static readonly Color Gold = new Color(255, 200, 80);
        private static readonly Color Scarlet = new Color(200, 50, 50);
        private static readonly Color Pink = new Color(255, 150, 180);
        private static readonly Color White = new Color(255, 240, 220);
        private static readonly Color EmberOrange = new Color(255, 140, 30);
        private static readonly Color FuneralCrimson = new Color(180, 30, 60);
        private static readonly Color FuneralAsh = new Color(80, 60, 50);

        // Movement state — updated each frame by the boss NPC via UpdateMovement()
        private static float _movement = 1f;
        private static float _heroIntensity = 0f;

        /// <summary>Called once per frame from the boss NPC AI to sync movement state.</summary>
        public static void UpdateMovement(int difficultyTier, bool isEnraged, float aggressionLevel)
        {
            _movement = isEnraged ? 4f : difficultyTier switch { 0 => 1f, 1 => 2f, _ => 3f };
            _heroIntensity = isEnraged ? 1f : MathHelper.Clamp(aggressionLevel * 0.6f + difficultyTier * 0.2f, 0f, 1f);
        }

        /// <summary>Returns the primary attack color shifted by current movement.</summary>
        private static Color GetPrimaryColor()
        {
            if (_movement < 1.5f) return Gold;
            if (_movement < 2.5f) return FuneralCrimson;
            if (_movement < 3.5f) return Scarlet;
            return White;
        }

        /// <summary>Returns the secondary accent color shifted by current movement.</summary>
        private static Color GetSecondaryColor()
        {
            if (_movement < 1.5f) return Scarlet;
            if (_movement < 2.5f) return FuneralAsh;
            if (_movement < 3.5f) return Gold;
            return Gold;
        }

        /// <summary>Returns a movement-scaled intensity multiplier.</summary>
        private static float IntensityMult()
        {
            if (_movement < 1.5f) return 1f;
            if (_movement < 2.5f) return 0.7f;
            if (_movement < 3.5f) return 1.2f;
            return 1.8f;
        }

        /// <summary>Returns movement-scaled screen shake strength.</summary>
        private static float ShakeMult()
        {
            if (_movement < 2.5f) return 1f;
            if (_movement < 3.5f) return 1.3f;
            return 2f;
        }

        #region Core Attack VFX

        /// <summary>SwordDash: Movement-aware scarlet/crimson streak with musical accents.</summary>
        public static void SwordDashTelegraph(Vector2 position, Vector2 direction)
        {
            Color primary = GetPrimaryColor();
            Color secondary = GetSecondaryColor();

            TelegraphSystem.ThreatLine(position, direction, 400f, 30, primary * 0.7f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 400f, primary, 0.6f * IntensityMult());
            BossVFXOptimizer.WarningFlare(position, 0.5f * IntensityMult());

            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = position + direction * (80f * i);
                Vector2 vel = direction * -2f + Main.rand.NextVector2Circular(0.5f, 0.5f);

                if (_movement >= 1.5f && _movement < 2.5f)
                    vel.Y += 1f; // M2: Particles drift downward (funeral weight)

                var bloom = new BloomParticle(pos, vel, Color.Lerp(primary, secondary, i / 5f), 0.2f + i * 0.05f, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // M3: Extra staccato spark bursts along telegraph line
            if (_movement >= 2.5f && _movement < 3.5f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = position + direction * (100f + i * 80f);
                    var spark = new GlowSparkParticle(pos, Main.rand.NextVector2Circular(2f, 2f),
                        i % 2 == 0 ? Gold : Scarlet, 0.15f, Main.rand.Next(8, 15));
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // M4: Double threat line width with white-hot overlay
            if (_movement >= 3.5f)
            {
                TelegraphSystem.ThreatLine(position, direction, 450f, 30, White * 0.4f);
            }
        }

        public static void SwordDashTrail(Vector2 position, Vector2 velocity)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            CustomParticles.GenericFlare(position, primary, 0.3f * iMult, 8);
            if (Main.rand.NextBool(3))
                ThemedParticles.EroicaMusicNotes(position, 1, 30f);
            CustomParticles.EroicaTrail(position, velocity, 0.4f * iMult);

            if (_movement < 2.5f)
            {
                // M1/M2: Ember sparks
                if (Main.rand.NextBool(2))
                {
                    Vector2 perpVel = new Vector2(-velocity.Y, velocity.X).SafeNormalize(Vector2.UnitX);
                    Vector2 sparkVel = perpVel * Main.rand.NextFloat(-3f, 3f) + Vector2.UnitY * Main.rand.NextFloat(-1f, 0.5f);

                    Color sparkColor = _movement < 1.5f ? EmberOrange : FuneralCrimson;
                    var spark = new GlowSparkParticle(position + Main.rand.NextVector2Circular(8f, 8f), sparkVel,
                        sparkColor, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(15, 30));
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                // M2: Lingering smoke particles behind the dash
                if (_movement >= 1.5f && Main.rand.NextBool(2))
                {
                    Vector2 smokeVel = -velocity * 0.1f + Vector2.UnitY * Main.rand.NextFloat(0.2f, 0.8f);
                    var smoke = new BloomParticle(position, smokeVel, FuneralAsh, 0.2f, Main.rand.Next(40, 70));
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
            else if (_movement < 3.5f)
            {
                // M3: Rapid staccato sparks — more frequent, alternating colors
                if (Main.rand.NextBool())
                {
                    Color col = Main.rand.NextBool() ? Gold : Scarlet;
                    Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                    var spark = new GlowSparkParticle(position, sparkVel, col, 0.12f, Main.rand.Next(6, 14));
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            else
            {
                // M4: Dense white-hot ember fountain + bloom cascade
                for (int i = 0; i < 2; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                    Color col = Color.Lerp(White, Gold, Main.rand.NextFloat());
                    var spark = new GlowSparkParticle(position + Main.rand.NextVector2Circular(10f, 10f), sparkVel,
                        col, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(15, 30));
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                Vector2 bloomVel = -velocity * 0.05f;
                var bloom = new BloomParticle(position, bloomVel, White, 0.25f, Main.rand.Next(15, 25));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        public static void SwordDashImpact(Vector2 position)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();
            float sMult = ShakeMult();

            MagnumScreenEffects.AddScreenShake(8f * sMult);

            // Sky flash — movement-aware
            if (_movement < 2.5f)
                EroicaSkySystem.TriggerScarletFlash(0.4f * iMult);
            else if (_movement < 3.5f)
                EroicaSkySystem.TriggerAttackFlash(0.5f, Gold);
            else
                EroicaSkySystem.TriggerAttackFlash(0.8f, White);

            // Layer 1: Central burst
            CustomParticles.EroicaImpactBurst(position, (int)(8 * iMult));
            Phase10BossVFX.CymbalCrashBurst(position, 0.8f * iMult);

            // Layer 2: Halo ring
            CustomParticles.HaloRing(position, primary, 0.5f * iMult, 15);

            // Layer 3: Music notes
            ThemedParticles.EroicaMusicNotes(position, (int)(4 * iMult), 50f);

            // Layer 4: Radial bloom shower — movement-colored
            int bloomCount = _movement < 3.5f ? 6 : 12;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * iMult;
                Color col = Color.Lerp(primary, GetSecondaryColor(), Main.rand.NextFloat());
                var bloom = new BloomParticle(position, vel, col, Main.rand.NextFloat(0.2f, 0.4f) * iMult, Main.rand.Next(20, 40));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // M2: Falling sakura tears on impact
            if (_movement >= 1.5f && _movement < 2.5f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(0.5f, 2f));
                    var petal = new BloomParticle(position + Main.rand.NextVector2Circular(30f, 10f), vel,
                        Color.Lerp(Pink, FuneralCrimson, Main.rand.NextFloat(0.5f)), 0.15f, Main.rand.Next(50, 90));
                    MagnumParticleHandler.SpawnParticle(petal);
                }
            }

            // M4: Additional concentric shockwave rings
            if (_movement >= 3.5f)
            {
                for (int ring = 0; ring < 3; ring++)
                {
                    Color ringColor = Color.Lerp(Gold, White, ring / 3f);
                    CustomParticles.HaloRing(position, ringColor, 0.3f + ring * 0.25f, 12 + ring * 4);
                }
            }
        }

        /// <summary>HeroicBarrage: Movement-aware streams of orbs with bloom cascades.</summary>
        public static void HeroicBarrageTelegraph(Vector2 position)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            TelegraphSystem.ConvergingRing(position, 100f, 6, primary * 0.5f);
            Phase10BossVFX.AccelerandoSpiral(position, GetSecondaryColor(), 0.5f * iMult);

            int orbCount = _movement >= 3.5f ? 5 : 3;
            for (int i = 0; i < orbCount; i++)
            {
                float angle = Main.GlobalTimeWrappedHourly * 2f + MathHelper.TwoPi * i / orbCount;
                Vector2 orbPos = position + angle.ToRotationVector2() * 60f;
                var sparkle = new SparkleParticle(orbPos, Main.rand.NextVector2Circular(0.5f, 0.5f), primary, 0.4f * iMult, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public static void HeroicBarrageRelease(Vector2 position, int burstIndex)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            float angle = burstIndex * 0.3f;
            Color color = Color.Lerp(primary, GetSecondaryColor(), (float)Math.Sin(angle) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.4f * iMult, 12);
            BossVFXOptimizer.OptimizedFlare(position, color, 0.3f * iMult, 15, 2);

            int particleCount = Math.Min(burstIndex + 1, _movement >= 3.5f ? 6 : 4);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f * iMult, 3f * iMult);
                var bloom = new BloomParticle(position, vel, color, 0.2f + burstIndex * 0.03f, Main.rand.Next(15, 30));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // M2: Slow deliberate pacing — lingering bloom on every burst
            if (_movement >= 1.5f && _movement < 2.5f)
            {
                var linger = new BloomParticle(position, Vector2.UnitY * 0.3f, FuneralCrimson, 0.2f, Main.rand.Next(40, 60));
                MagnumParticleHandler.SpawnParticle(linger);
            }

            // M3: Rapid staccato flash on every burst
            if (_movement >= 2.5f && _movement < 3.5f && burstIndex % 2 == 0)
                EroicaSkySystem.TriggerAttackFlash(0.15f, Gold);

            // Sky flash frequency based on movement
            int flashInterval = _movement >= 3.5f ? 2 : 3;
            if (burstIndex % flashInterval == 0)
                EroicaSkySystem.TriggerGoldenFlash(0.3f * iMult);
        }

        /// <summary>GoldenRain: Movement-aware descending projectile notation.</summary>
        public static void GoldenRainTelegraph(Vector2 targetArea)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -200), primary, 0.8f * iMult);
            TelegraphSystem.DangerZone(targetArea, 200f, 30, primary * 0.3f);

            int curtainCount = _movement >= 3.5f ? 8 : 4;
            for (int i = 0; i < curtainCount; i++)
            {
                Vector2 pos = targetArea + new Vector2(Main.rand.NextFloat(-120f, 120f), -200f + Main.rand.NextFloat(-30f, 30f));
                float fallSpeed = _movement >= 1.5f && _movement < 2.5f ? Main.rand.NextFloat(0.5f, 1.5f) : Main.rand.NextFloat(1f, 2.5f);
                var sparkle = new SparkleParticle(pos, Vector2.UnitY * fallSpeed, primary,
                    Main.rand.NextFloat(0.3f, 0.6f) * iMult, Main.rand.Next(40, 70));
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public static void GoldenRainParticle(Vector2 position)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            Color color = Color.Lerp(primary, White, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.25f * iMult, 6);
            if (Main.rand.NextBool(4))
                ThemedParticles.EroicaMusicNotes(position, 1, 20f);

            if (Main.rand.NextBool(3))
            {
                float yVel = _movement >= 1.5f && _movement < 2.5f ? Main.rand.NextFloat(0.8f, 2f) : Main.rand.NextFloat(0.5f, 1.5f);
                var spark = new GlowSparkParticle(position, Vector2.UnitY * yVel, primary, 0.12f, Main.rand.Next(10, 20));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        #endregion

        #region Phase 2B Attack VFX (70% HP — M2+)

        /// <summary>ValorCross: Movement-aware cross-pattern with radial bloom.</summary>
        public static void ValorCrossTelegraph(Vector2 center)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 * i;
                TelegraphSystem.ThreatLine(center, angle.ToRotationVector2(), 500f, 25, primary * 0.6f);
            }
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { GetSecondaryColor() }, 0.7f * iMult);

            var bloom = new BloomParticle(center, Vector2.Zero, primary, 0.6f * iMult, 0.3f, 25);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        public static void ValorCrossImpact(Vector2 position)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();
            float sMult = ShakeMult();

            CustomParticles.GlyphBurst(position, primary, (int)(6 * iMult));
            CustomParticles.HaloRing(position, primary, 0.6f * iMult, 18);
            Phase10BossVFX.ChordResolutionBloom(position, new[] { primary }, 1.0f * iMult);

            EroicaSkySystem.TriggerGoldenFlash(0.4f * iMult);
            MagnumScreenEffects.AddScreenShake(6f * sMult);

            int bloomCount = _movement >= 3.5f ? 14 : 8;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 4f) * iMult;
                var bloom = new BloomParticle(position, vel, Color.Lerp(primary, White, Main.rand.NextFloat(0.3f)),
                    0.25f * iMult, Main.rand.Next(20, 40));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        /// <summary>SakuraStorm: Movement-aware cascading petals with bloom hazes.</summary>
        public static void SakuraStormBurst(Vector2 center, int waveIndex)
        {
            float iMult = IntensityMult();

            // Petal color shifts per movement
            Color petalColor;
            if (_movement < 2.5f)
                petalColor = Pink;
            else if (_movement < 3.5f)
                petalColor = Color.Lerp(Pink, EmberOrange, 0.3f); // Ignited petals
            else
                petalColor = Color.Lerp(Pink, White, 0.5f); // Blazing white-pink

            ThemedParticles.SakuraPetals(center, (int)((8 + waveIndex * 4) * iMult), 80f + waveIndex * 20f);
            CustomParticles.GenericFlare(center, petalColor, 0.5f * iMult, 15);
            Phase10BossVFX.NoteConstellationWarning(center, petalColor, (0.4f + waveIndex * 0.15f) * iMult);

            for (int i = 0; i < (int)((waveIndex + 2) * iMult); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                if (_movement >= 2.5f && _movement < 3.5f)
                    vel *= 2f; // M3: Chaotic whirlwind

                Color col = Color.Lerp(petalColor, White, Main.rand.NextFloat(0.4f));
                var bloom = new BloomParticle(center + Main.rand.NextVector2Circular(40f, 40f), vel, col,
                    0.3f + waveIndex * 0.05f, Main.rand.Next(30, 50));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // M2: Extra falling petal tears
            if (_movement >= 1.5f && _movement < 2.5f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(0.5f, 2f));
                    var tear = new BloomParticle(center + Main.rand.NextVector2Circular(50f, 20f), vel,
                        Color.Lerp(Pink, FuneralCrimson, Main.rand.NextFloat(0.5f)), 0.12f, Main.rand.Next(60, 100));
                    MagnumParticleHandler.SpawnParticle(tear);
                }
            }

            if (waveIndex == 0)
                EroicaSkySystem.TriggerAttackFlash(0.35f * iMult, petalColor);
        }

        /// <summary>TriumphantCharge: Movement-aware multi-dash with bloom trails.</summary>
        public static void TriumphantChargeTelegraph(Vector2 position, Vector2 target)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, primary * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, primary, 0.9f * iMult);

            var bloom = new BloomParticle(position, Vector2.Zero, primary, 0.5f * iMult, 0.2f, 30);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        public static void TriumphantChargeAfterimage(Vector2 position, int dashNumber)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            float intensity = (0.3f + dashNumber * 0.15f) * iMult;
            Color trailColor = Color.Lerp(primary, GetSecondaryColor(), dashNumber / 5f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 10);
            CustomParticles.EroicaTrail(position, Vector2.Zero, intensity);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 15, 3);

            int bloomCount = Math.Min(dashNumber + 2, _movement >= 3.5f ? 10 : 6);
            for (int i = 0; i < bloomCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f * iMult, 3f * iMult);
                var bloom = new BloomParticle(position, vel, trailColor, 0.15f + dashNumber * 0.04f, Main.rand.Next(15, 30));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // M2: Lingering smoke behind each dash
            if (_movement >= 1.5f && _movement < 2.5f)
            {
                var smoke = new BloomParticle(position, Vector2.UnitY * 0.3f, FuneralAsh, 0.18f, Main.rand.Next(50, 80));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            if (dashNumber >= (_movement >= 3.5f ? 2 : 3))
                EroicaSkySystem.TriggerGoldenFlash((0.2f + dashNumber * 0.1f) * iMult);
        }

        #endregion

        #region Phase 2C Attack VFX (40% HP — M3+)

        /// <summary>PhoenixDive: Movement-aware massive diving attack with multi-layered impact.</summary>
        public static void PhoenixDiveTelegraph(Vector2 position, Vector2 target)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 800f, 30, primary * 0.8f);
            TelegraphSystem.ImpactPoint(target, 60f * iMult, 30);
            Phase10BossVFX.CrescendoDangerRings(target, primary, 0.8f * iMult);

            int pathCount = _movement >= 3.5f ? 10 : 6;
            for (int i = 0; i < pathCount; i++)
            {
                float t = i / (float)pathCount;
                Vector2 pathPos = Vector2.Lerp(position, target, t);
                Vector2 vel = -Vector2.UnitY * Main.rand.NextFloat(1f, 3f) * iMult + Main.rand.NextVector2Circular(1f, 0.5f);
                Color col = Color.Lerp(EmberOrange, primary, t);
                var spark = new GlowSparkParticle(pathPos, vel, col, (0.2f + t * 0.15f) * iMult, Main.rand.Next(25, 45));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // M4: White-hot secondary threat line
            if (_movement >= 3.5f)
                TelegraphSystem.ThreatLine(position, dir, 850f, 30, White * 0.5f);
        }

        public static void PhoenixDiveImpact(Vector2 position)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();
            float sMult = ShakeMult();

            // Layer 1: Screen effects
            MagnumScreenEffects.AddScreenShake(20f * sMult);
            EroicaSkySystem.TriggerAttackFlash(0.8f * iMult, _movement >= 3.5f ? White : new Color(255, 200, 100));

            // Layer 2: Central explosion
            CustomParticles.ExplosionBurst(position, primary, (int)(15 * iMult));
            CustomParticles.GenericFlare(position, White, 1.5f * iMult, 25);

            // Layer 3: Radial halo rings
            int ringCount = _movement >= 3.5f ? 12 : 8;
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount;
                Color ringColor = Color.Lerp(primary, GetSecondaryColor(), i / (float)ringCount);
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 40f * iMult, ringColor, 0.4f * iMult, 15);
            }

            // Layer 4: Sakura petals — movement-aware behavior
            if (_movement < 3.5f)
            {
                ThemedParticles.SakuraPetals(position, 30, 150f);
            }
            else
            {
                // M4: Blazing petal explosion
                ThemedParticles.SakuraPetals(position, 50, 200f);
            }

            // Layer 5: Musical signature
            Phase10BossVFX.TimpaniDrumrollImpact(position, primary, 1.5f * iMult);
            BossSignatureVFX.EroicaPhoenixDive(position);

            // Layer 6: Bloom particle shower
            int bloomCount = _movement >= 3.5f ? 20 : 12;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f) * iMult;
                Color col = Color.Lerp(primary, EmberOrange, Main.rand.NextFloat());
                var bloom = new BloomParticle(position, vel, col, Main.rand.NextFloat(0.3f, 0.7f) * iMult, Main.rand.Next(25, 50));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Layer 7: Ascending ember sparks
            int sparkCount = _movement >= 3.5f ? 14 : 8;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-4f, 4f) * iMult, Main.rand.NextFloat(-8f, -3f) * iMult);
                Color col = _movement >= 3.5f ? Color.Lerp(White, Gold, Main.rand.NextFloat()) : EmberOrange;
                var spark = new GlowSparkParticle(position + Main.rand.NextVector2Circular(30f, 30f), vel, col,
                    Main.rand.NextFloat(0.2f, 0.4f) * iMult, Main.rand.Next(30, 60));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // M4: Extra concentric shockwave rings
            if (_movement >= 3.5f)
            {
                for (int ring = 0; ring < 4; ring++)
                {
                    Color ringColor = Color.Lerp(White, Gold, ring / 4f);
                    CustomParticles.HaloRing(position, ringColor, 0.4f + ring * 0.3f, 15 + ring * 5);
                }
            }
        }

        /// <summary>HeroesJudgment: Movement-aware ultimate radial pattern with full bloom composition.</summary>
        public static void HeroesJudgmentTelegraph(Vector2 center)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            TelegraphSystem.ConvergingRing(center, 200f, 12, primary * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { primary }, 1.0f * iMult);
            Phase10BossVFX.FortissimoFlashWarning(center, GetSecondaryColor(), 1.2f * iMult);

            var bloom = new BloomParticle(center, Vector2.Zero, White, 0.8f * iMult, 0.3f, 30);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        public static void HeroesJudgmentRelease(Vector2 center)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();
            float sMult = ShakeMult();

            // Layer 1: Screen effects
            MagnumScreenEffects.AddScreenShake(15f * sMult);
            EroicaSkySystem.TriggerGoldenFlash(0.7f * iMult);

            // Layer 2: Central flare
            CustomParticles.GenericFlare(center, White, 1.2f * iMult, 20);

            // Layer 3: Movement-colored halo ring
            int ringCount = _movement >= 3.5f ? 16 : 12;
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount;
                Color color = i % 2 == 0 ? primary : GetSecondaryColor();
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f * iMult, color, 0.4f * iMult, 15);
            }

            // Layer 4: Full musical ensemble
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { primary, GetSecondaryColor(), White }, 1.5f * iMult);
            BossSignatureVFX.EroicaHeroesJudgment(center, 1, 1, 1.5f);

            // Layer 5: Dense bloom cascade
            int bloomCount = _movement >= 3.5f ? 24 : 16;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 6f) * iMult;
                Color col = i % 3 == 0 ? White : (i % 3 == 1 ? primary : GetSecondaryColor());
                var bloom = new BloomParticle(center, vel, col, Main.rand.NextFloat(0.2f, 0.5f) * iMult, Main.rand.Next(20, 45));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Layer 6: Music notes
            ThemedParticles.EroicaMusicNotes(center, (int)(6 * iMult), 80f);
        }

        /// <summary>UltimateValor: Movement-aware multi-phase spiral barrage finale.</summary>
        public static void UltimateValorTelegraph(Vector2 center)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            TelegraphSystem.ConvergingRing(center, 250f, 16, primary);
            Phase10BossVFX.StaffLineConvergence(center, primary, 1.2f * iMult);
            BossVFXOptimizer.WarningFlare(center, 1.0f * iMult);

            EroicaSkySystem.TriggerGoldenFlash(0.5f * iMult);
        }

        public static void UltimateValorWave(Vector2 center, int waveIndex)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();

            float angle = waveIndex * 0.15f;
            int armCount = _movement >= 3.5f ? 7 : 5;
            for (int arm = 0; arm < armCount; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / armCount;
                Vector2 pos = center + armAngle.ToRotationVector2() * 40f;
                Color color = Color.Lerp(primary, GetSecondaryColor(), arm / (float)armCount);
                CustomParticles.GenericFlare(pos, color, 0.3f * iMult, 10);

                if (Main.rand.NextBool(2))
                {
                    Vector2 vel = armAngle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f) * iMult;
                    var bloom = new BloomParticle(pos, vel, color, 0.15f * iMult, Main.rand.Next(15, 25));
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }

            ThemedParticles.SakuraPetals(center, (int)(5 * iMult), 60f);

            int flashInterval = _movement >= 3.5f ? 2 : 4;
            if (waveIndex % flashInterval == 0)
                EroicaSkySystem.TriggerGoldenFlash((0.2f + waveIndex * 0.02f) * iMult);
        }

        public static void UltimateValorFinale(Vector2 center)
        {
            Color primary = GetPrimaryColor();
            float iMult = IntensityMult();
            float sMult = ShakeMult();

            // Layer 1: Maximum screen impact
            MagnumScreenEffects.AddScreenShake(25f * sMult);
            EroicaSkySystem.TriggerAttackFlash(1f * iMult, _movement >= 3.5f ? White : primary);

            // Layer 2: Massive central flare
            CustomParticles.GenericFlare(center, White, 2f * iMult, 30);

            // Layer 3: Radial theme-colored bloom ring
            int ringCount = _movement >= 3.5f ? 28 : 20;
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount;
                Color color = Color.Lerp(primary, White, i / (float)ringCount);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 100f * iMult, color, 0.8f * iMult, 25);
            }

            // Layer 4: Musical finale
            Phase10BossVFX.CodaFinale(center, primary, GetSecondaryColor(), 2f * iMult);

            // Layer 5: Dense radial bloom shower
            int bloomCount = _movement >= 3.5f ? 30 : 20;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f) * iMult;
                Color col = Color.Lerp(primary, White, Main.rand.NextFloat());
                var bloom = new BloomParticle(center, vel, col, Main.rand.NextFloat(0.3f, 0.8f) * iMult, Main.rand.Next(30, 60));
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Layer 6: Sakura cascade
            ThemedParticles.SakuraPetals(center, (int)(40 * iMult), 200f);

            // Layer 7: Rising ember column
            int sparkCount = _movement >= 3.5f ? 16 : 10;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-10f, -4f) * iMult);
                Color col = _movement >= 3.5f ? Color.Lerp(White, Gold, Main.rand.NextFloat()) : EmberOrange;
                var spark = new GlowSparkParticle(center + Main.rand.NextVector2Circular(40f, 20f), vel,
                    col, Main.rand.NextFloat(0.3f, 0.6f) * iMult, Main.rand.Next(40, 70));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // M4: Extra screen flash and concentric rings
            if (_movement >= 3.5f)
            {
                EroicaSkySystem.TriggerGoldenFlash(1.0f);
                for (int ring = 0; ring < 5; ring++)
                {
                    Color ringColor = Color.Lerp(White, Gold, ring / 5f);
                    CustomParticles.HaloRing(center, ringColor, 0.5f + ring * 0.3f, 18 + ring * 5);
                }
            }
        }

        #endregion
    }
}
