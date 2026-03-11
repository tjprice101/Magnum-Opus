using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.DiesIrae.Bosses.Systems
{
    /// <summary>
    /// Dies Irae boss shader-driven rendering system.
    /// Manages hellfire aura, judgment trail, apocalypse ray,
    /// wrath escalation veins, final dissolve, boss glow, and musical accents.
    /// </summary>
    public static class DiesIraeBossShaderSystem
    {
        // Theme colors
        private static readonly Color BloodRed = new Color(200, 30, 20);
        private static readonly Color DarkCrimson = new Color(120, 15, 15);
        private static readonly Color EmberOrange = new Color(220, 100, 30);
        private static readonly Color AshenBlack = new Color(25, 15, 10);
        private static readonly Color HellfireWhite = new Color(255, 220, 180);
        private static readonly Color JudgmentGold = new Color(255, 180, 50);

        /// <summary>
        /// Draws the hellfire aura with 3-layer bloom stacking.
        /// Outer dark crimson halo, mid ember pulse, inner white-hot core.
        /// </summary>
        public static void DrawHellfireAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int hpTier, bool isEnraged)
        {
            float baseRadius = 90f + hpTier * 25f;
            float intensity = 0.4f + aggressionLevel * 0.4f + hpTier * 0.15f;
            if (isEnraged)
            {
                intensity *= 1.7f;
                baseRadius *= 1.3f;
            }

            Color primary = isEnraged ? EmberOrange : BloodRed;
            Color secondary = isEnraged ? BloodRed : DarkCrimson;

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.DiesHellfireAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.025f);

            // 3-layer bloom stacking over aura
            Vector2 drawPos = npc.Center - screenPos;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 origin = glow.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.06f) * 0.15f;
            float hpDrive = 1f - (npc.life / (float)npc.lifeMax);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer: dark crimson halo
            float outerScale = (baseRadius / glow.Width) * 3.5f * pulse;
            Color outerColor = DarkCrimson * (0.12f + hpDrive * 0.08f);
            outerColor.A = 0;
            sb.Draw(glow, drawPos, null, outerColor, 0f, origin, outerScale, SpriteEffects.None, 0f);

            // Mid: ember-orange pulse
            float midPulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.09f) * 0.2f;
            float midScale = (baseRadius / glow.Width) * 2.2f * midPulse;
            Color midColor = (isEnraged ? JudgmentGold : EmberOrange) * (0.18f + hpDrive * 0.1f);
            midColor.A = 0;
            sb.Draw(glow, drawPos, null, midColor, 0f, origin, midScale, SpriteEffects.None, 0f);

            // Core: white-hot center
            float coreScale = (baseRadius / glow.Width) * 1.0f;
            Color coreColor = HellfireWhite * (0.15f + hpDrive * 0.12f);
            coreColor.A = 0;
            sb.Draw(glow, drawPos, null, coreColor, 0f, origin, coreScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws the judgment trail during charges.
        /// </summary>
        public static void DrawJudgmentTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? EmberOrange : BloodRed;
            float width = isEnraged ? 1.7f : 1.2f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.DiesJudgmentTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.03f, 7f);
        }

        /// <summary>
        /// Draws the apocalypse ray beam flash.
        /// </summary>
        public static void DrawApocalypseRay(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.DiesApocalypseRay, BloodRed, intensity,
                (float)Main.timeForVisualEffects * 0.04f);
        }

        /// <summary>
        /// Draws the wrath escalation veins with ember sparks.
        /// </summary>
        public static void DrawWrathEscalation(SpriteBatch sb, NPC npc, Vector2 screenPos,
            int hpTier, float escalationProgress)
        {
            float veinCoverage = hpTier * 0.2f + escalationProgress * 0.1f;
            Color veinColor = Color.Lerp(DarkCrimson, EmberOrange, veinCoverage);
            float veinIntensity = 0.5f + veinCoverage * 1.5f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.DiesWrathEscalation, veinCoverage,
                DarkCrimson, EmberOrange, veinIntensity);

            if (hpTier >= 3 && Main.rand.NextBool(3))
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(
                    npc.width * 0.5f, npc.height * 0.5f);
                CustomParticles.GenericFlare(sparkPos, EmberOrange, 0.2f + hpTier * 0.05f, 10);
            }
        }

        /// <summary>
        /// Draws the final judgment dissolve with widening edge glow.
        /// </summary>
        public static void DrawFinalJudgmentDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.DiesFinalJudgmentDissolve, dissolveProgress,
                BloodRed, 0.05f);

            // Widening edge glow as dissolve progresses past 20%
            if (dissolveProgress > 0.2f)
            {
                float edgeIntensity = (dissolveProgress - 0.2f) / 0.8f;
                Vector2 drawPos = npc.Center - screenPos;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                Vector2 glowOrigin = glow.Size() / 2f;
                float glowScale = 0.8f + edgeIntensity * 1.6f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Color edgeColor = Color.Lerp(EmberOrange, HellfireWhite, edgeIntensity) * (0.2f + edgeIntensity * 0.3f);
                edgeColor.A = 0;
                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin, glowScale, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Draws the boss glow underlay. Normal = menacing crimson aura.
        /// Enraged = blazing gold-white inferno.
        /// Called as SHADER LAYER 0 before all other shader passes.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, bool isEnraged)
        {
            Vector2 drawPos = npc.Center - screenPos;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 origin = glow.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.12f;
            float hpDrive = 1f - (npc.life / (float)npc.lifeMax);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (isEnraged)
            {
                // Outer: blazing ember halo
                Color outerColor = JudgmentGold * (0.14f + hpDrive * 0.08f);
                outerColor.A = 0;
                sb.Draw(glow, drawPos, null, outerColor, 0f, origin, 2.2f * pulse, SpriteEffects.None, 0f);

                // Mid: crimson fire
                float midPulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.1f) * 0.2f;
                Color midColor = BloodRed * (0.2f + hpDrive * 0.1f);
                midColor.A = 0;
                sb.Draw(glow, drawPos, null, midColor, 0f, origin, 1.4f * midPulse, SpriteEffects.None, 0f);

                // Core: white-hot center
                Color coreColor = HellfireWhite * (0.18f + hpDrive * 0.1f);
                coreColor.A = 0;
                sb.Draw(glow, drawPos, null, coreColor, 0f, origin, 0.7f, SpriteEffects.None, 0f);
            }
            else
            {
                // Outer: dark crimson haze
                Color outerColor = DarkCrimson * (0.1f + hpDrive * 0.06f);
                outerColor.A = 0;
                sb.Draw(glow, drawPos, null, outerColor, 0f, origin, 1.8f * pulse, SpriteEffects.None, 0f);

                // Core: ember glow
                float corePulse = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.08f) * 0.15f;
                Color coreColor = EmberOrange * (0.12f + hpDrive * 0.08f);
                coreColor.A = 0;
                sb.Draw(glow, drawPos, null, coreColor, 0f, origin, 0.9f * corePulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Spawns musical accent particles with HP-driven intensity scaling.
        /// Intervals shrink and effects intensify as the Herald's wrath builds.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int hpTier, bool isEnraged)
        {
            float hpDrive = 1f - (npc.life / (float)npc.lifeMax);

            // Timpani drumroll impacts - interval shrinks with HP
            int timpaniInterval = Math.Max(1, (int)(80 - hpDrive * 40));
            if (timer % timpaniInterval == 0)
            {
                Phase10BossVFX.TimpaniDrumrollImpact(npc.Center, BloodRed, 0.6f + hpTier * 0.2f + hpDrive * 0.3f);
            }

            // Sforzando spikes
            int sforzandoInterval = Math.Max(1, (int)(50 - hpDrive * 25));
            if (timer % sforzandoInterval == 0 && hpTier >= 1)
            {
                Phase10BossVFX.SforzandoSpike(npc.Center, EmberOrange, 0.5f + hpTier * 0.15f + hpDrive * 0.2f);
            }

            // Hellfire burst accents
            int hellfireInterval = Math.Max(1, (int)(40 - hpDrive * 20));
            if (timer % hellfireInterval == 0 && hpTier >= 2)
            {
                CustomParticles.DiesIraeHellfireBurst(npc.Center, 4 + hpTier * 2 + (int)(hpDrive * 4));
            }

            // Enraged ember shower
            int emberInterval = Math.Max(1, (int)(15 - hpDrive * 8));
            if (isEnraged && timer % emberInterval == 0)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(60f, 60f);
                CustomParticles.GenericFlare(sparkPos, EmberOrange, 0.25f + hpDrive * 0.15f, 12);
            }

            // Dissonance storm at high tiers
            int dissonanceInterval = Math.Max(1, (int)(120 - hpDrive * 60));
            if (timer % dissonanceInterval == 0 && hpTier >= 3)
            {
                Phase10BossVFX.DissonanceStorm(npc.Center, 100f + hpDrive * 50f, BloodRed, EmberOrange);
            }

            // Bloom orbit every 10 frames at high intensity
            if (timer % 10 == 0 && hpDrive > 0.3f)
            {
                float angle = (float)Main.timeForVisualEffects * 0.04f + timer * 0.1f;
                Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * (50f + hpDrive * 30f);
                Color orbitColor = isEnraged ? JudgmentGold : EmberOrange;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(orbitPos, Vector2.Zero, orbitColor, 0.3f + hpDrive * 0.2f, 15));
            }

            // Ascending hellfire sparks at high wrath
            if (hpDrive > 0.5f && timer % Math.Max(1, (int)(20 - hpDrive * 12)) == 0)
            {
                Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 4f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, HellfireWhite, 0.3f, 25));
            }
        }
    }
}