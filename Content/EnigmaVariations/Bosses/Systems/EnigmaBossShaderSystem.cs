using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.EnigmaVariations.Bosses.Systems
{
    /// <summary>
    /// Enigma Variations boss shader-driven rendering system.
    /// Manages all 5 boss shaders: EnigmaVoidAura, EnigmaShadowTrail,
    /// EnigmaParadoxRift, EnigmaTeleportWarp, EnigmaUnveilingDissolve.
    ///
    /// Called from EnigmaTheHollowMystery.PreDraw/PostDraw to layer shader
    /// effects on top of the standard sprite drawing.
    /// </summary>
    public static class EnigmaBossShaderSystem
    {
        // Theme colors
        private static readonly Color VoidBlack = new Color(15, 5, 25);
        private static readonly Color DeepPurple = new Color(100, 30, 150);
        private static readonly Color EerieGreen = new Color(80, 200, 100);
        private static readonly Color MysteryWhite = new Color(200, 180, 220);

        /// <summary>
        /// Draws the swirling void aura with watching eye patterns.
        /// Intensity scales with phase  Emore eyes appear at higher phases.
        /// </summary>
        public static void DrawVoidAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 85f + difficultyTier * 22f;
            float intensity = 0.3f + aggressionLevel * 0.5f;
            if (isEnraged) intensity *= 1.4f;

            Color primary = isEnraged ? EerieGreen : DeepPurple;
            Color secondary = isEnraged ? MysteryWhite : VoidBlack;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.EnigmaVoidAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.012f);
        }

        /// <summary>
        /// Draws the dark teleport shadow trail during dashes and teleports.
        /// </summary>
        public static void DrawShadowTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? EerieGreen : DeepPurple;
            float width = isEnraged ? 1.3f : 0.9f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EnigmaShadowTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.018f, 3f);
        }

        /// <summary>
        /// Draws the paradox rift visual during reality-tearing attacks.
        /// </summary>
        public static void DrawParadoxRift(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.EnigmaParadoxRift, EerieGreen, intensity,
                (float)Main.timeForVisualEffects * 0.025f);
        }

        /// <summary>
        /// Draws the teleport warp-in/warp-out animation effect.
        /// </summary>
        public static void DrawTeleportWarp(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float warpProgress, bool isWarpingIn)
        {
            Color from = isWarpingIn ? VoidBlack : DeepPurple;
            Color to = isWarpingIn ? DeepPurple : VoidBlack;
            float intensity = isWarpingIn ? 0.8f : 1.1f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.EnigmaTeleportWarp, warpProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the unveiling dissolve death animation  Emystery revealed at last.
        /// Dissolves from void into eerie green revelation light.
        /// </summary>
        public static void DrawUnveilingDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            // Edge transitions from purple to green as the mystery unravels
            Color edgeColor = Color.Lerp(DeepPurple, EerieGreen, dissolveProgress);

            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EnigmaUnveilingDissolve, dissolveProgress,
                edgeColor, 0.06f);
        }

        /// <summary>
        /// Spawns musical VFX particles during various boss states.
        /// Eerie eyes watch from the void, arcane glyphs orbit the boss.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            // Mysterious trill vibrations every 2 seconds
            if (timer % 120 == 0)
            {
                Phase10Integration.Enigma.MysteriousTrill(npc.Center, timer);
            }

            // Paradox syncopation rhythmic accents
            if (timer % 60 == 0 && difficultyTier >= 1)
            {
                Phase10Integration.Enigma.ParadoxSyncopation(npc.Center, 120f);
            }

            // Watching eyes spawn near the boss at higher tiers
            if (timer % 90 == 0 && difficultyTier >= 2)
            {
                CustomParticles.EnigmaEyeGaze(
                    npc.Center + Main.rand.NextVector2Circular(80f, 80f),
                    EerieGreen, 0.4f);
            }

            // Orbiting glyphs
            if (timer % 150 == 0 && difficultyTier >= 1)
            {
                CustomParticles.GlyphCircle(npc.Center, DeepPurple, 4, 60f, 0.02f);
            }
        }
    }
}
