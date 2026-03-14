using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.SwanLake.Bosses.Systems
{
    /// <summary>
    /// Swan Lake boss shader-driven rendering system — 4-phase visual identity.
    ///
    /// Phase 1 (White Swan, >60% HP): Clean geometric white glow, silver elegance.
    ///         Pure monochrome — the white swan's perfection.
    /// Phase 2 (Black Swan, 60-45% HP): White cracks. Black mirror emerges.
    ///         Prismatic bleed at fracture seams. Doubled glow layers.
    /// Phase 3 (Duality War, 45-30% HP): Rapid black/white alternation.
    ///         Both colors pulse in opposition. Fractal beam intensity peaks.
    /// Enrage (Death of Swan, <30% HP): Color drains. Everything fades to gray.
    ///         Only desperate prismatic flickers survive at impact moments.
    /// </summary>
    public static class SwanLakeBossShaderSystem
    {
        // Core palette — strictly monochrome + prismatic at destruction
        private static readonly Color VoidBlack = new Color(5, 5, 8);
        private static readonly Color PureWhite = new Color(245, 245, 255);
        private static readonly Color SilverMist = new Color(180, 185, 200);
        private static readonly Color GhostWhite = new Color(230, 230, 240);
        private static readonly Color PearlShimmer = new Color(250, 248, 255);
        private static readonly Color DrainedGray = new Color(100, 100, 105);

        /// <summary>
        /// Determines which visual phase the boss is in based on HP ratio.
        /// This drives all rendering decisions independent of the AI mood system.
        /// </summary>
        private static int GetVisualPhase(NPC npc)
        {
            float hpRatio = npc.life / (float)npc.lifeMax;
            if (hpRatio > 0.6f) return 1;  // White Swan
            if (hpRatio > 0.45f) return 2; // Black Swan
            if (hpRatio > 0.3f) return 3;  // Duality War
            return 4;                       // Death of Swan
        }

        private static float GetHPRatio(NPC npc) => npc.life / (float)npc.lifeMax;

        /// <summary>
        /// Gets prismatic rainbow color — used ONLY at moments of fracture/destruction.
        /// </summary>
        private static Color GetFractureRainbow(float offset = 0f)
        {
            float hue = ((float)Main.timeForVisualEffects * 0.006f + offset) % 1f;
            return Main.hslToRgb(hue, 0.85f, 0.8f);
        }

        /// <summary>
        /// Returns the phase-appropriate drain multiplier.
        /// Phase 4: everything dims toward gray.
        /// </summary>
        private static float GetDrainMultiplier(NPC npc)
        {
            float hpRatio = GetHPRatio(npc);
            if (hpRatio >= 0.3f) return 1f;
            return MathHelper.Lerp(1f, 0.25f, (0.3f - hpRatio) / 0.3f);
        }

        /// <summary>
        /// Drains a color toward gray based on enrage progress.
        /// </summary>
        private static Color ApplyDrain(Color c, float drain)
        {
            if (drain >= 1f) return c;
            float gray = (c.R + c.G + c.B) / 3f / 255f;
            return Color.Lerp(new Color(gray, gray, gray + 0.01f), c, drain);
        }

        /// <summary>
        /// Sets uPhase and uDrain uniforms on a boss shader effect.
        /// Call BEFORE BossRenderHelper methods — values persist on the cached Effect.
        /// </summary>
        private static void SetPhaseUniforms(string shaderKey, int phase, float drain)
        {
            Effect shader = BossShaderManager.GetShader(shaderKey);
            if (shader == null) return;
            shader.Parameters["uPhase"]?.SetValue((float)phase);
            shader.Parameters["uDrain"]?.SetValue(1f - drain); // shader expects 0=no drain, 1=full drain
        }

        // ===================================================================
        //  PRISMATIC AURA — The radiant field surrounding the boss
        // ===================================================================

        /// <summary>
        /// Phase 1: Clean silver-white aura, geometric precision.
        /// Phase 2: Aura fractures — white core with black counter-aura emerging.
        /// Phase 3: Rapid black/white alternation — aura pulses between extremes.
        /// Enrage: Aura fades to near-invisible gray. Rare prismatic flickers.
        /// </summary>
        public static void DrawPrismaticAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            int mood, int difficultyTier, bool isEnraged)
        {
            int phase = GetVisualPhase(npc);
            float drain = GetDrainMultiplier(npc);
            float hpDrive = 1f - GetHPRatio(npc);

            float baseRadius = phase switch
            {
                1 => 65f + difficultyTier * 12f,
                2 => 85f + difficultyTier * 20f,
                3 => 100f + difficultyTier * 25f,
                _ => 40f + difficultyTier * 8f
            };

            float intensity = phase switch
            {
                1 => 0.25f,
                2 => 0.4f,
                3 => 0.55f + (isEnraged ? 0.2f : 0f),
                _ => 0.12f * drain
            };

            // Phase-specific primary/secondary colors
            Color primary, secondary;
            switch (phase)
            {
                case 1: // Pure white geometric
                    primary = PureWhite;
                    secondary = SilverMist;
                    break;
                case 2: // White fracturing — prismatic at seams
                    primary = PureWhite;
                    secondary = GetFractureRainbow();
                    break;
                case 3: // Alternating black/white
                    float altPulse = (float)Math.Sin(Main.timeForVisualEffects * 0.04f);
                    primary = altPulse > 0 ? PureWhite : VoidBlack;
                    secondary = altPulse > 0 ? VoidBlack : PureWhite;
                    break;
                default: // Drained
                    primary = ApplyDrain(SilverMist, drain);
                    secondary = DrainedGray;
                    break;
            }

            SetPhaseUniforms(BossShaderManager.SwanPrismaticAura, phase, drain);
            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.SwanPrismaticAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.018f);

            // Multi-layer bloom stacking
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.02f);
                float radiusScale = baseRadius / 60f;

                // Outer aura
                Color outerColor = ApplyDrain(secondary, drain) with { A = 0 } * 0.15f * intensity * pulse;
                sb.Draw(bloomTex, drawPos, null, outerColor, 0f, bloomOrigin, radiusScale * 1.8f, SpriteEffects.None, 0f);

                // Mid layer
                Color midColor = ApplyDrain(primary, drain) with { A = 0 } * 0.2f * intensity * pulse;
                sb.Draw(bloomTex, drawPos, null, midColor, 0f, bloomOrigin, radiusScale * 1.1f, SpriteEffects.None, 0f);

                // Core
                Color coreColor = ApplyDrain(PearlShimmer, drain) with { A = 0 } * 0.25f * intensity * pulse;
                sb.Draw(bloomTex, drawPos, null, coreColor, 0f, bloomOrigin, radiusScale * 0.5f, SpriteEffects.None, 0f);

                // Phase 2: Black counter-aura emerging behind the white
                if (phase == 2)
                {
                    float blackEmergence = MathHelper.Clamp((0.6f - GetHPRatio(npc)) / 0.15f, 0f, 1f);
                    Color blackAura = VoidBlack with { A = 0 } * 0.12f * blackEmergence * pulse;
                    sb.Draw(bloomTex, drawPos, null, blackAura, 0f, bloomOrigin, radiusScale * 1.4f, SpriteEffects.None, 0f);
                }

                // Phase 3: Second opposing bloom layer
                if (phase == 3)
                {
                    Color opposingColor = secondary with { A = 0 } * 0.18f * intensity * pulse;
                    float oppPulse = 0.85f + 0.15f * (float)Math.Cos(Main.timeForVisualEffects * 0.04f);
                    sb.Draw(bloomTex, drawPos, null, opposingColor * oppPulse, 0f, bloomOrigin, radiusScale * 1.3f, SpriteEffects.None, 0f);
                }
            }
        }

        // ===================================================================
        //  FEATHER TRAIL — Trailing wisps behind boss movement
        // ===================================================================

        /// <summary>
        /// Phase 1: White feather wisps — elegant, precise.
        /// Phase 2: White + emerging black counter-trail.
        /// Phase 3: Interleaved black/white trail segments.
        /// Enrage: Trail nearly invisible — ghost of movement.
        /// </summary>
        public static void DrawFeatherTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, int mood)
        {
            int phase = GetVisualPhase(npc);
            float drain = GetDrainMultiplier(npc);

            Color trailColor = phase switch
            {
                1 => PureWhite,
                2 => GhostWhite,
                3 => ((float)Math.Sin(Main.timeForVisualEffects * 0.05f) > 0) ? PureWhite : VoidBlack,
                _ => ApplyDrain(SilverMist, drain) * 0.4f
            };

            float width = phase switch
            {
                1 => 0.8f,
                2 => 1.0f,
                3 => 1.3f,
                _ => 0.5f * drain
            };

            SetPhaseUniforms(BossShaderManager.SwanFeatherTrail, phase, drain);
            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.SwanFeatherTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, 4f);

            // Phase 2+: Draw mirrored black trail
            if (phase == 2)
            {
                float blackTrailAlpha = MathHelper.Clamp((0.6f - GetHPRatio(npc)) / 0.15f, 0f, 0.6f);
                Color blackTrail = VoidBlack * blackTrailAlpha;
                BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                    texture, sourceRect, origin,
                    BossShaderManager.SwanFeatherTrail, blackTrail, width * 0.7f,
                    (float)Main.timeForVisualEffects * 0.02f + 0.5f, 3f);
            }
        }

        // ===================================================================
        //  FRACTAL BEAM — MonochromaticApocalypse rotating beam
        // ===================================================================

        /// <summary>
        /// Phase 1-2: Pure white beam — geometric precision.
        /// Phase 3: Alternating black/white beam segments.
        /// Enrage: Beam drains to gray with prismatic flickers at edges.
        /// </summary>
        public static void DrawFractalBeam(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity, float rotation)
        {
            // We don't have NPC reference here, so use BossIndexTracker
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.SwanLakeFractal);
            float hpRatio = boss != null ? boss.life / (float)boss.lifeMax : 0.5f;
            int phase = hpRatio > 0.6f ? 1 : hpRatio > 0.45f ? 2 : hpRatio > 0.3f ? 3 : 4;
            float drain = hpRatio < 0.3f ? MathHelper.Lerp(1f, 0.25f, (0.3f - hpRatio) / 0.3f) : 1f;

            Color beamColor = phase switch
            {
                1 => PureWhite,
                2 => PureWhite,
                3 => ((float)Math.Sin(Main.timeForVisualEffects * 0.06f) > 0) ? PureWhite : VoidBlack,
                _ => ApplyDrain(SilverMist, drain)
            };

            SetPhaseUniforms(BossShaderManager.SwanFractalBeam, phase, drain);
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.SwanFractalBeam, beamColor, intensity * drain, rotation);

            // Enrage: desperate prismatic edge flickers
            if (phase == 4 && intensity > 0.3f)
            {
                Color prismEdge = GetFractureRainbow(rotation) with { A = 0 } * 0.15f;
                BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                    BossShaderManager.SwanFractalBeam, prismEdge, intensity * 0.3f, rotation);
            }
        }

        // ===================================================================
        //  MOOD TRANSITION — Visual phase change effect
        // ===================================================================

        /// <summary>
        /// Phase transitions feel like the swan's form cracking and reforming.
        /// Graceful→Tempest: White shatters, black emerges from the cracks.
        /// Tempest→DyingSwan: Both colors drain, leaving gray ghost.
        /// </summary>
        public static void DrawMoodTransition(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, int fromMood, int toMood)
        {
            // From color: phase that's ending
            Color from = fromMood == 0 ? PureWhite : (fromMood == 1 ? GhostWhite : SilverMist);
            // To color: phase that's arriving
            Color to = toMood == 0 ? PureWhite : (toMood == 1 ? VoidBlack : DrainedGray);

            // Transition to Tempest (Phase 2-3): intense crack effect
            // Transition to DyingSwan (Phase 4): draining, fading
            float intensity = toMood == 2 ? 0.5f : 1.0f;

            // Map AI mood to visual phase for the shader
            int targetPhase = toMood == 0 ? 1 : toMood == 1 ? 2 : 4;
            SetPhaseUniforms(BossShaderManager.SwanMoodTransition, targetPhase, 1f);
            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.SwanMoodTransition, transitionProgress,
                from, to, intensity);

            // Prismatic flash at the moment of fracture (mid-transition)
            if (transitionProgress > 0.3f && transitionProgress < 0.7f)
            {
                var bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null)
                {
                    Vector2 drawPos = npc.Center - screenPos;
                    Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
                    float flashIntensity = 1f - Math.Abs(transitionProgress - 0.5f) * 5f;
                    Color prismFlash = GetFractureRainbow() with { A = 0 } * flashIntensity * 0.3f;
                    sb.Draw(bloomTex, drawPos, null, prismFlash, 0f, bloomOrigin, 2.5f, SpriteEffects.None, 0f);
                }
            }
        }

        // ===================================================================
        //  MONOCHROME DISSOLVE — Death sequence rendering
        // ===================================================================

        /// <summary>
        /// The swan's final dissolution — beautiful, tragic, heartbreaking.
        /// Begins as stark monochrome dissolve. At 40%, desperate prismatic light
        /// breaks through the cracks. At 80%, rainbow consumes everything.
        /// The color that was denied throughout the fight finally erupts at death.
        /// </summary>
        public static void DrawMonochromeDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            // Phase 1 of dissolve (0-40%): Monochrome edge — white fragmenting to gray
            // Phase 2 of dissolve (40-80%): Prismatic light breaks through cracks
            // Phase 3 of dissolve (80-100%): Full rainbow eruption consuming form
            Color edgeColor;
            if (dissolveProgress < 0.4f)
                edgeColor = SilverMist;
            else if (dissolveProgress < 0.8f)
                edgeColor = Color.Lerp(SilverMist, GetFractureRainbow(dissolveProgress), (dissolveProgress - 0.4f) / 0.4f);
            else
                edgeColor = GetFractureRainbow(dissolveProgress * 3f);

            SetPhaseUniforms(BossShaderManager.SwanMonochromeDissolve, 4, 1f);
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.SwanMonochromeDissolve, dissolveProgress,
                edgeColor, 0.08f);

            // Bloom layers track the dissolve
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null && dissolveProgress > 0.15f)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;

                // Widening edge glow
                float glowScale = 0.4f + dissolveProgress * 1.2f;
                float glowAlpha = dissolveProgress * 0.35f;
                Color edgeGlow = edgeColor with { A = 0 } * glowAlpha;
                sb.Draw(bloomTex, drawPos, null, edgeGlow, 0f, bloomOrigin, glowScale, SpriteEffects.None, 0f);

                // Inner prismatic core after 40% — the beauty breaking free
                if (dissolveProgress > 0.4f)
                {
                    float coreProgress = (dissolveProgress - 0.4f) / 0.6f;
                    Color coreColor = GetFractureRainbow(dissolveProgress * 2f) with { A = 0 } * coreProgress * 0.5f;
                    sb.Draw(bloomTex, drawPos, null, coreColor, 0f, bloomOrigin, glowScale * 0.5f, SpriteEffects.None, 0f);
                }

                // Final burst at 90%+ — the last gasp of color
                if (dissolveProgress > 0.9f)
                {
                    float burstAlpha = (dissolveProgress - 0.9f) / 0.1f;
                    Color burstColor = Color.White with { A = 0 } * burstAlpha * 0.4f;
                    sb.Draw(bloomTex, drawPos, null, burstColor, 0f, bloomOrigin, glowScale * 2f, SpriteEffects.None, 0f);
                }
            }
        }

        // ===================================================================
        //  BOSS GLOW — Ambient bloom around the boss sprite
        // ===================================================================

        /// <summary>
        /// Phase 1: Clean silver-white 3-layer bloom. Geometric. Precise.
        /// Phase 2: White glow + emerging black counter-glow behind it.
        /// Phase 3: Glow rapidly alternates between white and black.
        /// Enrage: Glow drains to barely-visible gray. Ghost of former beauty.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, int mood)
        {
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;

            int phase = GetVisualPhase(npc);
            float drain = GetDrainMultiplier(npc);

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width, bloomTex.Height) * 0.5f;
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.timeForVisualEffects * 0.025f);

            Color outerColor, midColor, innerColor;
            float outerScale, midScale, innerScale;

            switch (phase)
            {
                case 1: // White Swan — pure geometric silver elegance
                    outerColor = SilverMist with { A = 0 } * 0.12f * pulse;
                    midColor = PureWhite with { A = 0 } * 0.18f * pulse;
                    innerColor = PearlShimmer with { A = 0 } * 0.2f * pulse;
                    outerScale = 1.6f;
                    midScale = 1.0f;
                    innerScale = 0.5f;
                    break;
                case 2: // Black Swan — white glow with dark counter-glow
                    outerColor = GhostWhite with { A = 0 } * 0.15f * pulse;
                    midColor = PureWhite with { A = 0 } * 0.2f * pulse;
                    innerColor = PearlShimmer with { A = 0 } * 0.22f * pulse;
                    outerScale = 1.7f;
                    midScale = 1.05f;
                    innerScale = 0.55f;
                    break;
                case 3: // Duality War — alternating pulse
                    float alt = (float)Math.Sin(Main.timeForVisualEffects * 0.04f);
                    Color activeColor = alt > 0 ? PureWhite : new Color(20, 20, 25);
                    outerColor = activeColor with { A = 0 } * 0.18f * pulse;
                    midColor = activeColor with { A = 0 } * 0.22f * pulse;
                    innerColor = PearlShimmer with { A = 0 } * 0.25f * pulse;
                    outerScale = 1.8f;
                    midScale = 1.1f;
                    innerScale = 0.6f;
                    break;
                default: // Death of Swan — drained, fading
                    outerColor = ApplyDrain(SilverMist, drain) with { A = 0 } * 0.08f * pulse;
                    midColor = ApplyDrain(GhostWhite, drain) with { A = 0 } * 0.1f * pulse;
                    innerColor = DrainedGray with { A = 0 } * 0.06f * pulse;
                    outerScale = 1.3f;
                    midScale = 0.8f;
                    innerScale = 0.35f;
                    break;
            }

            sb.Draw(bloomTex, drawPos, null, outerColor, 0f, bloomOrigin, outerScale, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, midColor, 0f, bloomOrigin, midScale, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, innerColor, 0f, bloomOrigin, innerScale, SpriteEffects.None, 0f);

            // Phase 2: Black counter-glow layer behind the white
            if (phase == 2)
            {
                float emergence = MathHelper.Clamp((0.6f - GetHPRatio(npc)) / 0.15f, 0f, 1f);
                Color blackGlow = VoidBlack with { A = 0 } * 0.1f * emergence * pulse;
                sb.Draw(bloomTex, drawPos, null, blackGlow, 0f, bloomOrigin, outerScale * 1.2f, SpriteEffects.None, 0f);
            }
        }

        // ===================================================================
        //  MUSICAL ACCENTS — Phase-driven particle choreography
        // ===================================================================

        /// <summary>
        /// Phase 1: White feather drift, silver sparkles — balletic grace.
        /// Phase 2: White + black feathers, prismatic at collision points.
        /// Phase 3: Rapid alternating black/white particle bursts.
        /// Enrage: Gray mourning feathers, rare desperate prismatic flickers.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier, int mood)
        {
            int phase = GetVisualPhase(npc);
            float hpRatio = GetHPRatio(npc);
            float hpDrive = 1f - hpRatio;
            float drain = GetDrainMultiplier(npc);

            switch (phase)
            {
                case 1: // White Swan — elegant precision
                    SpawnWhiteSwanAccents(npc, timer, difficultyTier, hpDrive);
                    break;
                case 2: // Black Swan — duality emerging
                    SpawnBlackSwanAccents(npc, timer, difficultyTier, hpDrive);
                    break;
                case 3: // Duality War — chaotic alternation
                    SpawnDualityWarAccents(npc, timer, difficultyTier, hpDrive);
                    break;
                default: // Death of Swan — mourning
                    SpawnDyingSwanAccents(npc, timer, difficultyTier, hpDrive, drain);
                    break;
            }

            // Bloom orbiting motes — always present, phase-colored
            if (timer % 12 == 0)
            {
                float angle = timer * 0.06f;
                float radius = MathHelper.Lerp(50f, 80f, hpDrive);
                Vector2 orbitPos = npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                Color orbitColor = phase switch
                {
                    1 => PureWhite,
                    2 => timer % 24 < 12 ? PureWhite : VoidBlack,
                    3 => ((float)Math.Sin(Main.timeForVisualEffects * 0.06f) > 0) ? PureWhite : VoidBlack,
                    _ => ApplyDrain(SilverMist, drain)
                };

                var bloom = new BloomParticle(orbitPos, Vector2.Zero, orbitColor * drain, 0.3f + hpDrive * 0.12f, 15);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        /// <summary>Phase 1: White feathers, silver sparkles, ballet grace.</summary>
        private static void SpawnWhiteSwanAccents(NPC npc, int timer, int difficultyTier, float hpDrive)
        {
            // White feather drift — balletic precision
            int featherInterval = Math.Max(1, (int)MathHelper.Lerp(100, 60, hpDrive));
            if (timer % featherInterval == 0)
            {
                CustomParticles.SwanFeatherDrift(npc.Center + Main.rand.NextVector2Circular(60f, 60f),
                    PureWhite, 0.4f + hpDrive * 0.15f);
            }

            // Silver sparkle accents
            int sparkleInterval = Math.Max(1, (int)MathHelper.Lerp(80, 50, hpDrive));
            if (timer % sparkleInterval == 0 && difficultyTier >= 1)
            {
                CustomParticles.PrismaticSparkle(npc.Center, SilverMist, 0.25f + hpDrive * 0.1f);
            }

            // Rising silver wisps
            if (hpDrive > 0.2f && timer % 15 == 0)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(70f, 35f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-1.8f, -0.8f));
                var sparkle = new SparkleParticle(pos, vel, SilverMist, 0.2f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        /// <summary>Phase 2: White + emerging black, prismatic at seams.</summary>
        private static void SpawnBlackSwanAccents(NPC npc, int timer, int difficultyTier, float hpDrive)
        {
            // White feathers still present
            int featherInterval = Math.Max(1, (int)MathHelper.Lerp(90, 50, hpDrive));
            if (timer % featherInterval == 0)
            {
                CustomParticles.SwanFeatherDrift(npc.Center + Main.rand.NextVector2Circular(70f, 70f),
                    PureWhite, 0.35f);
            }

            // Black feathers emerging — mirroring the white
            if (timer % featherInterval == featherInterval / 2)
            {
                CustomParticles.SwanFeatherDrift(npc.Center + Main.rand.NextVector2Circular(70f, 70f),
                    VoidBlack, 0.35f);
            }

            // Prismatic sparkles at collision/seam points between black and white
            int prismInterval = Math.Max(1, (int)MathHelper.Lerp(60, 35, hpDrive));
            if (timer % prismInterval == 0 && difficultyTier >= 1)
            {
                CustomParticles.PrismaticSparkle(npc.Center + Main.rand.NextVector2Circular(40f, 40f),
                    GetFractureRainbow(Main.rand.NextFloat()), 0.2f);
            }

            // Ballet grace with emerging darkness
            int balletInterval = Math.Max(1, (int)MathHelper.Lerp(90, 55, hpDrive));
            if (timer % balletInterval == 0)
            {
                Phase10Integration.SwanLake.BalletGrace(npc.Center, npc.velocity, timer);
            }
        }

        /// <summary>Phase 3: Rapid alternating black/white bursts, doubled particles.</summary>
        private static void SpawnDualityWarAccents(NPC npc, int timer, int difficultyTier, float hpDrive)
        {
            // Rapid alternating feathers — black and white in opposition
            int featherInterval = Math.Max(1, (int)MathHelper.Lerp(50, 25, hpDrive));
            bool isWhiteBeat = (timer / 15) % 2 == 0;

            if (timer % featherInterval == 0)
            {
                Color featherColor = isWhiteBeat ? PureWhite : VoidBlack;
                CustomParticles.SwanFeatherDrift(npc.Center + Main.rand.NextVector2Circular(80f, 80f),
                    featherColor, 0.4f);
            }

            // Prismatic at collision points — intensified
            if (timer % 20 == 0 && difficultyTier >= 1)
            {
                CustomParticles.PrismaticSparkle(npc.Center + Main.rand.NextVector2Circular(50f, 50f),
                    GetFractureRainbow(Main.rand.NextFloat()), 0.3f);
            }

            // Ascending wisps — alternating color
            if (timer % 8 == 0)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(90f, 45f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-2.5f, -1.2f));
                Color wispColor = isWhiteBeat ? GhostWhite : new Color(30, 30, 35);
                var sparkle = new SparkleParticle(pos, vel, wispColor, 0.22f + hpDrive * 0.12f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        /// <summary>Phase 4: Mourning gray feathers, rare desperate prismatic.</summary>
        private static void SpawnDyingSwanAccents(NPC npc, int timer, int difficultyTier, float hpDrive, float drain)
        {
            // Slow mourning feathers — drained of color
            int featherInterval = Math.Max(1, (int)MathHelper.Lerp(80, 50, hpDrive));
            if (timer % featherInterval == 0)
            {
                Color mourningColor = ApplyDrain(SilverMist, drain);
                CustomParticles.SwanFeatherDrift(npc.Center + Main.rand.NextVector2Circular(50f, 50f),
                    mourningColor, 0.3f);
            }

            // Dying swan melancholy
            int melancholyInterval = Math.Max(1, (int)MathHelper.Lerp(60, 30, hpDrive));
            if (timer % melancholyInterval == 0)
            {
                Phase10Integration.SwanLake.DyingSwanMelancholy(npc.Center, GetHPRatio(npc));
            }

            // Rare desperate prismatic flicker — the beauty that refuses to die
            if (timer % 40 == 0 && Main.rand.NextBool(3))
            {
                CustomParticles.PrismaticSparkle(npc.Center + Main.rand.NextVector2Circular(30f, 30f),
                    GetFractureRainbow(Main.rand.NextFloat()), 0.15f);
            }

            // Fading gray wisps — barely visible, rising like the last breath
            if (timer % 18 == 0)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(60f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.15f, 0.15f), Main.rand.NextFloat(-1.2f, -0.4f));
                var sparkle = new SparkleParticle(pos, vel, DrainedGray * 0.5f, 0.15f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
}
