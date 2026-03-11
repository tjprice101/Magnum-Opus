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
        /// Draws the prismatic aura with rainbow edge shimmer plus multi-layer bloom stacking.
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

            // Multi-layer bloom stacking over the shader aura
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.02f);
                float radiusScale = baseRadius / 60f;

                // Wide outer prismatic shimmer
                Color outerColor = secondary with { A = 0 } * 0.15f * intensity * pulse;
                sb.Draw(bloomTex, drawPos, null, outerColor, 0f, bloomOrigin, radiusScale * 1.8f, SpriteEffects.None, 0f);

                // Mid silver/white aura
                Color midColor = primary with { A = 0 } * 0.2f * intensity * pulse;
                sb.Draw(bloomTex, drawPos, null, midColor, 0f, bloomOrigin, radiusScale * 1.1f, SpriteEffects.None, 0f);

                // Tight bright core
                Color coreColor = PearlShimmer with { A = 0 } * 0.25f * intensity * pulse;
                sb.Draw(bloomTex, drawPos, null, coreColor, 0f, bloomOrigin, radiusScale * 0.5f, SpriteEffects.None, 0f);
            }
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
        /// Draws the epic 10-second monochrome dissolve death — grayscale to rainbow shatter.
        /// Enhanced with widening edge glow and inner prismatic core after 40% dissolve.
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

            // Edge glow widens as dissolve progresses
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null && dissolveProgress > 0.2f)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                float glowScale = 0.4f + dissolveProgress * 0.8f;
                float glowAlpha = dissolveProgress * 0.4f;

                Color edgeGlow = edgeColor with { A = 0 } * glowAlpha;
                sb.Draw(bloomTex, drawPos, null, edgeGlow, 0f, bloomOrigin, glowScale, SpriteEffects.None, 0f);

                // Inner prismatic revelation core after 40%
                if (dissolveProgress > 0.4f)
                {
                    float coreAlpha = (dissolveProgress - 0.4f) / 0.6f * 0.5f;
                    Color coreColor = GetPrismaticColor(dissolveProgress * 2f) with { A = 0 } * coreAlpha;
                    sb.Draw(bloomTex, drawPos, null, coreColor, 0f, bloomOrigin, glowScale * 0.5f, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws a 3-layer bloom glow around the boss sprite.
        /// Mood-aware: Graceful=silver elegance, Tempest=fierce white, DyingSwan=fading prismatic.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, int mood)
        {
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.timeForVisualEffects * 0.025f);

            Color outerColor, midColor, innerColor;
            float outerScale, midScale, innerScale;

            switch (mood)
            {
                case 0: // Graceful — silver moonlit elegance
                    outerColor = SilverMoon with { A = 0 } * 0.12f * pulse;
                    midColor = PureWhite with { A = 0 } * 0.18f * pulse;
                    innerColor = PearlShimmer with { A = 0 } * 0.2f * pulse;
                    outerScale = 1.6f;
                    midScale = 1.0f;
                    innerScale = 0.5f;
                    break;
                case 1: // Tempest — fierce bright white
                    outerColor = PureWhite with { A = 0 } * 0.18f * pulse;
                    midColor = Color.White with { A = 0 } * 0.22f * pulse;
                    innerColor = PearlShimmer with { A = 0 } * 0.28f * pulse;
                    outerScale = 1.8f;
                    midScale = 1.1f;
                    innerScale = 0.6f;
                    break;
                default: // DyingSwan — fading prismatic ghost
                    Color prismatic = GetPrismaticColor();
                    outerColor = prismatic with { A = 0 } * 0.1f * pulse;
                    midColor = SilverMoon with { A = 0 } * 0.15f * pulse;
                    innerColor = PureWhite with { A = 0 } * 0.12f * pulse;
                    outerScale = 1.4f;
                    midScale = 0.9f;
                    innerScale = 0.4f;
                    break;
            }

            sb.Draw(bloomTex, drawPos, null, outerColor, 0f, bloomOrigin, outerScale, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, midColor, 0f, bloomOrigin, midScale, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, innerColor, 0f, bloomOrigin, innerScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Spawns musical VFX particles during various boss states.
        /// HP-driven frequency: particles intensify as boss takes damage.
        /// Bloom orbiting motes and mood-specific accents.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier, int mood)
        {
            float hpRatio = npc.life / (float)npc.lifeMax;
            float hpDrive = 1f - hpRatio;

            // Graceful feather drift — HP-driven interval (120→60 frames)
            int featherInterval = Math.Max(1, (int)MathHelper.Lerp(120, 60, hpDrive));
            if (timer % featherInterval == 0)
            {
                CustomParticles.SwanFeatherDrift(npc.Center + Main.rand.NextVector2Circular(60f, 60f),
                    mood == 2 ? SilverMoon : PureWhite, 0.4f + hpDrive * 0.2f);
            }

            // Prismatic sparkle accents — HP-driven interval (80→40 frames)
            int sparkleInterval = Math.Max(1, (int)MathHelper.Lerp(80, 40, hpDrive));
            if (timer % sparkleInterval == 0 && difficultyTier >= 1)
            {
                CustomParticles.PrismaticSparkle(npc.Center, GetPrismaticColor(), 0.3f + hpDrive * 0.15f);
            }

            // Ballet grace musical motifs during Tempest
            int balletInterval = Math.Max(1, (int)MathHelper.Lerp(90, 50, hpDrive));
            if (timer % balletInterval == 0 && mood == 1)
            {
                Phase10Integration.SwanLake.BalletGrace(npc.Center, npc.velocity, timer);
            }

            // Dying Swan melancholy at low HP
            int melancholyInterval = Math.Max(1, (int)MathHelper.Lerp(60, 30, hpDrive));
            if (timer % melancholyInterval == 0 && mood == 2)
            {
                Phase10Integration.SwanLake.DyingSwanMelancholy(npc.Center, hpRatio);
            }

            // Bloom orbiting motes — silver/white particles orbiting the boss
            if (timer % 12 == 0)
            {
                float angle = timer * 0.06f;
                float radius = MathHelper.Lerp(50f, 80f, hpDrive);
                Vector2 orbitPos = npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Color orbitColor = mood == 2 ? GetPrismaticColor(angle) : PureWhite;
                var bloom = new BloomParticle(orbitPos, Vector2.Zero, orbitColor, 0.3f + hpDrive * 0.15f, 15);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Low-HP ascending sparkle wisps — ethereal feather-like particles rising
            if (hpDrive > 0.5f && timer % 8 == 0)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(80f, 40f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-2.5f, -1.2f));
                Color sparkColor = mood == 2 ? GetPrismaticColor(Main.rand.NextFloat()) : SilverMoon;
                var sparkle = new SparkleParticle(pos, vel, sparkColor, 0.2f + hpDrive * 0.15f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
}
