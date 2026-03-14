using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Screen;
using GodRayStyle = MagnumOpus.Common.Systems.VFX.GodRaySystem.GodRayStyle;
using StrongBloomParticle = MagnumOpus.Common.Systems.VFX.StrongBloomParticle;

namespace MagnumOpus.Content.DiesIrae.Bosses.Systems
{
    /// <summary>
    /// HERALD OF JUDGEMENT — Complete VFX System (Redesigned from scratch)
    /// 
    /// DIES IRAE — THE DAY OF WRATH. Verdi's apocalyptic fury made real.
    /// 
    /// Phase 1: THE SUMMONING (>70% HP)
    ///   Blood-red sky, cracks of ember-orange light split the ground,
    ///   burning scripture scrolling across the arena floor as attack telegraphs,
    ///   concentric gavel-impact rings on each strike like a judge's verdict.
    ///   
    /// Phase 2: THE TRIAL (70-40% HP)
    ///   Fire pillars erupt with lingering heat distortion,
    ///   rain-of-fire descends from crimson sky leaving burning crater glows,
    ///   ember chains link attack sequences like a sentence being pronounced.
    ///   
    /// Phase 3: THE VERDICT (<40% HP)
    ///   Arena becomes a lake of fire with cooled obsidian platforms,
    ///   boss hovers wreathed in apocalyptic flame wings,
    ///   judgment beams sear across arena with screen-burn afterimage effects.
    ///   
    /// Enrage: THE EXECUTION
    ///   Screen pulses red with boss's heartbeat,
    ///   relentless fire columns leave permanent burn marks,
    ///   sky rains ash and ember, every hit sprays blood-red fountains
    ///   as if the world itself is bleeding.
    ///   
    /// Palette: blood red, dark crimson, ember orange, ash black, judgmental white.
    /// </summary>
    public static class HeraldOfJudgementVFX
    {
        #region Palette

        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color DarkCrimson = new Color(100, 10, 10);
        private static readonly Color EmberOrange = new Color(255, 100, 20);
        private static readonly Color AshBlack = new Color(30, 25, 20);
        private static readonly Color JudgmentWhite = new Color(255, 240, 230);
        private static readonly Color HellfireGold = new Color(255, 180, 50);
        private static readonly Color Crimson = new Color(200, 30, 30);
        private static readonly Color DeepBlood = new Color(80, 5, 5);
        private static readonly Color BrimstoneOrange = new Color(255, 140, 40);
        private static readonly Color MoltenCore = new Color(255, 220, 160);
        private static readonly Color SearingWhite = new Color(255, 250, 240);

        private static Color RandomFlameColor()
        {
            return Main.rand.Next(6) switch
            {
                0 => BloodRed,
                1 => EmberOrange,
                2 => Crimson,
                3 => HellfireGold,
                4 => BrimstoneOrange,
                _ => DarkCrimson
            };
        }

        /// <summary>
        /// Flame gradient: maps 0..1 from dark core to white-hot tip.
        /// </summary>
        private static Color FlameGradient(float t)
        {
            if (t < 0.25f) return Color.Lerp(DarkCrimson, BloodRed, t * 4f);
            if (t < 0.5f) return Color.Lerp(BloodRed, EmberOrange, (t - 0.25f) * 4f);
            if (t < 0.75f) return Color.Lerp(EmberOrange, HellfireGold, (t - 0.5f) * 4f);
            return Color.Lerp(HellfireGold, MoltenCore, (t - 0.75f) * 4f);
        }

        #endregion

        #region Phase-Agnostic Core Effects

        /// <summary>
        /// Charge-up effect — converging ember particles with escalating multi-scale bloom core.
        /// The buildup before divine judgment. Particles converge in a double-helix spiral,
        /// the core pulses like a wrathful heartbeat, and at peak charge a danger halo erupts.
        /// </summary>
        public static void ChargeUp(Vector2 center, float progress, float intensity)
        {
            progress = MathHelper.Clamp(progress, 0f, 1f);
            float scaledIntensity = intensity * progress;

            // === Multi-scale bloom core (3 layers: white-hot → crimson → blood) ===
            Color coreColor = Color.Lerp(DarkCrimson, SearingWhite, progress * 0.7f);
            coreColor.A = 0;
            float corePulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.15f * progress;
            var coreInner = new StrongBloomParticle(center, Vector2.Zero, coreColor,
                0.25f * scaledIntensity * corePulse, 6);
            MagnumParticleHandler.SpawnParticle(coreInner);

            Color midColor = Color.Lerp(Crimson, EmberOrange, progress * 0.4f);
            midColor.A = 0;
            var coreMid = new BloomParticle(center, Vector2.Zero, midColor,
                0.4f * scaledIntensity * corePulse, 0.6f * scaledIntensity * corePulse, 8, true);
            MagnumParticleHandler.SpawnParticle(coreMid);

            Color outerColor = BloodRed * (0.5f + progress * 0.5f);
            outerColor.A = 0;
            var coreOuter = new BloomParticle(center, Vector2.Zero, outerColor,
                0.6f * scaledIntensity, 0.9f * scaledIntensity, 10, true);
            MagnumParticleHandler.SpawnParticle(coreOuter);

            // === Double-helix converging spiral ===
            int convergeCount = (int)(8 * intensity * progress);
            float spiralTime = (float)Main.timeForVisualEffects * 0.06f;
            for (int i = 0; i < convergeCount; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / convergeCount;
                float radius = 140f * (1f - progress * 0.75f) * intensity;

                // Two helix arms offset by Pi
                for (int arm = 0; arm < 2; arm++)
                {
                    float angle = baseAngle + spiralTime + arm * MathHelper.Pi;
                    Vector2 spawnPos = center + angle.ToRotationVector2() * radius;
                    Vector2 vel = (center - spawnPos).SafeNormalize(Vector2.Zero) * (3f + progress * 6f);

                    Color emberColor = Color.Lerp(EmberOrange, HellfireGold, Main.rand.NextFloat());
                    emberColor.A = 0;
                    var ember = new SparkleParticle(spawnPos, vel, emberColor,
                        0.12f * intensity, 10 + Main.rand.Next(6));
                    MagnumParticleHandler.SpawnParticle(ember);
                }
            }

            // === Comet-trail streaks converging at high charge ===
            if (progress > 0.4f && Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = 180f * (1f - progress * 0.5f);
                Vector2 cometStart = center + angle.ToRotationVector2() * dist;
                Vector2 cometVel = (center - cometStart).SafeNormalize(Vector2.Zero) * (5f + progress * 8f);
                Color cometCore = Color.Lerp(HellfireGold, MoltenCore, progress);
                cometCore.A = 0;
                var comet = new CometParticle(cometStart, cometVel, cometCore, EmberOrange with { A = 0 },
                    0.08f * intensity, 12, 2f);
                MagnumParticleHandler.SpawnParticle(comet);
            }

            // === Pulsing danger halo with god rays at peak ===
            if (progress > 0.5f)
            {
                float haloPulse = (float)Math.Sin(Main.timeForVisualEffects * 0.1f) * 0.3f + 0.7f;
                Color haloColor = Color.Lerp(BloodRed, EmberOrange, haloPulse);
                haloColor.A = 0;
                var halo = new BloomRingParticle(center, Vector2.Zero, haloColor,
                    0.2f * scaledIntensity, 10, 0.1f * intensity);
                MagnumParticleHandler.SpawnParticle(halo);
            }

            // === God ray burst at near-full charge ===
            if (progress > 0.85f && Main.rand.NextBool(6))
            {
                GodRaySystem.CreateBurst(center, EmberOrange, 8, 100f * intensity, 12, GodRayStyle.Pulsing, HellfireGold);
            }

            Lighting.AddLight(center, EmberOrange.ToVector3() * scaledIntensity * 0.8f);
        }

