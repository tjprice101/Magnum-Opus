using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Nachtmusik.Bosses.Systems
{
    /// <summary>
    /// Nachtmusik boss shader-driven rendering system.
    /// Manages all 5 boss shaders: NachtmusikStarfieldAura, NachtmusikNebulaDashTrail,
    /// NachtmusikSupernovaBlast, NachtmusikPhase2Awakening, and NachtmusikStellarDissolve.
    ///
    /// Phase 1 (serene nocturnal) vs Phase 2 (violent cosmic storm) visual differentiation.
    /// Phase 2 activates after the fake-death sequence.
    /// </summary>
    public static class NachtmusikBossShaderSystem
    {
        // Theme colors  Edeep indigo night sky with starlight silver and cosmic blue
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color StarlightSilver = new Color(200, 210, 230);
        private static readonly Color CosmicBlue = new Color(80, 120, 200);
        private static readonly Color NebulaGold = new Color(220, 180, 100);

        /// <summary>
        /// Draws the starfield aura  Egentle in Phase 1, violent stellar storm in Phase 2.
        /// </summary>
        public static void DrawStarfieldAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int phase, bool isPhase2)
        {
            float baseRadius = 90f + phase * 15f;
            float intensity = 0.3f + aggressionLevel * 0.4f;
            if (isPhase2)
            {
                intensity *= 1.6f;
                baseRadius *= 1.4f;
            }

            Color primary = isPhase2 ? CosmicBlue : DeepIndigo;
            Color secondary = isPhase2 ? NebulaGold : StarlightSilver;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.NachtmusikStarfieldAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.018f);
        }

        /// <summary>
        /// Draws the nebula dash trail  Esilver streaks in Phase 1,
        /// violent blue-gold nebula wake in Phase 2.
        /// </summary>
        public static void DrawNebulaDashTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isPhase2)
        {
            Color trailColor = isPhase2 ? NebulaGold : StarlightSilver;
            float width = isPhase2 ? 1.6f : 1.0f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.NachtmusikNebulaDashTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, 7f);
        }

        /// <summary>
        /// Draws the supernova blast during SupernovaCollapse attack.
        /// Blinding stellar flash with radiating energy rings.
        /// </summary>
        public static void DrawSupernovaBlast(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.NachtmusikSupernovaBlast, StarlightSilver, intensity,
                (float)Main.timeForVisualEffects * 0.035f);
        }

        /// <summary>
        /// Draws the Phase 2 awakening  Ethe Queen rises from fake death with
        /// stellar fury, nebula clouds swirling around her.
        /// </summary>
        public static void DrawPhase2Awakening(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress)
        {
            Color from = DeepIndigo;
            Color to = NebulaGold;
            float intensity = 1.0f + transitionProgress * 0.5f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.NachtmusikPhase2Awakening, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the stellar dissolve death effect  Ethe Queen collapses into
        /// a shower of fading starlight.
        /// </summary>
        public static void DrawStellarDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.NachtmusikStellarDissolve, dissolveProgress,
                StarlightSilver, 0.05f);
        }

        /// <summary>
        /// Spawns musical VFX particles  Enocturnal serenades, stellar rhythm, constellation accents.
        /// Phase 2 dramatically intensifies all effects.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int phase, bool isPhase2)
        {
            // Gentle starlight convergence
            if (timer % 100 == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center,
                    isPhase2 ? CosmicBlue : StarlightSilver, 0.5f + phase * 0.15f);
            }

            // Rhythmic metronome
            if (timer % 60 == 0 && phase >= 1)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center,
                    isPhase2 ? NebulaGold : DeepIndigo, 3, 4);
            }

            // Phase 2  Ecosmic storm accents
            if (timer % 40 == 0 && isPhase2)
            {
                CustomParticles.GenericFlare(npc.Center + Main.rand.NextVector2Circular(60f, 60f),
                    Color.Lerp(CosmicBlue, NebulaGold, Main.rand.NextFloat()), 0.3f, 15);
            }

            // Nebula streaks in Phase 2
            if (timer % 30 == 0 && isPhase2)
            {
                Phase10BossVFX.SforzandoSpike(npc.Center, NebulaGold, 0.6f);
            }
        }
    }
}
