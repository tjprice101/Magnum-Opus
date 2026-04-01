using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Summer.Bosses.Systems
{
    /// <summary>
    /// L'Estate boss shader-driven rendering system.
    /// Phase-routing DrawBossVFX, corona flame, god rays, solar aura,
    /// afterburn accumulation, eclipse screen effect, musical accents.
    /// </summary>
    public static class LEstateBossShaderSystem
    {
        // Vivaldi's Summer palette
        private static readonly Color SunGold = new Color(255, 200, 50);
        private static readonly Color BlazingOrange = new Color(255, 140, 40);
        private static readonly Color WhiteHot = new Color(255, 250, 240);
        private static readonly Color DeepAmber = new Color(180, 100, 20);
        private static readonly Color StormAmber = new Color(150, 90, 30);
        private static readonly Color EclipseDark = new Color(30, 15, 5);
        private static readonly Color HeatRed = new Color(220, 60, 30);

        /// <summary>
        /// Main entry point: routes to phase-specific VFX layers.
        /// Call from LEstate.PreDraw.
        /// </summary>
        public static void DrawBossVFX(SpriteBatch sb, NPC npc, Vector2 screenPos,
            int phase, float phaseProgress, bool isEnraged, float aggressionLevel, int difficultyTier)
        {
            // Layer 0: Boss glow (behind everything)
            DrawBossGlow(sb, npc, screenPos, phase, isEnraged);

            // Layer 1: Solar aura (phase-morphing)
            float phaseIntensity = MathHelper.Clamp((phase - 1) / 3f + aggressionLevel * 0.2f, 0f, 1f);
            DrawSolarAura(sb, npc, screenPos, phaseIntensity, difficultyTier, isEnraged);

            // Layer 2: Phase-specific effects
            switch (phase)
            {
                case 1:
                    // Scorching Stillness: gentle heat shimmer glow only
                    break;
                case 2:
                    // Gathering Storm: corona flame begins
                    DrawCoronaFlame(sb, npc, screenPos, 0.4f + aggressionLevel * 0.3f, isEnraged);
                    break;
                case 3:
                    // Full Tempest: roaring corona + god rays
                    DrawCoronaFlame(sb, npc, screenPos, 0.8f + aggressionLevel * 0.2f, isEnraged);
                    DrawGodRays(sb, npc, screenPos, 0.6f);
                    break;
            }

            // Layer 3: Eclipse overlay (enrage only)
            if (isEnraged)
            {
                DrawCoronaFlame(sb, npc, screenPos, 1.2f, true);
                DrawGodRays(sb, npc, screenPos, 1.0f);
            }
        }

        /// <summary>
        /// 3-layer boss glow drawn behind everything.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, int phase, bool isEnraged)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
            float pulse = (float)Math.Sin((float)Main.timeForVisualEffects * 0.06f) * 0.08f + 1f;

            Color outerC, midC, coreC;
            float outerS, midS, coreS;

            if (isEnraged)
            {
                outerC = WhiteHot * 0.25f;
                midC = SunGold * 0.35f;
                coreC = WhiteHot * 0.50f;
                outerS = 3.0f; midS = 1.8f; coreS = 0.8f;
            }
            else
            {
                float phaseScale = 1f + (phase - 1) * 0.3f;
                outerC = SunGold * (0.12f * phaseScale);
                midC = BlazingOrange * (0.18f * phaseScale);
                coreC = WhiteHot * (0.25f * phaseScale);
                outerS = 1.6f * phaseScale; midS = 1.0f * phaseScale; coreS = 0.5f * phaseScale;
            }

            outerC.A = 0; midC.A = 0; coreC.A = 0;

            sb.Draw(glow, drawPos, null, outerC, 0f, glowOrigin, outerS * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, midC, 0f, glowOrigin, midS * pulse, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, coreC, 0f, glowOrigin, coreS * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws phase-morphing solar aura via shader.
        /// </summary>
        public static void DrawSolarAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float phaseIntensity, int difficultyTier, bool isEnraged)
        {
            float baseRadius = 95f + difficultyTier * 22f;
            float intensity = 0.35f + phaseIntensity * 0.5f;
            if (isEnraged) intensity *= 1.6f;

            Color primary = isEnraged ? WhiteHot : SunGold;
            Color secondary = isEnraged ? SunGold : BlazingOrange;

            Effect shader = BossShaderManager.GetShader(BossShaderManager.EstateSolarAura);
            if (shader != null)
            {
                // Set the new uPhaseIntensity parameter
                shader.Parameters["uPhaseIntensity"]?.SetValue(phaseIntensity);
            }

            BossRenderHelper.DrawShaderAura(sb, npc, screenPos,
                BossShaderManager.EstateSolarAura, primary, secondary,
                baseRadius, intensity, (float)Main.timeForVisualEffects * 0.025f);

            // 3-layer additive bloom on aura
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 bOrigin = new Vector2(bloom.Width / 2f, bloom.Height / 2f);
            float pulse = (float)Math.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.06f + 1f;

            Color outerBloom = (isEnraged ? HeatRed : BlazingOrange) * (0.10f * intensity);
            Color midBloom = Color.Lerp(SunGold, WhiteHot, 0.3f) * (0.15f * intensity);
            Color coreBloom = WhiteHot * (0.22f * intensity);
            outerBloom.A = 0; midBloom.A = 0; coreBloom.A = 0;

            sb.Draw(bloom, drawPos, null, outerBloom, 0f, bOrigin, 3.5f * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, midBloom, 0f, bOrigin, 2.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloom, drawPos, null, coreBloom, 0f, bOrigin, 1.0f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Corona flame: multi-layer FBM fire around boss (Phase 2+).
        /// Uses EstateCoronaFlame shader on a quad.
        /// </summary>
        public static void DrawCoronaFlame(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float intensity, bool isEnraged)
        {
            Effect shader = ShaderLoader.GetShader("Seasons/EstateCoronaFlame");
            Texture2D quad = MagnumTextureRegistry.GetSoftGlow();
            if (shader == null || quad == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = new Vector2(quad.Width / 2f, quad.Height / 2f);

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            shader.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.025f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uRadius"]?.SetValue(isEnraged ? 0.8f : 0.5f);
            shader.Parameters["uTurbulence"]?.SetValue(isEnraged ? 3.5f : 2.5f);
            shader.Parameters["uFlameSpeed"]?.SetValue(isEnraged ? 1.5f : 0.8f);
            shader.Parameters["uPrimaryColor"]?.SetValue(WhiteHot.ToVector4());
            shader.Parameters["uSecondaryColor"]?.SetValue(SunGold.ToVector4());
            shader.Parameters["uTertiaryColor"]?.SetValue((isEnraged ? HeatRed : BlazingOrange).ToVector4());
            shader.CurrentTechnique.Passes[0].Apply();

            float drawScale = (120f + intensity * 60f) / quad.Width;
            sb.Draw(quad, drawPos, null, Color.White, 0f, origin, drawScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// God rays: radial beams of light emanating from boss center.
        /// Uses existing beam textures for soft additive streaks.
        /// </summary>
        public static void DrawGodRays(SpriteBatch sb, NPC npc, Vector2 screenPos, float intensity)
        {
            Texture2D beam = MagnumTextureRegistry.GetBloom();
            if (beam == null) return;

            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = new Vector2(beam.Width / 2f, beam.Height / 2f);
            float time = (float)Main.timeForVisualEffects * 0.008f;
            int rayCount = 8;

            for (int i = 0; i < rayCount; i++)
            {
                float angle = MathHelper.TwoPi * i / rayCount + time;
                float flicker = (float)Math.Sin(time * 3f + i * 1.7f) * 0.3f + 0.7f;
                Color rayColor = Color.Lerp(SunGold, WhiteHot, (float)i / rayCount) * (intensity * 0.08f * flicker);
                rayColor.A = 0;

                float rayLength = 180f + intensity * 80f;
                Vector2 rayEnd = drawPos + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * rayLength;
                float rayAngle = (float)Math.Atan2(rayEnd.Y - drawPos.Y, rayEnd.X - drawPos.X);
                float len = Vector2.Distance(drawPos, rayEnd);

                sb.Draw(beam, drawPos, null, rayColor, rayAngle, origin,
                    new Vector2(len / beam.Width, 0.15f), SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws heat haze trail distortion when boss moves fast.
        /// </summary>
        public static void DrawHeatHazeTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, bool isEnraged)
        {
            Color trailColor = isEnraged ? HeatRed : BlazingOrange;
            float width = isEnraged ? 1.6f : 1.1f;

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EstateHeatHazeTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.03f, 7f);
        }

        /// <summary>
        /// Solar flare attack blast using the sunspot eruption shader.
        /// </summary>
        public static void DrawSolarFlare(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float progress)
        {
            Effect shader = BossShaderManager.GetShader(BossShaderManager.EstateSolarFlare);
            if (shader != null)
                shader.Parameters["uProgress"]?.SetValue(progress);

            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.EstateSolarFlare, SunGold, 1f,
                (float)Main.timeForVisualEffects * 0.035f);
        }

        /// <summary>
        /// Zenith beam with lightning-infused edges.
        /// </summary>
        public static void DrawZenithBeam(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, bool isLatePhase)
        {
            Color from = isLatePhase ? BlazingOrange : SunGold;
            Color to = isLatePhase ? WhiteHot : new Color(255, 240, 180);
            float intensity = isLatePhase ? 1.4f : 0.9f;

            BossRenderHelper.DrawPhaseTransition(sb, npc, screenPos,
                BossShaderManager.EstateZenithBeam, transitionProgress,
                from, to, intensity);
        }

        /// <summary>
        /// Supernova death dissolve - inward collapse to white-hot point.
        /// </summary>
        public static void DrawSupernovaDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.EstateSupernovaDissolve, dissolveProgress,
                SunGold, 0.08f);

            // Convergence glow: bright center that intensifies as dissolve progresses
            if (dissolveProgress > 0.15f)
            {
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow == null) return;

                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
                float convergence = (dissolveProgress - 0.15f) / 0.85f;
                float convergePow = convergence * convergence;

                // Core brightens as everything collapses inward
                Color coreColor = Color.Lerp(SunGold, WhiteHot, convergePow) * (convergePow * 0.6f);
                coreColor.A = 0;
                float coreScale = 2f - convergePow * 1.2f; // Shrinks as it brightens
                sb.Draw(glow, drawPos, null, coreColor, 0f, glowOrigin, coreScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// HP-driven musical accents: bloom orbit, ascending sparkles, rhythmic pulses.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int difficultyTier, int phase)
        {
            float hpDrive = 1f - (npc.life / (float)npc.lifeMax);

            // Solar flare pulse scales with HP and phase
            int flareInterval = Math.Max(1, (int)MathHelper.Lerp(120, 50, hpDrive) - (phase - 1) * 10);
            if (timer % flareInterval == 0)
                Phase10BossVFX.StaffLineConvergence(npc.Center, SunGold, 0.5f + difficultyTier * 0.2f);

            // Rhythmic heat pulse (Phase 2+)
            if (phase >= 2)
            {
                int heatInterval = Math.Max(1, (int)MathHelper.Lerp(60, 25, hpDrive));
                if (timer % heatInterval == 0)
                    Phase10BossVFX.MetronomeTickWarning(npc.Center, BlazingOrange, 3, 6);
            }

            // Solar burst accents (Phase 3+)
            if (phase >= 3)
            {
                int burstInterval = Math.Max(1, (int)MathHelper.Lerp(90, 40, hpDrive));
                if (timer % burstInterval == 0)
                {
                    Phase10BossVFX.AccelerandoSpiral(npc.Center, HeatRed, 0.7f);
                    CustomParticles.GenericMusicNotes(npc.Center, SunGold, 3, 50f);
                }
            }

            // Bloom orbit every 10 frames
            if (timer % 10 == 0)
            {
                float angle = (float)Main.timeForVisualEffects * 0.04f;
                Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * (55f + hpDrive * 25f);
                Color orbitColor = Color.Lerp(SunGold, HeatRed, hpDrive) * 0.3f;
                orbitColor.A = 0;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(orbitPos, Vector2.Zero, orbitColor, 0.2f + hpDrive * 0.15f, 8));
            }

            // Ascending sparkles at high damage
            if (hpDrive > 0.5f)
            {
                int sparkInterval = Math.Max(1, (int)MathHelper.Lerp(15, 6, hpDrive));
                if (timer % sparkInterval == 0)
                {
                    Vector2 sparkPos = npc.Center + Main.rand.NextVector2Circular(30f, 30f);
                    Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1.5f, 3f));
                    MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, WhiteHot, 0.25f, 20));
                }
            }
        }
    }
}
