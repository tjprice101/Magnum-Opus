using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// Primavera boss shader-driven rendering system.
    /// Manages all 5 boss shaders: BloomAura, PetalTrail, GrowthPulse,
    /// VernalStorm, and RebirthDissolve.
    ///
    /// Called from Primavera.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// </summary>
    public static class PrimaveraBossShaderSystem
    {
        // Theme colors
        private static readonly Color SpringGreen = new Color(100, 200, 80);
        private static readonly Color BlossomPink = new Color(240, 160, 180);
        private static readonly Color SunshineYellow = new Color(240, 220, 80);
        private static readonly Color SproutGreen = new Color(60, 160, 60);

        /// <summary>
        /// Draws the growing flower bloom aura with pollen particles.
        /// Intensity scales with aggression level and HP tier.
        /// </summary>
        public static void DrawBloomAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 85f + difficultyTier * 18f;
            float intensity = 0.3f + aggressionLevel * 0.5f;
            if (isEnraged) intensity *= 1.5f;

            Color primary = isEnraged ? new Color(255, 100, 140) : BlossomPink;
            Color secondary = isEnraged ? new Color(200, 80, 120) : SpringGreen;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.PrimaveraBloomAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.02f);
        }

        /// <summary>
        /// Draws the cherry blossom petal trail during movement.
        /// </summary>
        public static void DrawPetalTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? new Color(255, 120, 160) : BlossomPink;
            float width = isEnraged ? 1.4f : 1.0f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.PrimaveraPetalTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, 5f);
        }

        /// <summary>
        /// Draws the growth pulse healing/attack visual effect.
        /// </summary>
        public static void DrawGrowthPulse(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.PrimaveraGrowthPulse, SpringGreen, intensity,
                (float)Main.timeForVisualEffects * 0.025f);
        }

        /// <summary>
        /// Draws the vernal storm phase transition effect.
        /// </summary>
        public static void DrawVernalStorm(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? BlossomPink : SpringGreen;
            Color to = isPhase2Transition ? SunshineYellow : new Color(180, 255, 180);
            float intensity = isPhase2Transition ? 1.2f : 0.8f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.PrimaveraVernalStorm, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the rebirth dissolve  Ebody dissolves into blooming flowers.
        /// </summary>
        public static void DrawRebirthDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.PrimaveraRebirthDissolve, dissolveProgress,
                BlossomPink, 0.06f);
        }

        /// <summary>
        /// Spawns musical VFX particles with blossom and growth themes.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            // Petal spiral convergence every 2 seconds
            if (timer % 120 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, BlossomPink, 0.5f + difficultyTier * 0.2f);
            }

            // Rhythmic growth pulse on attack windows
            if (timer % 60 == 0 && difficultyTier >= 1)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center, SpringGreen, 3, 6);
            }

            // Spring bloom bursts at high aggression
            if (timer % 90 == 0 && difficultyTier >= 2)
            {
                BossSignatureVFX.SpringBloomBurst(npc.Center, 1.0f);
            }

            // Ambient pollen particles drifting around the boss
            if (timer % 12 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(70f, 70f);
                Color pollenColor = Color.Lerp(SunshineYellow, SpringGreen, Main.rand.NextFloat());
                CustomParticles.GenericFlare(npc.Center + offset, pollenColor, 0.15f, 35);
            }
        }
    }
}
