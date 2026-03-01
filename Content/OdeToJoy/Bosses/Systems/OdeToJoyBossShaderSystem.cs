using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.OdeToJoy.Bosses.Systems
{
    /// <summary>
    /// Ode to Joy boss shader-driven rendering system.
    /// Manages all 5 boss shaders: OdeGardenAura, OdeVineTrail,
    /// OdePetalStorm, OdeChromaticBloom, and OdeJubilantDissolve.
    ///
    /// Called from the Ode to Joy Conductor boss PreDraw/PostDraw.
    /// Warm gold / radiant amber / jubilant light with rose pink accents.
    /// Garden/nature visual language with chromatic rainbow elements in Phase 2.
    /// </summary>
    public static class OdeToJoyBossShaderSystem
    {
        // Theme colors  Ewarm sunlit garden radiating joy
        private static readonly Color WarmGold = new Color(255, 200, 50);
        private static readonly Color RadiantAmber = new Color(240, 160, 40);
        private static readonly Color JubilantLight = new Color(255, 240, 200);
        private static readonly Color RosePink = new Color(230, 130, 150);

        /// <summary>
        /// Draws the garden aura  Ewarm golden radiance with floating petal accents.
        /// Phase 2 adds chromatic rainbow shimmer.
        /// </summary>
        public static void DrawGardenAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int phase, bool isPhase2)
        {
            float baseRadius = 85f + phase * 18f;
            float intensity = 0.35f + aggressionLevel * 0.4f;
            if (isPhase2)
            {
                intensity *= 1.4f;
                baseRadius *= 1.2f;
            }

            Color primary = isPhase2 ? JubilantLight : WarmGold;
            Color secondary = isPhase2 ? RosePink : RadiantAmber;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.OdeGardenAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.02f);
        }

        /// <summary>
        /// Draws the vine trail during dashes  Eintertwining vine patterns
        /// with golden leaves. Phase 2 adds rainbow petal streaks.
        /// </summary>
        public static void DrawVineTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isPhase2)
        {
            Color trailColor = isPhase2 ? RosePink : WarmGold;
            float width = isPhase2 ? 1.3f : 1.0f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.OdeVineTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.022f, 6f);
        }

        /// <summary>
        /// Draws the petal storm attack effect  Ea swirling cyclone of rose petals
        /// and golden light during the PetalStorm attack.
        /// </summary>
        public static void DrawPetalStorm(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.OdePetalStorm, RosePink, intensity,
                (float)Main.timeForVisualEffects * 0.03f);
        }

        /// <summary>
        /// Draws the chromatic bloom phase transition  Ea flower blooming
        /// with rainbow chromatic light revealing the conductor's true power.
        /// </summary>
        public static void DrawChromaticBloom(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress)
        {
            Color from = WarmGold;
            Color to = JubilantLight;
            float intensity = 1.0f + transitionProgress * 0.4f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.OdeChromaticBloom, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the jubilant dissolve death  Ethe Conductor dissolves into
        /// a shower of golden petals and radiant amber light.
        /// </summary>
        public static void DrawJubilantDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.OdeJubilantDissolve, dissolveProgress,
                WarmGold, 0.06f);
        }

        /// <summary>
        /// Spawns musical VFX particles  Ejoyful fanfares, harmonic blooms,
        /// and garden-themed rhythm accents.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int phase, bool isPhase2)
        {
            // Golden staff line convergence
            if (timer % 100 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, WarmGold, 0.5f + phase * 0.15f);
            }

            // Harmonic chords on beat
            if (timer % 60 == 0 && phase >= 1)
            {
                Phase10BossVFX.ChordResolutionBloom(npc.Center,
                    new[] { WarmGold, RadiantAmber, RosePink }, 0.6f);
            }

            // Phase 2  Ejubilant rainbow sparkle accents
            if (timer % 45 == 0 && isPhase2)
            {
                float hue = (float)(Main.timeForVisualEffects * 0.01) % 1f;
                Color rainbow = Main.hslToRgb(hue, 0.8f, 0.6f);
                CustomParticles.GenericFlare(npc.Center + Main.rand.NextVector2Circular(50f, 50f),
                    rainbow, 0.3f, 15);
            }

            // Rose petal accents
            if (timer % 80 == 0)
            {
                Phase10BossVFX.PizzicatoPop(npc.Center + Main.rand.NextVector2Circular(40f, 40f), RosePink);
            }
        }
    }
}
