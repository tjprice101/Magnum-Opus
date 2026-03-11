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
    /// Calamity-tier Eroica boss shader-driven rendering system.
    /// Multi-layered aura (inner core + mid glow + outer ambient), shimmering afterimage trails,
    /// phoenix wing flare, phase transition with sakura burst, health-driven visual escalation,
    /// and musical accent particles. All methods delegate to BossRenderHelper but add
    /// additional hand-drawn overlay layers for depth.
    /// </summary>
    public static class EroicaBossShaderSystem
    {
        // Theme colors
        private static readonly Color ValorGold = new Color(255, 200, 80);
        private static readonly Color ValorScarlet = new Color(200, 50, 50);
        private static readonly Color SakuraPink = new Color(255, 150, 180);
        private static readonly Color PhoenixWhite = new Color(255, 240, 220);
        private static readonly Color EmberOrange = new Color(255, 140, 30);

        /// <summary>
        /// Draws a multi-layered heroic valor aura behind the boss.
        /// Three layers: tight bright core, mid energy glow, wide ambient haze.
        /// Intensity scales with aggression level, HP tier, and adds a breathing pulse.
        /// </summary>
        public static void DrawValorAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 80f + difficultyTier * 20f;
            float intensity = 0.3f + aggressionLevel * 0.5f;
            if (isEnraged) intensity *= 1.5f;

            // Breathing pulse on the aura (subtle at high HP, pronounced at low)
            float lifeRatio = (float)npc.life / npc.lifeMax;
            float pulseSpeed = MathHelper.Lerp(1.2f, 2.5f, 1f - lifeRatio);
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * pulseSpeed) * MathHelper.Lerp(0.05f, 0.15f, 1f - lifeRatio);

            Color primary = isEnraged ? new Color(255, 100, 50) : ValorGold;
            Color secondary = isEnraged ? new Color(180, 20, 20) : ValorScarlet;

            // Layer 1: Shader-driven aura (core)
            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.EroicaValorAura, primary, secondary,
                baseRadius * pulse, intensity, (float)Main.timeForVisualEffects * 0.02f);

            // Layer 2 & 3: Additive bloom overlays for depth (no shader needed)
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);

                // Mid energy glow (gold)
                Color midColor = primary * (0.12f * intensity * pulse);
                midColor.A = 0;
                float midScale = baseRadius * 2.8f * pulse / glow.Width;
                sb.Draw(glow, drawPos, null, midColor, 0f, glowOrigin, midScale, SpriteEffects.None, 0f);

                // Wide ambient haze (scarlet tint)
                Color outerColor = secondary * (0.06f * intensity);
                outerColor.A = 0;
                float outerScale = baseRadius * 4.5f * pulse / glow.Width;
                sb.Draw(glow, drawPos, null, outerColor, 0f, glowOrigin, outerScale, SpriteEffects.None, 0f);

                // Enrage: additional crimson ring
                if (isEnraged)
                {
                    float enragePulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.12f;
                    Color enrageColor = new Color(200, 30, 10) * (0.1f * enragePulse);
                    enrageColor.A = 0;
                    sb.Draw(glow, drawPos, null, enrageColor, 0f, glowOrigin, baseRadius * 5.5f * enragePulse / glow.Width, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws the heroic flame trail during dashes and charges.
        /// Enhanced with shimmer variation: each afterimage has slight scale/color variation
        /// plus an additive glow overlay on each ghost for visual richness.
        /// </summary>
        public static void DrawHeroicTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? new Color(255, 80, 40) : ValorGold;
            float width = isEnraged ? 1.5f : 1.0f;

            // Shader trail (core afterimages)
            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EroicaHeroicTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, 6f);

            // Additional: soft glow orb at each afterimage position for bloom trail
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null && npc.velocity.Length() >= 6f)
            {
                Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
                int trailLen = Math.Min(npc.oldPos.Length, 10);
                for (int i = 0; i < trailLen; i++)
                {
                    if (npc.oldPos[i] == Vector2.Zero) continue;
                    float progress = (float)i / trailLen;
                    float alpha = (1f - progress) * 0.15f;
                    // Shimmer: alternate between gold and light sakura
                    Color shimmer = i % 2 == 0 ? trailColor : Color.Lerp(trailColor, SakuraPink, 0.3f);
                    Color c = shimmer * alpha;
                    c.A = 0;
                    Vector2 pos = npc.oldPos[i] + npc.Size / 2f - screenPos;
                    float scale = (1f - progress * 0.3f) * 0.15f;
                    sb.Draw(glow, pos, null, c, 0f, glowOrigin, scale, SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws a phoenix flame blast during PhoenixDive attack.
        /// Enhanced with multi-layered radial light rays and a golden core.
        /// </summary>
        public static void DrawPhoenixFlame(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            // Core flash (uses shader if available)
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.EroicaPhoenixFlame, ValorGold, intensity,
                (float)Main.timeForVisualEffects * 0.03f);

            // Additional bloom layers for phoenix wing flare
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null && intensity > 0.1f)
            {
                Vector2 drawPos = position - screenPos;
                Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);

                // Wide golden flare
                Color gold = ValorGold * (0.3f * intensity);
                gold.A = 0;
                sb.Draw(glow, drawPos, null, gold, 0f, glowOrigin, intensity * 0.6f, SpriteEffects.None, 0f);

                // Inner white-hot core
                Color white = PhoenixWhite * (0.5f * intensity);
                white.A = 0;
                sb.Draw(glow, drawPos, null, white, 0f, glowOrigin, intensity * 0.2f, SpriteEffects.None, 0f);

                // Directional flare (wing-like spread) using stretched draws
                float wingAngle = (float)Math.Atan2(0, 1); // Horizontal spread
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 wingOffset = new Vector2(side * 40f * intensity, -15f * intensity);
                    Color wingColor = EmberOrange * (0.15f * intensity);
                    wingColor.A = 0;
                    sb.Draw(glow, drawPos + wingOffset, null, wingColor, wingAngle, glowOrigin, new Vector2(intensity * 0.5f, intensity * 0.15f), SpriteEffects.None, 0f);
                }
            }
        }

        /// <summary>
        /// Draws the sakura petal phase transition effect.
        /// Enhanced with secondary bloom ring and screen-edge golden flash.
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

            // Additional bloom ring expanding outward
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null && transitionProgress > 0f && transitionProgress < 1f)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
                float ringProgress = (float)Math.Sin(transitionProgress * MathHelper.Pi);

                // Golden expanding ring
                Color ringColor = Color.Lerp(from, to, transitionProgress) * (ringProgress * 0.25f * intensity);
                ringColor.A = 0;
                float ringScale = (0.2f + transitionProgress * 0.8f) * intensity;
                sb.Draw(glow, drawPos, null, ringColor, 0f, glowOrigin, ringScale, SpriteEffects.None, 0f);

                // On phase 2 transitions, trigger sky flash
                if (isPhase2Transition && transitionProgress > 0.4f && transitionProgress < 0.6f)
                    EroicaSkySystem.TriggerGoldenFlash(0.7f);
            }
        }

        /// <summary>
        /// Draws the heroic death dissolve effect.
        /// Enhanced with golden edge glow that brightens as dissolve progresses.
        /// </summary>
        public static void DrawDeathDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            // Wider edge glow as dissolve progresses — golden hero's last stand
            float edgeWidth = MathHelper.Lerp(0.04f, 0.12f, dissolveProgress);
            Color edgeColor = Color.Lerp(ValorGold, PhoenixWhite, dissolveProgress * 0.7f);

            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EroicaDeathDissolveFx, dissolveProgress,
                edgeColor, edgeWidth);

            // Ambient bloom intensifies as hero dissolves into light
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null && dissolveProgress > 0.2f)
            {
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
                float bloomStrength = (dissolveProgress - 0.2f) / 0.8f;
                Color bloomCol = PhoenixWhite * (0.2f * bloomStrength);
                bloomCol.A = 0;
                sb.Draw(glow, drawPos, null, bloomCol, 0f, glowOrigin, 0.3f + bloomStrength * 0.5f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a multi-layered NPC glow behind the boss sprite.
        /// Called from PreDraw before the main sprite. Creates the "radiant hero" look.
        /// Three stacked blooms: tight white core, gold mid, scarlet ambient.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, float lifeRatio, bool isEnraged)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.8f) * 0.06f;

            // Intensity increases as HP drops
            float hpIntensity = MathHelper.Lerp(0.6f, 1.3f, 1f - lifeRatio);

            // Layer 1: Tight white-gold core
            Color core = PhoenixWhite * (0.18f * hpIntensity * pulse);
            core.A = 0;
            sb.Draw(glow, drawPos, null, core, 0f, glowOrigin, 0.06f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Gold mid glow
            Color mid = ValorGold * (0.1f * hpIntensity);
            mid.A = 0;
            sb.Draw(glow, drawPos, null, mid, 0f, glowOrigin, 0.12f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Scarlet ambient
            Color ambient = (isEnraged ? new Color(220, 40, 30) : ValorScarlet) * (0.05f * hpIntensity);
            ambient.A = 0;
            sb.Draw(glow, drawPos, null, ambient, 0f, glowOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws heroic flame wings during PhoenixDive and enraged states.
        /// Two stretched bloom draws angled behind the boss to suggest fiery wings.
        /// </summary>
        public static void DrawPhoenixWings(SpriteBatch sb, NPC npc, Vector2 screenPos, float intensity)
        {
            if (intensity <= 0.05f) return;

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
            float time = (float)Main.GlobalTimeWrappedHourly;
            float wingFlap = (float)Math.Sin(time * 3f) * 0.15f;

            for (int side = -1; side <= 1; side += 2)
            {
                float baseAngle = side * (0.8f + wingFlap);
                Vector2 wingOffset = new Vector2(side * 35f * intensity, -20f * intensity);

                // Outer wing (ember orange)
                Color outer = EmberOrange * (0.12f * intensity);
                outer.A = 0;
                sb.Draw(glow, drawPos + wingOffset, null, outer, baseAngle, glowOrigin,
                    new Vector2(intensity * 0.4f, intensity * 0.1f), SpriteEffects.None, 0f);

                // Inner wing (bright gold)
                Color inner = ValorGold * (0.18f * intensity);
                inner.A = 0;
                sb.Draw(glow, drawPos + wingOffset * 0.5f, null, inner, baseAngle * 0.7f, glowOrigin,
                    new Vector2(intensity * 0.25f, intensity * 0.07f), SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Spawns musical VFX particles during various boss states.
        /// Enhanced with health-driven frequency and additional musical motifs.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier)
        {
            float lifeRatio = (float)npc.life / npc.lifeMax;

            // Staff line convergence — faster cadence at lower HP
            int staffInterval = lifeRatio > 0.5f ? 120 : 80;
            if (timer % staffInterval == 0)
            {
                float staffIntensity = 0.5f + difficultyTier * 0.2f + (1f - lifeRatio) * 0.3f;
                Phase10BossVFX.StaffLineConvergence(npc.Center, ValorGold, staffIntensity);
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

            // Low HP: crescendo danger rings (telegraphing the hero's final stand)
            if (lifeRatio < 0.3f && timer % 45 == 0)
            {
                Phase10BossVFX.CrescendoDangerRings(npc.Center, ValorScarlet, 0.8f);
            }

            // Ambient golden bloom particles near boss
            if (timer % 30 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(80f, 80f);
                Vector2 vel = -Vector2.UnitY * Main.rand.NextFloat(0.3f, 0.8f);
                Color col = Color.Lerp(ValorGold, EmberOrange, Main.rand.NextFloat());
                var bloom = new BloomParticle(npc.Center + offset, vel, col, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(40, 80));
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }
    }
}