        /// <summary>
        /// Fire impact — 5-layer bloom-stacked explosion with concentric rings,
        /// radial ember sparks, streak particles, and a heavy smoke plume.
        /// The visual gavel-strike of divine punishment.
        /// </summary>
        public static void FireImpact(Vector2 position, float intensity)
        {
            // === Layer 1: Searing white-hot core flash ===
            var coreFlash = new StrongBloomParticle(position, Vector2.Zero, SearingWhite with { A = 0 },
                0.5f * intensity, 10);
            MagnumParticleHandler.SpawnParticle(coreFlash);

            // === Layer 2: Molten inner glow ===
            var moltenLayer = new BloomParticle(position, Vector2.Zero, MoltenCore with { A = 0 },
                0.6f * intensity, 0.9f * intensity, 14, true);
            MagnumParticleHandler.SpawnParticle(moltenLayer);

            // === Layer 3: Crimson mid-layer ===
            var midLayer = new BloomParticle(position, Vector2.Zero, Crimson with { A = 0 },
                0.8f * intensity, 1.3f * intensity, 18, true);
            MagnumParticleHandler.SpawnParticle(midLayer);

            // === Layer 4: Blood-red outer glow ===
            var outerGlow = new BloomParticle(position, Vector2.Zero, BloodRed with { A = 0 },
                1.0f * intensity, 1.8f * intensity, 24, true);
            MagnumParticleHandler.SpawnParticle(outerGlow);

            // === Layer 5: Deep blood ambient halo ===
            var ambientHalo = new BloomParticle(position, Vector2.Zero, DeepBlood with { A = 0 },
                1.4f * intensity, 2.2f * intensity, 30, true);
            MagnumParticleHandler.SpawnParticle(ambientHalo);

            // === Concentric impact rings — 4 staggered rings ===
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = FlameGradient(1f - ring / 4f);
                ringColor.A = 0;
                var impactRing = new BloomRingParticle(position, Vector2.Zero, ringColor,
                    0.2f * intensity + ring * 0.12f, 16 + ring * 5, 0.13f * intensity);
                MagnumParticleHandler.SpawnParticle(impactRing);
            }

            // === Radial ember sparks — fast ejecting points ===
            int sparkCount = (int)(14 * intensity);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = Main.rand.NextFloat(3f, 10f) * intensity;
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color sparkColor = RandomFlameColor();
                sparkColor.A = 0;
                var spark = new SparkleParticle(position, vel, sparkColor,
                    0.18f * intensity, 16 + Main.rand.Next(10));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // === Directional streak particles — fast radial lines ===
            int streakCount = (int)(6 * intensity);
            for (int i = 0; i < streakCount; i++)
            {
                float angle = MathHelper.TwoPi * i / streakCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f) * intensity;
                Color streakCol = Color.Lerp(HellfireGold, SearingWhite, Main.rand.NextFloat(0.3f));
                streakCol.A = 0;
                var streak = new StreakParticle(position, vel, streakCol,
                    0.15f * intensity, 8 + Main.rand.Next(6));
                MagnumParticleHandler.SpawnParticle(streak);
            }

