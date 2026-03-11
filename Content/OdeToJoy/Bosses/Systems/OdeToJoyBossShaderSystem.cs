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
    /// Garden aura, vine trail, petal storm, chromatic bloom, jubilant dissolve,
    /// boss glow, and HP-driven musical accents.
    /// </summary>
    public static class OdeToJoyBossShaderSystem
    {
        // Theme colors
        private static readonly Color WarmGold = new Color(255, 200, 50);
        private static readonly Color RadiantAmber = new Color(240, 160, 40);
        private static readonly Color JubilantLight = new Color(255, 240, 200);
        private static readonly Color RosePink = new Color(230, 130, 150);

        /// <summary>
        /// Draws the garden aura with 3-layer bloom stacking.
        /// Outer warm amber halo, mid golden pulse, inner jubilant core.
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

            // 3-layer bloom stacking
            Vector2 drawPos = npc.Center - screenPos;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 origin = glow.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.05f) * 0.12f;
            float hpDrive = 1f - (npc.life / (float)npc.lifeMax);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer: warm amber halo
            float outerScale = (baseRadius / glow.Width) * 3.2f * pulse;
            Color outerColor = RadiantAmber * (0.1f + hpDrive * 0.06f);
            outerColor.A = 0;
            sb.Draw(glow, drawPos, null, outerColor, 0f, origin, outerScale, SpriteEffects.None, 0f);

            // Mid: golden pulse (Phase 2 shifts to rose)
            float midPulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.08f) * 0.18f;
            float midScale = (baseRadius / glow.Width) * 2.0f * midPulse;
            Color midColor = (isPhase2 ? RosePink : WarmGold) * (0.14f + hpDrive * 0.08f);
            midColor.A = 0;
            sb.Draw(glow, drawPos, null, midColor, 0f, origin, midScale, SpriteEffects.None, 0f);

            // Core: jubilant light center
            float coreScale = (baseRadius / glow.Width) * 0.9f;
            Color coreColor = JubilantLight * (0.12f + hpDrive * 0.1f);
            coreColor.A = 0;
            sb.Draw(glow, drawPos, null, coreColor, 0f, origin, coreScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws the vine trail during dashes.
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
        /// Draws the petal storm attack flash.
        /// </summary>
        public static void DrawPetalStorm(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.OdePetalStorm, RosePink, intensity,
                (float)Main.timeForVisualEffects * 0.03f);
        }

        /// <summary>
        /// Draws the chromatic bloom phase transition.
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
        /// Draws the jubilant dissolve death with widening edge glow.
        /// </summary>
        public static void DrawJubilantDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.OdeJubilantDissolve, dissolveProgress,
                WarmGold, 0.06f);

            // Widening edge glow past 20%
            if (dissolveProgress > 0.2f)
            {
                float edgeIntensity = (dissolveProgress - 0.2f) / 0.8f;
                Vector2 drawPos = npc.Center - screenPos;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                Vector2 glowOrigin = glow.Size() / 2f;
                float glowScale = 0.7f + edgeIntensity * 1.4f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Color edgeColor = Color.Lerp(WarmGold, JubilantLight, edgeIntensity) * (0.15f + edgeIntensity * 0.25f);
                edgeColor.A = 0;
                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin, glowScale, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Draws the boss glow underlay.
        /// Phase 1: warm golden garden glow.
        /// Phase 2: chromatic rose radiance.
        /// Called as SHADER LAYER 0.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, bool isPhase2)
        {
            Vector2 drawPos = npc.Center - screenPos;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 origin = glow.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.06f) * 0.1f;
            float hpDrive = 1f - (npc.life / (float)npc.lifeMax);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (isPhase2)
            {
                // Outer: rose-pink halo
                Color outerColor = RosePink * (0.1f + hpDrive * 0.06f);
                outerColor.A = 0;
                sb.Draw(glow, drawPos, null, outerColor, 0f, origin, 2.0f * pulse, SpriteEffects.None, 0f);

                // Mid: chromatic golden
                float midPulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.09f) * 0.15f;
                Color midColor = WarmGold * (0.14f + hpDrive * 0.08f);
                midColor.A = 0;
                sb.Draw(glow, drawPos, null, midColor, 0f, origin, 1.3f * midPulse, SpriteEffects.None, 0f);

                // Core: jubilant white
                Color coreColor = JubilantLight * (0.12f + hpDrive * 0.08f);
                coreColor.A = 0;
                sb.Draw(glow, drawPos, null, coreColor, 0f, origin, 0.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Outer: warm amber
                Color outerColor = RadiantAmber * (0.08f + hpDrive * 0.04f);
                outerColor.A = 0;
                sb.Draw(glow, drawPos, null, outerColor, 0f, origin, 1.6f * pulse, SpriteEffects.None, 0f);

                // Core: golden center
                float corePulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.12f;
                Color coreColor = WarmGold * (0.1f + hpDrive * 0.06f);
                coreColor.A = 0;
                sb.Draw(glow, drawPos, null, coreColor, 0f, origin, 0.8f * corePulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Spawns musical accent particles with HP-driven intensity scaling.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int phase, bool isPhase2)
        {
            float hpDrive = 1f - (npc.life / (float)npc.lifeMax);

            // Golden staff line convergence - interval shrinks
            int staffInterval = Math.Max(1, (int)(100 - hpDrive * 50));
            if (timer % staffInterval == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center, WarmGold, 0.5f + phase * 0.15f + hpDrive * 0.2f);
            }

            // Harmonic chords
            int chordInterval = Math.Max(1, (int)(60 - hpDrive * 30));
            if (timer % chordInterval == 0 && phase >= 1)
            {
                Phase10BossVFX.ChordResolutionBloom(npc.Center,
                    new[] { WarmGold, RadiantAmber, RosePink }, 0.6f + hpDrive * 0.3f);
            }

            // Phase 2 rainbow sparkle accents
            int rainbowInterval = Math.Max(1, (int)(45 - hpDrive * 22));
            if (timer % rainbowInterval == 0 && isPhase2)
            {
                float hue = (float)(Main.timeForVisualEffects * 0.01) % 1f;
                Color rainbow = Main.hslToRgb(hue, 0.8f, 0.6f);
                CustomParticles.GenericFlare(npc.Center + Main.rand.NextVector2Circular(50f, 50f),
                    rainbow, 0.3f + hpDrive * 0.15f, 15);
            }

            // Rose petal accents
            int petalInterval = Math.Max(1, (int)(80 - hpDrive * 40));
            if (timer % petalInterval == 0)
            {
                Phase10BossVFX.PizzicatoPop(npc.Center + Main.rand.NextVector2Circular(40f, 40f), RosePink);
            }

            // Bloom orbit every 12 frames at moderate+ intensity
            if (timer % 12 == 0 && hpDrive > 0.25f)
            {
                float angle = (float)Main.timeForVisualEffects * 0.035f + timer * 0.08f;
                Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * (45f + hpDrive * 25f);
                Color orbitColor = isPhase2 ? RosePink : WarmGold;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(orbitPos, Vector2.Zero, orbitColor, 0.25f + hpDrive * 0.15f, 15));
            }

            // Ascending golden sparkles at high drive
            if (hpDrive > 0.5f && timer % Math.Max(1, (int)(18 - hpDrive * 10)) == 0)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(2f, 3.5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, JubilantLight, 0.3f, 25));
            }
        }
    }
}