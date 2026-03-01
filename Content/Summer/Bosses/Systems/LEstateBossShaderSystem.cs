using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Summer.Bosses.Systems
{
    /// <summary>
    /// L'Estate boss shader-driven rendering system.
    /// Manages all 5 boss shaders: SolarAura, HeatHazeTrail, SolarFlare,
    /// ZenithBeam, and SupernovaDissolve.
    ///
    /// Called from LEstate.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// </summary>
    public static class LEstateBossShaderSystem
    {
        // Theme colors
        private static readonly Color SolarGold = new Color(255, 200, 50);
        private static readonly Color ScorchOrange = new Color(240, 130, 30);
        private static readonly Color ZenithWhite = new Color(255, 250, 230);
        private static readonly Color HeatRed = new Color(220, 60, 30);

        /// <summary>
        /// Draws the blazing solar corona aura with heat shimmer.
        /// Intensity scales with aggression level and HP tier.
        /// </summary>
        public static void DrawSolarAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 95f + difficultyTier * 22f;
            float intensity = 0.35f + aggressionLevel * 0.5f;
            if (isEnraged) intensity *= 1.6f;

            Color primary = isEnraged ? HeatRed : SolarGold;
            Color secondary = isEnraged ? new Color(255, 80, 20) : ScorchOrange;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.EstateSolarAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.025f);
        }

        /// <summary>
        /// Draws the rising heat distortion trail during dashes.
        /// </summary>
        public static void DrawHeatHazeTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? HeatRed : ScorchOrange;
            float width = isEnraged ? 1.6f : 1.1f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EstateHeatHazeTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.03f, 7f);
        }

        /// <summary>
        /// Draws the solar flare attack blast effect.
        /// </summary>
        public static void DrawSolarFlare(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.EstateSolarFlare, SolarGold, intensity,
                (float)Main.timeForVisualEffects * 0.035f);
        }

        /// <summary>
        /// Draws the concentrated zenith beam effect (Phase 2).
        /// </summary>
        public static void DrawZenithBeam(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? ScorchOrange : SolarGold;
            Color to = isPhase2Transition ? ZenithWhite : new Color(255, 240, 180);
            float intensity = isPhase2Transition ? 1.4f : 0.9f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.EstateZenithBeam, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the supernova death dissolve  Esolar explosion dissolve.
        /// </summary>
        public static void DrawSupernovaDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EstateSupernovaDissolve, dissolveProgress,
                SolarGold, 0.08f);
        }

        /// <summary>
        /// Spawns musical VFX particles with solar and heat themed accents.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            // Solar flare pulse convergence every 2 seconds
            if (timer % 120 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, SolarGold, 0.6f + difficultyTier * 0.2f);
            }

            // Rhythmic heat pulse on attack windows
            if (timer % 60 == 0 && difficultyTier >= 1)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center, ScorchOrange, 3, 6);
            }

            // Solar burst accents during high aggression
            if (timer % 90 == 0 && difficultyTier >= 2)
            {
                BossSignatureVFX.SummerHeatWave(npc.Center, 1.0f);
            }

            // Ambient heat shimmer particles rising around the boss
            if (timer % 10 == 0)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-60f, 60f), Main.rand.NextFloat(20f, 60f));
                Color heatColor = Color.Lerp(SolarGold, ScorchOrange, Main.rand.NextFloat());
                CustomParticles.GenericFlare(npc.Center + offset, heatColor, 0.18f, 20);
            }
        }
    }
}
