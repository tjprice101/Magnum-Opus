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
    /// Nachtmusik boss shader-driven rendering — Queen of Radiance.
    /// 4-phase system: Evening Star → Cosmic Dance → Celestial Crescendo → Supernova.
    /// Phase-appropriate aura, dash trail, glow, musical accents.
    /// Palette: deep indigo, starlight silver, cosmic blue, nebula purple, white radiance — NO gold.
    /// </summary>
    public static class NachtmusikBossShaderSystem
    {
        // Theme palette
        private static readonly Color DeepIndigo = new Color(25, 20, 65);
        private static readonly Color StarlightSilver = new Color(200, 215, 240);
        private static readonly Color CosmicBlue = new Color(60, 100, 190);
        private static readonly Color NebulaPurple = new Color(70, 40, 120);
        private static readonly Color WhiteRadiance = new Color(245, 245, 255);
        private static readonly Color MidnightIndigo = new Color(15, 12, 40);

        /// <summary>
        /// Draws the starfield aura with 3-layer bloom stacking.
        /// Evolves per phase: gentle indigo halo → orbiting cosmic → galaxy radiance → supernova blaze.
        /// </summary>
        public static void DrawStarfieldAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float aggressionLevel, int phase)
        {
            float baseRadius = 90f + phase * 20f;
            float intensity = 0.3f + aggressionLevel * 0.4f;
            if (phase >= 4)
            {
                intensity *= 2.0f;
                baseRadius *= 1.6f;
            }
            else if (phase >= 3)
            {
                intensity *= 1.5f;
                baseRadius *= 1.3f;
            }
            else if (phase >= 2)
            {
                intensity *= 1.3f;
                baseRadius *= 1.15f;
            }

            Color primary = phase >= 4 ? WhiteRadiance : (phase >= 3 ? CosmicBlue : (phase >= 2 ? NebulaPurple : DeepIndigo));
            Color secondary = phase >= 4 ? CosmicBlue : (phase >= 3 ? NebulaPurple : StarlightSilver);

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

            // Outer halo — phase-driven color
            Color outerColor = phase >= 4
                ? WhiteRadiance * 0.18f
                : (phase >= 3 ? CosmicBlue * 0.15f : (phase >= 2 ? NebulaPurple * 0.13f : DeepIndigo * 0.12f));
            outerColor.A = 0;
            sb.Draw(glow, drawPos, null, outerColor, 0f, glowOrigin, baseRadius / glow.Width * 2.275f, SpriteEffects.None, 0f);

            // Mid glow — pulsing
            Color midColor = phase >= 4
                ? CosmicBlue * (0.2f * pulse)
                : (phase >= 3 ? NebulaPurple * (0.18f * pulse) : StarlightSilver * (0.15f * pulse));
            midColor.A = 0;
            sb.Draw(glow, drawPos, null, midColor, 0f, glowOrigin, baseRadius / glow.Width * 1.43f, SpriteEffects.None, 0f);

            // Core — bright
            Color coreColor = WhiteRadiance * (phase >= 4 ? 0.3f : 0.2f);
            coreColor.A = 0;
            sb.Draw(glow, drawPos, null, coreColor, 0f, glowOrigin, baseRadius / glow.Width * 0.65f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws the nebula dash trail — silver-blue in early phases, prismatic in P3, blinding in P4.
        /// </summary>
        public static void DrawNebulaDashTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin, int phase)
        {
            Color trailColor = phase >= 4 ? WhiteRadiance : (phase >= 3 ? CosmicBlue : StarlightSilver);
            float width = phase >= 4 ? 2.0f : (phase >= 3 ? 1.5f : (phase >= 2 ? 1.2f : 1.0f));

            BossRenderHelper.DrawShaderTrail(sb, npc, screenPos,
                texture, sourceRect, origin,
                BossShaderManager.NachtmusikNebulaDashTrail, trailColor, width,
                (float)Main.timeForVisualEffects * 0.02f, 7f);
        }

        /// <summary>
        /// Draws the supernova blast during SupernovaCollapse attack.
        /// </summary>
        public static void DrawSupernovaBlast(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            float intensity)
        {
            BossRenderHelper.DrawAttackFlash(sb, position, screenPos,
                BossShaderManager.NachtmusikSupernovaBlast, WhiteRadiance, intensity,
                (float)Main.timeForVisualEffects * 0.035f);
        }

        /// <summary>
        /// Draws phase transition effect — used between any phase boundary.
        /// </summary>
        public static void DrawPhaseTransition(SpriteBatch sb, NPC npc, Vector2 screenPos,
            float transitionProgress, int fromPhase, int toPhase)
        {
            Color from = fromPhase >= 3 ? CosmicBlue : (fromPhase >= 2 ? NebulaPurple : DeepIndigo);
            Color to = toPhase >= 4 ? WhiteRadiance : (toPhase >= 3 ? CosmicBlue : (toPhase >= 2 ? NebulaPurple : StarlightSilver));
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

                Color edgeColor = WhiteRadiance * (0.25f * edgeIntensity);
                edgeColor.A = 0;
                float edgeScale = 0.8f + edgeIntensity * 1.2f;
                sb.Draw(glow, drawPos, null, edgeColor, 0f, glowOrigin, edgeScale, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Draws the boss glow underlay — SHADER LAYER 0.
        /// 4-phase progression: serene silver-indigo → cosmic blue → deep purple → blinding white.
        /// </summary>
        public static void DrawBossGlow(SpriteBatch sb, NPC npc, Vector2 screenPos, int phase)
        {
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Vector2 drawPos = npc.Center - screenPos;
            Vector2 origin = glow.Size() * 0.5f;
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.timeForVisualEffects * 0.05f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            switch (phase)
            {
                case 4: // Supernova — blinding white/silver core with cosmic blue halo
                    Color outer4 = CosmicBlue * 0.22f;
                    outer4.A = 0;
                    sb.Draw(glow, drawPos, null, outer4, 0f, origin, 2.4f, SpriteEffects.None, 0f);

                    Color mid4 = StarlightSilver * (0.25f * pulse);
                    mid4.A = 0;
                    sb.Draw(glow, drawPos, null, mid4, 0f, origin, 1.5f, SpriteEffects.None, 0f);

                    Color core4 = WhiteRadiance * 0.2f;
                    core4.A = 0;
                    sb.Draw(glow, drawPos, null, core4, 0f, origin, 0.7f, SpriteEffects.None, 0f);
                    break;

                case 3: // Celestial Crescendo — deep cosmic blue + purple shimmer
                    Color outer3 = CosmicBlue * 0.18f;
                    outer3.A = 0;
                    sb.Draw(glow, drawPos, null, outer3, 0f, origin, 2.0f, SpriteEffects.None, 0f);

                    Color mid3 = NebulaPurple * (0.18f * pulse);
                    mid3.A = 0;
                    sb.Draw(glow, drawPos, null, mid3, 0f, origin, 1.3f, SpriteEffects.None, 0f);

                    Color core3 = WhiteRadiance * 0.15f;
                    core3.A = 0;
                    sb.Draw(glow, drawPos, null, core3, 0f, origin, 0.6f, SpriteEffects.None, 0f);
                    break;

                case 2: // Cosmic Dance — nebula purple outer, cosmic blue core
                    Color outer2 = NebulaPurple * 0.15f;
                    outer2.A = 0;
                    sb.Draw(glow, drawPos, null, outer2, 0f, origin, 1.8f, SpriteEffects.None, 0f);

                    Color mid2 = CosmicBlue * (0.16f * pulse);
                    mid2.A = 0;
                    sb.Draw(glow, drawPos, null, mid2, 0f, origin, 1.1f, SpriteEffects.None, 0f);

                    Color core2 = StarlightSilver * 0.12f;
                    core2.A = 0;
                    sb.Draw(glow, drawPos, null, core2, 0f, origin, 0.5f, SpriteEffects.None, 0f);
                    break;

                default: // Evening Star — serene indigo + silver
                    Color outer1 = DeepIndigo * 0.14f;
                    outer1.A = 0;
                    sb.Draw(glow, drawPos, null, outer1, 0f, origin, 1.6f, SpriteEffects.None, 0f);

                    Color core1 = StarlightSilver * (0.12f * pulse);
                    core1.A = 0;
                    sb.Draw(glow, drawPos, null, core1, 0f, origin, 0.8f, SpriteEffects.None, 0f);
                    break;
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// HP-driven musical VFX accents. Escalates per phase.
        /// P1: gentle starlight convergence. P2: arpeggio + metronome. P3: prismatic bursts. P4: constant radiance.
        /// </summary>
        public static void SpawnMusicalAccents(NPC npc, int timer, int phase)
        {
            float hpDrive = 1f - (float)npc.life / npc.lifeMax;

            // Starlight convergence — interval shrinks with HP: 100→50
            int convergenceInterval = Math.Max(1, (int)(100 - hpDrive * 50));
            if (timer % convergenceInterval == 0)
            {
                Color convColor = phase >= 3 ? CosmicBlue : StarlightSilver;
                Phase10BossVFX.StaffLineConvergence(npc.Center, convColor, 0.5f + phase * 0.15f);
            }

            // Rhythmic metronome — interval: 60→30, phase 2+
            int metronomeInterval = Math.Max(1, (int)(60 - hpDrive * 30));
            if (timer % metronomeInterval == 0 && phase >= 2)
            {
                Color metColor = phase >= 3 ? NebulaPurple : DeepIndigo;
                Phase10BossVFX.MetronomeTickWarning(npc.Center, metColor, 3, 4);
            }

            // Phase 3+: prismatic flares — interval: 40→20
            int flareInterval = Math.Max(1, (int)(40 - hpDrive * 20));
            if (timer % flareInterval == 0 && phase >= 3)
            {
                Color flareColor = Color.Lerp(CosmicBlue, NebulaPurple, Main.rand.NextFloat());
                CustomParticles.GenericFlare(npc.Center + Main.rand.NextVector2Circular(60f, 60f),
                    flareColor, 0.3f, 15);
            }

            // Phase 4: sforzando spikes of white radiance — interval: 30→15
            int spikeInterval = Math.Max(1, (int)(30 - hpDrive * 15));
            if (timer % spikeInterval == 0 && phase >= 4)
            {
                Phase10BossVFX.SforzandoSpike(npc.Center, WhiteRadiance, 0.6f);
            }

            // Bloom orbit every 12 frames — phase-colored
            if (timer % 12 == 0)
            {
                float orbitAngle = timer * 0.08f;
                Vector2 orbitPos = npc.Center + orbitAngle.ToRotationVector2() * 70f;
                Color orbitColor = phase >= 4 ? WhiteRadiance : (phase >= 3 ? CosmicBlue : StarlightSilver);
                var bloom = new BloomParticle(orbitPos, Vector2.Zero, orbitColor * 0.4f, 0.3f, 15);
                MagnumParticleHandler.SpawnParticle(bloom);
            }

            // Ascending sparkles when HP < 50% — silver/white, never gold
            if (hpDrive > 0.5f && timer % 20 == 0)
            {
                Color sparkleColor = phase >= 4 ? WhiteRadiance : StarlightSilver;
                var sparkle = new SparkleParticle(
                    npc.Center + Main.rand.NextVector2Circular(40f, 40f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f),
                    sparkleColor, 0.35f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
}