            // === Heavy smoke plume — ash black smoke rising from impact ===
            for (int i = 0; i < (int)(3 * intensity); i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(1f, 3f));
                var smoke = new HeavySmokeParticle(position + Main.rand.NextVector2Circular(10f, 10f), smokeVel,
                    AshBlack * 0.5f, 0.4f * intensity, 35 + Main.rand.Next(15));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Glowy square debris — shattered ember fragments ===
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                Vector2 debrisVel = Main.rand.NextVector2Circular(5f, 5f) * intensity;
                debrisVel.Y -= Main.rand.NextFloat(1f, 3f);
                Color debrisCol = Color.Lerp(EmberOrange, HellfireGold, Main.rand.NextFloat());
                debrisCol.A = 0;
                var debris = new GlowySquareParticle(position, debrisVel, debrisCol,
                    0.06f * intensity, 20 + Main.rand.Next(10), true);
                MagnumParticleHandler.SpawnParticle(debris);
            }

            // Themed particle burst
            CustomParticles.DiesIraeImpactBurst(position, (int)(8 * intensity));

            Lighting.AddLight(position, HellfireGold.ToVector3() * intensity * 1.2f);
        }

        /// <summary>
        /// Fire trail — multi-layered ember stream with comet tails and smoke wisps.
        /// The burning wake of divine pursuit.
        /// </summary>
        public static void FireTrail(Vector2 position, Vector2 velocity, float intensity)
        {
            Vector2 trailDir = velocity.SafeNormalize(Vector2.Zero) * -1f;

            // === Core flame bloom — white-hot center ===
            if (Main.rand.NextBool(2))
            {
                Color coreCol = Color.Lerp(MoltenCore, HellfireGold, Main.rand.NextFloat(0.3f));
                coreCol.A = 0;
                var core = new BloomParticle(position, trailDir * 0.5f, coreCol,
                    0.1f * intensity, 0.03f, 8, true);
                MagnumParticleHandler.SpawnParticle(core);
            }

            // === Trailing ember bloom particles ===
            int trailCount = (int)(4 * intensity);
            for (int i = 0; i < trailCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f) * intensity;
                Vector2 vel = trailDir * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(1.5f, 1.5f);

                Color trailColor = FlameGradient(Main.rand.NextFloat());
                trailColor.A = 0;
                var trail = new BloomParticle(position + offset, vel, trailColor,
                    0.12f * intensity, 0.04f * intensity, 14 + Main.rand.Next(8), true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // === Comet tail sparks — fast streaking lines of fire ===
            if (Main.rand.NextBool(3))
            {
                Vector2 cometVel = trailDir * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1f, 1f);
                Color cometCol = Color.Lerp(EmberOrange, HellfireGold, Main.rand.NextFloat());
                cometCol.A = 0;
                var comet = new CometParticle(position, cometVel, cometCol, BloodRed with { A = 0 },
                    0.06f * intensity, 10, 1.5f);
                MagnumParticleHandler.SpawnParticle(comet);
            }

            // === Smoke wisps ===
            if (Main.rand.NextBool(4))
            {
                Vector2 smokeVel = trailDir * 0.5f + new Vector2(0, -0.5f);
                var smoke = new GenericGlowParticle(position, smokeVel,
                    AshBlack * 0.3f, 0.08f * intensity, 18 + Main.rand.Next(8));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Hot sparkle at emission point ===
            if (Main.rand.NextBool(2))
            {
                var spark = new SparkleParticle(position, trailDir * 2f + Main.rand.NextVector2Circular(2f, 2f),
                    HellfireGold with { A = 0 }, 0.1f * intensity, 8);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(position, EmberOrange.ToVector3() * intensity * 0.4f);
        }

        /// <summary>
        /// Warning flare — pulsing danger marker with escalating urgency.
        /// The world itself warns of impending judgment.
        /// </summary>
        public static void WarningFlare(Vector2 position, float intensity)
        {
            float pulse = (float)Math.Sin(Main.timeForVisualEffects * 0.15f) * 0.3f + 0.7f;
            float urgentPulse = (float)Math.Sin(Main.timeForVisualEffects * 0.3f) * 0.15f + 0.85f;

            // Double-pulsing bloom
            Color warnColor = Color.Lerp(Crimson, EmberOrange, pulse);
            warnColor.A = 0;
            var flare = new PulsingBloomParticle(position, Vector2.Zero, warnColor, BloodRed with { A = 0 },
                0.2f * intensity * pulse, 6, 0.4f, 0.2f);
            MagnumParticleHandler.SpawnParticle(flare);

            // Danger ring expanding
            if (Main.rand.NextBool(3))
            {
                var ring = new BloomRingParticle(position, Vector2.Zero, EmberOrange with { A = 0 },
                    0.05f * intensity, 8, 0.04f * intensity);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }

        /// <summary>
        /// Death explosion — cataclysmic final detonation with 8-layer bloom cascade,
        /// god ray supernova, radial comet shower, and music notes ascending.
        /// The final chord of the Dies Irae.
        /// </summary>
        public static void DeathExplosion(Vector2 center, float intensity)
        {
            // === Supernova core — blinding white ===
            var nova = new StrongBloomParticle(center, Vector2.Zero, Color.White with { A = 0 },
                2f * intensity, 40);
            MagnumParticleHandler.SpawnParticle(nova);

            // === 3-layer falling bloom cascade ===
            var moltenNova = new BloomParticle(center, Vector2.Zero, MoltenCore with { A = 0 },
                1.5f * intensity, 3f * intensity, 35, true);
            MagnumParticleHandler.SpawnParticle(moltenNova);

            var crimsonNova = new BloomParticle(center, Vector2.Zero, Crimson with { A = 0 },
                2f * intensity, 4f * intensity, 40, true);
            MagnumParticleHandler.SpawnParticle(crimsonNova);

            var bloodNova = new BloomParticle(center, Vector2.Zero, BloodRed with { A = 0 },
                2.5f * intensity, 5f * intensity, 45, true);
            MagnumParticleHandler.SpawnParticle(bloodNova);

            // === God ray supernova burst ===
            GodRaySystem.CreateBurst(center, HellfireGold, 20, 300f * intensity, 50, GodRayStyle.Explosion, SearingWhite);

            // === 10 cascading bloom rings ===
            for (int ring = 0; ring < 10; ring++)
            {
                Color ringColor = FlameGradient(1f - ring / 10f);
                ringColor.A = 0;
                var deathRing = new BloomRingParticle(center, Vector2.Zero, ringColor,
                    0.3f * intensity + ring * 0.3f, 25 + ring * 4, 0.18f * intensity);
                MagnumParticleHandler.SpawnParticle(deathRing);
            }

            // === Massive radial comet shower ===
            int cometCount = (int)(20 * intensity);
            for (int i = 0; i < cometCount; i++)
            {
                float angle = MathHelper.TwoPi * i / cometCount + Main.rand.NextFloat(-0.1f, 0.1f);
                float speed = Main.rand.NextFloat(5f, 15f) * intensity;
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color coreCol = FlameGradient(Main.rand.NextFloat(0.5f, 1f));
                coreCol.A = 0;
                var comet = new CometParticle(center, vel, coreCol, BloodRed with { A = 0 },
                    0.2f * intensity, 25 + Main.rand.Next(15), 2.5f);
                MagnumParticleHandler.SpawnParticle(comet);
            }

            // === Radial streak particles ===
            for (int i = 0; i < (int)(12 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (int)(12 * intensity);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 18f) * intensity;
                var streak = new StreakParticle(center, vel, JudgmentWhite with { A = 0 },
                    0.25f * intensity, 12 + Main.rand.Next(8));
                MagnumParticleHandler.SpawnParticle(streak);
            }

            // === Heavy smoke cloud ===
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f);
                smokeVel.Y -= Main.rand.NextFloat(1f, 3f);
                var smoke = new HeavySmokeParticle(center + Main.rand.NextVector2Circular(30f, 30f), smokeVel,
                    AshBlack * 0.4f, 0.5f * intensity, 45 + Main.rand.Next(20));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Music notes ascending — the wrath concludes ===
            CustomParticles.DiesIraeMusicNotes(center, (int)(10 * intensity), 60f);
            CustomParticles.DiesIraeHellfireBurst(center, (int)(20 * intensity));

            // Screen flash + chromatic aberration
            ScreenFlashSystem.Instance?.Flash(JudgmentWhite, 0.95f * intensity, 30);
            ScreenFlashSystem.Instance?.ChromaticFlash(0.4f * intensity, 20);

            Lighting.AddLight(center, HellfireGold.ToVector3() * 2.5f * intensity);
        }

        /// <summary>
        /// Spawn music note — single themed music note particle rising with ember trail.
        /// </summary>
        public static void SpawnMusicNote(Vector2 position, Vector2 velocity, Color color, float scale)
        {
            CustomParticles.DiesIraeMusicNotes(position, 1, 10f);
            // Add a tiny ember trail to the note
            var ember = new SparkleParticle(position, velocity * 0.5f, EmberOrange with { A = 0 }, 0.05f, 8);
            MagnumParticleHandler.SpawnParticle(ember);
        }

        #endregion

        #region Phase 1: THE SUMMONING (>70% HP)

        /// <summary>
        /// Ground cracks — jagged fracture lines of ember-orange light splitting the earth.
        /// Radial cracks emanate from center like reality breaking under divine pressure.
        /// Each crack segment glows, pulses, and spawns rising embers.
        /// </summary>
        public static void GroundCracks(Vector2 center, float radius, float intensity, int timer)
        {
            int crackCount = 10;
            float rotation = timer * 0.002f;

            for (int i = 0; i < crackCount; i++)
            {
                float angle = MathHelper.TwoPi * i / crackCount + rotation;
                int segments = (int)(radius / 20f);

                for (int seg = 0; seg < segments; seg++)
                {
                    float segProgress = (float)seg / segments;
                    float dist = segProgress * radius;

                    // Jagged crack path with sinusoidal offset
                    float jitter = (float)Math.Sin(seg * 3.7f + i * 5.1f + timer * 0.02f) * 10f;
                    float secondJitter = (float)Math.Cos(seg * 2.3f + i * 7.9f) * 5f;
                    Vector2 perp = new Vector2(-(float)Math.Sin(angle), (float)Math.Cos(angle));
                    Vector2 crackPos = center + angle.ToRotationVector2() * dist + perp * (jitter + secondJitter);

                    // Ember glow at crack point — brighter near center
                    Color crackColor = FlameGradient(1f - segProgress * 0.7f);
                    crackColor.A = 0;
                    float crackScale = (1f - segProgress * 0.5f) * 0.18f * intensity;

                    // Crack glow particles (sparse for performance)
                    if (Main.rand.NextBool(3))
                    {
                        float pulse = (float)Math.Sin(timer * 0.08f + seg * 0.5f) * 0.2f + 0.8f;
                        var crackGlow = new BloomParticle(crackPos, new Vector2(0, -0.2f), crackColor * pulse,
                            crackScale * pulse, crackScale * 0.3f, 6 + Main.rand.Next(4), true);
                        MagnumParticleHandler.SpawnParticle(crackGlow);
                    }

                    // Rising embers from cracks — like hellfire leaking through
                    if (Main.rand.NextBool(10))
                    {
                        Vector2 riseVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(1.5f, 4f));
                        Color emberCol = Color.Lerp(EmberOrange, HellfireGold, Main.rand.NextFloat());
                        emberCol.A = 0;
                        var risingEmber = new CometParticle(crackPos, riseVel, emberCol, BloodRed with { A = 0 },
                            0.06f * intensity, 18 + Main.rand.Next(10), 1.2f);
                        MagnumParticleHandler.SpawnParticle(risingEmber);
                    }

                    // Point lights along main cracks
                    if (seg % 3 == 0)
                        Lighting.AddLight(crackPos, EmberOrange.ToVector3() * intensity * 0.2f * (1f - segProgress));
                }
            }

            // Central crack node — brighter intersection glow
            if (timer % 8 == 0)
            {
                var nodeGlow = new PulsingBloomParticle(center, Vector2.Zero,
                    HellfireGold with { A = 0 }, EmberOrange with { A = 0 },
                    0.2f * intensity, 10, 0.3f, 0.15f);
                MagnumParticleHandler.SpawnParticle(nodeGlow);
            }
        }

        /// <summary>
        /// Burning scripture telegraph — glowing glyph-like patterns scrolling across the arena floor.
        /// Streams of burning text flow along a line like a sentence of divine judgment.
        /// Uses sparkle glyphs with sine-wave drift for organic scripture-scroll feel.
        /// </summary>
        public static void BurningScriptureTelegraph(Vector2 start, Vector2 direction, float length, int duration)
        {
            // Base telegraph warning line
            TelegraphSystem.ThreatLine(start, direction, length, duration, Crimson, 1.2f);
            // Converging ring at the origin
            TelegraphSystem.ConvergingRing(start, 80f, duration, EmberOrange);

            direction = direction.SafeNormalize(Vector2.UnitX);
            Vector2 perpDir = new Vector2(-direction.Y, direction.X);
            int glyphCount = (int)(length / 30f);

            for (int i = 0; i < glyphCount; i++)
            {
                float t = (float)i / glyphCount;
                Vector2 glyphPos = start + direction * (t * length);

                // Sine-wave drift for scrolling scripture motion
                float drift = (float)Math.Sin(t * 8f + Main.timeForVisualEffects * 0.08f) * 12f;
                glyphPos += perpDir * drift;

                if (Main.rand.NextBool(2))
                {
                    Vector2 vel = direction * Main.rand.NextFloat(2f, 5f);
                    Color glyphColor = FlameGradient(t * 0.8f + 0.2f);
                    glyphColor.A = 0;

                    // Main glyph particles — larger, brighter
                    var glyph = new GlowySquareParticle(glyphPos + Main.rand.NextVector2Circular(8f, 4f), vel,
                        glyphColor, 0.07f + Main.rand.NextFloat(0.05f), 12 + Main.rand.Next(8));
                    MagnumParticleHandler.SpawnParticle(glyph);

                    // Ember accent trailing behind glyph
                    if (Main.rand.NextBool(3))
                    {
                        var accent = new SparkleParticle(glyphPos, vel * 0.3f, HellfireGold with { A = 0 },
                            0.04f, 8);
                        MagnumParticleHandler.SpawnParticle(accent);
                    }
                }
            }

            // Impact point marker at endpoint
            Vector2 endpoint = start + direction * length;
            if (Main.rand.NextBool(2))
            {
                TelegraphSystem.ImpactPoint(endpoint, 40f, duration, EmberOrange);
            }
        }

        /// <summary>
        /// Gavel impact rings — concentric expanding rings like the strike of a judge's gavel.
        /// Each ring represents a shockwave of divine pronouncement.
        /// Rings cascade outward with staggered timing, white-hot center flash,
        /// vertical judgment dust forced downward, and god ray burst.
        /// </summary>
        public static void GavelImpactRings(Vector2 position, int ringCount, float intensity)
        {
            // === White-hot center flash — the gavel strikes ===
            var flash = new StrongBloomParticle(position, Vector2.Zero, JudgmentWhite with { A = 0 },
                0.9f * intensity, 10);
            MagnumParticleHandler.SpawnParticle(flash);

            // === God ray burst at impact — radial judgment light ===
            GodRaySystem.CreateBurst(position, HellfireGold, ringCount + 4, 80f * intensity, 15, GodRayStyle.Explosion, JudgmentWhite);

            // === Concentric rings with crimson-to-blood gradient ===
            for (int i = 0; i < ringCount; i++)
            {
                float ringProgress = (float)i / ringCount;
                Color ringColor = FlameGradient(1f - ringProgress);
                ringColor.A = 0;

                var ring = new BloomRingParticle(position, Vector2.Zero, ringColor,
                    0.12f + i * 0.08f, 18 + i * 5, 0.12f * intensity + i * 0.025f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // === Vertical judgment dust — particles forced downward like a gavel blow ===
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(2f, 6f));
                Color dustColor = Color.Lerp(EmberOrange, AshBlack, Main.rand.NextFloat());
                dustColor.A = 0;
                var dust = new GlowySquareParticle(position, vel, dustColor,
                    0.08f * intensity, 14 + Main.rand.Next(8), true);
                MagnumParticleHandler.SpawnParticle(dust);
            }

            // === Horizontal shockwave debris — ember fragments ejected radially ===
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                float angle = MathHelper.TwoPi * i / (int)(6 * intensity);
                Vector2 debrisVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                var debris = new StreakParticle(position, debrisVel, HellfireGold with { A = 0 },
                    0.1f * intensity, 10);
                MagnumParticleHandler.SpawnParticle(debris);
            }

            MagnumScreenEffects.AddScreenShake(5f * intensity);
        }

        /// <summary>
        /// Phase 1 ambient — the summoning atmosphere.
        /// Ground cracks leak ember light, rising embers climb like prayers to a burning sky,
        /// and a pulsing blood-red aura marks the Herald's presence.
        /// </summary>
        public static void SummoningAmbience(Vector2 bossCenter, int timer)
        {
            // === Slow ground crack animation centered below boss ===
            if (timer % 4 == 0)
                GroundCracks(bossCenter + new Vector2(0, 160f), 400f, 0.5f, timer);

            // === Rising ember wisps from below — hellfire seeping through earth ===
            if (timer % 6 == 0)
            {
                Vector2 emberPos = bossCenter + new Vector2(Main.rand.NextFloat(-350f, 350f), 200f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -Main.rand.NextFloat(1.5f, 3f));
                Color col = FlameGradient(Main.rand.NextFloat());
                col.A = 0;
                var ember = new CometParticle(emberPos, vel, col, DarkCrimson with { A = 0 },
                    0.06f + Main.rand.NextFloat(0.04f), 30 + Main.rand.Next(15), 1f);
                MagnumParticleHandler.SpawnParticle(ember);
            }

            // === Pulsing blood-red aura around boss (breathing rhythm) ===
            if (timer % 10 == 0)
            {
                float pulse = (float)Math.Sin(timer * 0.04f) * 0.3f + 0.7f;
                Color auraColor = Color.Lerp(BloodRed, DarkCrimson, pulse);
                auraColor.A = 0;
                var aura = new PulsingBloomParticle(bossCenter, Vector2.Zero, auraColor,
                    Crimson with { A = 0 }, 0.5f * pulse, 12, 0.2f, 0.06f);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // === Slow orbiting ember sparks — sentinels of judgment ===
            if (timer % 15 == 0)
            {
                float orbitAngle = timer * 0.02f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 orbitPos = bossCenter + orbitAngle.ToRotationVector2() * 120f;
                var orbiter = new SparkleParticle(orbitPos, orbitAngle.ToRotationVector2() * 0.5f,
                    EmberOrange with { A = 0 }, 0.08f, 20);
                MagnumParticleHandler.SpawnParticle(orbiter);
            }

            // === Ambient light ===
            float ambientPulse = (float)Math.Sin(timer * 0.03f) * 0.15f + 0.35f;
            Lighting.AddLight(bossCenter, BloodRed.ToVector3() * ambientPulse);
        }

        #endregion

        #region Phase 2: THE TRIAL (70-40% HP)

        /// <summary>
        /// Fire pillar — erupting column of fire at a ground position.
        /// Multi-layered: base eruption flash, ascending flame column with
        /// flickering segments, white-hot spark crown at tip, heavy smoke plume,
        /// and lingering heat distortion shimmer at base.
        /// </summary>
        public static void FirePillar(Vector2 basePosition, float height, float intensity)
        {
            // === Base eruption flash with god rays ===
            FireImpact(basePosition, intensity * 0.7f);
            GodRaySystem.CreateBurst(basePosition, EmberOrange, 6, 60f * intensity, 12, GodRayStyle.Explosion, HellfireGold);

            // === Rising flame column — segment-by-segment with flicker ===
            int segments = (int)(height / 25f);
            for (int seg = 0; seg < segments; seg++)
            {
                float segProgress = (float)seg / segments;
                float widthFalloff = 1f - segProgress * 0.7f;

                // Flickering horizontal offset for organic fire motion
                float flicker = (float)Math.Sin(seg * 2.7f + Main.timeForVisualEffects * 0.08f) * 15f * widthFalloff;
                Vector2 flamePos = basePosition + new Vector2(
                    flicker + Main.rand.NextFloat(-8f, 8f) * widthFalloff,
                    -seg * 25f);

                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -2.5f - segProgress * 4f);
                Color flameColor = FlameGradient(1f - segProgress * 0.8f);
                flameColor.A = 0;
                float scale = widthFalloff * 0.22f * intensity;

                var flame = new BloomParticle(flamePos, vel, flameColor,
                    scale, scale * 0.4f, 10 + Main.rand.Next(6), true);
                MagnumParticleHandler.SpawnParticle(flame);

                // Inner core — white-hot stripe up the center
                if (segProgress < 0.5f && Main.rand.NextBool(2))
                {
                    Color coreCol = Color.Lerp(MoltenCore, HellfireGold, segProgress * 2f);
                    coreCol.A = 0;
                    var core = new BloomParticle(flamePos, vel * 1.2f, coreCol,
                        scale * 0.5f, scale * 0.15f, 8, true);
                    MagnumParticleHandler.SpawnParticle(core);
                }
            }

            // === Pillar tip — white-hot spark crown with streak ejections ===
            Vector2 tipPos = basePosition + new Vector2(0, -height);
            for (int i = 0; i < (int)(5 * intensity); i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f) * intensity;
                sparkVel.Y -= 3f;
                var tipSpark = new CometParticle(tipPos + Main.rand.NextVector2Circular(12f, 12f), sparkVel,
                    MoltenCore with { A = 0 }, EmberOrange with { A = 0 },
                    0.08f * intensity, 12 + Main.rand.Next(6), 1.5f);
                MagnumParticleHandler.SpawnParticle(tipSpark);
            }

            // === Heavy smoke plume billowing from base ===
            for (int i = 0; i < (int)(3 * intensity); i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(0.5f, 2f));
                var smoke = new HeavySmokeParticle(basePosition + Main.rand.NextVector2Circular(20f, 5f), smokeVel,
                    AshBlack * 0.4f, 0.3f * intensity, 40 + Main.rand.Next(20));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Heat distortion at pillar base — shimmer effect ===
            ScreenHeatDistortionSystem.CreateHeatSource(basePosition, 150f * intensity, 0.05f * intensity, 2.5f);

            Lighting.AddLight(basePosition, HellfireGold.ToVector3() * intensity);
            Lighting.AddLight(tipPos, EmberOrange.ToVector3() * intensity * 0.6f);
        }

        /// <summary>
        /// Burning crater glow — smoldering ground mark where fire-rain impacts. 
        /// A molten circle that slowly fades from white-hot to dark crimson,
        /// with rising smoke wisps, heat shimmer, and crackling ember edge ring.
        /// </summary>
        public static void BurningCraterGlow(Vector2 position, float radius)
        {
            float scaleFactor = radius / 80f;

            // === Molten core flash ===
            var craterCore = new StrongBloomParticle(position, Vector2.Zero, MoltenCore with { A = 0 },
                scaleFactor * 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(craterCore);

            // === Crater body — fading ember glow ===
            var crater = new BloomParticle(position, Vector2.Zero, EmberOrange with { A = 0 },
                scaleFactor, scaleFactor * 0.6f, 70, true);
            MagnumParticleHandler.SpawnParticle(crater);

            // === Outer dark crimson ring ===
            var edge = new BloomRingParticle(position, Vector2.Zero, DarkCrimson with { A = 0 },
                scaleFactor * 0.8f, 55, 0.015f);
            MagnumParticleHandler.SpawnParticle(edge);

            // === Crackling ember edge particles — the crater smolders ===
            int edgeParticles = (int)(radius / 15f);
            for (int i = 0; i < edgeParticles; i++)
            {
                float angle = MathHelper.TwoPi * i / edgeParticles;
                Vector2 edgePos = position + angle.ToRotationVector2() * radius * 0.6f;
                if (Main.rand.NextBool(3))
                {
                    Vector2 crackleVel = angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 1.5f);
                    crackleVel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                    var crackle = new SparkleParticle(edgePos, crackleVel,
                        HellfireGold with { A = 0 }, 0.04f, 10 + Main.rand.Next(6));
                    MagnumParticleHandler.SpawnParticle(crackle);
                }
            }

            // === Rising smoke wisps from crater ===
            for (int i = 0; i < 5; i++)
            {
                Vector2 smokePos = position + Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.3f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.5f, 1.5f));
                var smoke = new HeavySmokeParticle(smokePos, vel, AshBlack * 0.35f,
                    0.2f, 35 + Main.rand.Next(15));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Heat shimmer — lingering distortion ===
            ScreenHeatDistortionSystem.CreateHeatSource(position, radius * 1.2f, 0.02f, 3.5f);
        }

        /// <summary>
        /// Ember chain link — connects two positions with a flowing chain of fire.
        /// Particles stream in a sine-wave chain-link pattern with dripping embers
        /// and a pulsing core — like a sentence of judgment being pronounced.
        /// </summary>
        public static void EmberChainLink(Vector2 start, Vector2 end, float intensity)
        {
            Vector2 delta = end - start;
            float length = delta.Length();
            Vector2 dir = delta.SafeNormalize(Vector2.UnitX);
            Vector2 perpDir = new Vector2(-dir.Y, dir.X);
            int segments = (int)(length / 16f);

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(start, end, t);

                // Double-wave chain-link offset — two interlocking sine waves
                float wave1 = (float)Math.Sin(t * MathHelper.TwoPi * 3.5f + Main.timeForVisualEffects * 0.1f) * 14f;
                float wave2 = (float)Math.Cos(t * MathHelper.TwoPi * 2f + Main.timeForVisualEffects * 0.07f) * 8f;
                pos += perpDir * (wave1 + wave2);

                if (Main.rand.NextBool(2))
                {
                    Color linkColor = FlameGradient(t * 0.7f + 0.3f);
                    linkColor.A = 0;
                    var link = new BloomParticle(pos, dir * 0.5f, linkColor,
                        0.07f * intensity, 0.025f, 8 + Main.rand.Next(4), true);
                    MagnumParticleHandler.SpawnParticle(link);
                }

                // Glowy square chain-link nodes at regular intervals
                if (i % 4 == 0)
                {
                    var node = new GlowySquareParticle(pos, dir * 0.3f+  Vector2.UnitY * -0.2f,
                        HellfireGold with { A = 0 }, 0.04f * intensity, 10);
                    MagnumParticleHandler.SpawnParticle(node);
                }

                // Dripping embers falling from chain
                if (Main.rand.NextBool(6))
                {
                    var drip = new CometParticle(pos, new Vector2(0, Main.rand.NextFloat(1.5f, 3f)),
                        EmberOrange with { A = 0 }, BloodRed with { A = 0 }, 0.04f * intensity, 14, 1f);
                    MagnumParticleHandler.SpawnParticle(drip);
                }
            }

            // Chain endpoint glow nodes
            var startNode = new SparkleParticle(start, Vector2.Zero, HellfireGold with { A = 0 }, 0.1f * intensity, 6);
            var endNode = new SparkleParticle(end, Vector2.Zero, HellfireGold with { A = 0 }, 0.1f * intensity, 6);
            MagnumParticleHandler.SpawnParticle(startNode);
            MagnumParticleHandler.SpawnParticle(endNode);
        }

        /// <summary>
        /// Phase 2 ambient — the trial atmosphere.
        /// Lingering heat distortion fills the arena, ember chains pulse outward
        /// like sentences being pronounced, rising flame plumes erupt randomly,
        /// and the boss's aura smolders with increasing fury.
        /// </summary>
        public static void TrialAmbience(Vector2 bossCenter, int timer)
        {
            // === Persistent heat distortion — the trial is sweltering ===
            if (timer % 25 == 0)
            {
                ScreenHeatDistortionSystem.CreateHeatSource(bossCenter, 300f, 0.025f, 1.5f);
            }

            // === Periodic ember chain segments pulsing outward — sentencing ===
            if (timer % 14 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 chainEnd = bossCenter + angle.ToRotationVector2() * Main.rand.NextFloat(180f, 400f);
                EmberChainLink(bossCenter, chainEnd, 0.6f);
            }

            // === Rising flame plumes at random arena positions ===
            if (timer % 18 == 0)
            {
                Vector2 plumePos = bossCenter + new Vector2(Main.rand.NextFloat(-450f, 450f), 200f);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2.5f, 5f));
                    Color col = FlameGradient(Main.rand.NextFloat(0.3f, 0.9f));
                    col.A = 0;
                    var plume = new CometParticle(plumePos, vel, col, DarkCrimson with { A = 0 },
                        0.1f + Main.rand.NextFloat(0.06f), 22 + Main.rand.Next(10), 1.2f);
                    MagnumParticleHandler.SpawnParticle(plume);
                }
            }

            // === Smoldering boss aura — fiercer than Phase 1 ===
            if (timer % 5 == 0)
            {
                Vector2 auraOffset = Main.rand.NextVector2Circular(45f, 45f);
                Color auraColor = FlameGradient(Main.rand.NextFloat(0.4f, 0.8f));
                auraColor.A = 0;
                var aura = new BloomParticle(bossCenter + auraOffset, new Vector2(0, -0.8f), auraColor,
                    0.13f, 0.04f, 15, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // === Smoke wisps rising around arena ===
            if (timer % 12 == 0)
            {
                Vector2 smokePos = bossCenter + new Vector2(Main.rand.NextFloat(-300f, 300f), 150f);
                var smoke = new HeavySmokeParticle(smokePos,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1.5f)),
                    AshBlack * 0.25f, 0.25f, 30 + Main.rand.Next(15));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Ambient light — more intense ===
            float ambientPulse = (float)Math.Sin(timer * 0.04f) * 0.15f + 0.45f;
            Lighting.AddLight(bossCenter, Crimson.ToVector3() * ambientPulse);
        }

        #endregion

        #region Phase 3: THE VERDICT (<40% HP)

        /// <summary>
        /// Lake of fire — arena-wide smoldering effect with flame patches.
        /// Bubbling fire across the arena floor with flame geysers,
        /// rolling smoke banks, localized heat shimmer, and molten flickers.
        /// The floor of hell made manifest.
        /// </summary>
        public static void LakeOfFire(Vector2 arenaCenter, float radius, int timer)
        {
            // === Bubbling fire patches with FlameGradient coloring ===
            if (timer % 3 == 0)
            {
                Vector2 patchPos = arenaCenter + new Vector2(
                    Main.rand.NextFloat(-radius, radius),
                    Main.rand.NextFloat(50f, 200f));

                // Spatial color variation — nearer center = hotter
                float distFromCenter = Math.Abs(patchPos.X - arenaCenter.X) / radius;
                Color fireColor = FlameGradient(1f - distFromCenter * 0.6f);
                fireColor.A = 0;

                var firePatch = new BloomParticle(patchPos, new Vector2(0, -Main.rand.NextFloat(0.3f, 1.2f)), fireColor,
                    0.22f + Main.rand.NextFloat(0.12f), 0.06f, 18 + Main.rand.Next(10), true);
                MagnumParticleHandler.SpawnParticle(firePatch);

                // GlowySquare molten flickers scattered across lake
                if (Main.rand.NextBool(3))
                {
                    var flicker = new GlowySquareParticle(
                        patchPos + Main.rand.NextVector2Circular(30f, 10f),
                        new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.5f, 1.5f)),
                        HellfireGold with { A = 0 }, 0.03f, 12 + Main.rand.Next(6));
                    MagnumParticleHandler.SpawnParticle(flicker);
                }
            }

            // === Periodic flame geysers — erupting columns with CometParticle spray ===
            if (timer % 22 == 0)
            {
                Vector2 geyserPos = arenaCenter + new Vector2(Main.rand.NextFloat(-radius * 0.8f, radius * 0.8f), 180f);
                for (int i = 0; i < 7; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(3f, 8f));
                    Color col = FlameGradient(Main.rand.NextFloat(0.4f, 1f));
                    col.A = 0;
                    var geyser = new CometParticle(geyserPos + Main.rand.NextVector2Circular(8f, 4f), vel,
                        col, DarkCrimson with { A = 0 }, 0.08f + Main.rand.NextFloat(0.05f),
                        15 + Main.rand.Next(8), 1.5f);
                    MagnumParticleHandler.SpawnParticle(geyser);
                }

                // Geyser base flash
                var geyserFlash = new StrongBloomParticle(geyserPos, Vector2.Zero,
                    EmberOrange with { A = 0 }, 0.25f, 8);
                MagnumParticleHandler.SpawnParticle(geyserFlash);
            }

            // === Rolling smoke banks along the lake ===
            if (timer % 8 == 0)
            {
                Vector2 smokePos = arenaCenter + new Vector2(Main.rand.NextFloat(-radius, radius), 160f);
                float drift = Main.rand.NextFloat(-0.8f, 0.8f);
                var smoke = new HeavySmokeParticle(smokePos,
                    new Vector2(drift, -Main.rand.NextFloat(0.3f, 0.8f)),
                    AshBlack * 0.3f, 0.3f, 35 + Main.rand.Next(15));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Localized heat distortion nodes across arena ===
            if (timer % 35 == 0)
            {
                ScreenHeatDistortionSystem.CreateHeatSource(
                    arenaCenter + new Vector2(Main.rand.NextFloat(-radius * 0.6f, radius * 0.6f), 100f),
                    220f, 0.03f, 2.5f);
            }
        }

        /// <summary>
        /// Apocalyptic flame wings — wreath of fire emanating from the boss's sides.
        /// Each wing is a layered arc of StreakParticle feathers with CometParticle
        /// dripping tips, a central bloom spine, and a pulsing core halo.
        /// The wings breathe — expanding and contracting with harmonic rhythm.
        /// </summary>
        public static void ApocalypticFlameWings(Vector2 bossCenter, int timer, float intensity)
        {
            float wingPulse = (float)Math.Sin(timer * 0.06f) * 0.2f + 0.8f;
            float breathe = (float)Math.Sin(timer * 0.035f) * 0.15f + 1f;

            for (int side = -1; side <= 1; side += 2)
            {
                int featherCount = 14;
                for (int f = 0; f < featherCount; f++)
                {
                    float featherAngle = MathHelper.Lerp(0.25f, 2.0f, (float)f / featherCount) * side;
                    float featherLength = (55f + f * 14f) * wingPulse * intensity * breathe;

                    Vector2 featherTip = bossCenter + new Vector2(
                        (float)Math.Cos(featherAngle - MathHelper.PiOver2) * featherLength,
                        (float)Math.Sin(featherAngle - MathHelper.PiOver2) * featherLength * 0.55f);

                    float featherProgress = (float)f / featherCount;

                    // === StreakParticle feathers — fiery directional streaks ===
                    if (Main.rand.NextBool(2))
                    {
                        Color wingColor = FlameGradient(1f - featherProgress * 0.7f);
                        wingColor.A = 0;
                        Vector2 featherDir = (featherTip - bossCenter).SafeNormalize(Vector2.Zero);
                        var feather = new StreakParticle(featherTip, featherDir * 1.5f, wingColor,
                            0.06f * intensity * wingPulse, 8 + Main.rand.Next(4));
                        MagnumParticleHandler.SpawnParticle(feather);
                    }

                    // === Bloom spine along wing ===
                    if (f % 3 == 0)
                    {
                        Vector2 spinePos = Vector2.Lerp(bossCenter, featherTip, 0.5f);
                        Color spineColor = Color.Lerp(HellfireGold, MoltenCore, featherProgress) with { A = 0 };
                        var spine = new BloomParticle(spinePos, Vector2.Zero, spineColor,
                            0.1f * intensity * wingPulse, 0.03f, 6, true);
                        MagnumParticleHandler.SpawnParticle(spine);
                    }

                    // === CometParticle dripping from wing tips ===
                    if (f >= featherCount - 3 && Main.rand.NextBool(3))
                    {
                        Vector2 dripVel = new Vector2(side * Main.rand.NextFloat(0.5f, 2f), Main.rand.NextFloat(1f, 3f));
                        var drip = new CometParticle(featherTip, dripVel,
                            EmberOrange with { A = 0 }, DarkCrimson with { A = 0 },
                            0.05f * intensity, 14, 1f);
                        MagnumParticleHandler.SpawnParticle(drip);
                    }
                }
            }

            // === Core wing halo — pulsing bloom behind boss ===
            Color coreColor = Color.Lerp(Crimson, BloodRed, wingPulse) * wingPulse;
            coreColor.A = 0;
            var wingCore = new StrongBloomParticle(bossCenter, Vector2.Zero, coreColor,
                0.7f * intensity * wingPulse, 5);
            MagnumParticleHandler.SpawnParticle(wingCore);

            // === BloomRing wing boundary ===
            var wingRing = new BloomRingParticle(bossCenter, Vector2.Zero,
                EmberOrange with { A = 0 } * (wingPulse * 0.4f), 0.6f * intensity * breathe, 6, 0.08f);
            MagnumParticleHandler.SpawnParticle(wingRing);
        }

        /// <summary>
        /// Judgment beam — searing line across the arena with screen-burn afterimage.
        /// Multi-layer beam: StrongBloom white-hot core, StreakParticle edges,
        /// GlowySquare sparking debris, god ray burst at origin, chromatic flash.
        /// The visual manifestation of divine sentencing.
        /// </summary>
        public static void JudgmentBeam(Vector2 start, Vector2 end, float width, float intensity)
        {
            Vector2 delta = end - start;
            float length = delta.Length();
            Vector2 dir = delta.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            int segments = (int)(length / 12f);

            // === God ray burst at beam origin ===
            GodRaySystem.CreateBurst(start, SearingWhite, 8, 80f * intensity, 10, GodRayStyle.Burst, HellfireGold);

            // === Main beam body — multi-layer ===
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(start, end, t);

                // Layer 1: StrongBloom white-hot core
                if (i % 2 == 0)
                {
                    var core = new StrongBloomParticle(pos, Vector2.Zero,
                        Color.Lerp(SearingWhite, MoltenCore, t * 0.3f) with { A = 0 },
                        width / 55f * intensity, 6);
                    MagnumParticleHandler.SpawnParticle(core);
                }

                // Layer 2: Ember bloom body
                Color bodyColor = FlameGradient(1f - t * 0.4f);
                bodyColor.A = 0;
                var body = new BloomParticle(pos, Vector2.Zero, bodyColor,
                    width / 50f * intensity, width / 80f * intensity, 8, true);
                MagnumParticleHandler.SpawnParticle(body);

                // Layer 3: StreakParticle edges — directional fire wisps
                if (Main.rand.NextBool(3))
                {
                    float edgeSign = Main.rand.NextBool() ? 1f : -1f;
                    float edgeDist = Main.rand.NextFloat(width * 0.25f, width * 0.55f);
                    var edge = new StreakParticle(pos + perp * edgeDist * edgeSign,
                        dir * 2f + perp * edgeSign * 0.5f,
                        Crimson with { A = 0 }, 0.06f * intensity, 8 + Main.rand.Next(4));
                    MagnumParticleHandler.SpawnParticle(edge);
                }

                // Layer 4: GlowySquare sparking debris
                if (Main.rand.NextBool(5))
                {
                    Vector2 debrisVel = perp * Main.rand.NextFloat(-3f, 3f) + dir * Main.rand.NextFloat(-1f, 1f);
                    var debris = new GlowySquareParticle(pos, debrisVel,
                        HellfireGold with { A = 0 }, 0.03f * intensity, 10 + Main.rand.Next(6));
                    MagnumParticleHandler.SpawnParticle(debris);
                }

                Lighting.AddLight(pos, MoltenCore.ToVector3() * intensity * 0.5f);
            }

            // === Beam endpoint burst ===
            var endBurst = new StrongBloomParticle(end, Vector2.Zero, EmberOrange with { A = 0 }, 0.4f * intensity, 10);
            MagnumParticleHandler.SpawnParticle(endBurst);
            for (int i = 0; i < 4; i++)
            {
                Vector2 splashVel = Main.rand.NextVector2CircularEdge(3f, 3f) * intensity;
                var splash = new CometParticle(end, splashVel, HellfireGold with { A = 0 },
                    BloodRed with { A = 0 }, 0.06f * intensity, 12, 1.2f);
                MagnumParticleHandler.SpawnParticle(splash);
            }

            // === Screen-burn afterimage — chromatic flash + heat distortion ===
            ScreenFlashSystem.Instance?.Flash(new Color(180, 30, 20), 0.35f * intensity, 18);
            ScreenFlashSystem.Instance?.ChromaticFlash(0.15f * intensity, 10);
            ScreenHeatDistortionSystem.FlashDistortion(Vector2.Lerp(start, end, 0.5f), length * 0.35f, 0.06f * intensity);
        }

        /// <summary>
        /// Phase 3 ambient — verdict atmosphere.
        /// Lake of fire across the arena, flame wings pulsing on boss,
        /// descending ash, heavy smoke banks, blood-red radial aura pulses,
        /// and an oppressive heat distortion that never lets up.
        /// </summary>
        public static void VerdictAmbience(Vector2 bossCenter, int timer)
        {
            // === Lake of fire across the arena floor ===
            LakeOfFire(bossCenter, 550f, timer);

            // === Flame wings pulsing on boss ===
            if (timer % 2 == 0)
                ApocalypticFlameWings(bossCenter, timer, 0.85f);

            // === Floating ash descending — thick curtain ===
            if (timer % 4 == 0)
            {
                Vector2 ashPos = bossCenter + new Vector2(Main.rand.NextFloat(-450f, 450f), -320f);
                Vector2 ashVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.8f, 2f));
                var ash = new GlowySquareParticle(ashPos, ashVel,
                    (AshBlack * 0.5f) with { A = 0 }, 0.025f + Main.rand.NextFloat(0.02f),
                    45 + Main.rand.Next(20));
                MagnumParticleHandler.SpawnParticle(ash);
            }

            // === Heavy smoke banks drifting across arena ===
            if (timer % 14 == 0)
            {
                Vector2 smokePos = bossCenter + new Vector2(Main.rand.NextFloat(-400f, 400f), 100f);
                var smoke = new HeavySmokeParticle(smokePos,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.2f, 0.6f)),
                    AshBlack * 0.3f, 0.35f, 40 + Main.rand.Next(20));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Wrathful aura — radial bloom ring pulses ===
            if (timer % 6 == 0)
            {
                float pulse = (float)Math.Sin(timer * 0.08f) * 0.3f + 0.8f;
                Color auraColor = Color.Lerp(BloodRed, DarkCrimson, pulse) * pulse;
                auraColor.A = 0;
                var aura = new BloomRingParticle(bossCenter, Vector2.Zero, auraColor,
                    0.25f * pulse, 10, 0.04f);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // === Boss body fire — leaking from every surface ===
            if (timer % 3 == 0)
            {
                Vector2 auraOffset = Main.rand.NextVector2Circular(35f, 35f);
                Color fireCol = FlameGradient(Main.rand.NextFloat(0.5f, 0.9f));
                fireCol.A = 0;
                var fire = new BloomParticle(bossCenter + auraOffset, new Vector2(0, -1.2f), fireCol,
                    0.18f, 0.05f, 12, true);
                MagnumParticleHandler.SpawnParticle(fire);
            }

            // === Ambient light — intense and oppressive ===
            float ambientPulse = (float)Math.Sin(timer * 0.06f) * 0.15f + 0.6f;
            Lighting.AddLight(bossCenter, BloodRed.ToVector3() * ambientPulse);
        }

        #endregion

        #region ENRAGE: THE EXECUTION

        /// <summary>
        /// Heartbeat pulse — the entire screen pulses blood-red in rhythm.
        /// Proper lub-dub cardiac rhythm: the first beat (lub) is stronger
        /// and slower, the second (dub) is sharper and faster, followed
        /// by a rest period. Includes a faint bloom ring expanding outward
        /// from screen center to sell the "cardiac" feeling.
        /// </summary>
        public static void HeartbeatPulse(int timer)
        {
            // Heartbeat rhythm: rapid double-pulse every 40 frames ("lub-dub")
            int beatPhase = timer % 40;
            bool isLub = beatPhase == 0;
            bool isDub = beatPhase == 8;

            if (isLub)
            {
                // LUB — deeper, more powerful beat
                ScreenFlashSystem.Instance?.Flash(BloodRed, 0.4f, 14);
                MagnumScreenEffects.AddScreenShake(2f, 6);

                // Cardiac bloom ring from screen center
                Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
                var heartRing = new BloomRingParticle(screenCenter, Vector2.Zero,
                    DarkCrimson with { A = 0 }, 0.4f, 18, 0.07f);
                MagnumParticleHandler.SpawnParticle(heartRing);
            }
            else if (isDub)
            {
                // DUB — sharper, quicker beat
                ScreenFlashSystem.Instance?.Flash(DarkCrimson, 0.25f, 8);
                MagnumScreenEffects.AddScreenShake(1.2f, 4);
            }
        }

        /// <summary>
        /// Relentless fire column — permanent-feeling columns of execution.
        /// Stronger than Phase 2 pillars with long-lived burn marks at the base,
        /// persistent ember fountains, and lingering smoke that stays.
        /// </summary>
        public static void RelentlessFireColumn(Vector2 basePosition, float intensity)
        {
            // === Amplified fire pillar ===
            FirePillar(basePosition, 420f * intensity, intensity * 1.4f);

            // === Long-lived ground burn mark (lingers long after the pillar fades) ===
            var burnMark = new StrongBloomParticle(basePosition, Vector2.Zero,
                DarkCrimson with { A = 0 }, 0.5f * intensity, 90);
            MagnumParticleHandler.SpawnParticle(burnMark);

            // === Persistent ember fountain at base ===
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(1f, 4f));
                var ember = new CometParticle(basePosition + Main.rand.NextVector2Circular(15f, 5f),
                    emberVel, EmberOrange with { A = 0 }, DarkCrimson with { A = 0 },
                    0.05f * intensity, 20 + Main.rand.Next(10), 1.2f);
                MagnumParticleHandler.SpawnParticle(ember);
            }

            // === Lingering black smoke column ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(0.5f, 1.5f));
                var smoke = new HeavySmokeParticle(basePosition + Main.rand.NextVector2Circular(25f, 8f),
                    smokeVel, AshBlack * 0.45f, 0.35f * intensity, 50 + Main.rand.Next(20));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Persistent heat distortion — these burn marks shimmer ===
            ScreenHeatDistortionSystem.CreateHeatSource(basePosition, 160f * intensity, 0.04f * intensity, 4f);
        }

        /// <summary>
        /// Ash and ember rain — the sky weeps fire and ash.
        /// CometParticle burning embers fall alongside GlowySquare ash flakes.
        /// The rain is thick and oppressive — judgment has been passed.
        /// </summary>
        public static void AshEmberRain(Vector2 screenCenter, int timer)
        {
            // === GlowySquare ash flakes — constant dark ash curtain ===
            if (timer % 2 == 0)
            {
                Vector2 ashPos = screenCenter + new Vector2(Main.rand.NextFloat(-550f, 550f), -380f);
                Vector2 ashVel = new Vector2(Main.rand.NextFloat(-1.2f, 0.5f), Main.rand.NextFloat(1.2f, 3.5f));
                var ash = new GlowySquareParticle(ashPos, ashVel,
                    (AshBlack * 0.45f) with { A = 0 }, 0.025f + Main.rand.NextFloat(0.03f),
                    50 + Main.rand.Next(25));
                MagnumParticleHandler.SpawnParticle(ash);
            }

            // === CometParticle burning embers — fiery descent ===
            if (timer % 3 == 0)
            {
                Vector2 emberPos = screenCenter + new Vector2(Main.rand.NextFloat(-500f, 500f), -370f);
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(2.5f, 6f));
                Color startCol = FlameGradient(Main.rand.NextFloat(0.5f, 1f));
                var ember = new CometParticle(emberPos, emberVel,
                    startCol with { A = 0 }, DarkCrimson with { A = 0 },
                    0.05f + Main.rand.NextFloat(0.03f), 30 + Main.rand.Next(15), 1.3f);
                MagnumParticleHandler.SpawnParticle(ember);
            }

            // === Occasional large burning chunk — slow descent ===
            if (timer % 18 == 0)
            {
                Vector2 chunkPos = screenCenter + new Vector2(Main.rand.NextFloat(-400f, 400f), -350f);
                Vector2 chunkVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(1f, 2f));
                var chunk = new CometParticle(chunkPos, chunkVel,
                    MoltenCore with { A = 0 }, BloodRed with { A = 0 },
                    0.1f, 40 + Main.rand.Next(15), 2f);
                MagnumParticleHandler.SpawnParticle(chunk);

                // Smoke trail behind large chunks
                var trail = new HeavySmokeParticle(chunkPos, chunkVel * 0.3f,
                    AshBlack * 0.3f, 0.2f, 25);
                MagnumParticleHandler.SpawnParticle(trail);
            }
        }

        /// <summary>
        /// World bleeding fountain — blood-red particle geysers on every boss hit.
        /// DramaticFlareParticle central eruption, CometParticle blood sprays arcing
        /// upward, StreakParticle directional debris, and a GodRay explosion burst.
        /// The world itself bleeds when judgment is wounded.
        /// </summary>
        public static void WorldBleedingFountain(Vector2 position, float intensity)
        {
            // === Central DramaticFlare eruption core ===
            var core = new DramaticFlareParticle(position, Vector2.Zero,
                BloodRed with { A = 0 }, 0.5f * intensity, 20);
            MagnumParticleHandler.SpawnParticle(core);

            // === StrongBloom flash ===
            var flash = new StrongBloomParticle(position, Vector2.Zero,
                MoltenCore with { A = 0 }, 0.4f * intensity, 8);
            MagnumParticleHandler.SpawnParticle(flash);

            // === God ray explosion ===
            GodRaySystem.CreateBurst(position, BloodRed, 10, 100f * intensity, 14, GodRayStyle.Explosion, DarkCrimson);

            // === CometParticle blood fountain sprays — arcing upward ===
            int sprayCount = (int)(12 * intensity);
            for (int i = 0; i < sprayCount; i++)
            {
                float angle = MathHelper.Lerp(-MathHelper.PiOver2 - 0.9f, -MathHelper.PiOver2 + 0.9f,
                    (float)i / sprayCount);
                float speed = Main.rand.NextFloat(4f, 11f) * intensity;
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color sprayColor = Color.Lerp(BloodRed, Crimson, Main.rand.NextFloat());
                var spray = new CometParticle(position, vel, sprayColor with { A = 0 },
                    DarkCrimson with { A = 0 }, 0.08f * intensity, 18 + Main.rand.Next(10), 1.5f);
                MagnumParticleHandler.SpawnParticle(spray);
            }

            // === StreakParticle directional debris ===
            for (int i = 0; i < (int)(6 * intensity); i++)
            {
                Vector2 debrisVel = Main.rand.NextVector2CircularEdge(5f, 5f) * intensity;
                debrisVel.Y -= Main.rand.NextFloat(2f, 4f);
                var debris = new StreakParticle(position + Main.rand.NextVector2Circular(10f, 10f),
                    debrisVel, Crimson with { A = 0 }, 0.05f * intensity, 12 + Main.rand.Next(6));
                MagnumParticleHandler.SpawnParticle(debris);
            }

            // === Blood droplets with GlowySquare for chunky debris ===
            for (int i = 0; i < (int)(5 * intensity); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 4f) * intensity;
                vel.Y -= Main.rand.NextFloat(2f, 5f);
                var drop = new GlowySquareParticle(position + Main.rand.NextVector2Circular(12f, 12f), vel,
                    DarkCrimson with { A = 0 }, 0.04f * intensity, 16 + Main.rand.Next(8));
                MagnumParticleHandler.SpawnParticle(drop);
            }

            // === Impact flash ===
            ScreenFlashSystem.Instance?.Flash(new Color(200, 40, 40), 0.18f * intensity, 6);
            MagnumScreenEffects.AddScreenShake(4f * intensity, 8);
        }

        /// <summary>
        /// Enrage ambient — the execution atmosphere of unending pressure.
        /// Heartbeat pulses, ash/ember rain, relentless heat distortion,
        /// fire leaking from the boss, violent aura ring pulses, and
        /// occasional screen distortion flickers.
        /// </summary>
        public static void EnrageAmbience(Vector2 bossCenter, int timer)
        {
            // === Heartbeat pulse rhythm ===
            HeartbeatPulse(timer);

            // === Ash and ember rain ===
            AshEmberRain(bossCenter, timer);

            // === Intense all-around heat distortion ===
            if (timer % 16 == 0)
            {
                ScreenHeatDistortionSystem.CreateHeatSource(bossCenter, 450f, 0.04f, 1.2f);
            }

            // === Fire leaking from every surface of the boss ===
            if (timer % 2 == 0)
            {
                Vector2 firePos = bossCenter + Main.rand.NextVector2Circular(55f, 55f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(1.5f, 4f));
                Color fireCol = FlameGradient(Main.rand.NextFloat(0.3f, 1f));
                fireCol.A = 0;
                var fire = new BloomParticle(firePos, vel, fireCol,
                    0.14f, 0.04f, 10 + Main.rand.Next(6), true);
                MagnumParticleHandler.SpawnParticle(fire);
            }

            // === CometParticle sparks ejecting from boss ===
            if (timer % 6 == 0)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                sparkVel.Y -= 1f;
                var spark = new CometParticle(bossCenter + Main.rand.NextVector2Circular(30f, 30f),
                    sparkVel, HellfireGold with { A = 0 }, BloodRed with { A = 0 },
                    0.06f, 12 + Main.rand.Next(6), 1.2f);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // === Violent aura ring pulses — rapidly expanding ===
            if (timer % 7 == 0)
            {
                float ringPulse = (float)Math.Sin(timer * 0.12f) * 0.3f + 0.7f;
                Color auraCol = Color.Lerp(BloodRed, DarkCrimson, ringPulse);
                auraCol.A = 0;
                var aura = new BloomRingParticle(bossCenter, Vector2.Zero, auraCol,
                    0.35f * ringPulse, 10, 0.08f);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // === HeavySmoke billowing from boss ===
            if (timer % 10 == 0)
            {
                var smoke = new HeavySmokeParticle(bossCenter + Main.rand.NextVector2Circular(40f, 40f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1.5f)),
                    AshBlack * 0.35f, 0.3f, 30 + Main.rand.Next(15));
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // === Ambient lighting — relentless crimson ===
            float ambientPulse = (float)Math.Sin(timer * 0.08f) * 0.15f + 0.7f;
            Lighting.AddLight(bossCenter, BloodRed.ToVector3() * ambientPulse);
        }

        #endregion

        #region Sky & Screen Integration

        /// <summary>
        /// Updates the DiesIraeSky system with phase-specific parameters.
        /// Call from boss AI each frame. Communicates enrage state to the
        /// sky system — phase intensification is already driven by BossLifeRatio.
        /// </summary>
        public static void UpdateSkyPhase(int difficultyTier, bool isEnraged)
        {
            DiesIraeSky.BossIsEnraged = isEnraged;
            // DiesIraeSky reads BossLifeRatio for progressive sky darkening.
            // Additional phase state communicated via BossIndexTracker.DiesIraePhase.
        }

        /// <summary>
        /// Triggers a dramatic wrath flash — blood-red screen flash with screen shake.
        /// Used for major attack windup moments and phase warnings.
        /// </summary>
        public static void TriggerWrathFlash(float intensity)
        {
            float clamped = Math.Min(intensity / 10f, 1f);
            ScreenFlashSystem.Instance?.Flash(BloodRed, 0.45f * clamped, 12);
            MagnumScreenEffects.AddScreenShake(3f * clamped, 8);
        }

        /// <summary>
        /// Triggers the full apocalypse flash — blinding searing-white-to-crimson
        /// transition for major phase changes and the most devastating attacks.
        /// Includes chromatic aberration and heavy screen shake.
        /// </summary>
        public static void TriggerApocalypseFlash(float intensity)
        {
            float clamped = Math.Min(intensity / 15f, 1f);
            ScreenFlashSystem.Instance?.Flash(SearingWhite, 0.75f * clamped, 22);
            ScreenFlashSystem.Instance?.ChromaticFlash(0.35f * clamped, 16);
            MagnumScreenEffects.AddScreenShake(5f * clamped, 12);
        }

        #endregion
    }
}
