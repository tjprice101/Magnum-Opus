using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Map;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.OdeToJoy.Bosses.Systems
{
    /// <summary>
    /// ODE TO JOY — BOSS FIGHT ARENA VISUALS
    /// 
    /// Manages the arena ambiance that scales across all four phases:
    /// Phase 1: Warm candlelight gold — like a single candle in darkness
    /// Phase 2: Multiple warm lights being lit — the chorus brightens the world
    /// Phase 3: Full warm illumination — every surface shimmers golden
    /// Enrage: Overflowing jubilant golden light — a thousand candles blazing
    /// 
    /// Handles: world tinting, ambient particle spawning, lighting modification,
    /// and the gradual brightening that mirrors the growing orchestra.
    /// </summary>
    public class OdeToJoyBossFightVisuals : ModSystem
    {
        private static bool bossActive = false;
        private static float overlayIntensity = 0f;
        private static int currentPhase = 0; // 0=P1, 1=P2, 2=P3/FullOrchestra
        private static bool isEnraged = false;

        private const float MaxIntensityP1 = 0.15f;
        private const float MaxIntensityP2 = 0.30f;
        private const float MaxIntensityP3 = 0.45f;
        private const float MaxIntensityEnrage = 0.60f;
        private const float FadeSpeed = 0.015f;

        // Phase colors
        private static readonly Color CandlelightTint = new Color(140, 110, 50);
        private static readonly Color ChorusTint = new Color(160, 130, 60);
        private static readonly Color OrchestraTint = new Color(180, 150, 70);
        private static readonly Color EnrageTint = new Color(200, 170, 80);

        /// <summary>
        /// External API: Set from the boss AI to communicate current fight state.
        /// </summary>
        public static void SetFightState(bool active, int phase, bool enraged)
        {
            bossActive = active;
            currentPhase = phase;
            isEnraged = enraged;
        }

        private static float GetMaxIntensity()
        {
            if (isEnraged) return MaxIntensityEnrage;
            return currentPhase switch
            {
                0 => MaxIntensityP1,
                1 => MaxIntensityP2,
                _ => MaxIntensityP3
            };
        }

        private static Color GetTintColor()
        {
            if (isEnraged) return EnrageTint;
            return currentPhase switch
            {
                0 => CandlelightTint,
                1 => ChorusTint,
                _ => OrchestraTint
            };
        }

        public override void PostUpdateNPCs()
        {
            float targetIntensity = bossActive ? GetMaxIntensity() : 0f;

            if (overlayIntensity < targetIntensity)
                overlayIntensity = Math.Min(overlayIntensity + FadeSpeed, targetIntensity);
            else if (overlayIntensity > targetIntensity)
                overlayIntensity = Math.Max(overlayIntensity - FadeSpeed, targetIntensity);

            if (!bossActive || Main.dedServ || overlayIntensity <= 0f)
                return;

            // Ambient particles scaled to phase intensity
            SpawnPhaseAmbientParticles();
        }

        private static void SpawnPhaseAmbientParticles()
        {
            // Base particle rate increases with phase
            int particleChance = isEnraged ? 2 : (6 - currentPhase * 2);
            particleChance = Math.Max(particleChance, 2);

            if (!Main.rand.NextBool(particleChance))
                return;

            Vector2 spawnPos = Main.screenPosition + new Vector2(
                Main.rand.NextFloat(0f, Main.screenWidth),
                Main.rand.NextFloat(-50f, Main.screenHeight));

            int choice = Main.rand.Next(isEnraged ? 5 : (2 + currentPhase));
            switch (choice)
            {
                case 0: // Warm golden glow dust — candlelight
                {
                    Dust dust = Dust.NewDustPerfect(spawnPos, Terraria.ID.DustID.GoldFlame,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.8f, 0.3f)),
                        100, default, Main.rand.NextFloat(1f, 1.4f));
                    dust.noGravity = true;
                    dust.fadeIn = 1.2f;
                    break;
                }
                case 1: // Soft amber motes
                {
                    Color amberColor = Color.Lerp(new Color(255, 200, 80), new Color(255, 170, 40), Main.rand.NextFloat());
                    var glow = new GenericGlowParticle(spawnPos,
                        new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, 0.2f)),
                        amberColor with { A = 0 }, 0.15f + overlayIntensity * 0.1f, 45, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                    break;
                }
                case 2: // Rose petals drifting (Phase 2+)
                {
                    Color petalColor = Color.Lerp(new Color(230, 120, 150), new Color(240, 150, 170), Main.rand.NextFloat());
                    RoseBudParticle.SpawnBurst(spawnPos, 1, 0.5f,
                        petalColor with { A = 0 }, new Color(255, 210, 60), 0.25f, 50);
                    break;
                }
                case 3: // Music notes floating (Phase 3+)
                {
                    Color noteColor = new Color(255, 210, 60) with { A = 0 };
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                    var note = new MusicNoteParticle(spawnPos, noteVel, noteColor, noteColor, 0.2f, 40, Main.rand.Next(4));
                    MagnumParticleHandler.SpawnParticle(note);
                    break;
                }
                case 4: // Sparkle confetti (Enrage)
                {
                    Color sparkColor = Main.rand.NextBool()
                        ? new Color(255, 220, 100) with { A = 0 }
                        : new Color(255, 180, 200) with { A = 0 };
                    var sparkle = new SparkleParticle(spawnPos,
                        new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1f, 0.5f)),
                        sparkColor, 0.15f, 35);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                    break;
                }
            }
        }

        public override void ModifyLightingBrightness(ref float scale)
        {
            if (overlayIntensity <= 0f) return;

            // Phase 1: Slightly dim (candlelight contrast). Phase 2+: Brighten the world.
            if (currentPhase == 0 && !isEnraged)
            {
                scale *= 1f - (overlayIntensity * 0.15f);
            }
            else
            {
                // Chorus and beyond — the world grows brighter
                float brightening = overlayIntensity * 0.1f * currentPhase;
                scale *= 1f + brightening;
            }
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (overlayIntensity <= 0f) return;

            Color tint = GetTintColor();
            float tintStrength = overlayIntensity;

            tileColor = Color.Lerp(tileColor, tint, tintStrength * 0.35f);
            backgroundColor = Color.Lerp(backgroundColor, tint, tintStrength * 0.45f);
        }

        public override void PreDrawMapIconOverlay(IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext)
        {
            // Required override
        }
    }
}
