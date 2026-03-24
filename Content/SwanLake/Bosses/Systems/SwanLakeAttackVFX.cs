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
    /// Swan Lake boss attack choreography — 4-phase visual identity.
    ///
    /// Phase 1 (White Swan, >60%): Clean white geometric telegraphs.
    ///         Ballet precision. White arcs on void black. Feathers drift like a sigh.
    /// Phase 2 (Black Swan, 60-45%): Attacks crack — paired white+black.
    ///         Telegraphs split into dual-tone. Prismatic bleed at collision seams.
    /// Phase 3 (Duality War, 45-30%): Attacks feel fractured — rapid alternation.
    ///         Double the particles, alternating colors, monochrome ring impacts.
    /// Enrage (Death, <30%): Attacks drain to gray. Only impacts carry desperate
    ///         prismatic color. Everything else is the ghost of what was.
    /// </summary>
    public static class SwanLakeAttackVFX
    {
        // Core palette — strictly White/Black/Prismatic-at-destruction
        private static readonly Color VoidBlack = new Color(5, 5, 8);
        private static readonly Color PureWhite = new Color(245, 245, 255);
        private static readonly Color SilverMist = new Color(180, 185, 200);
        private static readonly Color GhostWhite = new Color(230, 230, 240);
        private static readonly Color DrainedGray = new Color(100, 100, 105);

        /// <summary>
        /// Prismatic rainbow — reserved for moments of fracture and breaking.
        /// </summary>
        private static Color GetFractureRainbow(float offset = 0f)
        {
            float hue = ((float)Main.timeForVisualEffects * 0.006f + offset) % 1f;
            return Main.hslToRgb(hue, 0.85f, 0.8f);
        }

        /// <summary>HP-based visual phase from boss tracker.</summary>
        private static int GetPhase()
        {
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.SwanLakeFractal);
            if (boss == null) return 1;
            float hpRatio = boss.life / (float)boss.lifeMax;
            if (hpRatio > 0.6f) return 1;
            if (hpRatio > 0.45f) return 2;
            if (hpRatio > 0.3f) return 3;
            return 4;
        }

        /// <summary>Color drain multiplier for Phase 4.</summary>
        private static float GetDrain()
        {
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.SwanLakeFractal);
            if (boss == null) return 1f;
            float hpRatio = boss.life / (float)boss.lifeMax;
            if (hpRatio >= 0.3f) return 1f;
            return MathHelper.Lerp(1f, 0.25f, (0.3f - hpRatio) / 0.3f);
        }

        /// <summary>Drain toward gray.</summary>
        private static Color ApplyDrain(Color c, float drain)
        {
            if (drain >= 1f) return c;
            float gray = (c.R + c.G + c.B) / 3f / 255f;
            return Color.Lerp(new Color(gray, gray, gray + 0.01f), c, drain);
        }

        /// <summary>Phase-aware telegraph color — clean white in Phase 1, split in 2, alternating in 3, gray in 4.</summary>
        private static Color GetTelegraphColor(bool isSecondary = false)
        {
            int phase = GetPhase();
            float drain = GetDrain();
            return phase switch
            {
                1 => PureWhite * 0.6f,
                2 => isSecondary ? VoidBlack * 0.5f : PureWhite * 0.6f,
                3 => ((float)Math.Sin(Main.timeForVisualEffects * 0.05f) > 0) ? PureWhite * 0.6f : VoidBlack * 0.5f,
                _ => ApplyDrain(SilverMist, drain) * 0.4f
            };
        }

        /// <summary>Phase-aware impact color.</summary>
        private static Color GetImpactColor()
        {
            int phase = GetPhase();
            float drain = GetDrain();
            return phase switch
            {
                1 => PureWhite,
                2 => PureWhite,
                3 => ((float)Math.Sin(Main.timeForVisualEffects * 0.06f) > 0) ? PureWhite : VoidBlack,
                _ => ApplyDrain(SilverMist, drain)
            };
        }

        // ===================================================================
        //  GRACEFUL ATTACKS (Phase 1 primary, used in all phases)
        // ===================================================================

        #region Feather Cascade

        /// <summary>FeatherCascade: Waves of feathers descending gracefully.</summary>
        public static void FeatherCascadeTelegraph(Vector2 targetArea)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -200), GetTelegraphColor(), 0.6f);
            TelegraphSystem.DangerZone(targetArea, 200f, 30, GetTelegraphColor() * 0.5f);

            // Phase 2+: secondary black telegraph mirror
            if (phase >= 2 && phase < 4)
            {
                TelegraphSystem.DangerZone(targetArea, 180f, 30, VoidBlack * 0.3f);
            }
        }

        public static void FeatherCascadeParticle(Vector2 position)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // Draining: gray feathers, barely visible
                CustomParticles.SwanFeatherDrift(position, ApplyDrain(SilverMist, drain), 0.25f);
                // Rare desperate prismatic at destruction point
                if (Main.rand.NextBool(8))
                    CustomParticles.PrismaticSparkle(position, GetFractureRainbow(), 0.15f);
            }
            else if (phase == 3)
            {
                // Duality: alternating black/white feathers
                bool isWhiteBeat = Main.rand.NextBool();
                CustomParticles.SwanFeatherDrift(position, isWhiteBeat ? PureWhite : VoidBlack, 0.35f);
            }
            else if (phase == 2)
            {
                // Black swan: white feather + emerging black
                CustomParticles.SwanFeatherDrift(position, PureWhite, 0.3f);
                if (Main.rand.NextBool(3))
                    CustomParticles.SwanFeatherDrift(position + Main.rand.NextVector2Circular(15f, 15f), VoidBlack, 0.25f);
            }
            else
            {
                // White swan: clean white feathers, occasional silver sparkle
                CustomParticles.SwanFeatherDrift(position, PureWhite, 0.35f);
                if (Main.rand.NextBool(4))
                    CustomParticles.PrismaticSparkle(position, SilverMist, 0.2f);
            }
        }

        #endregion

        #region Prismatic Sparkle Ring

        /// <summary>PrismaticSparkleRing: Ring of sparkles expanding outward.</summary>
        public static void PrismaticSparkleRingTelegraph(Vector2 center)
        {
            int phase = GetPhase();
            Color telegraphColor = GetTelegraphColor();
            TelegraphSystem.ConvergingRing(center, 100f, 6, telegraphColor);
            Phase10BossVFX.NoteConstellationWarning(center, telegraphColor, 0.5f);

            // Phase 3: second opposing converging ring
            if (phase == 3)
                TelegraphSystem.ConvergingRing(center, 80f, 6, VoidBlack * 0.4f);
        }

        public static void PrismaticSparkleRingRelease(Vector2 center, int ringIndex)
        {
            int phase = GetPhase();
            float drain = GetDrain();
            int count = 12 + ringIndex * 4;

            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * (60f + ringIndex * 30f);

                if (phase == 4)
                {
                    // Drained sparkles — gray with rare prismatic
                    Color gray = ApplyDrain(GhostWhite, drain);
                    CustomParticles.PrismaticSparkle(pos, gray, 0.2f);
                    if (i % 4 == 0)
                        CustomParticles.PrismaticSparkle(pos, GetFractureRainbow(i / (float)count), 0.12f);
                }
                else if (phase == 3)
                {
                    // Alternating black/white ring
                    Color color = i % 2 == 0 ? PureWhite : VoidBlack;
                    CustomParticles.PrismaticSparkle(pos, color, 0.35f);
                }
                else if (phase == 2)
                {
                    // White ring with prismatic bleed at seams
                    CustomParticles.PrismaticSparkle(pos, PureWhite, 0.3f);
                    if (i % 3 == 0)
                        CustomParticles.PrismaticSparkle(pos, GetFractureRainbow(i / (float)count), 0.15f);
                }
                else
                {
                    // Clean white sparkle ring
                    CustomParticles.PrismaticSparkle(pos, SilverMist, 0.3f);
                }
            }

            Color haloColor = phase == 4 ? DrainedGray : (phase == 3 ? GetImpactColor() : PureWhite);
            CustomParticles.HaloRing(center, haloColor, 0.5f + ringIndex * 0.15f, 18);
        }

        #endregion

        #region Dual Swan Arc Slashes

        /// <summary>DualSwanArcSlashes: Twin arc attacks — the duality made visual.</summary>
        public static void DualSwanArcSlashesTelegraph(Vector2 position, Vector2 dir1, Vector2 dir2)
        {
            int phase = GetPhase();

            if (phase >= 2)
            {
                // Phase 2+: explicit white/black duality
                TelegraphSystem.ThreatLine(position, dir1, 350f, 30, PureWhite * 0.6f);
                TelegraphSystem.ThreatLine(position, dir2, 350f, 30, VoidBlack * 0.5f);
                Phase10BossVFX.CounterpointDuality(position, PureWhite, VoidBlack);
            }
            else
            {
                // Phase 1: both lines pure white — duality hasn't surfaced yet
                TelegraphSystem.ThreatLine(position, dir1, 350f, 30, PureWhite * 0.5f);
                TelegraphSystem.ThreatLine(position, dir2, 350f, 30, GhostWhite * 0.4f);
            }
        }

        public static void DualSwanArcSlashesImpact(Vector2 position, bool isWhite)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            Color impactColor;
            if (phase == 4)
            {
                // Drained gray — but the impact itself carries desperate prismatic
                impactColor = ApplyDrain(SilverMist, drain);
                TriggerWhiteFlash(6f);
                CustomParticles.SwanFeatherBurst(position, 6, 0.3f);
                CustomParticles.HaloRing(position, DrainedGray, 0.35f, 14);

                // Desperate prismatic at the moment of violence
                for (int i = 0; i < 3; i++)
                {
                    var bloom = new BloomParticle(position + Main.rand.NextVector2Circular(10f, 10f),
                        Main.rand.NextVector2Circular(1.5f, 1.5f),
                        GetFractureRainbow(Main.rand.NextFloat()), 0.25f, 10);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }
            else
            {
                impactColor = isWhite ? PureWhite : VoidBlack;
                TriggerWhiteFlash(8f);
                CustomParticles.SwanFeatherBurst(position, 8, 0.4f);
                CustomParticles.HaloRing(position, impactColor, 0.5f, 16);
                ThemedParticles.SwanLakeSparks(position, Vector2.UnitX, 6, 5f);
                BossSignatureVFX.SwanLakeGracefulStrike(position, Vector2.UnitX, 0.8f);
                var bloom = new BloomParticle(position, Vector2.Zero, impactColor, 0.5f, 12);
                MagnumParticleHandler.SpawnParticle(bloom);

                // Phase 3: alternating monochrome ring on impact
                if (phase == 3)
                {
                    Color oppositeColor = isWhite ? VoidBlack : PureWhite;
                    CustomParticles.HaloRing(position, oppositeColor, 0.4f, 14);
                }
            }
        }

        #endregion

        #region Graceful Dash

        /// <summary>GracefulDash: Elegant gliding charge with feather trail.</summary>
        public static void GracefulDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            Color telegraphColor = GetTelegraphColor();
            TelegraphSystem.ThreatLine(position, dir, 500f, 30, telegraphColor);
            Phase10BossVFX.GlissandoSlideWarning(position, target, telegraphColor, 0.5f);
        }

        public static void GracefulDashTrail(Vector2 position, Vector2 velocity)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // Ghost trail — barely visible
                CustomParticles.SwanFeatherTrail(position, velocity, 0.15f);
            }
            else if (phase == 3)
            {
                // Alternating black/white trail segments
                bool isWhite = (int)(Main.timeForVisualEffects * 0.05f) % 2 == 0;
                CustomParticles.SwanFeatherTrail(position, velocity, 0.3f);
                var bloom = new BloomParticle(position, Vector2.Zero, isWhite ? PureWhite : VoidBlack, 0.15f, 8);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            else
            {
                // Clean white trail with occasional silver sparkle
                CustomParticles.SwanFeatherTrail(position, velocity, 0.3f);
                if (Main.rand.NextBool(3))
                    CustomParticles.PrismaticSparkle(position, SilverMist, 0.2f);
            }
        }

        #endregion

        // ===================================================================
        //  TEMPEST ATTACKS (Phase 2-3 primary)
        // ===================================================================

        #region Lightning Fractal Storm

        /// <summary>LightningFractalStorm: Fractal lightning across the arena.</summary>
        public static void LightningFractalStormTelegraph(Vector2 center)
        {
            int phase = GetPhase();
            Color color = GetTelegraphColor();

            TelegraphSystem.ConvergingRing(center, 150f, 10, color);
            Phase10BossVFX.CrescendoDangerRings(center, color, 0.9f, 5);
            Phase10BossVFX.FortissimoFlashWarning(center, color, 1.0f);

            // Phase 3: opposing ring
            if (phase == 3)
                TelegraphSystem.ConvergingRing(center, 130f, 8, VoidBlack * 0.5f);
        }

        public static void LightningFractalStormBolt(Vector2 start, Vector2 end)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            Color boltColor;
            if (phase == 4)
                boltColor = ApplyDrain(GhostWhite, drain);
            else if (phase == 3)
                boltColor = Main.rand.NextBool() ? PureWhite : VoidBlack;
            else
                boltColor = PureWhite;

            Phase10BossVFX.StaffLineLaser(start, end, boltColor, 25f);
            CustomParticles.PrismaticSparkleBurst(start, boltColor, 6);

            // Phase 2+: prismatic sparks at fracture points along bolt
            if (phase >= 2 && phase < 4)
            {
                Vector2 mid = Vector2.Lerp(start, end, 0.5f);
                CustomParticles.PrismaticSparkle(mid, GetFractureRainbow(), 0.2f);
            }
        }

        #endregion

        #region Tempest Dash

        /// <summary>TempestDash: Violent charge — black and white collide.</summary>
        public static void TempestDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            int phase = GetPhase();
            Color color = GetTelegraphColor();

            TelegraphSystem.ThreatLine(position, dir, 600f, 30, color);
            Phase10BossVFX.FortissimoFlashWarning(position, color, 0.9f);

            // Phase 2+: second opposing colored threat line
            if (phase >= 2 && phase < 4)
            {
                Color secondColor = GetTelegraphColor(true);
                TelegraphSystem.ThreatLine(position, dir, 550f, 25, secondColor);
            }
        }

        public static void TempestDashImpact(Vector2 position)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // Drained impact — muted, but prismatic erupts desperately
                TriggerWhiteFlash(6f);
                MagnumScreenEffects.AddScreenShake(8f);
                CustomParticles.SwanFeatherExplosion(position, 8, 0.3f);
                CustomParticles.HaloRing(position, DrainedGray, 0.4f, 14);

                // Desperate prismatic burst — the beauty refusing to die
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f;
                    Vector2 vel = angle.ToRotationVector2() * 2f;
                    var bloom = new BloomParticle(position, vel, GetFractureRainbow(i / 4f), 0.3f, 12);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }
            else
            {
                TriggerPrismaticFlash(10f);
                MagnumScreenEffects.AddScreenShake(12f);
                CustomParticles.SwanFeatherExplosion(position, 15, 0.5f);
                CustomParticles.HaloRing(position, PureWhite, 0.7f, 18);

                // Phase 2+: black counter-ring
                if (phase >= 2)
                    CustomParticles.HaloRing(position, VoidBlack, 0.5f, 20);

                // Bloom burst
                int bloomCount = phase == 3 ? 8 : 6;
                for (int i = 0; i < bloomCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / bloomCount;
                    Vector2 vel = angle.ToRotationVector2() * 3f;
                    Color color = phase == 3
                        ? (i % 2 == 0 ? PureWhite : VoidBlack)
                        : PureWhite;
                    var bloom = new BloomParticle(position, vel, color, 0.4f, 15);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }

                // Phase 2: prismatic at the seam of collision
                if (phase == 2)
                    ThemedParticles.SwanLakeRainbowExplosion(position, 0.6f);
            }
        }

        #endregion

        #region Prismatic Barrage

        /// <summary>PrismaticBarrage: Streams of projectiles — phase-colored.</summary>
        public static void PrismaticBarrageTelegraph(Vector2 position)
        {
            Color color = GetTelegraphColor();
            Phase10BossVFX.AccelerandoSpiral(position, color, 0.7f, 12);
            TelegraphSystem.ConvergingRing(position, 120f, 8, color);
        }

        public static void PrismaticBarrageRelease(Vector2 position, int burstIndex)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            Color color;
            if (phase == 4)
                color = ApplyDrain(SilverMist, drain);
            else if (phase == 3)
                color = burstIndex % 2 == 0 ? PureWhite : VoidBlack;
            else if (phase == 2)
                color = PureWhite;
            else
                color = GhostWhite;

            CustomParticles.GenericFlare(position, color, 0.4f, 14);
            CustomParticles.PrismaticSparkle(position, color, 0.25f);
            BossVFXOptimizer.OptimizedFlare(position, color, 0.3f, 10);

            // Phase 2: faint prismatic at launch point
            if (phase == 2 && burstIndex % 3 == 0)
                CustomParticles.PrismaticSparkle(position, GetFractureRainbow(burstIndex * 0.1f), 0.15f);
        }

        #endregion

        #region Feather Storm

        /// <summary>FeatherStorm: Massive feather burst — black and white together.</summary>
        public static void FeatherStormRelease(Vector2 center)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // Drained storm — muted feathers falling like ash
                TriggerWhiteFlash(5f);
                MagnumScreenEffects.AddScreenShake(5f);
                CustomParticles.SwanFeatherExplosion(center, 10, 0.25f);
                var grayBloom = new BloomParticle(center, Vector2.Zero, DrainedGray, 0.3f, 14);
                MagnumParticleHandler.SpawnParticle(grayBloom);
            }
            else
            {
                TriggerWhiteFlash(8f);
                MagnumScreenEffects.AddScreenShake(8f);
                CustomParticles.SwanFeatherExplosion(center, 20, 0.45f);

                if (phase >= 2)
                    CustomParticles.SwanFeatherDuality(center, 10, 0.4f);

                ThemedParticles.SwanLakeShockwave(center, phase == 3 ? 1.3f : 1.0f);
                var bloom = new BloomParticle(center, Vector2.Zero, GetImpactColor(), 0.6f, 18);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        #endregion

        #region Mirror Dance

        /// <summary>MirrorDance: Mirrored attack from twin positions — duality made physical.</summary>
        public static void MirrorDanceTelegraph(Vector2 pos1, Vector2 pos2)
        {
            int phase = GetPhase();
            Vector2 midpoint = Vector2.Lerp(pos1, pos2, 0.5f);

            if (phase >= 2)
            {
                // Explicit white/black duality
                TelegraphSystem.ThreatLine(pos1, (pos2 - pos1).SafeNormalize(Vector2.UnitX), (pos2 - pos1).Length(), 30, PureWhite * 0.5f);
                TelegraphSystem.ThreatLine(pos2, (pos1 - pos2).SafeNormalize(Vector2.UnitX), (pos2 - pos1).Length(), 30, VoidBlack * 0.4f);
                Phase10BossVFX.CounterpointDuality(midpoint, PureWhite, VoidBlack);
            }
            else
            {
                // Phase 1: clean white line
                TelegraphSystem.ThreatLine(pos1, (pos2 - pos1).SafeNormalize(Vector2.UnitX), (pos2 - pos1).Length(), 30, PureWhite * 0.5f);
            }
        }

        public static void MirrorDanceImpact(Vector2 position)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // Drained mirror impact
                CustomParticles.SwanFeatherBurst(position, 6, 0.2f);
                ThemedParticles.SwanLakeImpact(position, 0.4f);
                // Desperate prismatic flicker
                if (Main.rand.NextBool(2))
                {
                    var bloom = new BloomParticle(position, Vector2.Zero, GetFractureRainbow(), 0.2f, 8);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }
            else
            {
                CustomParticles.SwanFeatherBurst(position, 10, 0.35f);
                ThemedParticles.SwanLakeImpact(position, 0.8f);

                // Phase 3: monochrome ring on impact
                if (phase == 3)
                {
                    bool isWhite = Main.rand.NextBool();
                    CustomParticles.HaloRing(position, isWhite ? PureWhite : VoidBlack, 0.35f, 12);
                }
            }
        }

        #endregion

        // ===================================================================
        //  DYING SWAN ATTACKS (Phase 3-4 primary)
        // ===================================================================

        #region Monochromatic Apocalypse

        /// <summary>MonochromaticApocalypse: The ultimate rotating beam.</summary>
        public static void MonochromaticApocalypseTelegraph(Vector2 center)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // Drained telegraph — ghost of the apocalypse
                TriggerMonochromeFlash(10f);
                TelegraphSystem.ConvergingRing(center, 250f, 12, DrainedGray * 0.5f);
                Phase10BossVFX.FortissimoFlashWarning(center, DrainedGray, 0.8f);
            }
            else
            {
                TriggerMonochromeFlash(15f);
                TelegraphSystem.ConvergingRing(center, 250f, 16, PureWhite);
                Phase10BossVFX.ChordBuildupSpiral(center, new[] { PureWhite, VoidBlack, GetFractureRainbow() }, 0.8f);
                Phase10BossVFX.FortissimoFlashWarning(center, PureWhite, 1.5f);
                BossVFXOptimizer.WarningFlare(center, 1.2f);
            }
        }

        public static void MonochromaticApocalypseBeam(Vector2 origin, float rotation, float length)
        {
            int phase = GetPhase();

            BossSignatureVFX.SwanLakeFractalLaser(origin, rotation, length, phase == 4 ? 0.8f : 1.5f);
            Phase10Integration.SwanLake.MonochromaticApocalypseVFX(origin, rotation, phase == 4 ? 0.6f : 1.2f);
        }

        #endregion

        #region Dying Swan Lament

        /// <summary>DyingSwanLament: Fading feather bursts — the final grace.</summary>
        public static void DyingSwanLamentRelease(Vector2 center)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // The lament itself is draining — barely visible
                TriggerMonochromeFlash(5f);
                CustomParticles.SwanFeatherExplosion(center, 6, 0.2f);
                Phase10BossVFX.DiminuendoFade(center, DrainedGray, 0.3f);

                // But at the moment of release, a single desperate prismatic tear
                var prismBloom = new BloomParticle(center, new Vector2(0, -1f),
                    GetFractureRainbow(), 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(prismBloom);
            }
            else
            {
                TriggerMonochromeFlash(8f);
                CustomParticles.SwanFeatherExplosion(center, 12, 0.35f);
                ThemedParticles.SwanLakeSparkles(center, 10, 50f);
                Phase10BossVFX.DiminuendoFade(center, PureWhite, 0.5f);

                // Ascending sparkle wisps
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1.5f));
                    Color wispColor = phase == 3
                        ? (i % 2 == 0 ? PureWhite : VoidBlack)
                        : GhostWhite;
                    var sparkle = new SparkleParticle(center + Main.rand.NextVector2Circular(30f, 30f), vel, wispColor, 0.3f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }

        #endregion

        #region Final Serenade

        /// <summary>FinalSerenade: The death attack — all-consuming.</summary>
        public static void FinalSerenadeTelegraph(Vector2 center)
        {
            int phase = GetPhase();
            Color telegraphColor = phase == 4 ? DrainedGray : PureWhite;

            TelegraphSystem.ConvergingRing(center, 300f, 20, telegraphColor);
            Phase10BossVFX.StaffLineConvergence(center, telegraphColor, 1.2f);
        }

        public static void FinalSerenadeRelease(Vector2 center)
        {
            // The Final Serenade is always spectacular — even when drained,
            // this is the moment where all suppressed color ERUPTS.
            // The rainbow that was denied throughout the fight breaks free.
            TriggerDeathFlash(20f);
            MagnumScreenEffects.AddScreenShake(25f);

            // Core white flash
            CustomParticles.GenericFlare(center, PureWhite, 1.2f, 30);

            // Prismatic eruption — the swan's true colors breaking through
            ThemedParticles.SwanLakeRainbowExplosion(center, 2.0f);
            CustomParticles.PrismaticSparkleRainbow(center, 20);

            // Massive bloom supernova ring — alternating black/white with prismatic seams
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Color color = i % 3 == 0 ? GetFractureRainbow(i / 16f) : (i % 2 == 0 ? PureWhite : VoidBlack);
                var bloom = new BloomParticle(center, vel, color, 0.7f, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Outer ring — feathers and flares radiating outward
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color color = i % 4 == 0 ? GetFractureRainbow(i / 20f) : (i % 2 == 0 ? PureWhite : VoidBlack);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 100f, color, 0.8f, 25);
                CustomParticles.SwanFeatherDrift(center + angle.ToRotationVector2() * 80f, color, 0.4f);
            }

            Phase10BossVFX.CodaFinale(center, PureWhite, VoidBlack, 2.0f);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { PureWhite, VoidBlack, GetFractureRainbow() }, 2.0f);
            BossSignatureVFX.SwanLakeSerenade(center, 5, 5, 2.0f);
        }

        #endregion

        #region Ghost Swan Dash

        /// <summary>GhostSwanDash: Spectral dash — the ghost of grace.</summary>
        public static void GhostSwanDashTrail(Vector2 position, Vector2 velocity)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // Nearly invisible ghost trail
                CustomParticles.SwanFeatherTrail(position, velocity, 0.1f);
                CustomParticles.GenericGlow(position, DrainedGray * 0.3f, 0.1f, 12);
            }
            else
            {
                Color trailColor = phase == 3
                    ? ((int)(Main.timeForVisualEffects * 0.06f) % 2 == 0 ? GhostWhite : new Color(30, 30, 35))
                    : GhostWhite;
                CustomParticles.SwanFeatherTrail(position, velocity, 0.25f);
                CustomParticles.GenericGlow(position, trailColor * 0.4f, 0.2f, 15);
            }
        }

        #endregion

        #region Shattered Reflection

        /// <summary>ShatteredReflection: Fragments fly outward — the fractal shatters.</summary>
        public static void ShatteredReflectionBurst(Vector2 center, int fragmentCount)
        {
            int phase = GetPhase();
            float drain = GetDrain();

            if (phase == 4)
            {
                // Drained shattering — gray fragments with prismatic bleeding through
                TriggerWhiteFlash(8f);
                for (int i = 0; i < fragmentCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / fragmentCount;
                    Vector2 pos = center + angle.ToRotationVector2() * 50f;
                    Color color = ApplyDrain(i % 2 == 0 ? GhostWhite : new Color(40, 40, 45), drain);
                    CustomParticles.GenericFlare(pos, color, 0.35f, 15);

                    // Every fragment bleeds desperate prismatic
                    var bloom = new BloomParticle(pos, angle.ToRotationVector2() * 1.5f,
                        GetFractureRainbow(i / (float)fragmentCount), 0.2f, 12);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
            }
            else
            {
                TriggerPrismaticFlash(12f);
                for (int i = 0; i < fragmentCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / fragmentCount;
                    Vector2 pos = center + angle.ToRotationVector2() * 50f;
                    Color color = i % 2 == 0 ? PureWhite : VoidBlack;
                    CustomParticles.GenericFlare(pos, color, 0.5f, 18);
                    CustomParticles.SwanFeatherBurst(pos, 3, 0.3f);
                    var bloom = new BloomParticle(pos, angle.ToRotationVector2() * 2f, color, 0.35f, 15);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
                CustomParticles.PrismaticSparkleBurst(center, PureWhite, 12);
            }
        }

        #endregion
    }
}
