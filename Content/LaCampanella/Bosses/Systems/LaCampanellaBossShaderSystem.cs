using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.LaCampanella.Bosses.Systems
{
    /// <summary>
    /// La Campanella boss shader-driven rendering system.
    /// Manages all 5 boss shaders: CampanellaBellAura, CampanellaInfernalTrail,
    /// CampanellaResonanceWave, CampanellaFirewall, CampanellaChimeDissolve.
    ///
    /// Called from LaCampanellaChimeOfLife.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// </summary>
    public static class LaCampanellaBossShaderSystem
    {
        // Theme colors
        private static readonly Color InfernalOrange = new Color(255, 140, 40);
        private static readonly Color SmokeBlack = new Color(30, 20, 15);
        private static readonly Color BellGold = new Color(220, 180, 80);
        private static readonly Color FlameWhite = new Color(255, 230, 200);

        /// <summary>
        /// Draws concentric bell resonance rings behind the boss.
        /// Intensity scales with phase and HP tier.
        /// </summary>
        public static void DrawBellAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 90f + difficultyTier * 25f;
            float intensity = 0.35f + aggressionLevel * 0.45f;
            if (isEnraged) intensity *= 1.6f;

            Color primary = isEnraged ? FlameWhite : InfernalOrange;
            Color secondary = isEnraged ? new Color(255, 80, 20) : BellGold;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.CampanellaBellAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.015f);
        }

        /// <summary>
        /// Draws black smoke trail during boss charges and dashes.
        /// </summary>
        public static void DrawInfernalTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? InfernalOrange : SmokeBlack;
            float width = isEnraged ? 1.6f : 1.1f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.CampanellaInfernalTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.025f, 5f);
        }

        /// <summary>
        /// Draws resonance wave rings after bell toll/slam attacks.
        /// </summary>
        public static void DrawResonanceWave(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.CampanellaResonanceWave, InfernalOrange, intensity,
                (float)Main.timeForVisualEffects * 0.03f);
        }

        /// <summary>
        /// Draws the infernal firewall during phase transitions.
        /// </summary>
        public static void DrawFirewall(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? InfernalOrange : BellGold;
            Color to = isPhase2Transition ? FlameWhite : InfernalOrange;
            float intensity = isPhase2Transition ? 1.4f : 0.9f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.CampanellaFirewall, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the chime dissolve death animation  Ebell shattering into embers.
        /// </summary>
        public static void DrawChimeDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.CampanellaChimeDissolve, dissolveProgress,
                InfernalOrange, 0.07f);
        }

        /// <summary>
        /// Spawns musical VFX particles during various boss states.
        /// Enhances the existing ambient particles with bell-flame musical accents.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            // Bell resonance convergence every 2 seconds
            if (timer % 120 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, InfernalOrange, 0.5f + difficultyTier * 0.2f);
            }

            // Rhythmic bell tick on attack windows
            if (timer % 60 == 0 && difficultyTier >= 1)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center, BellGold, 3, 6);
            }

            // Infernal flame accents during high aggression
            if (timer % 90 == 0 && difficultyTier >= 2)
            {
                Phase10Integration.LaCampanella.InfernalTremolo(npc.Center);
            }
        }
    }
}
