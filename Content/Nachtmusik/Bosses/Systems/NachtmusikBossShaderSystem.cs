using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Nachtmusik.Bosses.Systems
{
    /// <summary>
    /// Nachtmusik boss shader-driven rendering system.
    /// Phase 1 (serene nocturnal) vs Phase 2 (violent cosmic storm) visual differentiation.
    /// Enhanced with 3-layer bloom stacking, DrawBossGlow, HP-driven musical accents.
    /// </summary>
    public static class NachtmusikBossShaderSystem
    {
        // Theme colors
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color StarlightSilver = new Color(200, 210, 230);
        private static readonly Color CosmicBlue = new Color(80, 120, 200);
        private static readonly Color NebulaGold = new Color(220, 180, 100);
        private static readonly Color MidnightBlue = new Color(15, 15, 45);
        private static readonly Color StarWhite = new Color(200, 210, 240);

        /// <summary>
        /// Draws the starfield aura with 3-layer bloom stacking.
        /// Phase 1: gentle indigo halo. Phase 2: fierce cosmic storm aura.
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

            // 3-layer bloom stacking
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 glowOrigin = glow.Size() * 0.5f;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.04f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer halo
            Color outerColor = (isPhase2 ? CosmicBlue : DeepIndigo) * 0.12f;
            outerColor.A = 0;
            sb.Draw(glow, drawPos, null, outerColor, 0f, glowOrigin, baseRadius / glow.Width * 3.5f, SpriteEffects.None, 0f);

            // Mid glow - pulsing
            Color midColor = (isPhase2 ? NebulaGold : StarlightSilver) * (0.15f * pulse);
            midColor.A = 0;
            sb.Draw(glow, drawPos, null, midColor, 0f, glowOrigin, baseRadius / glow.Width * 2.2f, SpriteEffects.None, 0f);

            // Core - bright edge
            Color coreColor = StarWhite * 0.2f;
            coreColor.A = 0;
            sb.Draw(glow, drawPos, null, coreColor, 0f, glowOrigin, baseRadius / glow.Width * 1.0f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws the nebula dash trail.
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
        /// Draws the supernova blast during SupernovaCollapse.
        /// </summary>
        public static void DrawSupernovaBlast(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.NachtmusikSupernovaBlast, StarlightSilver, intensity,
                (float)Main.timeForVisualEffects * 0.035f);
        }

        /// <summary>
        /// Draws the Phase 2 awakening transition.
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
        /// Draws the stellar dissolve death effect with widening edge glow.
        /// </summary>
        public static void DrawStellarDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, float dissolveProgress)
        {
            BossRenderHelper.DrawDissolve(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.NachtmusikStellarDissolve, dissolveProgress,
                StarlightSilver, 0.05f);

            // Widening edge glow after 20% dissolve
            if (dissolveProgress > 0.2f)
            {
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                Vector2 drawPos = npc.Center - screenPos;
                Vector2 glowOrigin = glow.Size() * 0.5f;
                float edgeIntensity = (dissolveProgress - 0.2f) / 0.8f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Color edgeColor = StarWhite * (0.25f * edgeIntensity);
                edgeColor.A = 0;
                float edgeScale = 0.8f + edgeIntensity * 1.2f;
                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin, edgeScale, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Draws the boss glow underlay - SHADER LAYER 0.
        /// Phase 1: serene silver-indigo. Phase 2: fierce gold-blue cosmic storm.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, bool isPhase2)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() * 0.5f;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.05f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (isPhase2)
            {
                // Fierce cosmic storm glow
                Color outer = NebulaGold * 0.18f;
                outer.A = 0;
                sb.Draw(glow, drawPos, null, outer, 0f, origin, 2.0f, SpriteEffects.None, 0f);

                Color mid = CosmicBlue * (0.2f * pulse);
                mid.A = 0;
                sb.Draw(glow, drawPos, null, mid, 0f, origin, 1.3f, SpriteEffects.None, 0f);

                Color core = StarWhite * 0.15f;
                core.A = 0;
                sb.Draw(glow, drawPos, null, core, 0f, origin, 0.6f, SpriteEffects.None, 0f);
            }
            else
            {
                // Serene nocturnal glow
                Color outer = DeepIndigo * 0.14f;
                outer.A = 0;
                sb.Draw(glow, drawPos, null, outer, 0f, origin, 1.6f, SpriteEffects.None, 0f);

                Color core = StarlightSilver * (0.12f * pulse);
                core.A = 0;
                sb.Draw(glow, drawPos, null, core, 0f, origin, 0.8f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// HP-driven musical VFX. Lower HP = faster intervals, bloom orbit, ascending sparkles.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int phase, bool isPhase2)
        {
            float hpDrive = 1f - (float)npc.life / npc.lifeMax;

            // Starlight convergence - interval shrinks: 100 to 50
            int convergenceInterval = Math.Max(1, (int)(100 - hpDrive * 50));
            if (timer % convergenceInterval == 0)
            {
                Phase10BossVFX.StaffLineConvergence(npc.Center,
                    isPhase2 ? CosmicBlue : StarlightSilver, 0.5f + phase * 0.15f);
            }

            // Rhythmic metronome - interval: 60 to 30
            int metronomeInterval = Math.Max(1, (int)(60 - hpDrive * 30));
            if (timer % metronomeInterval == 0 && phase >= 1)
            {
                Phase10BossVFX.MetronomeTickWarning(npc.Center,
                    isPhase2 ? NebulaGold : DeepIndigo, 3, 4);
            }

            // Phase 2 cosmic storm flares - interval: 40 to 20
            int flareInterval = Math.Max(1, (int)(40 - hpDrive * 20));
            if (timer % flareInterval == 0 && isPhase2)
            {
                CustomParticles.GenericFlare(npc.Center + Main.rand.NextVector2Circular(60f, 60f),
                    Color.Lerp(CosmicBlue, NebulaGold, Main.rand.NextFloat()), 0.3f, 15);
            }

            // Phase 2 sforzando spikes - interval: 30 to 15
            int spikeInterval = Math.Max(1, (int)(30 - hpDrive * 15));
            if (timer % spikeInterval == 0 && isPhase2)
            {
                Phase10BossVFX.SforzandoSpike(npc.Center, NebulaGold, 0.6f);
            }

            // Bloom orbit every 12 frames
            if (timer % 12 == 0)
            {
                float orbitAngle = timer * 0.08f;
                Vector2 orbitPos = npc.Center + orbitAngle.ToRotationVector2() * 70f;
                Color orbitColor = isPhase2 ? NebulaGold : StarlightSilver;
                var bloom = new BloomParticle(orbitPos, Vector2.Zero, orbitColor * 0.4f, 0.3f, 15);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Ascending sparkles when HP < 50%
            if (hpDrive > 0.5f && timer % 20 == 0)
            {
                Color sparkleColor = isPhase2 ? StarWhite : StarlightSilver;
                var sparkle = new SparkleParticle(
                    npc.Center + Main.rand.NextVector2Circular(40f, 40f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f),
                    sparkleColor, 0.35f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
}