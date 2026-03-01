using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.SwanLake.Bosses.Systems
{
    /// <summary>
    /// Swan Lake boss shader-driven rendering system.
    /// Manages all 5 boss shaders: SwanPrismaticAura, SwanFeatherTrail,
    /// SwanFractalBeam, SwanMoodTransition, SwanMonochromeDissolve.
    ///
    /// Swan Lake's three moods (Graceful, Tempest, DyingSwan) drive
    /// different visual intensities and color shifts in each shader.
    /// </summary>
    public static class SwanLakeBossShaderSystem
    {
        // Theme colors
        private static readonly Color PureWhite = new Color(240, 240, 255);
        private static readonly Color JetBlack = new Color(15, 15, 20);
        private static readonly Color PearlShimmer = new Color(250, 248, 255);
        private static readonly Color SilverMoon = new Color(200, 210, 230);

        /// <summary>
        /// Gets the prismatic hue based on current time for rainbow edge effects.
        /// </summary>
        private static Color GetPrismaticColor(float offset = 0f)
        {
            float hue = ((float)Main.timeForVisualEffects * 0.005f + offset) % 1f;
            return Main.hslToRgb(hue, 0.6f, 0.8f);
        }

        /// <summary>
        /// Draws the prismatic aura with rainbow edge shimmer.
        /// Intensity varies by mood: Graceful=soft, Tempest=fierce, DyingSwan=fading.
        /// </summary>
        public static void DrawPrismaticAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            int mood, int difficultyTier, bool isEnraged)
        {
            float baseRadius = mood switch
            {
                0 => 70f + difficultyTier * 15f,  // Graceful: elegant, restrained
                1 => 100f + difficultyTier * 25f,  // Tempest: fierce, expanding
                _ => 50f + difficultyTier * 10f     // DyingSwan: fading, delicate
            };

            float intensity = mood switch
            {
                0 => 0.3f,
                1 => 0.6f + (isEnraged ? 0.3f : 0f),
                _ => 0.2f
            };

            Color primary = mood == 2 ? SilverMoon : PureWhite;
            Color secondary = GetPrismaticColor();

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.SwanPrismaticAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.018f);
        }

        /// <summary>
        /// Draws white feather wisps with rainbow edges during movement.
        /// </summary>
        public static void DrawFeatherTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, int mood)
        {
            Color trailColor = mood == 2 ? SilverMoon * 0.6f : PureWhite;
            float width = mood == 1 ? 1.4f : 0.9f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.SwanFeatherTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, 4f);
        }

        /// <summary>
        /// Draws the fractal beam for MonochromaticApocalypse rotating beam attack.
        /// </summary>
        public static void DrawFractalBeam(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity, float rotation)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.SwanFractalBeam, PureWhite, intensity,
                rotation);
        }

        /// <summary>
        /// Draws the mood transition effect during Graceful→Tempest→DyingSwan changes.
        /// </summary>
        public static void DrawMoodTransition(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, int fromMood, int toMood)
        {
            Color from = fromMood == 0 ? PureWhite : (fromMood == 1 ? GetPrismaticColor() : SilverMoon);
            Color to = toMood == 0 ? PureWhite : (toMood == 1 ? GetPrismaticColor(0.5f) : SilverMoon);
            float intensity = toMood == 1 ? 1.3f : 0.7f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.SwanMoodTransition, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Draws the epic 10-second monochrome dissolve death  Egrayscale to rainbow shatter.
        /// </summary>
        public static void DrawMonochromeDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            // Transition from grayscale edge to prismatic shatter
            Color edgeColor = dissolveProgress < 0.6f ? SilverMoon : GetPrismaticColor(dissolveProgress);

            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.SwanMonochromeDissolve, dissolveProgress,
                edgeColor, 0.08f);
        }

        /// <summary>
        /// Spawns musical VFX particles during various boss states.
        /// Feathers and prismatic sparkles drift gracefully around the boss.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier, int mood)
        {
            // Graceful feather drift every 2 seconds
            if (timer % 120 == 0)
            {
                CustomParticles.SwanFeatherDrift(npc.Center + Main.rand.NextVector2Circular(60f, 60f),
                    mood == 2 ? SilverMoon : PureWhite, 0.4f);
            }

            // Prismatic sparkle accents
            if (timer % 80 == 0 && difficultyTier >= 1)
            {
                CustomParticles.PrismaticSparkle(npc.Center, GetPrismaticColor(), 0.3f);
            }

            // Ballet grace musical motifs during Tempest
            if (timer % 90 == 0 && mood == 1)
            {
                Phase10Integration.SwanLake.BalletGrace(npc.Center, npc.velocity, timer);
            }

            // Dying Swan melancholy at low HP
            if (timer % 60 == 0 && mood == 2)
            {
                float hpRatio = npc.life / (float)npc.lifeMax;
                Phase10Integration.SwanLake.DyingSwanMelancholy(npc.Center, hpRatio);
            }
        }
    }
}
