using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// Eroica boss shader-driven rendering system.
    /// Manages all 5 boss shaders: ValorAura, HeroicTrail, PhoenixFlame,
    /// SakuraTransition, and DeathDissolve.
    /// 
    /// Called from EroicasRetribution.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// </summary>
    public static class EroicaBossShaderSystem
    {
        // Theme colors
        private static readonly Color ValorGold = new Color(255, 200, 80);
        private static readonly Color ValorScarlet = new Color(200, 50, 50);
        private static readonly Color SakuraPink = new Color(255, 150, 180);
        private static readonly Color PhoenixWhite = new Color(255, 240, 220);

        /// <summary>
        /// Draws the heroic valor aura behind the boss.
        /// Intensity scales with aggression level and HP tier.
        /// </summary>
        public static void DrawValorAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 80f + difficultyTier * 20f;
            float intensity = 0.3f + aggressionLevel * 0.5f;
            if (isEnraged) intensity *= 1.5f;

            Color primary = isEnraged ? new Color(255, 100, 50) : ValorGold;
            Color secondary = isEnraged ? new Color(180, 20, 20) : ValorScarlet;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.EroicaValorAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.02f);
        }

        /// <summary>
        /// Draws the heroic flame trail during dashes and charges.
        /// </summary>
        public static void DrawHeroicTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? new Color(255, 80, 40) : ValorGold;
            float width = isEnraged ? 1.5f : 1.0f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EroicaHeroicTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, 6f);
        }

        /// <summary>
        /// Draws a phoenix flame blast during PhoenixDive attack.
        /// </summary>
        public static void DrawPhoenixFlame(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.EroicaPhoenixFlame, ValorGold, intensity,
                (float)Main.timeForVisualEffects * 0.03f);
        }

        /// <summary>
        /// Draws the sakura petal phase transition effect.
        /// </summary>
        public static void DrawPhaseTransition(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? ValorScarlet : ValorGold;
            Color to = isPhase2Transition ? ValorGold : PhoenixWhite;
            float intensity = isPhase2Transition ? 1.2f : 0.8f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.EroicaSakuraTransition, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the heroic death dissolve effect.
        /// </summary>
        public static void DrawDeathDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EroicaDeathDissolveFx, dissolveProgress,
                ValorGold, 0.06f);
        }

        /// <summary>
        /// Spawns musical VFX particles during various boss states.
        /// Enhances the existing ambient particles with shader-aware effects.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            // Staff line convergence every 2 seconds
            if (timer % 120 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, ValorGold, 0.5f + difficultyTier * 0.2f);
            }

            // Rhythmic metronome tick on attack windows
            if (timer % 60 == 0 && difficultyTier >= 1)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center, ValorScarlet, 3, 6);
            }

            // Heroic fanfare accents during high aggression
            if (timer % 90 == 0 && difficultyTier >= 2)
            {
                Phase10Integration.Eroica.HeroicImpact(npc.Center, 1.0f);
            }
        }
    }
}
