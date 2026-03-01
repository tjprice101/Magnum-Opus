using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Autumn.Bosses.Systems
{
    /// <summary>
    /// Autunno boss shader-driven rendering system.
    /// Manages all 5 boss shaders: DecayAura, LeafTrail, WitheringWind,
    /// HarvestMoon, and FinalHarvest.
    ///
    /// Called from Autunno.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// </summary>
    public static class AutunnoBossShaderSystem
    {
        // Theme colors
        private static readonly Color AutumnOrange = new Color(200, 120, 40);
        private static readonly Color DecayBrown = new Color(100, 60, 30);
        private static readonly Color HarvestGold = new Color(180, 160, 60);
        private static readonly Color WitheredRed = new Color(150, 50, 30);

        /// <summary>
        /// Draws the swirling autumn leaves decay aura behind the boss.
        /// Intensity scales with aggression level and HP tier.
        /// </summary>
        public static void DrawDecayAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 90f + difficultyTier * 18f;
            float intensity = 0.35f + aggressionLevel * 0.45f;
            if (isEnraged) intensity *= 1.4f;

            Color primary = isEnraged ? WitheredRed : AutumnOrange;
            Color secondary = isEnraged ? new Color(120, 40, 20) : DecayBrown;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.AutunnoDecayAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.018f);
        }

        /// <summary>
        /// Draws tumbling leaf afterimage trail during dashes and charges.
        /// </summary>
        public static void DrawLeafTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? WitheredRed : HarvestGold;
            float width = isEnraged ? 1.4f : 1.0f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.AutunnoLeafTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.022f, 5f);
        }

        /// <summary>
        /// Draws the withering wind attack blast effect.
        /// </summary>
        public static void DrawWitheringWind(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.AutunnoWitheringWind, AutumnOrange, intensity,
                (float)Main.timeForVisualEffects * 0.025f);
        }

        /// <summary>
        /// Draws the harvest moon golden glow phase transition effect.
        /// </summary>
        public static void DrawHarvestMoonPhase(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isPhase2Transition)
        {
            Color from = isPhase2Transition ? AutumnOrange : HarvestGold;
            Color to = isPhase2Transition ? HarvestGold : new Color(255, 220, 120);
            float intensity = isPhase2Transition ? 1.3f : 0.9f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.AutunnoHarvestMoon, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the final harvest death dissolve  Ebody crumbles to swirling leaves.
        /// </summary>
        public static void DrawFinalHarvestDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.AutunnoFinalHarvest, dissolveProgress,
                HarvestGold, 0.07f);
        }

        /// <summary>
        /// Spawns musical VFX particles with leaf and wind themed accents.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            // Leaf spiral convergence every 2 seconds
            if (timer % 120 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, HarvestGold, 0.5f + difficultyTier * 0.2f);
            }

            // Rhythmic wind gust warning on attack windows
            if (timer % 60 == 0 && difficultyTier >= 1)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center, AutumnOrange, 3, 6);
            }

            // Withering leaf accents during high aggression
            if (timer % 90 == 0 && difficultyTier >= 2)
            {
                BossSignatureVFX.AutumnLeafStorm(npc.Center, 1.0f);
            }

            // Ambient leaf motes drifting around the boss
            if (timer % 15 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(80f, 80f);
                Color leafColor = Color.Lerp(AutumnOrange, HarvestGold, Main.rand.NextFloat());
                CustomParticles.GenericFlare(npc.Center + offset, leafColor, 0.2f, 30);
            }
        }
    }
}
